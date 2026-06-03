## ADDED Requirements

### Requirement: Test Lab is accessible from the main navigation
The system SHALL expose a "Test Lab" entry in the sidebar navigation linking to `/lab`. It SHALL be visually distinct (e.g. a flask icon or "Lab" label) to indicate it is a diagnostic tool, not a production feature.

#### Scenario: Nav entry visible
- **WHEN** a user opens the app
- **THEN** "Test Lab" appears in the sidebar navigation
- **AND** clicking it navigates to `/lab`

### Requirement: Test Lab accepts a single file upload and runs all three approaches
The Test Lab page SHALL accept one file upload and run Approaches A, B, and C concurrently. While running, each approach SHALL show a loading indicator. When all three complete, results are displayed side by side.

#### Scenario: File uploaded and approaches run
- **WHEN** a user uploads a file on the Lab page
- **THEN** all three approaches start simultaneously
- **AND** each shows a spinner while in progress
- **AND** results appear as each approach completes

#### Scenario: Re-run with same or different file
- **WHEN** a user clicks "Run Again" after results are shown
- **THEN** previous results are cleared and the file picker is re-shown

### Requirement: Comparison view highlights disagreements between approaches
The side-by-side comparison SHALL highlight any cell where approaches disagree (e.g. different classification decisions) so discrepancies are immediately visible.

#### Scenario: Classification disagreement highlighted
- **WHEN** Approach A returns `narrative` and Approach C returns `structured`
- **THEN** both classification cells are highlighted with a distinct warning colour

#### Scenario: Expandable full output per approach
- **WHEN** a user clicks "Show details" on any approach column
- **THEN** the full extracted `data` fields are shown for that approach

### Requirement: Lab results are ephemeral
Results SHALL only exist in memory for the current page session. Navigating away and returning SHALL show a fresh empty lab. No results are written to `ResultsRepository`.

#### Scenario: Navigate away and return
- **WHEN** a user navigates to Results page and back to Lab
- **THEN** the Lab page is empty with no previous results shown
