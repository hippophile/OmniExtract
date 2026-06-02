using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;

namespace OmniExtract.App.Services;

public class ExtractionService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private const string SystemPrompt = """
        You are a universal document extraction engine.

        Given the content of a document (text or images), perform the following:
        1. Infer the document type (e.g. invoice, contract, report, email, form, meeting minutes, financial statement, etc.)
        2. Extract ALL fields, values, tables, dates, names, amounts, and entities present in the document.
        3. Generate a flat array of descriptive tags (document type, language(s), year, domain, entity names, etc.)
        4. Assign hierarchical categories: domain (finance/legal/hr/technical/general), subdomain, and sensitivity (public/internal/confidential/restricted)
        5. Score your confidence in extraction quality from 0.0 to 1.0
        6. List any fields you expected for this document type but could not find or were uncertain about

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
          ],
          "gaps": ["description of missing/uncertain field"]
        }

        Never wrap the JSON in markdown fences. Respond with raw JSON only.
        """;

    private readonly GptClient _gpt;
    private readonly TokenCounter _tokens;
    private readonly ProcessingSettings _settings;
    private readonly ILogger<ExtractionService> _logger;

    public ExtractionService(GptClient gpt, TokenCounter tokens, IOptions<ProcessingSettings> settings, ILogger<ExtractionService> logger)
    {
        _gpt = gpt;
        _tokens = tokens;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<UniversalOutput> ExtractAsync(string filePath, ExtractionResult extracted, CancellationToken ct = default)
    {
        var meta = new OutputMeta { SourceFile = Path.GetFileName(filePath) };

        if (extracted.Error is not null && string.IsNullOrWhiteSpace(extracted.Text) && extracted.Images.Count == 0)
        {
            meta.ExtractionMethod = "failed";
            meta.Warnings.Add(extracted.Error);
            return new UniversalOutput { Meta = meta, Gaps = ["Extraction failed: " + extracted.Error] };
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
                Data = new Dictionary<string, object?> { ["raw_error"] = ex.Message },
                Gaps = ["AI extraction failed"]
            };
        }
    }

    private async Task<UniversalOutput> ExtractTextAsync(string text, CancellationToken ct)
    {
        var systemMsg = new SystemGptMessage(SystemPrompt);
        var baseTokens = _tokens.CountTokens(SystemPrompt) + _tokens.CountTokens(text) + _settings.MaxOutputTokens + _settings.TokenBuffer;

        if (baseTokens <= _settings.ModelContextLimit)
        {
            var messages = new List<GptMessage> { systemMsg, new UserGptMessage(text) };
            var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
            return ParseResponse(response, "text");
        }

        // Chunked extraction
        _logger.LogInformation("Document too large — chunking...");
        var available = _settings.ModelContextLimit - _tokens.CountTokens(SystemPrompt) - _settings.MaxOutputTokens - _settings.TokenBuffer;
        var chunks = ChunkText(text, available);
        _logger.LogInformation("Split into {Count} chunks", chunks.Count);

        var results = new List<UniversalOutput>();
        for (var i = 0; i < chunks.Count; i++)
        {
            _logger.LogInformation("Processing chunk {I}/{Total}", i + 1, chunks.Count);
            var prompt = chunks.Count > 1
                ? $"[Chunk {i + 1} of {chunks.Count}]\n\n{chunks[i]}"
                : chunks[i];
            var messages = new List<GptMessage> { systemMsg, new UserGptMessage(prompt) };
            var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
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

            var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
            results.Add(ParseResponse(response, "vision"));
        }

        return MergeResults(results);
    }

    private static UniversalOutput ParseResponse(string json, string method)
    {
        try
        {
            var output = JsonSerializer.Deserialize<UniversalOutput>(json, JsonOpts);
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
            Data = new Dictionary<string, object?> { ["raw_response"] = json },
            Gaps = ["AI response could not be parsed as JSON"]
        };
    }

    private static UniversalOutput MergeResults(List<UniversalOutput> results)
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
            Tables = results.SelectMany(r => r.Tables).ToList(),
            Gaps = results.SelectMany(r => r.Gaps).Distinct().ToList()
        };

        return merged;
    }

    private List<string> ChunkText(string text, int maxTokensPerChunk)
    {
        var chunks = new List<string>();
        var words = text.Split(' ');
        var current = new StringBuilder();
        var currentTokens = 0;

        foreach (var word in words)
        {
            var wordTokens = _tokens.CountTokens(word + " ");
            if (currentTokens + wordTokens > maxTokensPerChunk && current.Length > 0)
            {
                chunks.Add(current.ToString().Trim());
                current.Clear();
                currentTokens = 0;
            }
            current.Append(word);
            current.Append(' ');
            currentTokens += wordTokens;
        }

        if (current.Length > 0)
            chunks.Add(current.ToString().Trim());

        return chunks;
    }
}
