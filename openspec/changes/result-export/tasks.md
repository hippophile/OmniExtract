## 1. JS Download Helper

- [ ] 1.1 Create `OmniExtract.Web/wwwroot/js/download.js` with `triggerDownload(filename, mimeType, content)` using Blob + object URL, auto-revoke after 60 s
- [ ] 1.2 Add `<script src="js/download.js"></script>` to `OmniExtract.Web/Components/App.razor`

## 2. ExportService

- [ ] 2.1 Create `OmniExtract.Web/Services/ExportService.cs` with `GetJsonBytes(string json): byte[]` (UTF-8 encode existing JSON string)
- [ ] 2.2 Add `BuildCsv(UniversalOutput output): string` to `ExportService` — data fields header + value row, then `# Table N` + rows per table, RFC 4180 quoting
- [ ] 2.3 Register `ExportService` as `Scoped` in `Program.cs`

## 3. ResultDetail Export Buttons

- [ ] 3.1 Inject `IJSRuntime` and `ExportService` into `ResultDetail.razor`
- [ ] 3.2 Add `DownloadJson()` async method — calls `ExportService.GetJsonBytes(_json)`, invokes `triggerDownload` via JS interop with filename `<documentType>-<id>.json` and MIME `application/json`
- [ ] 3.3 Add `DownloadCsv()` async method — calls `ExportService.BuildCsv(_entry.Output)`, invokes `triggerDownload` via JS interop with filename `<documentType>-<id>.csv` and MIME `text/csv`
- [ ] 3.4 Add "Download JSON" and "Download CSV" buttons to the `topbar-right` div in the page markup, only rendered when `_entry` is not null

## 4. Verification

- [ ] 4.1 Run the app, open a result detail page, click "Download JSON" — verify file downloads with correct name and valid JSON content
- [ ] 4.2 Click "Download CSV" — verify file downloads, open in a spreadsheet, confirm data fields and tables are present and correctly formatted
- [ ] 4.3 Verify buttons are absent on the "Result not found" error state
