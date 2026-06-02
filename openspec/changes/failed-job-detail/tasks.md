## 1. Data Model

- [ ] 1.1 Add `ErrorDetail` (string?) property to `ProcessingJob` in `OmniExtract.Web/Models/ProcessingJob.cs`
- [ ] 1.2 Add `IsFailed` (bool), `ErrorMessage` (string?), and `ErrorDetail` (string?) properties to `ResultsEntry` in `OmniExtract.Web/Services/ResultsRepository.cs`
- [ ] 1.3 Add `AddFailed(string fileName, string errorMessage, string errorDetail)` method to `ResultsRepository` that creates and inserts a `ResultsEntry` with `IsFailed = true` and a default (empty) `UniversalOutput`

## 2. Orchestrator — Capture and Persist Failures

- [ ] 2.1 In `ExtractionOrchestrator.ProcessJobAsync` catch block, set `job.ErrorDetail = ex.ToString()`
- [ ] 2.2 In the same catch block, call `_resultsRepository.AddFailed(job.FileName, ex.Message, ex.ToString())` and store the returned entry's Id into `job.ResultId`

## 3. Upload Page — Error Detail UI

- [ ] 3.1 In `Upload.razor` failed job card, add a `bool` per-job expand state (e.g. `HashSet<string> _expandedErrors`) to track which failed job error panels are open
- [ ] 3.2 Render an "Error detail ▾ / ▴" toggle button on failed cards when `job.ErrorDetail` is non-null
- [ ] 3.3 Render the full `job.ErrorDetail` in a `<pre>` monospace block inside the collapsible panel when expanded
- [ ] 3.4 Add a "View" button to failed job cards (same as Done cards) that navigates to `/results/{job.ResultId}` when `job.ResultId` is set

## 4. Results Page — Failed Entry Display

- [ ] 4.1 In `Results.razor`, guard all `entry.Output.*` field accesses in the grid template with an `IsFailed` check (or skip those fields entirely for failed entries)
- [ ] 4.2 Render a red "Failed" badge in the card footer for `IsFailed` entries instead of sensitivity badge and confidence score
- [ ] 4.3 Exclude failed entries from domain filter chips (domain is unknown for failed entries)

## 5. Results Detail Page — Failed Entry View

- [ ] 5.1 In `ResultDetail.razor`, after loading the entry, check `entry.IsFailed` and branch to a dedicated error view
- [ ] 5.2 Render the `entry.ErrorMessage` prominently in the error view
- [ ] 5.3 Render the full `entry.ErrorDetail` in a collapsible `<pre>` block (collapsed by default) in the error view
- [ ] 5.4 Render a "Re-upload" button in the error view that navigates to `/upload`
- [ ] 5.5 Ensure the normal extraction output sections are NOT rendered for failed entries
