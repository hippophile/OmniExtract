## Why

The Results page shows a flat, unfiltered list with only a text search box and domain chips — there is no way to narrow results by document type, sensitivity level, or date, making it hard to find specific extractions as the list grows.

## What Changes

- Add a **document type** filter dropdown (populated from distinct `Meta.DocumentType` values in the result set)
- Add a **sensitivity** filter (public / internal / confidential / restricted) as chip toggles
- Add a **date range** filter (preset quick-picks: Today, Last 7 days, Last 30 days, All time)
- Existing text search and domain chips remain unchanged
- Active filters are reflected in the live document count in the topbar
- A single "Clear all filters" control resets every active filter at once

## Capabilities

### New Capabilities

- `results-filter-bar`: Filter controls for document type, sensitivity, and date range on the Results page

### Modified Capabilities

- none

## Impact

- `OmniExtract.Web/Components/Pages/Results.razor` — filter logic and UI additions
- No backend changes required; all filtering is client-side over the in-memory `ResultsRepository`
- No new dependencies
