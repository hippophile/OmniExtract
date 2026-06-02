## ADDED Requirements

### Requirement: Active job card expands into pipeline visualizer
When a job transitions to Processing status, its card on the upload page SHALL expand to show a full pipeline visualizer. Queued and Done/Failed jobs SHALL remain as compact list items.

#### Scenario: Job starts processing
- **WHEN** a job's `Status` changes to `JobStatus.Processing`
- **THEN** its card expands to show the pipeline visualizer layout
- **THEN** compact cards for all other jobs remain unchanged

#### Scenario: Job completes
- **WHEN** a job's `Status` changes to `JobStatus.Done` or `JobStatus.Failed`
- **THEN** its card collapses back to the compact layout
- **THEN** the compact card shows the final status (checkmark or error icon)

### Requirement: Pipeline shows 5 named steps with status indicators
The visualizer SHALL display exactly 5 pipeline steps in order: Parse, Extract Text, Chunk, AI Analysis, Done. Each step SHALL show one of three states: pending, active, or complete.

#### Scenario: Job is on "Extracting content..." stage
- **WHEN** `job.Stage` equals "Extracting content..."
- **THEN** Parse step shows as complete (green)
- **THEN** Extract Text step shows as active (animated pulse)
- **THEN** Chunk, AI Analysis, Done steps show as pending (grey)

#### Scenario: Job is on "Running AI analysis..." stage
- **WHEN** `job.Stage` equals "Running AI analysis..."
- **THEN** Parse, Extract Text, Chunk steps show as complete
- **THEN** AI Analysis step shows as active (animated pulse)
- **THEN** Done step shows as pending

#### Scenario: Job completes successfully
- **WHEN** `job.Stage` equals "Complete"
- **THEN** all 5 steps show as complete (green)

#### Scenario: Job fails
- **WHEN** `job.Status` equals `JobStatus.Failed`
- **THEN** completed steps show as complete, the failed step shows as error (red)

### Requirement: Elapsed time ticks up in real time
The visualizer SHALL display elapsed time for the current active step, updating at least every second.

#### Scenario: Active step timer
- **WHEN** a job is in Processing status
- **THEN** the elapsed time for the current stage is shown (e.g., "3s", "1m 12s")
- **THEN** the displayed time updates at least every second without user interaction

#### Scenario: Timer stops on completion
- **WHEN** a job transitions to Done or Failed
- **THEN** the elapsed time stops updating and shows the final duration

### Requirement: Live stats line shows token and chunk count
During and after the AI Analysis step, the visualizer SHALL show the token count and chunk count if available.

#### Scenario: Token count available
- **WHEN** `job.TokenCount > 0` and the stage is AI Analysis or later
- **THEN** the stats line shows "N tokens · M chunks → model-name"

#### Scenario: Vision mode (no token count)
- **WHEN** `job.TokenCount == 0`
- **THEN** the stats line shows "Vision mode → model-name" instead of token count

#### Scenario: Token count not yet computed
- **WHEN** the stage is Parse or Extract Text (before chunking)
- **THEN** no stats line is shown

### Requirement: Orchestrator emits granular stage updates
The `ExtractionOrchestrator` SHALL set `job.Stage` at 5 distinct points during processing, not the current 2.

#### Scenario: Full stage sequence for a text document
- **WHEN** a text document is processed end-to-end
- **THEN** `job.Stage` transitions through: "Parsing file..." → "Extracting content..." → "Chunking document..." → "Running AI analysis..." → "Complete"
- **THEN** `StateChanged` is fired after each transition

#### Scenario: Token and chunk count populated
- **WHEN** the orchestrator has called `DocumentProcessor` and computed chunk count
- **THEN** `job.TokenCount` is set to the token count of the extracted text
- **THEN** `job.ChunkCount` is set to the number of chunks the document was split into
