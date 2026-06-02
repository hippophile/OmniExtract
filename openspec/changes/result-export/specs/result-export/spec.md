## ADDED Requirements

### Requirement: Download JSON export
The result detail page SHALL provide a "Download JSON" button that triggers a client-side download of the full `UniversalOutput` for that result as a `.json` file. The file SHALL be serialised with indented formatting.

#### Scenario: User downloads JSON
- **WHEN** the user clicks "Download JSON" on the result detail page
- **THEN** the browser downloads a file named `<documentType>-<id>.json` containing the full `UniversalOutput` as indented JSON

#### Scenario: JSON file is well-formed
- **WHEN** the downloaded JSON file is opened
- **THEN** it SHALL be valid JSON containing `meta`, `tags`, `categories`, `data`, and `tables` fields matching the displayed result

### Requirement: Download CSV export
The result detail page SHALL provide a "Download CSV" button that triggers a client-side download of the extracted data as a `.csv` file. The CSV SHALL contain the flattened `Data` key-value fields and all `Tables` entries in a single file.

#### Scenario: User downloads CSV
- **WHEN** the user clicks "Download CSV" on the result detail page
- **THEN** the browser downloads a file named `<documentType>-<id>.csv`

#### Scenario: CSV contains data fields
- **WHEN** the result has `Data` fields
- **THEN** the CSV SHALL start with a header row of field keys and a second row of corresponding values

#### Scenario: CSV contains tables
- **WHEN** the result has one or more `Tables`
- **THEN** each table SHALL appear after the data fields section, preceded by a comment line `# Table N` and rendered with its own header row and data rows

#### Scenario: CSV values are RFC 4180 compliant
- **WHEN** a field value or cell contains a comma, double-quote, or newline
- **THEN** the value SHALL be wrapped in double-quotes and internal double-quotes SHALL be escaped as `""`

#### Scenario: Result has no data or tables
- **WHEN** both `Data` and `Tables` are empty
- **THEN** the downloaded CSV SHALL contain only a header comment indicating no data was extracted

### Requirement: Export buttons placement
Export buttons SHALL be visible in the result detail page header area (topbar-right) alongside the existing "All Results" navigation button, so they are reachable without scrolling.

#### Scenario: Buttons visible on load
- **WHEN** the result detail page loads with a valid result
- **THEN** both "Download JSON" and "Download CSV" buttons SHALL be visible in the topbar

#### Scenario: Buttons absent for missing result
- **WHEN** the result detail page loads with an invalid or missing result ID
- **THEN** no export buttons SHALL be rendered
