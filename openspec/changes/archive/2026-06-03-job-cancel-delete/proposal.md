## Why

Users have no way to stop a runaway job or clean up the queue — a mis-uploaded file or a massive spreadsheet that will take 30 minutes must run to completion before the queue clears. This blocks the semaphore and wastes API quota.

## What Changes

- Each `ProcessingJob` holds a `CancellationTokenSource` so its task can be cancelled at any time.
- `ExtractionOrchestrator` exposes `CancelJobAsync(id)` and `RemoveJobAsync(id)`.
- Job processing uses the per-job token instead of `CancellationToken.None`.
- Upload page job list gains contextual action buttons:
  - **Queued** → "Remove" (cancels before processing starts, removes from list)
  - **Processing** → "Stop" (signals cancellation, job marked Cancelled)
  - **Done / Failed** → "Delete" (removes from queue view; result stays in `ResultsRepository`)
- A new `Cancelled` status is added to `JobStatus`.

## Capabilities

### New Capabilities
- `job-cancellation`: Cancel queued or in-progress jobs via per-job CancellationTokenSource; new Cancelled status.
- `queue-management`: Remove any job from the queue view regardless of status.

### Modified Capabilities
_(none — ResultsRepository behaviour is unchanged)_

## Impact

- `OmniExtract.Core/Models/ProcessingJob.cs` — add `Cts`, `Cancelled` status
- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — wire CTS, add cancel/remove methods
- `OmniExtract.Web/Components/Pages/Upload.razor` — add action buttons per job status
- `OmniExtract.Web/wwwroot/app.css` — button styles for stop/delete actions
