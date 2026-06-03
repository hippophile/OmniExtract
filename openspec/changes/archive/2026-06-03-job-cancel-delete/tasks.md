## 1. Core Model

- [x] 1.1 Add `Cancelled` to `JobStatus` enum in `OmniExtract.Core/Models/`
- [x] 1.2 Add `CancellationTokenSource Cts` property to `ProcessingJob`

## 2. Orchestrator

- [x] 2.1 In `EnqueueAsync`, create a new `CancellationTokenSource` and assign to `job.Cts`
- [x] 2.2 Change `Task.Run(() => ProcessJobAsync(job, CancellationToken.None))` to pass `job.Cts.Token`
- [x] 2.3 In `ProcessJobAsync` catch block, detect `OperationCanceledException` and set `job.Status = JobStatus.Cancelled` instead of `Failed`
- [x] 2.4 Add `CancelJobAsync(string id)` method — finds job, calls `job.Cts.Cancel()`
- [x] 2.5 Add `RemoveJobAsync(string id)` method — cancels if not finished, removes from `_jobs`, calls `NotifyState()`

## 3. UI — Upload Page

- [x] 3.1 Add "Stop" button to Processing job cards in `Upload.razor` — calls `Orchestrator.CancelJobAsync(job.Id)`
- [x] 3.2 Add "Remove" button to Queued job cards — calls `Orchestrator.RemoveJobAsync(job.Id)`
- [x] 3.3 Add "Delete" button to Done/Failed/Cancelled compact cards — calls `Orchestrator.RemoveJobAsync(job.Id)`
- [x] 3.4 Add `Cancelled` case to the status icon switch (distinct icon, e.g. X in grey)
- [x] 3.5 Add `.job-item.cancelled` CSS rule and `Cancelled` step label handling in `GetActiveStep`

## 4. CSS

- [x] 4.1 Add `.job-status-icon.cancelled` style (grey background, muted colour)
- [x] 4.2 Add `.job-item.cancelled` border colour (muted/grey, distinct from failed red)
