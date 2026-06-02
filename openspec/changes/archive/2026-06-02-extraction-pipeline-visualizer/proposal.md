## Why

The extraction process takes 30–60 seconds but the UI shows only a spinner and a single static string ("Running AI analysis..."). For a POC whose entire purpose is to observe and judge AI extraction, this is a blind spot — users can't see what stage failed, how many tokens were sent, or how long each phase took. The process should feel like a laboratory instrument, not a loading screen.

## What Changes

- **Add** granular stage updates to `ExtractionOrchestrator` — currently only 2 stage strings, expanding to 5 named pipeline steps
- **Add** timing and token metadata to `ProcessingJob` (`StageStartedAt`, `TokenCount`, `ChunkCount`)
- **Add** a live pipeline visualizer card on the upload page — expands when a job is active, collapses when queued/done
- **Add** per-step progress nodes that light up as each stage completes (Parse → Extract Text → Chunk → AI Analysis → Done)
- **Add** a live stats line during AI Analysis: token count + model name
- **Add** elapsed time ticker per stage (Blazor `Timer` polling `StateHasChanged`)
- **Add** CSS keyframe animations for the active step pulse/glow

## Capabilities

### New Capabilities
- `pipeline-visualizer`: Live visual pipeline card on the upload page showing named stages, elapsed time, token count, and step-by-step progress for active extraction jobs

### Modified Capabilities
- none

## Impact

- `OmniExtract.Web/Models/ProcessingJob.cs` — add `StageStartedAt`, `TokenCount`, `ChunkCount` fields
- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — add 3 additional stage notifications + populate new fields
- `OmniExtract.Web/Components/Pages/Upload.razor` — replace flat job card with expanded pipeline card for active jobs
- `OmniExtract.Web/wwwroot/app.css` — pipeline card layout + CSS keyframe animations
- No changes to Core, App, or backend services
