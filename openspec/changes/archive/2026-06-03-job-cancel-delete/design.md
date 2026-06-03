## Context

Jobs are fire-and-forget: `Task.Run(() => ProcessJobAsync(job, CancellationToken.None))`. Once started, a job cannot be interrupted. The semaphore (concurrency = 1) means a stuck or slow job blocks everything behind it. There is also no way to clean the queue view after jobs complete.

## Goals / Non-Goals

**Goals:**
- Allow users to cancel Queued jobs before they start.
- Allow users to stop Processing jobs mid-execution.
- Allow users to remove Done/Failed/Cancelled jobs from the queue view.
- Mark cancelled jobs with a distinct `Cancelled` status (visible in UI).

**Non-Goals:**
- Deleting results from `ResultsRepository` via the queue (handled separately by the existing delete flow on the Results page).
- Pausing and resuming jobs.
- Cancelling individual chunks within a chunked extraction.

## Decisions

### 1. Per-job CancellationTokenSource on ProcessingJob

Each `ProcessingJob` gets a `CancellationTokenSource Cts` property created at enqueue time. `ProcessJobAsync` receives `job.Cts.Token` instead of `CancellationToken.None`.

**Alternative considered**: A shared dictionary `<jobId, CTS>` on the orchestrator. Rejected — co-locating CTS on the job model is simpler and avoids synchronisation between the dict and the job list.

### 2. New `Cancelled` JobStatus value

Add `Cancelled` to the `JobStatus` enum. When a job's token is cancelled mid-processing, the `OperationCanceledException` is caught and the job is marked `Cancelled` rather than `Failed`. This gives a distinct visual state.

**Alternative considered**: Reuse `Failed` with a special error message. Rejected — pollutes result history and makes UI logic more fragile.

### 3. `CancelJobAsync` + `RemoveJobAsync` on orchestrator

- `CancelJobAsync(string id)`: finds job by id, calls `job.Cts.Cancel()`. For Queued jobs this prevents `_semaphore.WaitAsync` from proceeding; for Processing jobs it propagates into the extraction pipeline.
- `RemoveJobAsync(string id)`: cancels if not finished, then removes from `_jobs`.

### 4. UI buttons scoped by status

| Status     | Button  | Action                        |
|------------|---------|-------------------------------|
| Queued     | Remove  | `RemoveJobAsync`              |
| Processing | Stop    | `CancelJobAsync`              |
| Done/Failed/Cancelled | Delete | `RemoveJobAsync` |

Buttons are small ghost/danger style to not dominate the card layout.

## Risks / Trade-offs

- **In-flight AI call not immediately killed**: `CancellationToken` is checked between async awaits. An ongoing `GptClient.CallAsync` honours the token at its `WaitAsync` and `Task.WaitAsync(ct)` boundaries, so cancellation propagates within one timeout window (~240s worst case) — acceptable for a POC.
- **CTS leak if job removed before task starts**: Mitigated by calling `Cts.Cancel()` before removal in `RemoveJobAsync`, and the `finally` block in `ProcessJobAsync` guards against null/disposed CTS.
