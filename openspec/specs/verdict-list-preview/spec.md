## ADDED Requirements

### Requirement: Results list shows verdict summary inline
The results list page SHALL display the verdict summary beneath the document type on each result card, if a verdict is present. The summary SHALL be truncated at 120 characters with an ellipsis. The summary SHALL NOT render if `Data["verdict"]` is absent or if the summary field is empty.

#### Scenario: Result with verdict shown in list
- **WHEN** a result entry has `Data["verdict"]` with a non-empty `summary` field
- **THEN** the results list card displays the summary text beneath the document type in muted style
- **THEN** the summary is truncated to 120 characters with ellipsis if longer

#### Scenario: Result without verdict shown in list
- **WHEN** a result entry has no `Data["verdict"]` key
- **THEN** the results list card renders exactly as before with no empty placeholder

#### Scenario: Failed result shown in list
- **WHEN** a result entry has `IsFailed = true`
- **THEN** no verdict summary is shown regardless of data contents
