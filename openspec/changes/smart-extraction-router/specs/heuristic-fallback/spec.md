## ADDED Requirements

### Requirement: Heuristic extraction triggers fallback on low field count
The system SHALL automatically escalate from `Heuristic` to `TextRich` extraction when the heuristic result contains fewer than 3 meaningful data fields.

#### Scenario: Low-yield heuristic triggers TextRich retry
- **WHEN** `Heuristic` extraction completes AND `Data` contains fewer than 3 fields excluding injected metadata keys (`current_datetime` etc.)
- **THEN** the system re-runs extraction using the `TextRich` (AdaptivePrompt) path
- **THEN** `OutputMeta.Strategy` is set to `"heuristic→text-rich"` to indicate escalation

#### Scenario: Sufficient heuristic yield skips fallback
- **WHEN** `Heuristic` extraction completes AND `Data` contains 3 or more meaningful fields
- **THEN** no fallback is triggered
- **THEN** `OutputMeta.Strategy` remains `"heuristic"`

#### Scenario: Fallback result is used over heuristic result
- **WHEN** fallback is triggered and TextRich extraction succeeds
- **THEN** the TextRich result is returned, not the low-yield heuristic result

### Requirement: Meaningful field count excludes injected metadata
The system SHALL exclude known injected keys (`current_datetime`, `raw_response`, `raw_error`) when counting fields for the fallback threshold.

#### Scenario: Document with only current_datetime triggers fallback
- **WHEN** heuristic extraction returns only `{"current_datetime": "..."}` in `Data`
- **THEN** meaningful field count is 0, fallback is triggered

### Requirement: Fallback threshold is configurable
The fallback threshold SHALL be configurable via `ProcessingSettings.HeuristicFallbackThreshold` with a default of 3.

#### Scenario: Default threshold applies when not configured
- **WHEN** `HeuristicFallbackThreshold` is not set in appsettings.json
- **THEN** the system uses 3 as the threshold
