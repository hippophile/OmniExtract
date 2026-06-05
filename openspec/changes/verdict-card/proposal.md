## Why

The result detail page currently shows extracted fields and raw JSON — useful for engineers but meaningless to a business user. A manager looking at the output sees data, not understanding. The verdict card transforms the output from "AI JSON extractor" into "document intelligence" by surfacing what matters and what to do next, in plain language.

## What Changes

- New AI pass runs after spec-first extraction, using the already-extracted flat fields as input (cheap — no re-reading the full document)
- Produces a structured verdict: 2–3 sentence analyst-style summary + bullet-point action items / flags
- Verdict stored in `result.Data["verdict"]` as a structured JSON object
- Result detail page gains a prominent verdict card rendered above extracted fields
- Processing pipeline gains a new "Generating verdict..." stage visible in the upload UI
- Verdict pass runs alongside the existing agent recommendation pass (both are post-extraction enrichment)

## Capabilities

### New Capabilities
- `verdict-card`: AI-generated human-readable brief shown at the top of every result — document summary, key facts, and flagged action items (deadlines, missing signatures, risks, amounts requiring approval)

### Modified Capabilities

## Impact

- `ExtractionService.cs` — new `VerdictPassAsync` method, new `VerdictPrompt` constant
- `ExtractionOrchestrator.cs` — call verdict pass after extraction, new "Generating verdict..." stage
- `ResultDetail.razor` — render verdict card above extracted fields section
- `Upload.razor` — add "Verdict" step to pipeline UI
- `UniversalOutput` model — `verdict` key stored in `Data` dict (no schema change needed)
