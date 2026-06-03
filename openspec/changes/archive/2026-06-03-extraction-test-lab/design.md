## Context

The Test Lab is a diagnostic tool — it runs three different extraction strategies in parallel on the same document and presents a comparison. It shares the same `ExtractionService` and `GptClient` infrastructure as the main pipeline but adds two new strategy methods and a dedicated Blazor page.

Results are ephemeral (in-memory `List<LabResult>` on the page component) — no `ResultsRepository` writes, no job queue. This keeps the lab isolated from production data.

The lab depends on `adaptive-extraction` being implemented first, since Approach C reuses `ClassifyDocument()` from that change.

## Goals / Non-Goals

**Goals:**
- Run all three approaches on the same uploaded file concurrently using `Task.WhenAll`.
- Show classification decision, confidence, elapsed time, top 10 data keys, and warnings per approach.
- Let the user re-run with a different file without navigating away.

**Non-Goals:**
- Persisting lab results.
- Running the lab on multiple files simultaneously.
- Exporting comparison results.
- Using the job queue or semaphore — lab runs bypass the orchestrator entirely.

## Decisions

### 1. Three approach methods on ExtractionService

```
RunApproachAAsync(text, ext, ct)  — ClassifyPrompt call → StructuredPrompt or NarrativePrompt
RunApproachBAsync(text, ext, ct)  — AdaptivePrompt (classify + extract in one call)
RunApproachCAsync(text, ext, ct)  — ClassifyDocument() heuristic → StructuredPrompt or NarrativePrompt
```

Each returns a `LabApproachResult` record:
```csharp
record LabApproachResult(
    string ApproachName,
    string Classification,
    float Confidence,
    UniversalOutput? Output,
    TimeSpan Elapsed,
    string? Error
);
```

### 2. Parallel execution with Task.WhenAll

All three run concurrently from Lab.razor — the semaphore in `GptClient` already throttles to `ApiConcurrency`. The lab fires three independent pipelines; Approach A uses 2 API calls, B and C use 1 each — total 4 calls, all gated by the semaphore.

### 3. Approach B — AdaptivePrompt design

Single prompt that asks GPT to:
1. Classify the document (`structured` or `narrative`)
2. Extract according to that classification in the same response
3. Return a unified JSON: `{ "classification": "structured|narrative", "classification_confidence": 0.0, "extraction": { ...UniversalOutput... } }`

Outer wrapper parsed first, `extraction` field parsed as `UniversalOutput`.

### 4. Approach A — ClassifyPrompt design

Minimal prompt: "Classify this document as 'structured' (data/form) or 'narrative' (prose/report). Return JSON: `{\"classification\": \"structured|narrative\", \"confidence\": 0.0, \"reason\": \"one sentence\"}`". Then extraction runs with the appropriate existing prompt.

### 5. Comparison view layout

```
┌─────────────────────────────────────────────────────────────┐
│  Test Lab — file: report.pdf                    [Run Again]  │
├───────────────┬───────────────┬────────────────────────────-─┤
│  Approach A   │  Approach B   │  Approach C                  │
│  Classify+Ex  │  Adaptive     │  Heuristic                   │
├───────────────┼───────────────┼──────────────────────────────┤
│ Class: narr.  │ Class: narr.  │ Class: structured  ← DIFF    │
│ Conf:  0.92   │ Conf:  0.88   │ Conf:  n/a                   │
│ Time:  8.2s   │ Time:  5.1s   │ Time:  4.9s                  │
│ Fields: 12    │ Fields: 14    │ Fields: 9                     │
│ Warnings: 0   │ Warnings: 1   │ Warnings: 0                  │
└───────────────┴───────────────┴──────────────────────────────┘
```

Cells where approaches disagree are highlighted. Expandable section per approach shows full extracted data.

## Risks / Trade-offs

- **4 API calls per lab run**: Lab is for testing, not production — acceptable cost. User is explicitly triggering it.
- **Lab bypasses job semaphore**: Three concurrent calls from lab + any background jobs = up to `ApiConcurrency + 3` concurrent calls. Mitigated by the `GptClient` semaphore which already caps concurrency.
- **Depends on adaptive-extraction**: `RunApproachCAsync` calls `ClassifyDocument()` which doesn't exist until `adaptive-extraction` is implemented. Lab.razor should show a clear error if the method is unavailable, or approach C can be implemented inline in the lab as a standalone duplicate until the dependency lands.
