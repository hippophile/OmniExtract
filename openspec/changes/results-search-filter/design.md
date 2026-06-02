## Context

The Results page (`Results.razor`) already has a text search input and domain chip filters wired to a computed `_filtered` list. All data lives in the singleton `ResultsRepository` (in-memory). No server calls are made at filter time. The existing filter bar UI uses `.filter-row` / `.filter-chip` / `.search-wrap` CSS classes from the project stylesheet.

## Goals / Non-Goals

**Goals:**
- Add document-type, sensitivity, and date-range filtering to the existing filter bar
- Keep all filter state in the Razor component (no URL params, no service layer)
- Reuse existing CSS primitives (`.filter-chip`, `.btn-ghost`) for new controls
- Maintain instant, client-side reactivity matching the existing search behaviour

**Non-Goals:**
- Persisting filter state across navigation
- Server-side or paginated querying
- Sorting controls (separate concern)
- Free-text tag search (already covered by existing `_search` field)

## Decisions

### D1 — All filtering stays in the component

Alternatives considered: a `FilterState` service injected into the component, or a query-string approach. Both add indirection with no benefit for a POC cockpit. Keeping state as private fields in the Razor component is simpler and consistent with what already exists.

### D2 — Document type and sensitivity use chip toggles, not a dropdown

The number of distinct values is small (≤ 6 typical). Chips give immediate visual feedback and match the existing domain-chip pattern already in the UI. A `<select>` would be inconsistent.

### D3 — Date range uses named preset buttons, not a date-picker

A calendar date-picker adds JS interop complexity. Named presets (Today / Last 7 days / Last 30 days / All time) cover all practical use cases for a cockpit and stay purely in Blazor.

### D4 — "Clear all filters" resets every dimension at once

Individually clearing each filter type would require multiple clicks. A single reset control is faster and consistent with typical data-table UX.

## Risks / Trade-offs

- **Chip row wraps on small screens** → Acceptable for a POC; the filter row is already a flex-wrap container.
- **Sensitivity values are strings from AI output** → Values may be inconsistent across documents. Mitigation: derive distinct values dynamically from `_all`, same as the domain chips today.
- **Document-type list can be long** → Cap display at the top 8 most-frequent values with an overflow indicator if needed; revisit if real-world data shows this.
