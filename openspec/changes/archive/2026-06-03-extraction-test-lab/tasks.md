## 1. ExtractionService — Lab Approach Methods

- [x] 1.1 Add `LabApproachResult` record to `ExtractionService` (or a shared models file): `(string ApproachName, string Classification, float Confidence, UniversalOutput? Output, TimeSpan Elapsed, string? Error)`
- [x] 1.2 Add `ClassifyPrompt` constant — minimal prompt returning `{classification, confidence, reason}`
- [x] 1.3 Add `AdaptivePrompt` constant — single prompt returning `{classification, classification_confidence, extraction: {UniversalOutput schema}}`
- [x] 1.4 Add `RunApproachAAsync(string text, string ext, CancellationToken ct)` — calls `ClassifyPrompt`, then calls `StructuredPrompt` or `NarrativePrompt` based on result; returns `LabApproachResult` with elapsed time spanning both calls
- [x] 1.5 Add `RunApproachBAsync(string text, string ext, CancellationToken ct)` — calls `AdaptivePrompt`, parses outer wrapper then inner `extraction` as `UniversalOutput`; returns `LabApproachResult`
- [x] 1.6 Add `RunApproachCAsync(string text, string ext, CancellationToken ct)` — calls `ClassifyDocument()` (from adaptive-extraction), then calls appropriate prompt; returns `LabApproachResult` with `Confidence = -1` (heuristic has no confidence score) and near-zero elapsed for classification step

## 2. Lab Page

- [x] 2.1 Create `OmniExtract.Web/Components/Pages/Lab.razor` at route `/lab`
- [x] 2.2 Add file upload input (single file, using `InputFile`) and "Run Lab" button
- [x] 2.3 On upload: read file to temp path, extract text via `DocumentProcessor.ExtractAsync`, then call `Task.WhenAll(RunApproachAAsync, RunApproachBAsync, RunApproachCAsync)`
- [x] 2.4 While running: show three columns each with a spinner and approach name
- [x] 2.5 On completion: render comparison table — one column per approach showing: approach name, classification badge, confidence, elapsed time, field count, warning count
- [x] 2.6 Highlight classification cells in yellow/orange when approaches disagree
- [x] 2.7 Add "Show details" toggle per column that expands to show full `data` key-value list
- [x] 2.8 Add "Run Again" button that clears results and resets the file picker
- [x] 2.9 Handle per-approach errors — show error message in that column without blocking the others

## 3. Navigation

- [x] 3.1 Add "Test Lab" entry to `NavMenu.razor` linking to `/lab` with a flask/beaker icon (SVG inline)

## 4. CSS

- [x] 4.1 Add `.lab-grid` 3-column comparison layout in `app.css`
- [x] 4.2 Add `.lab-col` card style per approach column
- [x] 4.3 Add `.lab-cell-diff` highlight style for disagreeing cells (amber background)
- [x] 4.4 Add `.approach-badge` style (A/B/C label with colour coding)
