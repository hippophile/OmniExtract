## 1. Results List — Verdict Preview

- [x] 1.1 Add `GetVerdictSummary(ResultsEntry)` helper in `Results.razor` — reads `Data["verdict"]` via `ToJsonString` + `ParseJsonObjectFlat`, returns summary string or null
- [x] 1.2 Render verdict summary line on each non-failed result card — beneath doc type, muted style, only if summary is non-null
- [x] 1.3 Truncate summary at 120 chars with ellipsis in the helper
- [x] 1.4 Add `.result-verdict-preview` CSS — small font, muted color, single line, no wrapping

## 2. Verdict Card — Confidence Caveat

- [x] 2.1 In `ResultDetail.razor` verdict card block, read `m.Confidence` alongside verdict data
- [x] 2.2 Render caveat badge in the verdict header row when `m.Confidence < 0.75` — text: "partial extraction · NN%"
- [x] 2.3 Add `.verdict-caveat` CSS — amber tone, small pill, sits right of the kicker in the header row
