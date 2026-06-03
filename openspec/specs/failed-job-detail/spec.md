## ADDED Requirements

### Requirement: Failed job captures full error detail
When a job fails during processing, the system SHALL capture the full exception detail including the stack trace (via `ex.ToString()`) and store it on the job as `ErrorDetail`, in addition to the existing short `ErrorMessage`.

#### Scenario: Exception captured with stack trace
- **WHEN** an exception is thrown during `ProcessJobAsync`
- **THEN** `job.ErrorDetail` SHALL contain the full `ex.ToString()` string (message + stack trace)
- **THEN** `job.ErrorMessage` SHALL contain `ex.Message` (short message, unchanged)

### Requirement: Failed job persisted to ResultsRepository
When a job fails, the system SHALL add a failed entry to `ResultsRepository` so that it persists beyond the upload session and appears in the Results list.

#### Scenario: Failed job appears in Results list
- **WHEN** a job fails processing
- **THEN** `ResultsRepository.AddFailed(fileName, errorMessage, errorDetail)` SHALL be called
- **THEN** the returned entry SHALL have `IsFailed = true`, `ErrorMessage` set, and `ErrorDetail` set
- **THEN** `ResultsRepository.GetAll()` SHALL include the failed entry

#### Scenario: Failed entry has safe default output
- **WHEN** a failed entry is retrieved from `ResultsRepository`
- **THEN** `entry.Output` SHALL be a non-null `UniversalOutput` instance with default (empty) field values

### Requirement: Upload queue shows full error detail for failed jobs
The upload queue UI SHALL display the full error detail for failed jobs in a collapsible panel.

#### Scenario: Failed card shows error message
- **WHEN** a job has `Status == JobStatus.Failed`
- **THEN** the job card SHALL display the short `ErrorMessage` inline (existing behaviour, unchanged)

#### Scenario: User can expand stack trace
- **WHEN** a failed job card is rendered and `ErrorDetail` is non-null
- **THEN** the card SHALL include an expand/collapse toggle
- **WHEN** user activates the toggle
- **THEN** the full `ErrorDetail` text SHALL be displayed in a monospace pre-formatted block

### Requirement: Results list shows failed entries with distinct badge
The Results grid SHALL render failed entries with a "Failed" visual indicator distinct from successful entries.

#### Scenario: Failed entry renders with error badge
- **WHEN** a `ResultsEntry` with `IsFailed = true` is in the Results list
- **THEN** it SHALL display a red "Failed" badge instead of domain/confidence information
- **THEN** it SHALL still display the `FileName`

#### Scenario: Failed entry is not filtered by domain
- **WHEN** the user applies a domain filter on the Results page
- **THEN** failed entries SHALL be excluded from domain-based filtering (they have no domain)

### Requirement: Results detail page handles failed entries
The Results detail page (`/results/{id}`) SHALL render a dedicated error view for failed entries.

#### Scenario: Detail page shows error information for failed entry
- **WHEN** the user navigates to `/results/{id}` for a failed entry
- **THEN** the page SHALL display the `ErrorMessage`
- **THEN** the page SHALL display the full `ErrorDetail` in a collapsible monospace block
- **THEN** the page SHALL NOT attempt to render extraction output fields

#### Scenario: Re-upload CTA navigates to upload page
- **WHEN** the user is viewing a failed entry's detail page
- **THEN** the page SHALL display a "Re-upload" button
- **WHEN** user clicks "Re-upload"
- **THEN** the user SHALL be navigated to `/upload`
