## ADDED Requirements

### Requirement: Documents are classified as structured or narrative before extraction
The system SHALL classify every text document as either `structured` or `narrative` using a heuristic scorer before any AI extraction call is made. The classification SHALL use only information already in memory (file extension, first 500 characters, total character count) and SHALL NOT make any additional API calls.

#### Scenario: Invoice classified as structured
- **WHEN** a document has extension `.xlsx` or its first 500 chars contain 3+ numeric patterns and delimiter characters
- **THEN** the classifier returns `structured`
- **AND** the structured extraction prompt is used

#### Scenario: Essay classified as narrative
- **WHEN** a document has extension `.txt` or `.md` and its first 500 chars contain 3+ sentences with average word length > 4
- **THEN** the classifier returns `narrative`
- **AND** the narrative extraction prompt is used

#### Scenario: Extension overrides prose score for tabular formats
- **WHEN** a document has extension `.csv`, `.tsv`, `.xlsx`, `.xls`, or `.json`
- **THEN** the classifier returns `structured` regardless of prose indicators in the content

#### Scenario: Classification stored on result
- **WHEN** extraction completes
- **THEN** `meta.extraction_method` includes the classification used (e.g. `text/structured` or `text/narrative`)
