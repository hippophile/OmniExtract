## 1. Expose MergeResults for Cross-Assembly Use

- [x] 1.1 In `OmniExtract.App/Services/ExtractionService.cs`, change `MergeResults` from `private static` to `public static`
- [x] 1.2 Verify the method signature: `public static UniversalOutput MergeResults(List<UniversalOutput> results)`

## 2. Register ArchiveHandler in Web DI

- [x] 2.1 In `OmniExtract.Web/Program.cs`, add `builder.Services.AddSingleton<ArchiveHandler>();`
- [x] 2.2 Confirm `OmniExtract.Web` project already references `OmniExtract.App` (check `.csproj`); add reference if missing

## 3. Inject ArchiveHandler into ExtractionOrchestrator

- [x] 3.1 Add `ArchiveHandler _archiveHandler` field to `ExtractionOrchestrator`
- [x] 3.2 Add `ArchiveHandler archiveHandler` parameter to the constructor and assign to the field

## 4. Implement Archive Processing Branch

- [x] 4.1 In `ProcessJobAsync`, after saving the uploaded file, check `ArchiveHandler.IsArchive(job.TempPath)` and branch to a new `ProcessArchiveJobAsync` method if true
- [x] 4.2 Create `private async Task ProcessArchiveJobAsync(ProcessingJob job, CancellationToken ct)` in `ExtractionOrchestrator`
- [x] 4.3 In `ProcessArchiveJobAsync`, declare a `List<UniversalOutput> memberResults` and a `memberIndex` counter
- [x] 4.4 Call `await _archiveHandler.ExtractAsync(job.TempPath, async (memberPath, ct) => { ... }, ct)` with a callback that: updates `job.Stage` to `"Processing file N of M..."` (use a captured counter; M can be `"?"` since total is unknown until complete), calls `_documentProcessor.ExtractAsync` then `_extractionService.ExtractAsync`, appends the result to `memberResults`
- [x] 4.5 After `ExtractAsync` returns, if `memberResults` is empty, create a fallback `UniversalOutput` with a warning `"ZIP contained no processable files"`
- [x] 4.6 Call `ExtractionService.MergeResults(memberResults)` to get the merged output (or return the single result directly if count == 1)
- [x] 4.7 Set `merged.Meta.SourceFile = job.FileName`
- [x] 4.8 Set `job.Result`, `job.Status = JobStatus.Done`, `job.Stage = "Complete"`, `job.CompletedAt`
- [x] 4.9 Call `_resultsRepository.Add(job.FileName, merged)` and assign `job.ResultId`
- [x] 4.10 Call `NotifyState()` at appropriate points (after stage updates and on completion)

## 5. Build and Verify

- [x] 5.1 Run `dotnet build` from the solution root and confirm zero errors
- [x] 5.2 Upload a ZIP containing 2–3 mixed files (e.g., a PDF and a `.txt`) via the web UI and confirm a single merged result appears in the results list
- [x] 5.3 Confirm `Meta.SourceFile` on the result equals the ZIP filename
- [x] 5.4 Upload a ZIP containing another ZIP (nested) and confirm it completes without error
- [x] 5.5 Upload an empty ZIP and confirm the job completes with a warning rather than crashing
