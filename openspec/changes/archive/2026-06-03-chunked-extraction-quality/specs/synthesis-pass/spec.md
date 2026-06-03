## ADDED Requirements

### Requirement: Merge uses first non-null value per data key
When merging chunk results, the system SHALL prefer the first non-null value for each `data` key over the first value regardless of nullness.

#### Scenario: Later chunk has real value, earlier chunk has null
- **WHEN** chunk 1 extracts `{ "invoice_total": null }` and chunk 3 extracts `{ "invoice_total": "€1,200" }`
- **THEN** the merged result contains `{ "invoice_total": "€1,200" }`

#### Scenario: First chunk has real value
- **WHEN** chunk 1 extracts `{ "vendor": "Acme" }` and chunk 2 also extracts `{ "vendor": "Acme Corp" }`
- **THEN** the merged result contains `{ "vendor": "Acme" }` (first non-null wins)

### Requirement: All chunks are processed regardless of count
The system SHALL process every chunk produced from a document without an upper cap. The system SHALL add a human-readable time estimate to `meta.warnings` when chunk count exceeds 8.

#### Scenario: Document produces more than 8 chunks
- **WHEN** a document is split into 16 chunks
- **THEN** all 16 chunks are sent to the AI and their results are merged
- **AND** the result's warnings include an estimated processing time notice

#### Scenario: Time estimate warning content
- **WHEN** chunk count N exceeds 8
- **THEN** the warning reads approximately `"Large document: N API calls required, extraction may take ~T minutes."`

### Requirement: Synthesis pass consolidates multi-chunk data
For documents split into more than one chunk, the system SHALL perform one additional AI call after the programmatic merge. This call receives only the merged `data` JSON and SHALL return a consolidated `data` object with duplicates resolved, contradictions settled, and cross-chunk relationships filled.

#### Scenario: Successful synthesis
- **WHEN** a document is processed in 3 or more chunks
- **THEN** after the per-chunk AI calls and programmatic merge, one synthesis AI call is made
- **AND** the synthesis response replaces the `data` field of the final result

#### Scenario: Synthesis failure falls back gracefully
- **WHEN** the synthesis AI call fails or returns unparseable JSON
- **THEN** the system logs a warning and retains the programmatically merged `data`
- **AND** the job still completes with status Done

#### Scenario: Merged data too large for synthesis
- **WHEN** the merged data JSON exceeds 80 000 characters
- **THEN** the synthesis pass is skipped
- **AND** a warning is added: `"Merged data too large for synthesis pass — using programmatic merge."`
