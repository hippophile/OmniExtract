## ADDED Requirements

### Requirement: README provides project overview
The README SHALL open with a clear one-paragraph description of what OmniExtract is, what problem it solves, and that it is a POC/testing ground rather than a production tool.

#### Scenario: Reader understands purpose immediately
- **WHEN** a developer opens README.md
- **THEN** they can determine within 10 seconds whether this project is relevant to them

### Requirement: README lists prerequisites
The README SHALL list all prerequisites required to run OmniExtract locally, including: .NET SDK version, GitHub Copilot CLI (for AI backend auth), and LibreOffice (optional, for legacy format support).

#### Scenario: Prerequisites are complete and accurate
- **WHEN** a developer installs all listed prerequisites
- **THEN** the CLI and Web UI can be started without additional setup steps

### Requirement: README provides CLI quick-start
The README SHALL include a CLI quick-start section with copy-pasteable commands covering: cloning the repo, building, and running the CLI against a sample file with expected output described.

#### Scenario: Single-file extraction example
- **WHEN** a developer follows the CLI quick-start
- **THEN** they can extract a document and see a `.json` output file produced

### Requirement: README provides Web UI quick-start
The README SHALL include a Web UI quick-start section showing how to run `OmniExtract.Web` and open it in a browser.

#### Scenario: Web UI launches successfully
- **WHEN** a developer follows the Web UI quick-start commands
- **THEN** the Blazor Server app starts and is accessible at localhost

### Requirement: README lists all supported formats
The README SHALL include a table of all supported input formats grouped by category (Document, Spreadsheet, Presentation, Image, Email, Data, Archive) with the file extensions for each.

#### Scenario: Format table is complete
- **WHEN** a developer consults the supported formats table
- **THEN** every format handled by DocumentProcessor.NativeExtensions and ArchiveHandler is listed

### Requirement: README links to deeper docs
The README SHALL include a section linking to docs/architecture.md, docs/output-schema.md, and docs/extending.md with a one-line description of each.

#### Scenario: Navigation from README to docs
- **WHEN** a developer wants deeper technical information
- **THEN** they can find the relevant doc file via a link in the README
