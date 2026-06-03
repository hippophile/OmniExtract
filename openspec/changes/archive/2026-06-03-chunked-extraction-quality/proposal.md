## Why

Large documents are split into chunks and each chunk is sent to the AI independently. The resulting JSON objects are then merged programmatically. This pipeline has three correctness defects: the merge is lossy (nulls beat real values), the chunk cap silently drops content, and cross-chunk relationships (e.g. an invoice header in chunk 1, its total in chunk 5) are never reconciled. Together these produce incomplete and sometimes wrong extraction results for any document that requires chunking.

## What Changes

- **Fix merge**: `MergeResults` switches from first-value-wins to first-non-null-wins per `data` key, so a real value in a later chunk is not silently discarded.
- **Remove chunk cap**: `MaxChunks` setting and the truncation logic are removed. All chunks are processed. A time-estimate warning is added to the job stage label so users know to expect a long run.
- **Synthesis pass**: After multi-chunk extraction and programmatic merge, send only the merged `data` JSON (not the original text) to the AI once with a dedicated synthesis prompt asking it to deduplicate keys, resolve contradictions, and surface cross-chunk relationships. The synthesis result replaces the `data` field of the merged output; `tables`, `tags`, and `meta` are unchanged.

## Capabilities

### New Capabilities
- `synthesis-pass`: Post-merge AI consolidation pass for multi-chunk documents.

### Modified Capabilities
- *(none — merge fix and cap removal are internal correctness fixes, not behaviour-visible spec changes)*

## Impact

- `OmniExtract.Core/Config/AppSettings.cs` — remove `MaxChunks`
- `OmniExtract.App/Services/ExtractionService.cs` — fix `MergeResults`, remove cap logic, add `SynthesisPassAsync`
- `OmniExtract.Web/appsettings.json` — remove `MaxChunks` entry
