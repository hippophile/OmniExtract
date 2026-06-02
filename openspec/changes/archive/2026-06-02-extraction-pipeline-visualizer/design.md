## Context

`ExtractionOrchestrator.ProcessJobAsync` currently sets `job.Stage` at 3 points: start, before AI call, and on completion. The `ProcessingJob` model has no timing or token metadata. The upload page renders all jobs identically as compact list items regardless of status.

The Blazor `StateChanged` event is already wired up — `Upload.razor` calls `StateHasChanged()` on every notification. The render loop is ready; it just needs richer data and a richer template.

## Goals / Non-Goals

**Goals:**
- Make the active job card visually distinct and informative
- Show 5 named pipeline steps, each with status (pending/active/done/failed)
- Show elapsed time ticking up for the active step (client-side timer)
- Show token count + chunk count when available (populated by orchestrator)
- Pure Blazor + CSS — no JS, no SignalR changes

**Non-Goals:**
- Streaming token-by-token output from the AI response
- Per-chunk progress within the AI Analysis step
- Changing the backend concurrency model (still processes one at a time via semaphore)

## Decisions

**Decision: 5 named pipeline steps mapped from Stage strings**

| Step label      | Triggered when Stage equals              |
|-----------------|------------------------------------------|
| Parse           | "Parsing file..."                        |
| Extract Text    | "Extracting content..."                  |
| Chunk           | "Chunking document..."                   |
| AI Analysis     | "Running AI analysis..."                 |
| Done / Failed   | "Complete" / "Failed"                    |

The orchestrator gains two new intermediate `job.Stage` assignments: `"Parsing file..."` (before DocumentProcessor) and `"Chunking document..."` (between DocumentProcessor and ExtractionService). The `Complete` and `Failed` terminal states are unchanged.

**Decision: Elapsed time via `System.Timers.Timer` in Upload.razor**
- Start a 500ms interval timer when any job enters Processing status
- On each tick: call `InvokeAsync(StateHasChanged)` — Blazor re-renders elapsed time
- Stop timer when no jobs are in Processing status
- Alternative considered: JS `setInterval` — rejected, no JS is the constraint

**Decision: Token count populated in orchestrator, not in GptClient**
- `ProcessingJob.TokenCount` is set by orchestrator after `DocumentProcessor` returns, using `TokenCounter.Count()` on the extracted text
- This keeps `GptClient` changes out of scope (it doesn't know about `ProcessingJob`)
- Alternative: hook into `GptClient.CallWithMetadataAsync` — rejected, more invasive

**Decision: Expanded card only for the single active (Processing) job**
- Queued and Done/Failed jobs remain compact list items
- Only one job processes at a time (semaphore), so at most one expanded card exists
- Avoids layout thrash from multiple simultaneous expanded cards

**Decision: Step nodes as inline SVG circles, not Unicode characters**
- Consistent sizing and color control via CSS variables
- Active step: pulsing glow animation via `@keyframes`

## Risks / Trade-offs

- [Token count may be 0 for vision-only (image/scanned PDF) jobs] → Show "vision mode" label instead of token count
- [Timer keeps ticking if Blazor component is navigated away from] → `IDisposable.Dispose()` already stops the `StateChanged` subscription; add timer stop there too
- [Stage strings are stringly-typed] → Map defensively with a fallback; unknown stages show as "Processing..."
