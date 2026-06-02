## ADDED Requirements

### Requirement: Document type filter chips
The Results page SHALL display a row of chip buttons for each distinct `Meta.DocumentType` value present in the result set. Activating a chip SHALL restrict the visible results to entries whose `Meta.DocumentType` matches the selected value. Only one document-type chip may be active at a time; clicking the active chip again SHALL deactivate it.

#### Scenario: Filter by document type
- **WHEN** the user clicks a document-type chip (e.g. "Invoice")
- **THEN** only results with `Meta.DocumentType == "Invoice"` are shown

#### Scenario: Deactivate document type filter
- **WHEN** the user clicks the currently active document-type chip
- **THEN** the document-type filter is cleared and all results (subject to other active filters) are shown

### Requirement: Sensitivity filter chips
The Results page SHALL display a chip for each distinct `Categories.Sensitivity` value present in the result set. Activating a sensitivity chip SHALL restrict visible results to entries matching that sensitivity level. Only one sensitivity chip may be active at a time.

#### Scenario: Filter by sensitivity
- **WHEN** the user clicks a sensitivity chip (e.g. "confidential")
- **THEN** only results whose `Categories.Sensitivity == "confidential"` are shown

#### Scenario: Toggle off sensitivity filter
- **WHEN** the active sensitivity chip is clicked again
- **THEN** the sensitivity filter is cleared

### Requirement: Date range presets
The Results page SHALL provide date-range preset controls: **Today**, **Last 7 days**, **Last 30 days**, and **All time** (default). Selecting a preset SHALL restrict visible results to entries whose `ProcessedAt` falls within the chosen range. Exactly one preset SHALL be active at all times; the default is **All time**.

#### Scenario: Apply "Today" preset
- **WHEN** the user selects "Today"
- **THEN** only results processed on the current UTC calendar date are shown

#### Scenario: Apply "Last 7 days" preset
- **WHEN** the user selects "Last 7 days"
- **THEN** only results processed within the past 7 days (inclusive) are shown

#### Scenario: Default shows all results
- **WHEN** no date preset has been explicitly selected (or "All time" is active)
- **THEN** results are not filtered by date

### Requirement: Combined filter evaluation
All active filters (text search, domain, document type, sensitivity, date range) SHALL be applied together as a logical AND. The topbar document count SHALL reflect the number of items that pass all active filters.

#### Scenario: Multiple filters active simultaneously
- **WHEN** domain chip "finance" and sensitivity chip "confidential" are both active
- **THEN** only results that are both domain=finance AND sensitivity=confidential are shown

#### Scenario: Count updates with filters
- **WHEN** any filter changes
- **THEN** the topbar subtitle count updates to reflect the filtered result count

### Requirement: Clear all filters
The Results page SHALL provide a "Clear filters" control that becomes visible whenever any filter (domain, document type, sensitivity, or date preset other than All time) is active. Activating it SHALL reset all filters to their defaults simultaneously.

#### Scenario: Clear all active filters
- **WHEN** the user activates "Clear filters" while one or more filters are active
- **THEN** all filter fields are reset (domain=null, docType=null, sensitivity=null, date=AllTime, search unchanged) and the full result set is shown

#### Scenario: Control hidden when no filters active
- **WHEN** no filters are active (domain=null, docType=null, sensitivity=null, date=AllTime)
- **THEN** the "Clear filters" control is not visible
