## ADDED Requirements

### Requirement: Verdict pass generates analyst brief from extracted fields
After spec-first extraction completes, the system SHALL run a verdict pass that takes the extracted `Data` fields (document type, domain, tags, field names and values) as input and returns a structured verdict containing a summary and action items. The verdict pass SHALL NOT re-read the original document text. The system SHALL skip the verdict pass and add a meta warning if fewer than 3 meaningful fields were extracted.

#### Scenario: Verdict generated successfully
- **WHEN** extraction produces 3 or more meaningful fields
- **THEN** the system runs the verdict pass using extracted fields as input
- **THEN** the verdict is stored in `Data["verdict"]` as a JSON object with `summary`, `action_items`, and `flags` keys

#### Scenario: Verdict skipped on sparse extraction
- **WHEN** extraction produces fewer than 3 meaningful fields
- **THEN** the system skips the verdict pass
- **THEN** a warning is added to `meta.warnings` indicating verdict was skipped

#### Scenario: Verdict pass fails or times out
- **WHEN** the verdict AI call throws an exception or exceeds 45 seconds
- **THEN** the result is saved without a verdict
- **THEN** a warning is added to `meta.warnings`
- **THEN** no error is surfaced to the user

### Requirement: Verdict action items have priority levels
The verdict pass SHALL produce action items with a `priority` field of `high`, `medium`, or `low`. The system SHALL only flag items explicitly present in the extracted data — it SHALL NOT infer or hallucinate action items.

#### Scenario: High priority action item
- **WHEN** the extracted fields contain a deadline within 30 days or a missing required signature
- **THEN** the action item is assigned priority `high`

#### Scenario: No action items present
- **WHEN** the document contains no deadlines, missing signatures, risks, or approval-required amounts
- **THEN** `action_items` is an empty array

### Requirement: Verdict card rendered prominently on result detail page
The result detail page SHALL render the verdict card as a full-width panel above the extracted fields section and below any warnings. The card SHALL always be visible (not collapsed). The card SHALL NOT render if no verdict is present in `Data`.

#### Scenario: Verdict card shown
- **WHEN** `Data["verdict"]` is present and contains a non-empty summary
- **THEN** the verdict card renders above extracted fields
- **THEN** the summary text is displayed prominently
- **THEN** each action item is displayed with a priority badge (high=red, medium=amber, low=green)

#### Scenario: Verdict card absent on old results
- **WHEN** a result does not contain `Data["verdict"]`
- **THEN** no verdict card renders and no empty placeholder is shown

### Requirement: Verdict generation is a visible pipeline step
The upload processing UI SHALL show "Generating verdict..." as a distinct pipeline step between "Agent recommendation..." and "Done".

#### Scenario: Verdict step shown during processing
- **WHEN** a document is being processed and reaches the verdict stage
- **THEN** the pipeline UI shows "Generating verdict..." as the active step
- **THEN** prior steps (Build Spec, Extract Fields, Recommend) are marked done
