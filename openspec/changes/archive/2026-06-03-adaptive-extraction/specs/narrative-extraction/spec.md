## ADDED Requirements

### Requirement: Narrative documents use a dedicated extraction prompt
When the classifier returns `narrative`, the system SHALL use a narrative extraction prompt that returns structured output capturing the meaning, flow, and key content of the document — not flat field extraction.

#### Scenario: Narrative prompt output schema
- **WHEN** a narrative document is extracted in Standard mode
- **THEN** the result `data` field SHALL contain at minimum: `title`, `summary`, `sections` (array of `{heading, key_points, summary}`), `conclusions`, `key_entities`
- **AND** `meta.document_type` reflects the document's nature (e.g. "financial report", "academic essay", "business email")

#### Scenario: Email extraction returns narrative output
- **WHEN** an `.eml` file with a long body is classified as narrative
- **THEN** `data.summary` contains the email's core message
- **AND** `data.key_entities` contains sender, recipients, and any named organisations or amounts

#### Scenario: Multi-chunk narrative document merges sections
- **WHEN** a narrative document is split into multiple chunks
- **THEN** each chunk returns its own `sections` array
- **AND** the merge combines all sections in order, deduplicating overlapping headings

### Requirement: Standard narrative extraction is deterministic
The system SHALL use temperature=0 for narrative extraction in Standard mode, producing consistent, objective output on repeated runs of the same document.

#### Scenario: Repeated extraction produces identical results
- **WHEN** the same narrative document is extracted twice in Standard mode
- **THEN** both results have identical `data.summary` and `data.sections` content
