## Context

The CLI path already handles ZIPs correctly: `Program.cs` calls `ArchiveHandler.IsArchive()` and routes to `archiveHandler.ExtractAsync()`, which extracts to a temp dir, processes each member via a callback, and supports nested archives recursively.

The web path (`ExtractionOrchestrator`) has no equivalent: it calls `_documentProcessor.ExtractAsync(job.TempPath)` unconditionally. `DocumentProcessor` has no ZIP logic and will either fail or misparse the binary.

`ExtractionService.MergeResults` already knows how to merge multiple `UniversalOutput` objects; it is private static and needs to be made internal (or accessible) so the orchestrator can invoke it.

## Goals / Non-Goals

**Goals:**
- Web uploads of ZIP files produce a single merged `UniversalOutput` in the results repository
- Each file inside the ZIP is processed independently (same pipeline as a direct upload)
- Nested archives are handled (already in `ArchiveHandler`)
- Job progress updates reflect multi-file work (stage labels show `N/M files`)
- DI registration of `ArchiveHandler` in the web host
- `ExtractionService.MergeResults` accessible from `ExtractionOrchestrator`

**Non-Goals:**
- Streaming per-file results as separate repository entries (one job → one merged result)
- UI tree view of individual archive members
- CLI changes (already works)
- Other archive formats beyond what `ArchiveHandler` already supports

## Decisions

### Reuse `ArchiveHandler` as-is
`ArchiveHandler.ExtractAsync` takes a `Func<string, CancellationToken, Task>` callback and handles extraction, recursion, and temp-dir cleanup. Rather than duplicating that logic in `ExtractionOrchestrator`, register `ArchiveHandler` in the web DI container and inject it.

Alternative: inline ZIP extraction in `ExtractionOrchestrator`. Rejected — duplicates logic already tested by the CLI.

### Single merged result per ZIP job
All per-member outputs are collected and merged via `ExtractionService.MergeResults` into one `UniversalOutput`. This keeps the repository model simple (1 upload → 1 result entry) and aligns with how the chunked/vision paths already work.

Alternative: one result entry per member file. Rejected — breaks the job model, complicates the UI, and requires schema changes.

### Expose `MergeResults` as `internal static`
`ExtractionService.MergeResults` is already correct; it just needs visibility. Making it `internal static` (same assembly or `InternalsVisibleTo` for the web project) avoids turning it into a public API.

Alternative: duplicate the merge logic in `ExtractionOrchestrator`. Rejected — maintenance burden.

Alternative: move `MergeResults` to `UniversalOutput` as a static helper. Could work but is a larger refactor out of scope here.

### `SourceFile` on merged output = archive filename
The merged `Meta.SourceFile` is set to the original ZIP filename so the results repository entry is traceable. Individual member names are added to `Meta.Warnings` (info level) as a manifest.

## Risks / Trade-offs

[Large ZIPs with many files] → Temp disk usage proportional to archive size; mitigated by `ArchiveHandler`'s existing temp-dir cleanup in the `finally` block.

[Memory: all per-member outputs held in `List<UniversalOutput>` until merge] → For archives with hundreds of files this could be significant. Acceptable for a POC; note in code with a TODO.

[Member files already in `DocumentProcessor.NativeExtensions` may fail] → `ExtractionService` already wraps errors gracefully and returns a low-confidence output with warnings; these will be included in the merge.

[`InternalsVisibleTo` or assembly boundary for `MergeResults`] → `OmniExtract.Web` references `OmniExtract.App` as a project reference; making `MergeResults` `internal` still requires `[assembly: InternalsVisibleTo("OmniExtract.Web")]` in `OmniExtract.App`. Alternatively, promote to `public static` since there's no security concern.

## Migration Plan

1. Add `[assembly: InternalsVisibleTo("OmniExtract.Web")]` to `OmniExtract.App` — or make `MergeResults` public static.
2. Register `ArchiveHandler` in `OmniExtract.Web/Program.cs`.
3. Inject `ArchiveHandler` into `ExtractionOrchestrator`.
4. In `ProcessJobAsync`: detect ZIP via `ArchiveHandler.IsArchive(job.TempPath)`, branch to new `ProcessArchiveJobAsync` helper.
5. `ProcessArchiveJobAsync` collects per-member outputs, merges, stores result.
6. Update job stage labels to reflect archive processing.

No database migrations. No rollback complexity — the change is additive; non-ZIP uploads follow the existing code path unchanged.

## Open Questions

- Should the member file manifest (list of files in the ZIP) be stored in `Meta.Warnings` (low ceremony) or a dedicated `Meta.ArchiveMembers` field (more structured)? Default: `Warnings` with `[Archive] member.ext` prefix for minimal schema churn.
- Max member count guard? Current `ArchiveHandler` has no limit. A configurable `MaxArchiveMembers` in `ProcessingSettings` could prevent runaway jobs. Out of scope for this change but recommended as follow-up.
