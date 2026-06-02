## ADDED Requirements

### Requirement: Upload page format chips are collapsed by default
The upload page SHALL hide the format chips row on initial load behind a toggle. The toggle SHALL be visible and clearly labelled.

#### Scenario: Upload page first load
- **WHEN** the user navigates to `/upload`
- **THEN** the format chips (PDF, DOCX, XLSX, etc.) are not visible
- **THEN** a "Supported formats" toggle/link is visible below the drop zone

#### Scenario: User expands format chips
- **WHEN** the user clicks the "Supported formats" toggle
- **THEN** all format chips become visible
- **THEN** the toggle label changes to indicate it can be collapsed (e.g., "Hide formats")

#### Scenario: User collapses format chips
- **WHEN** the format chips are expanded and the user clicks the toggle again
- **THEN** the format chips are hidden again

### Requirement: Result detail output sections are collapsed by default
Each named section of the structured extraction output on the result detail page (`/results/{id}`) SHALL render collapsed by default, showing only a summary line.

#### Scenario: Result detail page loads
- **WHEN** the user navigates to `/results/{id}`
- **THEN** each structured output section shows only its header/name and a summary (e.g., field count)
- **THEN** the full JSON or field content is not visible until the user expands the section

#### Scenario: User expands a section
- **WHEN** the user clicks a collapsed section header
- **THEN** the full content of that section becomes visible
- **THEN** the section header shows an expanded indicator (e.g., chevron pointing down)

#### Scenario: User collapses an expanded section
- **WHEN** the user clicks an expanded section header
- **THEN** the section content is hidden again
- **THEN** the chevron indicator returns to the collapsed state

#### Scenario: Independent section toggling
- **WHEN** the user expands section A and then expands section B
- **THEN** both sections A and B remain expanded simultaneously
- **THEN** collapsing section A does not affect section B
