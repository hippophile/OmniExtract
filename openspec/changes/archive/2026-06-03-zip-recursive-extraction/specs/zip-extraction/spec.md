## ADDED Requirements

### Requirement: ZIP files uploaded via web UI are processed as archives
When a user uploads a ZIP file through the Blazor web UI, the system SHALL extract its contents, process each member file through the standard extraction pipeline, merge all results into a single `UniversalOutput`, and store that as the job's result — rather than attempting to parse the ZIP binary directly.

#### Scenario: Single-level ZIP with processable files
- **WHEN** a user uploads a ZIP containing two PDF files
- **THEN** each PDF is extracted to a temp directory and processed individually
- **AND** the two `UniversalOutput` results are merged into one
- **AND** the job result is stored with `SourceFile` set to the ZIP filename

#### Scenario: ZIP containing unsupported or unprocessable files
- **WHEN** a ZIP member file fails extraction or AI parsing
- **THEN** the failure is captured as a low-confidence `UniversalOutput` with the error in `Meta.Warnings`
- **AND** it is included in the merge (not dropped)
- **AND** the job still completes with `JobStatus.Done`

#### Scenario: Nested ZIP (ZIP inside ZIP)
- **WHEN** a ZIP contains another ZIP file as a member
- **THEN** the inner ZIP is recursively extracted and its members processed
- **AND** all results (from all nesting levels) are merged into the single top-level job output

#### Scenario: Empty ZIP
- **WHEN** a user uploads a ZIP with no extractable files
- **THEN** the job completes with `JobStatus.Done`
- **AND** the result is an empty `UniversalOutput` with a warning indicating zero members were found

### Requirement: Archive extraction registered in web DI container
The web application SHALL register `ArchiveHandler` in its dependency injection container so it can be injected into `ExtractionOrchestrator`.

#### Scenario: Web host starts successfully with ArchiveHandler registered
- **WHEN** the Blazor web application starts
- **THEN** `ArchiveHandler` is resolvable from the DI container without error

### Requirement: Job stage labels reflect archive processing progress
During ZIP processing, the job's `Stage` field SHALL update to indicate how many member files have been processed.

#### Scenario: Progress updates during archive extraction
- **WHEN** a ZIP with multiple files is being processed
- **THEN** the job stage label shows "Processing file N of M..." for each member file
- **AND** the final stage shows "Complete" after merging

### Requirement: Merged result SourceFile identifies the archive
The merged `UniversalOutput.Meta.SourceFile` SHALL be set to the original ZIP filename.

#### Scenario: Result traceable to source archive
- **WHEN** a ZIP named `contracts-2024.zip` is uploaded and processed
- **THEN** the stored result has `Meta.SourceFile` = `"contracts-2024.zip"`

### Requirement: MergeResults accessible for use by ExtractionOrchestrator
`ExtractionService.MergeResults` SHALL be accessible from `ExtractionOrchestrator` to combine per-member extraction outputs.

#### Scenario: MergeResults combines multiple outputs correctly
- **WHEN** `MergeResults` receives a list of two `UniversalOutput` objects
- **THEN** it returns one `UniversalOutput` with tags deduplicated, tables concatenated, data fields merged (first-wins on key collision), confidence averaged, and languages unioned
