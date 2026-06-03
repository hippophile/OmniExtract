## Context

`ExtractionService.ExtractTextAsync` splits large documents into 80 000-char chunks, calls the AI once per chunk, collects `UniversalOutput` objects, and calls `MergeResults`. The merge is purely programmatic. A recent change capped chunks at 8 (`MaxChunks`) to limit API call time — but this silently drops content beyond chunk 8.

Current merge for `data`:
```csharp
Data = results.SelectMany(r => r.Data)
              .GroupBy(kv => kv.Key)
              .ToDictionary(g => g.Key, g => g.First().Value)
```
`g.First()` takes the first occurrence regardless of whether its value is null.

## Goals / Non-Goals

**Goals:**
- First non-null value wins in `data` merge.
- All chunks processed regardless of count; user sees an estimated call count in the job stage.
- One synthesis AI call per multi-chunk document to consolidate `data`.

**Non-Goals:**
- Overlapping chunks or chunk re-ordering.
- Synthesis of `tables` (concatenation is correct for tabular data).
- Synthesis for single-chunk or vision documents.
- Changing the chunk size (80 000 chars stays).

## Decisions

### 1. First-non-null merge

```csharp
Data = results
    .SelectMany(r => r.Data)
    .GroupBy(kv => kv.Key)
    .ToDictionary(
        g => g.Key,
        g => g.FirstOrDefault(kv => kv.Value is not null).Value
              ?? g.First().Value)
```

Simple one-liner change. No schema impact.

### 2. Remove MaxChunks — replace with time estimate warning

Remove the `MaxChunks` property and cap logic. Instead, before the chunk loop begins, add a warning to `meta.Warnings` if chunk count > 8:

> `"Large document: ~{N} API calls required, extraction may take {N*4} minutes."`

This is surfaced in the result detail page's warnings section so users understand the wait.

**Alternative considered**: Keep cap but make it configurable to a very high number. Rejected — a cap always risks silent data loss and the setting adds confusion.

### 3. Synthesis pass

After the chunk loop and programmatic merge, if `chunks.Count > 1`:

1. Serialize merged `data` to compact JSON (typically < 20 K tokens for 8+ chunks).
2. Call `_gpt.CallAsync` once with a dedicated `SynthesisPrompt`:

```
You are a data consolidation engine. You have received partial extraction results
from multiple sections of the same document, merged into one JSON object.

Your task:
1. Deduplicate keys — if the same field appears with slightly different names, unify them.
2. Resolve contradictions — prefer the most specific, non-null value.
3. Fill gaps — if one section implies a value that another section makes explicit, use the explicit value.
4. Do NOT invent data. Only work with what is present.

Respond ONLY with a valid JSON object of the consolidated data fields.
No markdown, no commentary, no extra keys.
```

3. Parse the response as `Dictionary<string, object?>` and replace `merged.Data`.
4. If synthesis fails (parse error, API error), log a warning and keep the programmatically merged data — synthesis is best-effort.

**Why not send the full merged JSON including tables/tags/meta?**
Tables are already correct (concatenation). Tags are deduped. Meta is taken from the first chunk. Only `data` benefits from AI reconciliation — and keeping the payload small reduces token cost and failure risk.

**Token budget**: 8 chunks × ~4 096 output tokens = ~32 K tokens of data JSON. Well within the 128 K context limit. For 20+ chunks the merged JSON could grow larger — if it exceeds ~80 K chars, skip the synthesis pass and log a warning.

## Risks / Trade-offs

- **Synthesis latency**: One extra API call per multi-chunk doc. Acceptable — it's one call vs. the N already made.
- **Synthesis hallucination**: The prompt is strict ("do NOT invent data") and operates only on already-extracted JSON, not raw text. Risk is lower than per-chunk extraction.
- **Synthesis JSON parse failure**: Handled gracefully — fall back to programmatic merge, add warning.
- **Very large merged data (> 80 K chars)**: Skip synthesis, log warning. Unlikely in practice.
