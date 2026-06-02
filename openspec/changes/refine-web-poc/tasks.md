## 1. Config Cleanup

- [x] 1.1 Remove `ApiKeyEnvVar` key from `OmniExtract.Web/appsettings.json`
- [x] 1.2 Update sidebar footer in `MainLayout.razor` — replace "v1.0.0 · GPT-4.1 via GitHub Models" with "v1.0.0"

## 2. Upload Page — Remove Token Warning

- [x] 2.1 Delete `_showTokenWarning` field and the `GITHUB_TOKEN` env var check in `Upload.razor`
- [x] 2.2 Delete the warning banner `<div class="warn-item" ...>` block from `Upload.razor`

## 3. Upload Page — Collapsible Format Chips

- [x] 3.1 Add `bool _formatsExpanded = false` state field to `Upload.razor`
- [x] 3.2 Wrap the format chips `<div class="upload-formats">` in a conditional render block
- [x] 3.3 Add a "Supported formats ▾ / Hide formats ▴" toggle button above the chips that flips `_formatsExpanded`

## 4. Dashboard Redesign

- [x] 4.1 Delete the `<div class="stats-grid">` block and all four `<div class="stat-card">` elements from `Home.razor`
- [x] 4.2 Delete the bar chart sections (document type breakdown, domain breakdown) from `Home.razor`
- [x] 4.3 Remove the fake inflation from `@code` block: delete `TotalDocs`, `AvgConfidence`, `SuccessRate`, `DocTypes`, `DomainCount`, `TypeRows`, `DomainRows`, `MaxCount`, `MaxDomainCount`, `ChartColors`
- [x] 4.4 Add a prominent extract CTA section — a styled card/zone with "Extract Document" button linking to `/upload`
- [x] 4.5 Keep the "Recent Extractions" list, cap it at 5 items, add empty state message when list is empty

## 5. Result Detail — Collapsible Output Sections

- [x] 5.1 Audit `ResultDetail.razor` to identify all named output sections (e.g., Entities, Flags, Raw Fields, Categories)
- [x] 5.2 Add a `Dictionary<string, bool>` or per-section `bool` fields to track expanded/collapsed state (all default `false`)
- [x] 5.3 Wrap each section's content in a conditional block driven by its expanded state
- [x] 5.4 Add a clickable header for each section that toggles its state, with a chevron indicator (▶ collapsed / ▾ expanded)
- [x] 5.5 Show a one-line summary when collapsed (section name + field count or type label)

## 6. Test Documents Folder

- [x] 6.1 Create directory `~/Desktop/OmniExtract_TestDocs/`
- [x] 6.2 Write `invoice.txt` — realistic fake invoice (vendor, line items, totals, tax, invoice number)
- [x] 6.3 Write `data.csv` — tabular financial data (5+ columns, 10+ rows, mixed numeric/text)
- [x] 6.4 Write `email.eml` — business email with full headers (From, To, Subject, Date, body)
- [x] 6.5 Write `contract.txt` — legal contract excerpt (parties, effective date, 3+ clauses, termination)
- [x] 6.6 Write `receipt.txt` — purchase receipt (store, items, individual prices, subtotal, tax, total)
- [x] 6.7 Write `report.txt` — business report with sections, headings, and key figures
- [x] 6.8 Write `multilang.txt` — document with English and one other language (e.g., French or Spanish) mixed
- [x] 6.9 Ensure each file starts with a "TEST DATA — NOT REAL" header comment
