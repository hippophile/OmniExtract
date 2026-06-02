## 1. Enrich ProcessingJob Model

- [x] 1.1 Add `DateTime StageStartedAt` field to `ProcessingJob.cs` (set to `DateTime.UtcNow` when Stage changes)
- [x] 1.2 Add `int TokenCount` field to `ProcessingJob.cs` (0 = unknown/vision)
- [x] 1.3 Add `int ChunkCount` field to `ProcessingJob.cs` (0 = unknown)

## 2. Add Granular Stage Updates to Orchestrator

- [x] 2.1 Add `"Parsing file..."` stage + `NotifyState()` at the very start of `ProcessJobAsync` (before `DocumentProcessor.ExtractAsync`)
- [x] 2.2 Add `"Chunking document..."` stage + `NotifyState()` after `DocumentProcessor` returns and before `ExtractionService.ExtractAsync`
- [x] 2.3 Populate `job.TokenCount` using `TokenCounter` on the extracted text after DocumentProcessor returns
- [x] 2.4 Populate `job.ChunkCount` from the extracted result chunk count if available (check `ExtractionResult` model for chunk info)
- [x] 2.5 Update `StageStartedAt = DateTime.UtcNow` each time `job.Stage` is set

## 3. Pipeline Step Mapping Helper

- [x] 3.1 Add a static helper method (inline in Upload.razor `@code` or a small static class) that maps a `Stage` string + `JobStatus` to a step index (0–4), so the template can determine which steps are complete/active/pending

## 4. Upload Page — Expanded Pipeline Card

- [x] 4.1 In `Upload.razor`, replace the single job card template with a conditional: if `job.Status == JobStatus.Processing` render the expanded pipeline card, else render the existing compact card
- [x] 4.2 Build the expanded pipeline card markup: 5 step nodes in a row (Parse → Extract Text → Chunk → AI Analysis → Done), each with a status indicator circle and label below
- [x] 4.3 Add step connector lines between nodes (horizontal lines that fill green as steps complete)
- [x] 4.4 Add the current stage detail line below the step row (e.g., "Converting to plain text via LibreOffice...")
- [x] 4.5 Add the stats line (token count · chunk count → model) — show only when `job.TokenCount > 0` or stage is AI Analysis+
- [x] 4.6 Add elapsed time display — compute from `job.StageStartedAt` to `DateTime.UtcNow` on each render

## 5. Blazor Timer for Live Updates

- [x] 5.1 Add a `System.Timers.Timer _ticker` field to `Upload.razor @code`
- [x] 5.2 Start the timer (500ms interval) in `OnInitialized` — on each tick call `InvokeAsync(StateHasChanged)`
- [x] 5.3 Stop and dispose the timer in `Dispose()` (already implements `IDisposable`)

## 6. CSS — Pipeline Card Styles and Animations

- [x] 6.1 Add `.pipeline-card` layout styles to `app.css` — full-width, padded, distinct background
- [x] 6.2 Add `.pipe-steps` row layout with `.pipe-step` nodes and `.pipe-connector` lines between them
- [x] 6.3 Add step state classes: `.pipe-step.done` (green fill), `.pipe-step.active` (accent color), `.pipe-step.pending` (grey), `.pipe-step.failed` (red)
- [x] 6.4 Add `@keyframes pipe-pulse` animation for the active step — subtle glow/scale pulse
- [x] 6.5 Add `.pipe-stats` and `.pipe-elapsed` text styles
- [x] 6.6 Add `.pipe-connector.done` fill transition (green line expanding left-to-right)
