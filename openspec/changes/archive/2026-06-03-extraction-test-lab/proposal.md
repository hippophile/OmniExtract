## Why

Before committing to one document classification strategy, we need empirical evidence of which approach works best across different document types. The three candidate approaches (classify-first, single adaptive prompt, heuristic) have different trade-offs in accuracy, latency, and API cost that can only be evaluated by running them against real documents. A Test Lab page lets us do this directly in the app rather than in throwaway scripts.

## What Changes

- **New nav page "Test Lab"**: Upload a single document and run all three classification approaches simultaneously. Results shown side by side in a comparison table.
- **Approach A — Classify-first**: Dedicated API call that returns document class (`structured`/`narrative`) before the extraction call. Two API calls total.
- **Approach B — Single adaptive prompt**: One prompt that asks GPT to classify and extract simultaneously. One API call, richer prompt.
- **Approach C — Heuristic**: No extra API call — uses extension + first-500-chars scoring. Zero cost classification.
- **Comparison view**: For each approach, shows classification decision, confidence, key extracted fields, warnings, and elapsed time. No persistence — ephemeral in-memory results for the current session only.

## Capabilities

### New Capabilities
- `test-lab-page`: New Blazor page at `/lab` with nav entry, file upload, parallel approach runner, and side-by-side comparison view.
- `classify-first-approach`: Dedicated classification API call followed by extraction — implementation of approach A.
- `single-adaptive-prompt-approach`: Combined classification+extraction in one prompt — implementation of approach B.

### Modified Capabilities
*(none — heuristic approach C reuses the classifier from `adaptive-extraction`)*

## Impact

- `OmniExtract.Web/Components/Pages/Lab.razor` — new page (created)
- `OmniExtract.Web/Components/Layout/NavMenu.razor` — add "Test Lab" nav entry
- `OmniExtract.App/Services/ExtractionService.cs` — add `RunApproachAAsync`, `RunApproachBAsync` methods; `RunApproachCAsync` reuses existing heuristic classifier
- `OmniExtract.Web/wwwroot/app.css` — comparison table and lab page styles
