## ADDED Requirements

### Requirement: Output schema doc covers all top-level fields
The output schema doc SHALL document every field in `UniversalOutput`: `meta`, `tags`, `categories`, `data`, and `tables` — with type, description, and whether the field is always present or conditional.

#### Scenario: All fields documented
- **WHEN** a developer reads the output schema doc
- **THEN** they can find a description for every key that appears in an extracted JSON output file

### Requirement: Output schema doc documents the meta object fully
The output schema doc SHALL document all fields within `OutputMeta`: `source_file`, `document_type`, `language`, `confidence`, `extraction_method`, and `warnings` — including the allowed values for `extraction_method` (`text`, `vision`, `failed`).

#### Scenario: Meta fields are unambiguous
- **WHEN** a developer inspects the `meta` field of an output
- **THEN** they understand what each field means and what values are valid

### Requirement: Output schema doc explains the data field
The output schema doc SHALL explain that `data` is a free-form `Dictionary<string, object?>` whose keys are determined by the AI based on document content, and include example key-value pairs for common document types (invoice, contract, email).

#### Scenario: Dynamic nature of data field is clear
- **WHEN** a developer reads the data field documentation
- **THEN** they understand that keys are not fixed and vary by document type

### Requirement: Output schema doc includes a full annotated example
The output schema doc SHALL include at least one complete, realistic JSON output example with inline comments or an adjacent annotation table explaining each field.

#### Scenario: Example is realistic and complete
- **WHEN** a developer compares a real extraction output against the example
- **THEN** every field in the real output has a corresponding explanation in the example

### Requirement: Output schema doc documents the tables field
The output schema doc SHALL explain the three-dimensional array structure of `tables`: `tables[tableIndex][rowIndex][cellIndex]` with an example.

#### Scenario: Table structure is unambiguous
- **WHEN** a developer accesses `tables[0][0][1]`
- **THEN** they understand from the doc that this is the second cell of the first row of the first table
