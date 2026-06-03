## 1. Root README

- [x] 1.1 Create `README.md` with project overview paragraph (what OmniExtract is, POC nature)
- [x] 1.2 Add prerequisites section: .NET SDK version, GitHub Copilot CLI requirement, LibreOffice (optional)
- [x] 1.3 Add CLI quick-start: clone, build, run single file, run folder mode, run watch mode
- [x] 1.4 Add Web UI quick-start: `dotnet run` in OmniExtract.Web, open browser
- [x] 1.5 Add supported formats table grouped by category (Document, Spreadsheet, Presentation, Image, Email, Data, Archive)
- [x] 1.6 Add links section pointing to docs/architecture.md, docs/output-schema.md, docs/extending.md

## 2. Architecture Doc

- [x] 2.1 Create `docs/architecture.md` with project structure section describing Core, App, Web responsibilities
- [x] 2.2 Add Mermaid flowchart of extraction pipeline (DocumentProcessor native → LibreOffice fallback → vision fallback)
- [x] 2.3 Add AI backend section explaining Copilot SDK, session-based call pattern, retry logic, text vs. vision models
- [x] 2.4 Add chunking and merging section explaining 80,000-char chunk size and MergeResults behaviour
- [x] 2.5 Add storage section explaining OutputWriter flat-file pattern (`<filename>.json` alongside source)

## 3. Output Schema Doc

- [x] 3.1 Create `docs/output-schema.md` with top-level fields table (meta, tags, categories, data, tables)
- [x] 3.2 Document all OutputMeta fields including allowed values for extraction_method
- [x] 3.3 Document the `data` field — explain dynamic keys, include examples for invoice/contract/email
- [x] 3.4 Document the `tables` field — explain 3D array structure with access pattern example
- [x] 3.5 Add full annotated JSON example showing a realistic extraction output

## 4. Extension Guide

- [x] 4.1 Create `docs/extending.md` with step-by-step guide for adding a new format parser
- [x] 4.2 Document ExtractionResult contract: when to return Text vs. Images vs. Error
- [x] 4.3 Add section explaining system prompt location and how to modify extraction behaviour
- [x] 4.4 Add section explaining how to change AI models via appsettings.json with Copilot availability note
