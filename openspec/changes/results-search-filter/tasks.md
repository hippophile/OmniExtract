## 1. Filter State

- [ ] 1.1 Add `_activeDocType` (string?) and `_activeSensitivity` (string?) fields to `Results.razor`
- [ ] 1.2 Add `_datePreset` enum or string field (default `"all"`) for the four presets
- [ ] 1.3 Derive `_docTypes` list from `_all` (distinct `Meta.DocumentType`, sorted) in `OnInitialized`
- [ ] 1.4 Derive `_sensitivities` list from `_all` (distinct `Categories.Sensitivity`, sorted) in `OnInitialized`

## 2. Filter Logic

- [ ] 2.1 Extend `_filtered` computed property to AND-filter on `_activeDocType` (when non-null)
- [ ] 2.2 Extend `_filtered` to AND-filter on `_activeSensitivity` (when non-null)
- [ ] 2.3 Extend `_filtered` to AND-filter on `_datePreset` using `ProcessedAt` UTC comparisons (Today / Last7 / Last30 / All)
- [ ] 2.4 Add `_anyFilterActive` bool property: true when domain, docType, sensitivity, or non-All date preset is set

## 3. Filter Bar UI

- [ ] 3.1 Add document-type chip row to the `.filter-row` in `Results.razor`, using the existing `.filter-chip` / `.active` pattern
- [ ] 3.2 Add sensitivity chip row using the same `.filter-chip` pattern with `ToggleSensitivity` handler
- [ ] 3.3 Add date preset buttons (Today / Last 7 days / Last 30 days / All time) as chips with active state
- [ ] 3.4 Add "Clear filters" button (`.btn-ghost`) — render only when `_anyFilterActive` is true
- [ ] 3.5 Wire "Clear filters" to a `ClearAllFilters()` method that resets domain, docType, sensitivity, and date preset

## 4. Polish

- [ ] 4.1 Verify topbar count reflects filtered count (already uses `_filtered.Count` — confirm still correct)
- [ ] 4.2 Confirm empty state message is shown when all filters together produce zero results
- [ ] 4.3 Manual smoke-test: activate each filter dimension individually and in combination
