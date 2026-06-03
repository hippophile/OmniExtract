## Why

The current extraction pipeline uses a single prompt for every document type, which works well for structured documents (invoices, forms, spreadsheets) but produces shallow, near-empty results for narrative documents (essays, reports, emails, contracts) — discarding the actual content in favour of a few metadata fields. Users uploading financial reports, company decisions, or long-form text get back 3 fields instead of meaningful analysis.

## What Changes

- **Document classifier**: Heuristic pre-classification (no extra API call) — peeks at first 500 chars + file extension + character density to distinguish structured vs narrative documents before extraction begins.
- **Narrative extraction prompt**: New prompt for text-heavy documents returning title, author, summary, per-section key points, conclusions, key entities, and document flow — instead of flat field extraction.
- **Structured extraction prompt**: Existing prompt retained unchanged for invoices, forms, spreadsheets, data files.
- **Deep Analysis mode**: Optional user-selectable mode on the Upload page. When enabled, the document is distilled into ~1.5 pages of prioritised findings — exact facts, flagged risks, certainty levels — suited for legal, financial, and sensitive documents. Uses slightly higher temperature for interpretive output.
- **Standard mode** remains temp=0, deterministic, objective viewer.

## Capabilities

### New Capabilities
- `document-classifier`: Heuristic classification of documents as structured or narrative before extraction, with no additional API call.
- `narrative-extraction`: Extraction prompt and output schema for text-heavy documents — summary, sections, conclusions, key entities.
- `deep-analysis-mode`: User-selectable high-priority analysis mode producing a condensed, distilled findings brief from long-form documents.

### Modified Capabilities
- *(none — structured extraction behaviour is unchanged)*

## Impact

- `OmniExtract.App/Services/ExtractionService.cs` — add classifier, narrative prompt, deep analysis prompt, mode-aware dispatch
- `OmniExtract.Core/Models/UniversalOutput.cs` — extend `data` schema or add typed fields for narrative output (sections, summary)
- `OmniExtract.Web/Components/Pages/Upload.razor` — add Deep Analysis toggle on upload UI
- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — pass analysis mode through to extraction service
- `OmniExtract.Web/Components/Pages/Results.razor` — render narrative output (sections, summary, key points) differently from flat key-value data
