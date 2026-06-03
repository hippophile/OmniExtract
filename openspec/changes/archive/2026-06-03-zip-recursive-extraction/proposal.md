## Why

ZIP files are listed as a supported format in `ArchiveHandler.IsArchive()` and the CLI's `ProcessFile` routes them through `archiveHandler.ExtractAsync()`, but the web UI's `ExtractionOrchestrator` passes every uploaded file directly to `DocumentProcessor.ExtractAsync()` — which has no ZIP handling — causing ZIPs to either fail silently or produce garbage output. The fix closes this gap and produces a single merged `UniversalOutput` per ZIP in both the CLI and web paths.

## What Changes

- `ExtractionOrchestrator` gains ZIP-awareness: when the uploaded file is a ZIP, extract its contents, process each member file independently, merge all `UniversalOutput` results into one, and store that merged result as the job's output.
- `ArchiveHandler` already supports recursive nesting (nested ZIPs) via its existing `ProcessDirectory` → `ExtractAsync` recursion — this path is preserved and exercised.
- `ExtractionService` exposes (or an existing `MergeResults` helper is reused) a public merge method so `ExtractionOrchestrator` can combine per-file results.
- The CLI path (`Program.cs → ProcessFile`) already works correctly; no changes needed there.
- Job UI in the web app reflects the multi-file nature: stage labels and final `FileName` indicate archive contents count.

## Capabilities

### New Capabilities

- `zip-extraction`: Web-side ZIP upload processing — extract archive members, run AI extraction on each, merge into one `UniversalOutput`, store as a single job result.

### Modified Capabilities

- (none — CLI path already works; `ArchiveHandler` and `ExtractionService.MergeResults` are reused as-is)

## Impact

- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — primary change site
- `OmniExtract.App/Services/ExtractionService.cs` — `MergeResults` made `internal` or `public static` for reuse
- `OmniExtract.App/Services/ArchiveHandler.cs` — no logic changes; injected into web DI container
- `OmniExtract.Web/Program.cs` — register `ArchiveHandler` in DI
- No new NuGet dependencies (SharpCompress already referenced by `OmniExtract.App`)
