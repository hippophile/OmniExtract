## 1. Remove Chunk Cap

- [x] 1.1 Remove `MaxChunks` property from `ProcessingSettings` in `OmniExtract.Core/Config/AppSettings.cs`
- [x] 1.2 Remove `MaxChunks` entry from `OmniExtract.Web/appsettings.json`
- [x] 1.3 In `ExtractionService.ExtractTextAsync`, remove the `allChunks.Take(_settings.MaxChunks)` cap and truncation warning
- [x] 1.4 Before the chunk loop, if `chunks.Count > 8` add a warning to a local list: `"Large document: {N} API calls required, extraction may take ~{N*4} minutes."`
- [x] 1.5 After `MergeResults`, append that warning to `merged.Meta.Warnings` if it exists

## 2. Fix Merge — First Non-Null Wins

- [x] 2.1 In `ExtractionService.MergeResults`, replace the `Data` merge line:
  - Old: `g => g.First().Value`
  - New: `g => g.FirstOrDefault(kv => kv.Value is not null).Value ?? g.First().Value`

## 3. Synthesis Pass

- [x] 3.1 Add `SynthesisPrompt` constant to `ExtractionService` (deduplicate/consolidate instruction, strict no-hallucination rules, return raw JSON only)
- [x] 3.2 Add private `SynthesisPassAsync(Dictionary<string, object?> data, CancellationToken ct)` method that:
  - Serialises `data` to compact JSON
  - If JSON length > 80 000 chars, returns `null` (skip)
  - Calls `_gpt.CallAsync` with synthesis prompt + data JSON as user message
  - Parses response as `Dictionary<string, object?>`
  - Returns parsed dict on success, `null` on any failure (logs warning)
- [x] 3.3 In `ExtractTextAsync`, after `MergeResults` and only if `chunks.Count > 1`, call `SynthesisPassAsync`
- [x] 3.4 If synthesis returns non-null, replace `merged.Data` with the synthesis result and add `meta.Warnings` entry: `"Synthesis pass applied."`
- [x] 3.5 If synthesis returns null (skipped or failed), add `meta.Warnings` entry: `"Synthesis pass skipped — using programmatic merge."`
