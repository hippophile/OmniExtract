## Context

`ExtractionService` currently uses one `SystemPrompt` constant for every document regardless of type. The prompt asks GPT to extract "fields and values" — correct for invoices but wrong for essays, reports, and emails where the value IS the prose. Result: narrative documents return 2–5 metadata fields and discard all content.

The classifier must run before the first API call so the right prompt is chosen upfront. An extra classification API call would double cost for single-chunk docs — rejected in favour of a heuristic that reads the text already in memory.

## Goals / Non-Goals

**Goals:**
- Classify every document as `structured` or `narrative` before extraction, with zero extra API calls.
- Use the appropriate prompt per classification.
- Let users opt into Deep Analysis mode for high-stakes narrative documents.
- Render narrative output meaningfully in the Results UI.

**Non-Goals:**
- Multi-class classification (invoice vs contract vs essay) — binary structured/narrative is sufficient for now.
- Per-chunk classification for chunked documents — classify once on first chunk.
- Changing the vision extraction path — images stay on the current prompt.
- Retraining or fine-tuning any model.

## Decisions

### 1. Heuristic classifier — no extra API call

Classify based on signals available before any API call:

```
Score +1 for each:
  - Extension in structured set (.csv, .xlsx, .json, .xml, .tsv, ...)
  - First 500 chars contain ≥3 numeric patterns (dates, amounts, codes)
  - First 500 chars contain table-like delimiters (|, \t, ,) at high density
  - Char count < 5 000 (short = likely a form/data file)

Score +1 for each:
  - Extension in narrative set (.md, .txt, .rst, .html, .eml, ...)
  - First 500 chars average word length > 4 (prose indicator)
  - First 500 chars sentence count > 3 (paragraphs, not fields)
  - Char count > 20 000

structured if structured_score > narrative_score, else narrative
```

**Alternative considered**: LLM classification call. Rejected — adds latency and cost for every document.
**Alternative considered**: Extension-only. Rejected — `.txt` can be an invoice or a novel.

### 2. Three prompts, not two

- `StructuredPrompt` — existing `SystemPrompt`, unchanged.
- `NarrativePrompt` — returns `{title, author, date, summary, sections:[{heading, key_points[], summary}], conclusions, key_entities, word_count}`.
- `DeepAnalysisPrompt` — returns `{document_type, domain, distilled_findings:[{finding, certainty, source_section}], risks:[], key_facts:[], flags:[], one_page_summary}`. Temperature 0.3 (slight interpretive latitude). Only runs when user enables Deep Analysis toggle.

### 3. `AnalysisMode` enum threaded from UI → Orchestrator → ExtractionService

```
AnalysisMode { Standard, DeepAnalysis }
```

Added to `ProcessingJob`, passed through `EnqueueAsync`, received by `ExtractionService.ExtractAsync`. Deep Analysis only applies to narrative documents — if classifier says `structured`, Deep Analysis is silently ignored (structured docs don't benefit).

### 4. Output schema — use existing `Data` dict, add typed top-level fields

Rather than changing `UniversalOutput` schema, narrative and deep analysis results store their output in `Data` with well-known keys (`"summary"`, `"sections"`, `"distilled_findings"` etc.). Results.razor detects these keys and renders them with a dedicated layout.

**Alternative considered**: Add `NarrativeOutput` typed class. Rejected — adds model complexity for a POC; well-known keys in `Data` are sufficient.

### 5. Deep Analysis UI — toggle on Upload page, not a separate flow

A simple checkbox "Deep Analysis" on the upload drop zone. Applies to all files in that upload batch. Compact, no new pages needed.

## Risks / Trade-offs

- **Heuristic misclassification**: A tabular CSV with a long header comment could score as narrative. Mitigation: extension check scores higher weight; `.csv`/`.xlsx` always structured regardless of prose score.
- **Narrative output in existing Results UI**: Flat key-value renderer shows `sections` as a raw JSON blob. Mitigation: detect `"sections"` key and switch to narrative renderer in Results.razor.
- **Deep Analysis on large docs**: 72-chunk doc gets synthesis pass + deep analysis pass = 2 extra API calls. Acceptable for POC — user opted in.
- **Temperature 0.3 for Deep Analysis**: Slight non-determinism. Acceptable — deep analysis is interpretive by nature; deterministic output is wrong here.
