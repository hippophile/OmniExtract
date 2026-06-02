## Why

Extracted data is trapped in the UI — users have no way to get results out for downstream use, archival, or integration. Export is the minimum viable data egress for a document extraction tool.

## What Changes

- Add **Download JSON** button to `ResultDetail` page — downloads the full `UniversalOutput` as a `.json` file
- Add **Download CSV** button to `ResultDetail` page — downloads a flattened CSV of `Data` fields, followed by each `Tables` entry as a separate CSV section
- Both downloads are client-triggered file downloads (no server endpoint required — Blazor JS interop)
- A small `ExportService` (or static helper) handles serialisation/formatting logic

## Capabilities

### New Capabilities

- `result-export`: Download extracted results from the result detail page as JSON or CSV

### Modified Capabilities

<!-- none -->

## Impact

- `OmniExtract.Web/Components/Pages/ResultDetail.razor` — add export buttons and wire up JS interop calls
- `OmniExtract.Web/Services/ExportService.cs` (new) — JSON serialisation and CSV flattening logic
- `OmniExtract.Web/wwwroot/js/download.js` (new) — `triggerDownload(filename, mimeType, content)` JS helper
- `OmniExtract.Web/Components/App.razor` — reference the new JS file
- No new dependencies; uses `System.Text.Json` (already present) and vanilla JS
