## Context

`ExtractionOrchestrator` currently catches exceptions, sets `job.Status = JobStatus.Failed` and `job.ErrorMessage = ex.Message`, but never calls `ResultsRepository`. Failed jobs exist only in memory in the orchestrator's `_jobs` list — they vanish on page reload and never appear in the Results list. The Upload page renders the short `ErrorMessage` inline but provides no stack trace or retry path.

## Goals / Non-Goals

**Goals:**
- Capture full exception detail (`ex.ToString()`) on failure and surface it in the Upload queue UI
- Persist failed jobs to `ResultsRepository` so they appear in the Results list
- Provide a distinct visual treatment for failed entries in both the queue and Results list
- Allow retrying a failed job from the Results detail page (re-upload prompt, since temp files are cleaned up after failure)

**Non-Goals:**
- Automatic retry / retry scheduling
- Persisting jobs to disk (still in-memory; no file system or DB storage)
- Changing how successful jobs are stored or displayed
- Modifying the AI pipeline or error recovery logic

## Decisions

### 1. Extend `ResultsEntry` rather than create a parallel `FailedResultsEntry` type

**Decision**: Add `bool IsFailed`, `string? ErrorMessage`, and `string? ErrorDetail` to the existing `ResultsEntry` class. Set `Output` to `new UniversalOutput()` (empty sentinel) for failed entries.

**Rationale**: A single type for the Results list keeps `ResultsRepository`, `Results.razor`, and `ResultDetail.razor` simpler — one list, one `GetAll()` call, one navigation route. A parallel type would require discriminated-union handling everywhere.

**Alternative considered**: Separate `FailedResultsEntry` list — rejected because it complicates `GetAll()`, filtering, and routing.

### 2. Capture `ex.ToString()` (full stack trace) in `ErrorDetail`

**Decision**: Store `ex.ToString()` in both `ProcessingJob.ErrorDetail` and the persisted `ResultsEntry.ErrorDetail`.

**Rationale**: `ex.Message` is often too terse (e.g. "Object reference not set"). The full stack trace is essential for debugging. Developers already have access to logs; surfacing the trace in the UI speeds up the feedback loop without requiring log access.

### 3. Collapsible error panel in Upload queue card

**Decision**: Failed job cards on Upload.razor get an expandable `<details>` / toggle panel showing the full `ErrorDetail` in a `<pre>` block, collapsed by default.

**Rationale**: Stack traces are long; hiding them by default keeps the queue readable. Users who need the detail can expand.

### 4. Failed entries in Results list use a "Failed" badge; detail page shows error + re-upload CTA

**Decision**: Failed `ResultsEntry` items render in the Results grid with a red "Failed" badge instead of domain/confidence info. Navigating to `/results/{id}` for a failed entry shows a dedicated error panel (message + collapsible stack trace) and a "Re-upload" button that navigates to `/upload`.

**Rationale**: Temp files are deleted after failure (see orchestrator's `finally` block), so in-place retry is not possible. Re-upload is the honest affordance.

## Risks / Trade-offs

- [Displaying raw stack traces] may expose internal paths or library versions → Acceptable for a POC cockpit; not for a public product
- [Empty `UniversalOutput` sentinel for failed entries] means Results.razor filter/search code must guard against accessing `Output.Meta`, `Output.Tags`, etc. on failed entries → Mitigate by checking `IsFailed` before accessing those fields, or by initialising `UniversalOutput` with safe defaults
- [In-memory only] failed entries disappear on server restart → Accepted; matches existing behaviour for all entries

## Migration Plan

No data migration needed (all in-memory). Deploy is a standard rebuild + restart.
