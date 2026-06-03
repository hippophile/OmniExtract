## ADDED Requirements

### Requirement: Approach B classifies and extracts in a single API call
Approach B SHALL use one prompt that instructs GPT to both classify the document and extract its content in a single response. The response SHALL include a `classification` field and an `extraction` field containing the full `UniversalOutput`-compatible payload.

#### Scenario: One API call made for Approach B
- **WHEN** Approach B runs on any text document
- **THEN** exactly one API call is made
- **AND** the response contains both `classification` and `extraction` fields

#### Scenario: Extraction parsed from nested field
- **WHEN** Approach B response is parsed
- **THEN** the `extraction` field is deserialised as a `UniversalOutput`
- **AND** `LabApproachResult.Output` is populated with that result

#### Scenario: Approach B parse failure falls back gracefully
- **WHEN** Approach B returns a response that cannot be parsed as the expected schema
- **THEN** `LabApproachResult.Error` is set to a descriptive message
- **AND** the other two approaches are unaffected
