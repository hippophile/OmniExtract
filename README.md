# OmniExtract

Universal AI-powered document extraction engine. Drop any document in any format — PDF, scanned image, Word, Excel, email, archive — and get back clean, structured JSON. No hardcoded schemas. No templates.

> **This is a POC.** The web UI is a testing cockpit for evaluating extraction quality, not a production application.

---

## Prerequisites

| Requirement | Version / Notes |
|---|---|
| .NET SDK | 8.0 or later |
| GitHub Copilot CLI | Required — the AI backend uses `GitHub.Copilot.SDK` which authenticates via Copilot |
| LibreOffice | *Optional* — enables legacy formats (`.doc`, `.xls`, `.odt`, `.odp`, etc.) |

**GitHub Copilot CLI setup:**
```bash
# Install the GitHub CLI if not already installed
# Then authenticate with Copilot
gh auth login
gh extension install github/gh-copilot
```

**LibreOffice (optional, Linux/macOS):**
```bash
sudo apt install libreoffice   # Debian/Ubuntu
brew install --cask libreoffice  # macOS
```

---

## CLI Quick-start

The CLI (`OmniExtract.App`) processes files and writes JSON output alongside each source file.

```bash
# Clone and build
git clone <repo-url>
cd OmniExtract
dotnet build

# Process a single file
dotnet run --project OmniExtract.App -- path/to/invoice.pdf

# Process all files in a folder (recursively, skips already-processed)
dotnet run --project OmniExtract.App -- path/to/documents/

# Watch mode: process existing files, then watch for new ones
dotnet run --project OmniExtract.App -- --watch path/to/inbox/
```

Output is written as `<filename>.json` alongside each source file (e.g. `invoice.pdf` → `invoice.pdf.json`).

---

## Web UI Quick-start

The Blazor Server web UI (`OmniExtract.Web`) provides a drag-and-drop extraction cockpit with live queue monitoring and result inspection.

```bash
cd OmniExtract.Web
dotnet run
```

Then open `https://localhost:5001` (or the URL shown in the terminal).

- **Upload** — drag and drop files, monitor the extraction pipeline in real time
- **Results** — browse, search, and filter all extracted results
- **Result detail** — inspect extracted fields, tables, tags, and raw JSON; rate extraction quality; download JSON or CSV

---

## Supported Formats

| Category | Formats |
|---|---|
| **Document** | PDF, DOCX, DOC\*, RTF, ODT\* |
| **Spreadsheet** | XLSX, XLS\*, ODS\*, CSV, TSV |
| **Presentation** | PPTX, PPT\*, ODP\* |
| **Image / Scanned** | PNG, JPG/JPEG, TIFF, BMP, WEBP |
| **Email** | EML |
| **Data / Text** | JSON, JSONL, YAML, XML, TXT, MD |
| **Archive** | ZIP (recursive extraction of all members) |

\* Requires LibreOffice installed — converted to PDF before extraction.

---

## Documentation

| Document | Description |
|---|---|
| [docs/architecture.md](docs/architecture.md) | Extraction pipeline, component graph, AI backend, chunking |
| [docs/output-schema.md](docs/output-schema.md) | Full `UniversalOutput` JSON schema with annotated example |
| [docs/extending.md](docs/extending.md) | How to add a new format parser or modify AI behaviour |
| [docs/optima-bank-use-cases.md](docs/optima-bank-use-cases.md) | Optima Bank Greece deployment use cases |
