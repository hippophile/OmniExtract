## ADDED Requirements

### Requirement: Approach A classifies before extracting using a dedicated API call
Approach A SHALL make a first API call with a minimal classification prompt that returns `structured` or `narrative` and a confidence score. A second API call then extracts using the appropriate prompt for that classification.

#### Scenario: Two API calls made for Approach A
- **WHEN** Approach A runs on any text document
- **THEN** exactly two API calls are made: one classification call and one extraction call
- **AND** the extraction prompt is selected based on the classification result

#### Scenario: Classification result included in output
- **WHEN** Approach A completes
- **THEN** `LabApproachResult.Classification` reflects the GPT-returned classification
- **AND** `LabApproachResult.Confidence` reflects the GPT-returned confidence score

### Requirement: Approach A elapsed time includes both API calls
The elapsed time reported for Approach A SHALL cover the total wall-clock time from start to finish, including both the classification call and the extraction call.

#### Scenario: Elapsed time reported correctly
- **WHEN** Approach A completes
- **THEN** `LabApproachResult.Elapsed` is the sum of both API call durations
