## 1. Verdict AI Pass (ExtractionService)

- [x] 1.1 Add `VerdictPrompt` constant — instructs model to produce `{ summary, action_items: [{ item, priority }], flags }` from extracted fields only, no hallucination
- [x] 1.2 Add `VerdictPassAsync(UniversalOutput result, CancellationToken ct)` method — serialises `Data` fields + document type + tags as compact input, calls AI, parses response
- [x] 1.3 Skip verdict and add warning if `Data` has fewer than 3 meaningful fields
- [x] 1.4 Wrap in try/catch with 45s timeout — fail silent, add warning on failure

## 2. Orchestrator Integration

- [x] 2.1 Add "Generating verdict..." stage in `ProcessJobAsync` — update `job.Stage` and notify UI
- [x] 2.2 Call `VerdictPassAsync` after agent recommendation pass (run concurrently with `Task.WhenAll`)
- [x] 2.3 Store verdict result in `result.Data["verdict"]` before saving to repository

## 3. Upload UI — Pipeline Step

- [x] 3.1 Add `"Verdict"` to `_stepLabels` array in `Upload.razor` between `"Recommend"` and `"Done"`
- [x] 3.2 Add `"Generating verdict..."` case to `GetActiveStep` switch returning correct index

## 4. Result Detail — Verdict Card UI

- [x] 4.1 Add verdict card rendering block in `ResultDetail.razor` — positioned below warnings, above extracted fields, only if `Data["verdict"]` present
- [x] 4.2 Parse verdict JSON from `Data["verdict"]` — extract `summary`, `action_items`, `flags`
- [x] 4.3 Render summary as large prominent text in a distinct dark panel
- [x] 4.4 Render each action item with priority badge (high=red, medium=amber, low=green)
- [x] 4.5 Add verdict card CSS — full-width panel, premium visual treatment distinct from other sections
