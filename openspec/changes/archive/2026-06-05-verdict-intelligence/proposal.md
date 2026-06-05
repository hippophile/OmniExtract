## Why

The results list is a file browser today — filename, doc type, confidence. You have to open every result to understand what a document means. And on the detail page, a verdict generated from a 55% confidence extraction looks identical to one from a 99% extraction, creating false trust. Both problems make the tool feel like raw output, not intelligence.

## What Changes

- Results list cards gain an inline verdict summary line beneath the doc type — one sentence, muted, visible without clicking
- Verdict card on the result detail page shows a confidence caveat badge when extraction confidence is below 75% — "based on partial extraction (62%)" — visually distinct from high-confidence verdicts
- No verdict summary on the list card if the result has no verdict (old results, failed extractions, sparse docs)
- Caveat badge only appears when confidence < 0.75; high-confidence verdicts are unchanged

## Capabilities

### New Capabilities
- `verdict-list-preview`: One-line verdict summary shown inline on each result card in the results list
- `verdict-confidence-caveat`: Confidence caveat shown on the verdict card when extraction confidence < 75%

### Modified Capabilities

## Impact

- `Results.razor` — read `Data["verdict"]` per entry, render summary line on each card
- `ResultDetail.razor` — read `Meta.Confidence` alongside verdict, render caveat badge when below threshold
- No backend changes needed — both features read already-stored data
