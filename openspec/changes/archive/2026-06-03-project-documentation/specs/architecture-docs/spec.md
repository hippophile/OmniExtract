## ADDED Requirements

### Requirement: Architecture doc describes project structure
The architecture doc SHALL describe the four-project solution layout (Core, App, Web, CLI) and the responsibility of each project.

#### Scenario: Developer understands where to look for code
- **WHEN** a developer reads the architecture doc
- **THEN** they can identify which project contains any given piece of functionality

### Requirement: Architecture doc shows extraction pipeline as a flowchart
The architecture doc SHALL include a Mermaid flowchart showing the full extraction pipeline: file input → DocumentProcessor (native parse → LibreOffice fallback → vision fallback) → ExtractionService (text path vs. vision path, chunking) → UniversalOutput.

#### Scenario: Pipeline branches are visible
- **WHEN** a developer reads the pipeline flowchart
- **THEN** all three fallback paths (native, LibreOffice, vision) are clearly shown with their trigger conditions

### Requirement: Architecture doc describes the AI backend
The architecture doc SHALL explain that AI calls use the GitHub Copilot SDK (`GptClient.cs`), not a direct OpenAI API key, and describe the session-based call pattern, retry/rate-limit handling, and the two models (text and vision).

#### Scenario: Developer understands AI setup requirement
- **WHEN** a developer reads the AI backend section
- **THEN** they understand that GitHub Copilot CLI auth is required, not a `GITHUB_TOKEN` env var

### Requirement: Architecture doc describes chunking and merging
The architecture doc SHALL explain how large documents are split into chunks and how `MergeResults` combines multiple `UniversalOutput` objects.

#### Scenario: Chunking behavior is documented
- **WHEN** a developer encounters a large document behaving unexpectedly
- **THEN** the architecture doc explains the 80,000-char chunk size and merge strategy

### Requirement: Architecture doc describes storage
The architecture doc SHALL explain the `OutputWriter` flat-file storage pattern: output JSON is written alongside the source file as `<filename>.json`.

#### Scenario: Output location is clear
- **WHEN** a developer processes a file
- **THEN** they know exactly where to find the output JSON based on the architecture doc
