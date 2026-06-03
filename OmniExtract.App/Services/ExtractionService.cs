using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;
using OmniExtract.App.Services;

namespace OmniExtract.App.Services;

public class ExtractionService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private const string SystemPrompt = """
        You are a universal document extraction engine.

        Given the content of a document (text or images), extract everything that is actually present.
        Do NOT infer, assume, or flag missing fields — only output what the document contains.

        Perform the following:
        1. Identify the document type (e.g. invoice, contract, report, email, form, etc.)
        2. Extract ALL fields, values, tables, dates, names, amounts, and entities present in the document
        3. Generate a flat array of descriptive tags (document type, language(s), year, domain, entity names, etc.)
        4. Assign categories: domain (finance/legal/hr/technical/general), subdomain, and sensitivity (public/internal/confidential/restricted)
        5. Score your confidence in extraction quality from 0.0 to 1.0

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "meta": {
            "document_type": "string",
            "language": ["ISO-code"],
            "confidence": 0.0,
            "extraction_method": "text|vision",
            "warnings": []
          },
          "tags": ["tag1", "tag2"],
          "categories": {
            "domain": "string",
            "subdomain": "string",
            "sensitivity": "public|internal|confidential|restricted"
          },
          "data": { "field_name": "value" },
          "tables": [
            [["header1", "header2"], ["row1col1", "row1col2"]]
          ]
        }

        STRICT RULES:
        - Never wrap the JSON in markdown fences or code blocks
        - Never truncate tables or arrays — include ALL rows
        - Never add comments (no // or /* */ inside JSON)
        - Never invent fields that are not present in the document
        - Respond with raw, valid JSON only
        """;

    private readonly GptClient _gpt;
    private readonly TokenCounter _tokens;
    private readonly ProcessingSettings _settings;
    private readonly string _visionModel;
    private readonly ILogger<ExtractionService> _logger;

    public ExtractionService(GptClient gpt, TokenCounter tokens, IOptions<ProcessingSettings> settings, IOptions<OpenAISettings> openai, ILogger<ExtractionService> logger)
    {
        _gpt = gpt;
        _tokens = tokens;
        _settings = settings.Value;
        _visionModel = openai.Value.VisionModel;
        _logger = logger;
    }

    public async Task<UniversalOutput> ExtractAsync(string filePath, ExtractionResult extracted, CancellationToken ct = default)
    {
        var meta = new OutputMeta { SourceFile = Path.GetFileName(filePath) };

        if (extracted.Error is not null && string.IsNullOrWhiteSpace(extracted.Text) && extracted.Images.Count == 0)
        {
            meta.ExtractionMethod = "failed";
            meta.Warnings.Add(extracted.Error);
            return new UniversalOutput { Meta = meta };
        }

        try
        {
            UniversalOutput result;
            if (extracted.Images.Count > 0)
            {
                meta.ExtractionMethod = "vision";
                result = await ExtractVisionAsync(extracted.Images, ct);
            }
            else
            {
                meta.ExtractionMethod = "text";
                result = await ExtractTextAsync(extracted.Text, ct);
            }

            // Merge provenance from our meta
            result.Meta.SourceFile = meta.SourceFile;
            if (string.IsNullOrWhiteSpace(result.Meta.ExtractionMethod))
                result.Meta.ExtractionMethod = meta.ExtractionMethod;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI extraction failed for {File}", Path.GetFileName(filePath));
            meta.ExtractionMethod = "failed";
            meta.Confidence = 0;
            meta.Warnings.Add(ex.Message);
            return new UniversalOutput
            {
                Meta = meta,
                Data = new Dictionary<string, object?> { ["raw_error"] = ex.Message }
            };
        }
    }

    private async Task<UniversalOutput> ExtractTextAsync(string text, CancellationToken ct)
    {
        var systemMsg = new SystemGptMessage(SystemPrompt);
        var baseTokens = _tokens.CountTokens(SystemPrompt) + _tokens.CountTokens(text) + _settings.MaxOutputTokens + _settings.TokenBuffer;

        if (baseTokens <= _settings.ModelContextLimit)
        {
            try
            {
                var messages = new List<GptMessage> { systemMsg, new UserGptMessage(text) };
                var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
                return ParseResponse(response, "text");
            }
            catch (Exception ex) when (GptClient.IsContextLimitExceeded(ex))
            {
                _logger.LogWarning("Context limit hit despite pre-flight check — falling back to chunking");
            }
        }

        // Chunked extraction — use conservative char-based splitting to avoid tokenizer skew
        _logger.LogInformation("Document too large — chunking...");
        const int InitialChunkChars = 80_000;
        var chunks = ChunkTextByChars(text, InitialChunkChars);
        _logger.LogInformation("Split into {Count} chunks", chunks.Count);

        var results = new List<UniversalOutput>();
        for (var i = 0; i < chunks.Count; i++)
        {
            _logger.LogInformation("Processing chunk {I}/{Total}", i + 1, chunks.Count);
            var chunkText = chunks[i];
            var prompt = chunks.Count > 1 ? $"[Chunk {i + 1} of {chunks.Count}]\n\n{chunkText}" : chunkText;
            var messages = new List<GptMessage> { systemMsg, new UserGptMessage(prompt) };
            var response = await CallWithHalvingAsync(messages, chunkText, systemMsg, i + 1, chunks.Count, ct);
            results.Add(ParseResponse(response, "text"));
        }

        return MergeResults(results);
    }

    private async Task<UniversalOutput> ExtractVisionAsync(List<string> images, CancellationToken ct)
    {
        var chunkSize = _settings.VisionChunkSize;
        var chunks = images.Chunk(chunkSize).ToList();
        var results = new List<UniversalOutput>();

        for (var i = 0; i < chunks.Count; i++)
        {
            _logger.LogInformation("Vision chunk {I}/{Total} ({Count} pages)", i + 1, chunks.Count, chunks[i].Length);
            var prompt = chunks.Count > 1
                ? $"[Pages {i * chunkSize + 1}-{Math.Min((i + 1) * chunkSize, images.Count)} of {images.Count}]\n\nExtract content from these pages."
                : "Extract all content from this document.";

            var messages = new List<GptMessage>
            {
                new SystemGptMessage(SystemPrompt),
                new UserGptMessage(prompt, chunks[i].ToList())
            };

            var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct, model: _visionModel);
            results.Add(ParseResponse(response, "vision"));
        }

        return MergeResults(results);
    }

    // Only strip // comments that appear at line start (after whitespace) — never inside string values
    private static readonly System.Text.RegularExpressions.Regex LineCommentRegex =
        new(@"^\s*//[^\n]*\n?", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Compiled);

    private static UniversalOutput ParseResponse(string json, string method)
    {
        try
        {
            // Strip JS-style line comments the model sometimes adds
            var cleaned = LineCommentRegex.Replace(json, string.Empty);
            var output = JsonSerializer.Deserialize<UniversalOutput>(cleaned, JsonOpts);
            if (output is not null)
            {
                if (string.IsNullOrWhiteSpace(output.Meta.ExtractionMethod))
                    output.Meta.ExtractionMethod = method;
                return output;
            }
        }
        catch { }

        return new UniversalOutput
        {
            Meta = new OutputMeta
            {
                Confidence = 0,
                ExtractionMethod = method,
                Warnings = ["JSON parse failed — raw response stored"]
            },
            Data = new Dictionary<string, object?> { ["raw_response"] = json }
        };
    }

    public static UniversalOutput MergeResults(List<UniversalOutput> results)
    {
        if (results.Count == 1) return results[0];

        var first = results[0];
        var merged = new UniversalOutput
        {
            Meta = new OutputMeta
            {
                SourceFile = first.Meta.SourceFile,
                DocumentType = first.Meta.DocumentType,
                Language = results.SelectMany(r => r.Meta.Language).Distinct().ToList(),
                Confidence = results.Average(r => r.Meta.Confidence),
                ExtractionMethod = first.Meta.ExtractionMethod,
                Warnings = results.SelectMany(r => r.Meta.Warnings).Distinct().ToList()
            },
            Tags = results.SelectMany(r => r.Tags).Distinct().ToList(),
            Categories = first.Categories,
            Data = results.SelectMany(r => r.Data).GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First().Value),
            Tables = results.SelectMany(r => r.Tables).ToList()
        };

        return merged;
    }

    private async Task<string> CallWithHalvingAsync(List<GptMessage> messages, string chunkText, SystemGptMessage systemMsg, int chunkNum, int totalChunks, CancellationToken ct, int depth = 0)
    {
        try
        {
            return await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
        }
        catch (Exception ex) when (GptClient.IsContextLimitExceeded(ex) && depth < 4)
        {
            _logger.LogWarning("Chunk {N} still too large — halving (depth {D})", chunkNum, depth + 1);
            var half = chunkText.Length / 2;
            var a = chunkText[..half];
            var b = chunkText[half..];
            var ma = new List<GptMessage> { systemMsg, new UserGptMessage($"[Sub-chunk {chunkNum}a of {totalChunks}]\n\n{a}") };
            var mb = new List<GptMessage> { systemMsg, new UserGptMessage($"[Sub-chunk {chunkNum}b of {totalChunks}]\n\n{b}") };
            var ra = await CallWithHalvingAsync(ma, a, systemMsg, chunkNum, totalChunks, ct, depth + 1);
            var rb = await CallWithHalvingAsync(mb, b, systemMsg, chunkNum, totalChunks, ct, depth + 1);
            var merged = MergeResults([ParseResponse(ra, "text"), ParseResponse(rb, "text")]);
            return System.Text.Json.JsonSerializer.Serialize(merged, new System.Text.Json.JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }
    }

    private static List<string> ChunkTextByChars(string text, int maxChars)
    {
        var chunks = new List<string>();
        var pos = 0;
        while (pos < text.Length)
        {
            var len = Math.Min(maxChars, text.Length - pos);
            // Break on newline boundary if possible
            if (pos + len < text.Length)
            {
                var nl = text.LastIndexOf('\n', pos + len, len / 2);
                if (nl > pos) len = nl - pos;
            }
            chunks.Add(text.Substring(pos, len).Trim());
            pos += len;
        }
        return chunks;
    }
}
