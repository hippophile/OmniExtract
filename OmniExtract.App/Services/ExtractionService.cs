using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;
using OmniExtract.App.Services;

namespace OmniExtract.App.Services;

public record LabApproachResult(
    string ApproachName,
    string Classification,
    float Confidence,
    UniversalOutput? Output,
    TimeSpan Elapsed,
    string? Error,
    string? DetectionReason = null
);

public class ExtractionService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // ── Main extraction prompts ───────────────────────────────────

    private const string StructuredPrompt = """
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
        - Extract values exactly as they appear — do not interpret, convert, or reformat them
        """;

    private const string NarrativePrompt = """
        You are a document analysis engine specialised in narrative and prose documents.

        Given the content of the document, extract its meaning, structure, and key content — not flat fields.
        Behave as an objective, neutral reader. Do not editorialize.

        Perform the following:
        1. Identify the document type (e.g. financial report, business email, legal contract, academic essay, news article, etc.)
        2. Extract the document's narrative structure into sections with key points
        3. Generate a flat array of descriptive tags (document type, language(s), year, domain, key entities, etc.)
        4. Assign categories: domain (finance/legal/hr/technical/general), subdomain, sensitivity (public/internal/confidential/restricted)
        5. Score your confidence in extraction quality from 0.0 to 1.0

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "meta": {
            "document_type": "string",
            "language": ["ISO-code"],
            "confidence": 0.0,
            "extraction_method": "text/narrative",
            "warnings": []
          },
          "tags": ["tag1", "tag2"],
          "categories": {
            "domain": "string",
            "subdomain": "string",
            "sensitivity": "public|internal|confidential|restricted"
          },
          "data": {
            "title": "string",
            "author": "string or null",
            "date": "string or null",
            "summary": "2-4 sentence summary of the document",
            "sections": [
              {
                "heading": "section heading",
                "key_points": ["point 1", "point 2"],
                "summary": "one sentence"
              }
            ],
            "conclusions": "key conclusions or null",
            "key_entities": ["entity1", "entity2"],
            "word_count": 0
          },
          "tables": []
        }

        STRICT RULES:
        - Never wrap the JSON in markdown fences or code blocks
        - Never add comments (no // or /* */ inside JSON)
        - Never invent content — only summarise what is present
        - Respond with raw, valid JSON only
        """;

    private const string DeepAnalysisPrompt = """
        You are an expert document analyst. You have received extracted data from a document.
        Your task is to distil it into a prioritised intelligence brief of approximately 1-2 pages.

        Surface exact facts, flagged risks, certainty levels, and a concise one-page summary.
        Be precise, analytical, and objective. Do not pad the output.

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "document_type": "string",
          "domain": "string",
          "one_page_summary": "comprehensive single-paragraph summary",
          "distilled_findings": [
            {
              "finding": "precise finding statement",
              "certainty": "high|medium|low",
              "source_section": "section heading or null"
            }
          ],
          "risks": ["risk statement 1", "risk statement 2"],
          "key_facts": ["fact 1", "fact 2"],
          "flags": ["notable anomaly or sensitive item"]
        }

        STRICT RULES:
        - Never wrap the JSON in markdown fences or code blocks
        - Never add comments (no // or /* */ inside JSON)
        - Prioritise findings by importance — most critical first
        - Respond with raw, valid JSON only
        """;

    private const string RecommendationPrompt = """
        You are a document routing engine for a specialist AI agent pipeline.

        You will receive a compact summary of an extracted document (document type, domain, sensitivity, tags, data field names, warnings).
        Based on these signals, classify the document domain and recommend the single best specialist agent to handle it next.

        Agent roster:
        - FinancialAgent: revenue, costs, investments, financial statements, budgets, invoices
        - LegalAgent: contracts, agreements, court filings, legal briefs, compliance notices
        - FraudAgent: anomalies, inconsistencies, duplicate entries, suspicious amounts, fraud indicators
        - ComplianceAgent: sensitive/restricted documents, regulatory filings, audit reports, privacy-related content
        - BusinessAgent: general business correspondence, reports, memos, presentations, HR documents

        Priority: FraudAgent > ComplianceAgent > LegalAgent > FinancialAgent > BusinessAgent.
        If fraud signals exist, always recommend FraudAgent regardless of primary domain.

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "recommended_agent": "FinancialAgent|LegalAgent|FraudAgent|ComplianceAgent|BusinessAgent",
          "domain": "financial|legal|fraud|sensitive|business-general",
          "confidence": 0.0,
          "reasoning": "2-3 sentences citing specific signals from the document (field names, tags, sensitivity, document type)",
          "signals": ["specific signal 1", "specific signal 2"]
        }

        STRICT RULES:
        - Never wrap the JSON in markdown fences or code blocks
        - reasoning MUST cite specific evidence (field names, tags, or document type) — no generic statements
        - signals MUST list 2-5 concrete signals found in the input
        - Respond with raw, valid JSON only
        """;

    // ── Lab prompts (Approach A and B) ────────────────────────────

    private const string ClassifyPrompt = """
        Classify the following document as either 'structured' (data-heavy: forms, invoices, tables, spreadsheets) or 'narrative' (prose-heavy: reports, emails, contracts, articles).
        Return ONLY valid JSON with no markdown fences:
        {"classification":"structured|narrative","confidence":0.0,"reason":"one sentence"}
        """;

    private const string AdaptivePrompt = """
        You are a universal document intelligence engine.

        Step 1 — Identify:
        - What kind of document is this? (invoice, contract, medical report, academic paper, financial statement, email, court filing, research abstract, earnings call, etc.)
        - What domain? (finance, legal, medical, academic, technical, HR, logistics, general)
        - Is it data-driven (tables, numbers, forms) or narrative (prose, arguments, findings)?

        Step 2 — Decide what matters:
        Based on the document type, decide which fields are actually meaningful to extract.
        Do NOT use a generic schema. Ask: "what would an expert in this domain want to know?"

        Examples of domain-specific fields:
        - Medical report → diagnosis, medications, dosages, doctor, patient, dates, recommendations, risk_factors
        - Legal contract → parties, effective_date, obligations, penalties, governing_law, key_clauses, termination_conditions
        - Financial statement → revenue, ebitda, net_income, period, currency, key_ratios, auditor
        - Academic paper → title, authors, abstract, methodology, findings, conclusions, keywords, citations_count
        - Invoice → vendor, buyer, invoice_number, date, line_items, total, currency, payment_terms
        - Earnings call → company, quarter, speakers, key_metrics, guidance, risks_mentioned, sentiment
        - For ANY document: extract what an expert reader would underline or highlight

        Step 3 — Extract everything meaningful:
        - For data-driven documents: extract all tables with full rows and columns, plus key metrics in data{}
        - For narrative documents: extract the domain-specific fields you identified, plus a concise summary
        - Always include: language, document type, confidence in your extraction quality

        Respond ONLY with valid JSON, no markdown fences:
        {
          "classification": "structured|narrative",
          "classification_confidence": 0.0,
          "classification_reasoning": "one sentence: what signals in the document led to this classification",
          "extraction": {
            "meta": {
              "document_type": "specific type (e.g. 'medical discharge summary', not just 'document')",
              "language": ["ISO-code"],
              "confidence": 0.0,
              "extraction_method": "text",
              "warnings": []
            },
            "tags": ["domain-specific tags"],
            "categories": {
              "domain": "string",
              "subdomain": "string",
              "sensitivity": "public|internal|confidential|restricted"
            },
            "data": {
              "comment": "use field names that make sense for THIS document type — do not use generic names"
            },
            "tables": []
          }
        }

        RULES:
        - Never truncate tables or arrays
        - Never invent data not present in the document
        - Never use generic field names like 'field_1' or 'content' when a specific name applies
        - Raw JSON only — no prose, no markdown
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

    public async Task<UniversalOutput> ExtractAsync(string filePath, ExtractionResult extracted, AnalysisMode mode = AnalysisMode.Standard, CancellationToken ct = default)
    {
        var meta = new OutputMeta { SourceFile = Path.GetFileName(filePath) };

        if (extracted.Error is not null && string.IsNullOrWhiteSpace(extracted.Text) && extracted.Images.Count == 0)
        {
            meta.ExtractionMethod = "failed";
            meta.Warnings.Add(extracted.Error);
            return new UniversalOutput { Meta = meta };
        }

        var ext = Path.GetExtension(filePath);

        try
        {
            var strategy = RouteDocument(extracted, ext);
            UniversalOutput result;

            switch (strategy)
            {
                case ExtractionStrategy.Vision:
                    meta.ExtractionMethod = "vision";
                    result = await ExtractVisionAsync(extracted.Images, ct);
                    result.Meta.Strategy = "vision";
                    break;

                case ExtractionStrategy.Heuristic:
                    meta.ExtractionMethod = "text";
                    result = await ExtractTextAsync(extracted.Text, ext, mode, ct);
                    var meaningfulCount = CountMeaningfulFields(result.Data);
                    if (meaningfulCount < _settings.HeuristicFallbackThreshold)
                    {
                        _logger.LogInformation("Heuristic yielded {Count} meaningful fields — escalating to TextRich", meaningfulCount);
                        result = await ExtractTextAdaptiveAsync(extracted.Text, mode, ct);
                        result.Meta.Strategy = "heuristic→text-rich";
                    }
                    else
                    {
                        result.Meta.Strategy = "heuristic";
                    }
                    break;

                default: // TextRich
                    meta.ExtractionMethod = "text";
                    result = await ExtractTextAdaptiveAsync(extracted.Text, mode, ct);
                    result.Meta.Strategy = "text-rich";
                    break;
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

    public ExtractionStrategy RouteDocument(ExtractionResult extracted, string ext)
    {
        if (extracted.Images.Count > 0) return ExtractionStrategy.Vision;
        if (ForceStructuredExts.Contains(ext)) return ExtractionStrategy.Heuristic;
        return ExtractionStrategy.TextRich;
    }

    private static readonly HashSet<string> InjectedMetaKeys = new(StringComparer.OrdinalIgnoreCase)
        { "current_datetime", "raw_response", "raw_error" };

    private static int CountMeaningfulFields(Dictionary<string, object?> data) =>
        data.Keys.Count(k => !InjectedMetaKeys.Contains(k));

    private async Task<UniversalOutput> ExtractTextAdaptiveAsync(string text, AnalysisMode mode, CancellationToken ct)
    {
        var messages = new List<GptMessage>
        {
            new SystemGptMessage(AdaptivePrompt),
            new UserGptMessage(text)
        };
        var raw = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
        var cleaned = LineCommentRegex.Replace(raw, string.Empty);
        try
        {
            var wrapper = JsonSerializer.Deserialize<JsonNode>(cleaned, JsonOpts);
            var extractionNode = wrapper?["extraction"];
            if (extractionNode is not null)
                return ParseResponse(extractionNode.ToJsonString(), "text/adaptive");
        }
        catch { }
        return ParseResponse(raw, "text/adaptive");
    }

    private async Task<UniversalOutput> ExtractTextAsync(string text, string ext, AnalysisMode mode, CancellationToken ct)
    {
        var classification = ClassifyDocument(text, ext);
        _logger.LogInformation("Document classified as: {Class}", classification);

        var promptContent = classification == "structured" ? StructuredPrompt : NarrativePrompt;
        var systemMsg = new SystemGptMessage(promptContent);
        var baseTokens = _tokens.CountTokens(promptContent) + _tokens.CountTokens(text) + _settings.MaxOutputTokens + _settings.TokenBuffer;
        var extractionMethod = $"text/{classification}";

        UniversalOutput merged;

        if (baseTokens <= _settings.ModelContextLimit)
        {
            try
            {
                var messages = new List<GptMessage> { systemMsg, new UserGptMessage(text) };
                var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
                merged = ParseResponse(response, extractionMethod);
                goto deepAnalysis;
            }
            catch (Exception ex) when (GptClient.IsContextLimitExceeded(ex))
            {
                _logger.LogWarning("Context limit hit despite pre-flight check — falling back to chunking");
            }
        }

        // Chunked extraction
        _logger.LogInformation("Document too large — chunking...");
        const int InitialChunkChars = 80_000;
        var chunks = ChunkTextByChars(text, InitialChunkChars);
        _logger.LogInformation("Split into {Total} chunks", chunks.Count);

        var largeDocWarning = chunks.Count > 8
            ? $"Large document: {chunks.Count} API calls required, extraction may take ~{chunks.Count * 4} minutes."
            : null;

        var results = new List<UniversalOutput>();
        for (var i = 0; i < chunks.Count; i++)
        {
            _logger.LogInformation("Processing chunk {I}/{Total}", i + 1, chunks.Count);
            var chunkText = chunks[i];
            var prompt = $"[Chunk {i + 1} of {chunks.Count}]\n\n{chunkText}";
            var messages = new List<GptMessage> { systemMsg, new UserGptMessage(prompt) };
            var response = await CallWithHalvingAsync(messages, chunkText, systemMsg, i + 1, chunks.Count, ct);
            results.Add(ParseResponse(response, extractionMethod));
        }

        merged = MergeResults(results);
        if (largeDocWarning is not null)
            merged.Meta.Warnings.Add(largeDocWarning);

        if (chunks.Count > 1)
        {
            var synthesized = await SynthesisPassAsync(merged.Data, ct);
            if (synthesized is not null)
            {
                merged.Data = synthesized;
                merged.Meta.Warnings.Add("Synthesis pass applied.");
            }
            else
            {
                merged.Meta.Warnings.Add("Synthesis pass skipped — using programmatic merge.");
            }
        }

        deepAnalysis:
        if (mode == AnalysisMode.DeepAnalysis)
        {
            if (classification == "narrative")
            {
                _logger.LogInformation("Running Deep Analysis pass...");
                var deepData = await DeepAnalysisPassAsync(merged.Data, ct);
                if (deepData is not null)
                {
                    merged.Data = deepData;
                    merged.Meta.Warnings.Add("Deep Analysis pass applied.");
                }
                else
                {
                    merged.Meta.Warnings.Add("Deep Analysis pass failed — standard extraction retained.");
                }
            }
            else
            {
                merged.Meta.Warnings.Add("Deep Analysis skipped — document classified as structured.");
            }
        }

        return merged;
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
                new SystemGptMessage(StructuredPrompt),
                new UserGptMessage(prompt, chunks[i].ToList())
            };

            var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct, model: _visionModel);
            results.Add(ParseResponse(response, "vision"));
        }

        return MergeResults(results);
    }

    // Only strip // comments that appear at line start (after whitespace) — never inside string values
    private static readonly Regex LineCommentRegex =
        new(@"^\s*//[^\n]*\n?", RegexOptions.Multiline | RegexOptions.Compiled);

    // Strip ```json ... ``` or ``` ... ``` fences the model sometimes emits despite instructions
    private static readonly Regex MarkdownFenceRegex =
        new(@"^```(?:json)?\s*\n?([\s\S]*?)\n?```\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static UniversalOutput ParseResponse(string json, string method)
    {
        try
        {
            var fenceMatch = MarkdownFenceRegex.Match(json.Trim());
            var stripped = fenceMatch.Success ? fenceMatch.Groups[1].Value : json;
            var first = stripped.IndexOf('{');
            var last = stripped.LastIndexOf('}');
            if (first >= 0 && last > first) stripped = stripped[first..(last + 1)];
            var cleaned = LineCommentRegex.Replace(stripped, string.Empty);

            // Parse as raw document first so we can normalize tables before binding
            var doc = JsonSerializer.Deserialize<JsonNode>(cleaned, JsonOpts);
            if (doc is not null)
            {
                // Normalize tables in-place before full deserialization
                var tablesNode = doc["tables"];
                if (tablesNode is JsonArray)
                    doc["tables"] = NormalizeTables(tablesNode.AsArray());

                var output = JsonSerializer.Deserialize<UniversalOutput>(doc.ToJsonString(), JsonOpts);
                if (output is not null)
                {
                    if (string.IsNullOrWhiteSpace(output.Meta.ExtractionMethod))
                        output.Meta.ExtractionMethod = method;
                    return output;
                }
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

    private static JsonArray NormalizeTables(JsonArray raw)
    {
        var result = new JsonArray();
        if (raw.Count == 0) return result;

        foreach (var item in raw)
        {
            if (item is null) continue;
            var table = NormalizeTable(item);
            if (table.Count > 0) result.Add(table);
        }

        // Handle: model returned a single unwrapped table [[row],[row]] instead of [[[row],[row]]]
        if (result.Count == 0 && raw.Count > 0 && raw[0] is JsonArray firstRow && firstRow.Count > 0)
        {
            var single = new JsonArray();
            foreach (var row in raw)
                single.Add(row?.DeepClone());
            result.Add(single);
        }

        return result;
    }

    private static JsonArray NormalizeTable(JsonNode item)
    {
        var table = new JsonArray();

        // Already array-of-arrays: [["h1","h2"],["v1","v2"]]
        if (item is JsonArray rows)
        {
            foreach (var row in rows)
            {
                if (row is JsonArray cells)
                {
                    var r = new JsonArray();
                    foreach (var c in cells) r.Add(c?.GetValue<string>() ?? c?.ToString() ?? "");
                    table.Add(r);
                }
                else if (row is JsonObject obj)
                {
                    // Row is an object — flatten values
                    var r = new JsonArray();
                    foreach (var p in obj) r.Add(p.Value?.ToString() ?? "");
                    table.Add(r);
                }
            }
            return table;
        }

        // Object with headers/rows keys: {"headers":[...],"rows":[[...]]}
        if (item is JsonObject tableObj)
        {
            var headers = tableObj["headers"] as JsonArray ?? tableObj["columns"] as JsonArray;
            var dataRows = tableObj["rows"] as JsonArray ?? tableObj["data"] as JsonArray;

            if (headers is not null)
            {
                var h = new JsonArray();
                foreach (var c in headers) h.Add(c?.GetValue<string>() ?? c?.ToString() ?? "");
                table.Add(h);
            }

            if (dataRows is not null)
            {
                foreach (var row in dataRows)
                {
                    var r = new JsonArray();
                    if (row is JsonArray cells)
                        foreach (var c in cells) r.Add(c?.GetValue<string>() ?? c?.ToString() ?? "");
                    else if (row is JsonObject rowObj)
                        foreach (var p in rowObj) r.Add(p.Value?.ToString() ?? "");
                    table.Add(r);
                }
            }
            else if (headers is null)
            {
                // Object where each key is a column: {"Product":"A","Units":"150"}
                var h = new JsonArray();
                var vals = new JsonArray();
                foreach (var p in tableObj) { h.Add(p.Key); vals.Add(p.Value?.ToString() ?? ""); }
                table.Add(h);
                table.Add(vals);
            }
        }

        return table;
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
                .ToDictionary(g => g.Key, g => g.FirstOrDefault(kv => kv.Value is not null).Value ?? g.First().Value),
            Tables = results.SelectMany(r => r.Tables).ToList()
        };

        return merged;
    }

    private const string SynthesisPrompt = """
        You are a data consolidation engine. You have received partial extraction results
        from multiple sections of the same document, merged into one JSON object.

        Your task:
        1. Deduplicate keys — if the same field appears with slightly different names, unify them.
        2. Resolve contradictions — prefer the most specific, non-null value.
        3. Fill gaps — if one section implies a value that another section makes explicit, use the explicit value.
        4. Do NOT invent data. Only work with what is present.

        Respond ONLY with a valid JSON object of the consolidated data fields.
        No markdown, no commentary, no extra keys.
        """;

    private async Task<Dictionary<string, object?>?> SynthesisPassAsync(Dictionary<string, object?> data, CancellationToken ct)
    {
        string dataJson;
        try { dataJson = JsonSerializer.Serialize(data); }
        catch { return null; }

        if (dataJson.Length > 80_000)
        {
            _logger.LogWarning("Merged data too large for synthesis pass ({Len} chars) — skipping", dataJson.Length);
            return null;
        }

        try
        {
            var messages = new List<GptMessage>
            {
                new SystemGptMessage(SynthesisPrompt),
                new UserGptMessage(dataJson)
            };
            var response = await _gpt.CallAsync(messages, maxTokens: _settings.MaxOutputTokens, ct: ct);
            var cleaned = LineCommentRegex.Replace(response, string.Empty);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(cleaned, JsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Synthesis pass failed — falling back to programmatic merge");
            return null;
        }
    }

    private async Task<Dictionary<string, object?>?> DeepAnalysisPassAsync(Dictionary<string, object?> data, CancellationToken ct)
    {
        string dataJson;
        try { dataJson = JsonSerializer.Serialize(data); }
        catch { return null; }

        try
        {
            var messages = new List<GptMessage>
            {
                new SystemGptMessage(DeepAnalysisPrompt),
                new UserGptMessage(dataJson)
            };
            var response = await _gpt.CallAsync(messages, temperature: 0.3f, maxTokens: _settings.MaxOutputTokens, ct: ct);
            var cleaned = LineCommentRegex.Replace(response, string.Empty);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(cleaned, JsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Deep Analysis pass failed");
            return null;
        }
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
            return JsonSerializer.Serialize(merged, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }
    }

    private static List<string> ChunkTextByChars(string text, int maxChars)
    {
        var chunks = new List<string>();
        var pos = 0;
        while (pos < text.Length)
        {
            var len = Math.Min(maxChars, text.Length - pos);
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

    // ── Document Classifier ───────────────────────────────────────

    private static readonly HashSet<string> ForceStructuredExts = new(StringComparer.OrdinalIgnoreCase)
        { ".csv", ".tsv", ".xlsx", ".xls", ".ods", ".json", ".jsonl", ".xml", ".yaml", ".yml" };

    private static readonly HashSet<string> NarrativeExts = new(StringComparer.OrdinalIgnoreCase)
        { ".md", ".txt", ".rst", ".html", ".htm", ".eml" };

    private static readonly Regex NumericPattern = new(@"\d{1,4}[\/\-\.]\d{1,4}|\d+[\.,]\d+|\b\d{4,}\b", RegexOptions.Compiled);
    private static readonly Regex DelimiterPattern = new(@"[|\t]|(?:,.*){2,}", RegexOptions.Compiled);
    private static readonly Regex SentenceEnd = new(@"[.!?]\s", RegexOptions.Compiled);

    public static string ClassifyDocument(string text, string ext)
    {
        // Force structured for known tabular/data extensions
        if (ForceStructuredExts.Contains(ext)) return "structured";

        var sample = text.Length > 500 ? text[..500] : text;
        int structuredScore = 0;
        int narrativeScore = 0;

        // Extension signals
        if (NarrativeExts.Contains(ext)) narrativeScore++;

        // Numeric pattern density
        var numericMatches = NumericPattern.Matches(sample).Count;
        if (numericMatches >= 3) structuredScore++;

        // Delimiter density
        if (DelimiterPattern.IsMatch(sample)) structuredScore++;

        // Short document → likely a form/data file
        if (text.Length < 5_000) structuredScore++;

        // Long document → likely prose
        if (text.Length > 20_000) narrativeScore++;

        // Avg word length > 4 → prose indicator
        var words = sample.Split(new char[] { ' ', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0 && words.Average(w => (double)w.Length) > 4) narrativeScore++;

        // Sentence count > 3 → paragraph text
        var sentenceCount = SentenceEnd.Matches(sample).Count;
        if (sentenceCount > 3) narrativeScore++;

        return structuredScore > narrativeScore ? "structured" : "narrative";
    }

    public static (string Classification, string Reason) ClassifyDocumentWithReason(string text, string ext)
    {
        if (ForceStructuredExts.Contains(ext))
            return ("structured", $"Extension '{ext}' is a known tabular/data format.");

        var sample = text.Length > 500 ? text[..500] : text;
        var signals = new List<string>();
        int structuredScore = 0, narrativeScore = 0;

        if (NarrativeExts.Contains(ext)) { narrativeScore++; signals.Add($"narrative extension '{ext}'"); }

        var numericMatches = NumericPattern.Matches(sample).Count;
        if (numericMatches >= 3) { structuredScore++; signals.Add($"{numericMatches} numeric patterns in opening text"); }

        if (DelimiterPattern.IsMatch(sample)) { structuredScore++; signals.Add("delimiter/table characters detected"); }

        if (text.Length < 5_000) { structuredScore++; signals.Add($"short document ({text.Length} chars)"); }
        else if (text.Length > 20_000) { narrativeScore++; signals.Add($"long document ({text.Length} chars)"); }

        var words = sample.Split(new char[] { ' ', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0 && words.Average(w => (double)w.Length) > 4)
        { narrativeScore++; signals.Add("high average word length (prose indicator)"); }

        var sentenceCount = SentenceEnd.Matches(sample).Count;
        if (sentenceCount > 3) { narrativeScore++; signals.Add($"{sentenceCount} sentences detected"); }

        var result = structuredScore > narrativeScore ? "structured" : "narrative";
        var reason = $"Scores: structured={structuredScore}, narrative={narrativeScore}. Signals: {string.Join("; ", signals)}.";
        return (result, reason);
    }

    // ── Recommendation pass ───────────────────────────────────────

    public async Task<Dictionary<string, object?>?> RecommendationPassAsync(UniversalOutput result, CancellationToken ct)
    {
        try
        {
            var summary = BuildRecommendationInput(result);
            var messages = new List<GptMessage>
            {
                new SystemGptMessage(RecommendationPrompt),
                new UserGptMessage(summary)
            };
            var response = await _gpt.CallAsync(messages, temperature: 0, ct: ct);
            var cleaned = LineCommentRegex.Replace(response, string.Empty);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(cleaned, JsonOpts);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Recommendation pass timed out — skipping");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Recommendation pass failed — skipping");
            return null;
        }
    }

    private static string BuildRecommendationInput(UniversalOutput result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"document_type: {result.Meta.DocumentType}");
        sb.AppendLine($"domain: {result.Categories.Domain}");
        sb.AppendLine($"subdomain: {result.Categories.Subdomain}");
        sb.AppendLine($"sensitivity: {result.Categories.Sensitivity}");
        sb.AppendLine($"confidence: {result.Meta.Confidence:F2}");
        sb.AppendLine($"extraction_method: {result.Meta.ExtractionMethod}");

        if (result.Tags.Any())
            sb.AppendLine($"tags: {string.Join(", ", result.Tags.Take(20))}");

        if (result.Meta.Warnings.Any())
            sb.AppendLine($"warnings: {string.Join("; ", result.Meta.Warnings)}");

        var dataKeys = result.Data.Keys.Take(20).ToList();
        if (dataKeys.Any())
            sb.AppendLine($"data_fields: {string.Join(", ", dataKeys)}");

        return sb.ToString().Trim();
    }

    // ── Lab approach runners ──────────────────────────────────────

    public async Task<LabApproachResult> RunApproachAAsync(string text, string ext, List<string>? images, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (images is { Count: > 0 })
            {
                var output = await ExtractVisionAsync(images, ct);
                sw.Stop();
                return new LabApproachResult("A — Classify-first", output.Meta.DocumentType ?? "structured", (float)output.Meta.Confidence, output, sw.Elapsed, null);
            }

            // Call 1: classify via API
            var classifyMessages = new List<GptMessage>
            {
                new SystemGptMessage(ClassifyPrompt),
                new UserGptMessage(text)
            };
            var classifyRaw = await _gpt.CallAsync(classifyMessages, ct: ct);
            var classifyJson = JsonSerializer.Deserialize<JsonNode>(classifyRaw, JsonOpts);
            var classification = classifyJson?["classification"]?.GetValue<string>() ?? "narrative";
            var confidence = classifyJson?["confidence"]?.GetValue<float>() ?? 0f;
            var reason = classifyJson?["reason"]?.GetValue<string>();

            // Call 2: extract with appropriate prompt
            var extractPrompt = classification == "structured" ? StructuredPrompt : NarrativePrompt;
            var extractMessages = new List<GptMessage>
            {
                new SystemGptMessage(extractPrompt),
                new UserGptMessage(text)
            };
            var extractRaw = await _gpt.CallAsync(extractMessages, ct: ct);
            var textOutput = ParseResponse(extractRaw, $"text/{classification}");

            sw.Stop();
            return new LabApproachResult("A — Classify-first", classification, confidence, textOutput, sw.Elapsed, null, reason);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new LabApproachResult("A — Classify-first", "error", 0f, null, sw.Elapsed, ex.Message);
        }
    }

    public async Task<LabApproachResult> RunApproachBAsync(string text, string ext, List<string>? images, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (images is { Count: > 0 })
            {
                var output = await ExtractVisionAsync(images, ct);
                sw.Stop();
                return new LabApproachResult("B — Adaptive prompt", output.Meta.DocumentType ?? "structured", (float)output.Meta.Confidence, output, sw.Elapsed, null);
            }

            var messages = new List<GptMessage>
            {
                new SystemGptMessage(AdaptivePrompt),
                new UserGptMessage(text)
            };
            var raw = await _gpt.CallAsync(messages, ct: ct);
            var cleaned = LineCommentRegex.Replace(raw, string.Empty);
            var wrapper = JsonSerializer.Deserialize<JsonNode>(cleaned, JsonOpts);

            if (wrapper is null) throw new InvalidOperationException("Empty response");

            var classification = wrapper["classification"]?.GetValue<string>() ?? "unknown";
            var confidence = wrapper["classification_confidence"]?.GetValue<float>() ?? 0f;
            var reason = wrapper["classification_reasoning"]?.GetValue<string>();
            var extractionNode = wrapper["extraction"];
            UniversalOutput? output2 = null;
            if (extractionNode is not null)
                output2 = ParseResponse(extractionNode.ToJsonString(), $"text/{classification}");

            sw.Stop();
            return new LabApproachResult("B — Adaptive prompt", classification, confidence, output2, sw.Elapsed, null, reason);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new LabApproachResult("B — Adaptive prompt", "error", 0f, null, sw.Elapsed, ex.Message);
        }
    }

    public async Task<LabApproachResult> RunApproachCAsync(string text, string ext, List<string>? images, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (images is { Count: > 0 })
            {
                var output = await ExtractVisionAsync(images, ct);
                sw.Stop();
                return new LabApproachResult("C — Heuristic", output.Meta.DocumentType ?? "structured", -1f, output, sw.Elapsed, null);
            }

            var (classification, cReason) = ClassifyDocumentWithReason(text, ext);

            var extractPrompt = classification == "structured" ? StructuredPrompt : NarrativePrompt;
            var extractMessages = new List<GptMessage>
            {
                new SystemGptMessage(extractPrompt),
                new UserGptMessage(text)
            };
            var raw = await _gpt.CallAsync(extractMessages, ct: ct);
            var textOutput = ParseResponse(raw, $"text/{classification}");

            sw.Stop();
            return new LabApproachResult("C — Heuristic", classification, -1f, textOutput, sw.Elapsed, null, cReason);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new LabApproachResult("C — Heuristic", "error", -1f, null, sw.Elapsed, ex.Message);
        }
    }
}
