## Why

The extraction pipeline currently treats every document the same way — it always runs the same classify→extract path regardless of whether the input is a spreadsheet, a scanned image, or a 20-page contract. This wastes tokens, misroutes documents, and produces empty results when a fast heuristic path picks the wrong strategy (e.g. a text-rich PDF classified as structured returns only `current_datetime`).

## What Changes

- Introduce an `ExtractionStrategy` enum: `Heuristic`, `Vision`, `TextRich`, `Mixed` (future)
- Add a `RouteDocument()` function that inspects extension, text presence, and image presence to select the correct strategy before extraction begins
- Wire the router into `ExtractAsync` so strategy selection is automatic and explicit
- Add confidence-gated fallback: if `Heuristic` extraction returns fewer than 3 meaningful fields, automatically escalate to `TextRich` (B path) and retry
- Expose the selected strategy in `OutputMeta` so the lab and result views can show which path was taken

## Capabilities

### New Capabilities
- `extraction-router`: Selects extraction strategy based on document signals (extension, text, images) before any AI call is made
- `heuristic-fallback`: Detects low-yield heuristic results and escalates to domain-aware extraction automatically

### Modified Capabilities
- `single-adaptive-prompt-approach`: Now invoked explicitly by the router for `TextRich` strategy, not as one of three always-running lab approaches
- `document-classifier`: Router replaces the ad-hoc classification logic scattered across `ExtractionService` and `DocumentProcessor`

## Impact

- `OmniExtract.App/Services/ExtractionService.cs` — add `ExtractionStrategy` enum, `RouteDocument()`, fallback logic
- `OmniExtract.Core/Models/OutputMeta.cs` — add `Strategy` field
- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — no change needed, routes through `ExtractAsync`
- Lab UI — optionally show strategy badge alongside classification label
