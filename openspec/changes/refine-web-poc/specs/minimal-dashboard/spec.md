## ADDED Requirements

### Requirement: Dashboard shows extract CTA
The dashboard (`/`) SHALL display a prominent call-to-action that navigates to `/upload`, visible without scrolling on first load.

#### Scenario: User opens dashboard with no prior extractions
- **WHEN** the user navigates to `/`
- **THEN** a drop zone or "Extract Document" button is shown as the primary visual element
- **THEN** no KPI cards, charts, or stat rows are displayed

#### Scenario: User opens dashboard with prior extractions
- **WHEN** the user navigates to `/` and `ResultsRepository` returns one or more results
- **THEN** the extract CTA is shown above a "Recent Extractions" list
- **THEN** the list shows at most 5 items, each linking to `/results/{id}`

### Requirement: Dashboard shows only real data
The dashboard SHALL NOT display hardcoded, inflated, or estimated numbers. All displayed values MUST come directly from `ResultsRepository.GetAll()`.

#### Scenario: Empty result store
- **WHEN** `ResultsRepository.GetAll()` returns an empty list
- **THEN** the recent extractions area shows an empty state message (e.g., "No extractions yet")
- **THEN** no counts, percentages, or deltas are shown

#### Scenario: Non-empty result store
- **WHEN** `ResultsRepository.GetAll()` returns N items
- **THEN** the dashboard shows at most 5 of the most recent items
- **THEN** no values add artificial offsets (no `+ 40`, no hardcoded `"96.8%"`)

### Requirement: GitHub token references removed from UI
The upload page and sidebar SHALL NOT reference GITHUB_TOKEN, GitHub Models, or any token setup instructions.

#### Scenario: Upload page loads
- **WHEN** the user navigates to `/upload`
- **THEN** no warning banner about GITHUB_TOKEN is displayed regardless of environment variables
- **THEN** no text mentions "GitHub Models" or "set the environment variable"

#### Scenario: Sidebar renders
- **WHEN** any page with the sidebar layout is displayed
- **THEN** the sidebar footer shows only the version string (e.g., "v1.0.0") with no API/model references
