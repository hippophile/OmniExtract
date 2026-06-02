## 1. Root README

- [ ] 1.1 Create `README.md` with project overview paragraph (what OmniExtract is, POC nature)
- [ ] 1.2 Add prerequisites section: .NET SDK version, GitHub Copilot CLI requirement, LibreOffice (optional)
- [ ] 1.3 Add CLI quick-start: clone, build, run single file, run folder mode, run watch mode
- [ ] 1.4 Add Web UI quick-start: `dotnet run` in OmniExtract.Web, open browser
- [ ] 1.5 Add supported formats table grouped by category (Document, Spreadsheet, Presentation, Image, Email, Data, Archive)
- [ ] 1.6 Add links section pointing to docs/architecture.md, docs/output-schema.md, docs/extending.md

## 2. Architecture Doc

- [ ] 2.1 Create `docs/architecture.md` with project structure section describing Core, App, Web responsibilities
- [ ] 2.2 Add Mermaid flowchart of extraction pipeline (DocumentProcessor native → LibreOffice fallback → vision fallback)
- [ ] 2.3 Add AI backend section explaining Copilot SDK, session-based call pattern, retry logic, text vs. vision models
- [ ] 2.4 Add chunking and merging section explaining 80,000-char chunk size and MergeResults behaviour
- [ ] 2.5 Add storage section explaining OutputWriter flat-file pattern (`<filename>.json` alongside source)

## 3. Output Schema Doc

- [ ] 3.1 Create `docs/output-schema.md` with top-level fields table (meta, tags, categories, data, tables)
- [ ] 3.2 Document all OutputMeta fields including allowed values for extraction_method
- [ ] 3.3 Document the `data` field — explain dynamic keys, include examples for invoice/contract/email
- [ ] 3.4 Document the `tables` field — explain 3D array structure with access pattern example
- [ ] 3.5 Add full annotated JSON example showing a realistic extraction output

## 4. Extension Guide

- [ ] 4.1 Create `docs/extending.md` with step-by-step guide for adding a new format parser
- [ ] 4.2 Document ExtractionResult contract: when to return Text vs. Images vs. Error
- [ ] 4.3 Add section explaining system prompt location and how to modify extraction behaviour
- [ ] 4.4 Add section explaining how to change AI models via appsettings.json with Copilot availability note
