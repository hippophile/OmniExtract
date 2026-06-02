## Why

When a job fails, the UI only shows "Failed" with a brief `ex.Message`, giving no actionable diagnostic information. Developers and users cannot debug failures without checking server logs. Failed jobs are also discarded from `ResultsRepository`, so they never appear in the Results list and cannot be retried.

## What Changes

- Add `ErrorDetail` (stack trace / full exception string) field to `ProcessingJob`
- Store failed jobs in `ResultsRepository` as a `FailedResultsEntry` (or extend `ResultsEntry` with failure info) so they surface in the Results list
- Upload page: expand the failed job card to show full error message and stack trace in a collapsible panel
- Results page: show failed entries with a distinct "Failed" badge; clicking navigates to a detail view showing error info
- Results detail page: handle failed entries (no `UniversalOutput`) — show error detail and a "Retry" button that re-enqueues the file (if temp path still exists) or prompts re-upload
- `ExtractionOrchestrator`: capture `ex.ToString()` (full stack trace) in `job.ErrorDetail` on failure, and call `ResultsRepository.AddFailed(...)` 

## Capabilities

### New Capabilities

- `failed-job-detail`: Surface full error detail (message + stack trace) for failed jobs in the Upload queue UI and persist failed jobs to ResultsRepository so they appear in the Results list with retry support

### Modified Capabilities

- (none — no existing spec-level requirements are changing)

## Impact

- `OmniExtract.Web/Models/ProcessingJob.cs` — add `ErrorDetail` property
- `OmniExtract.Web/Services/ResultsRepository.cs` — add `FailedEntry` type and `AddFailed` / `GetAllWithFailed` methods
- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — capture full stack trace, call `AddFailed` in catch block
- `OmniExtract.Web/Components/Pages/Upload.razor` — expand failed card UI with collapsible error panel
- `OmniExtract.Web/Components/Pages/Results.razor` — render failed entries with error badge
- `OmniExtract.Web/Components/Pages/ResultDetail.razor` — handle failed entry route, show error + retry CTA
