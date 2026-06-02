## Context

`ResultDetail.razor` displays extracted data from `ResultsEntry.Output` (a `UniversalOutput`). The page already serialises the full output to `_json` for the "Raw JSON" section. There is no export path — users must copy-paste from the UI. Blazor Server has no built-in file-download mechanism; JS interop is the standard approach.

## Goals / Non-Goals

**Goals:**
- One-click JSON download of the full `UniversalOutput` for a result
- One-click CSV download of `Data` fields and `Tables` for a result
- Zero new NuGet dependencies
- No server-side controller or endpoint — pure client-side download via JS interop

**Non-Goals:**
- Bulk export of multiple results
- Excel (`.xlsx`) format
- Export from the Results list page
- Streaming large files (all results are small AI extractions)

## Decisions

### Decision: JS interop for download trigger

Blazor Server runs on the server; you cannot set `Content-Disposition` response headers on a SignalR connection. The standard pattern is to call a small JS helper that creates an `<a>` element with a `data:` or `blob:` URL and programmatically clicks it.

**Alternatives considered:**
- ASP.NET minimal API endpoint (`/api/export/{id}`) — adds routing complexity, requires HTTP auth context; overkill for a POC tool.
- Blazor `NavigationManager.NavigateTo` with a data URL — works for small payloads but unreliable above ~1 MB and not all browsers support `data:` download links consistently.

**Choice:** Add `wwwroot/js/download.js` with a single `triggerDownload(filename, mimeType, content)` function using a Blob + object URL. Register via `<script>` in `App.razor`.

### Decision: ExportService in Web layer

CSV serialisation and JSON formatting belong in a testable service, not inline in a Razor component. `ExportService` is a plain C# class (no interface needed — not mocked in this POC) injected as `Scoped`.

**CSV format:** Header row from `Data` keys, one data row, then a blank row separator, then each table as its own block with a `# Table N` comment header. This keeps all data in one file while remaining parseable.

### Decision: Reuse existing `_json` string for JSON export

`ResultDetail.razor` already builds `_json` (indented, full output). Pass it directly to `ExportService.GetJsonBytes()` (or skip the service call entirely — just encode the string). Avoids double-serialisation.

## Risks / Trade-offs

- [Blob URLs leak if the page is navigated away before revoke] → JS helper calls `URL.revokeObjectURL` after 60 s via `setTimeout`.
- [CSV with commas/quotes in field values] → `ExportService` wraps all values in double-quotes and escapes internal quotes (RFC 4180).
- [Table column counts may be ragged] → CSV writer pads missing cells with empty strings.
- [JS file not loaded on first hot-reload in dev] → Non-issue in production; dev workaround is F5.
