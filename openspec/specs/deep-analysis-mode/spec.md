## ADDED Requirements

### Requirement: User can select Deep Analysis mode before uploading
The system SHALL provide a "Deep Analysis" toggle on the Upload page. When enabled, all documents in that upload batch are processed with the deep analysis prompt instead of the standard prompt.

#### Scenario: Toggle visible on upload page
- **WHEN** the user visits the Upload page
- **THEN** a "Deep Analysis" checkbox or toggle is visible near the drop zone
- **AND** it is unchecked by default

#### Scenario: Deep Analysis mode stored on job
- **WHEN** a file is enqueued with Deep Analysis enabled
- **THEN** the job's analysis mode is set to `DeepAnalysis`
- **AND** this is visible in the job card (e.g. a badge or label)

### Requirement: Deep Analysis produces a distilled findings brief
When Deep Analysis mode is active on a narrative document, the system SHALL perform an additional AI pass that condenses the document into a prioritised brief of approximately 1–2 pages. The brief SHALL surface exact facts, flagged risks, certainty levels, and a one-page summary.

#### Scenario: Deep Analysis output schema
- **WHEN** a narrative document is extracted in Deep Analysis mode
- **THEN** `data` SHALL contain: `distilled_findings` (array of `{finding, certainty, source_section}`), `risks` (array), `key_facts` (array), `flags` (array of notable anomalies or sensitive items), `one_page_summary`

#### Scenario: Deep Analysis on structured document is ignored
- **WHEN** a structured document (invoice, spreadsheet) is submitted with Deep Analysis enabled
- **THEN** standard structured extraction runs unchanged
- **AND** a warning is added: `"Deep Analysis skipped — document classified as structured."`

#### Scenario: Deep Analysis uses interpretive temperature
- **WHEN** the deep analysis prompt is called
- **THEN** the API call uses temperature=0.3 to allow interpretive synthesis
- **AND** standard extraction calls within the same job still use temperature=0

### Requirement: Deep Analysis result is rendered distinctly in the UI
The system SHALL render Deep Analysis results with a dedicated layout that highlights findings, risks, and the one-page summary — distinct from both the flat key-value structured view and the narrative sections view.

#### Scenario: Deep Analysis result page shows findings panel
- **WHEN** a user views a result produced with Deep Analysis mode
- **THEN** the result detail page shows a "Key Findings" panel with the distilled findings list
- **AND** each finding shows its certainty level and source section reference
