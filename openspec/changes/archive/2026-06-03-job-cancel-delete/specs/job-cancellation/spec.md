## ADDED Requirements

### Requirement: Queued job can be removed before processing
The system SHALL allow a user to remove a Queued job before it begins processing. Removing a queued job SHALL cancel its CancellationTokenSource and remove it from the job list immediately.

#### Scenario: Remove queued job
- **WHEN** user clicks "Remove" on a job with status Queued
- **THEN** the job disappears from the queue and never starts processing

### Requirement: Processing job can be stopped
The system SHALL allow a user to stop a Processing job. Stopping SHALL cancel the job's CancellationTokenSource, causing in-progress extraction steps to abort at the next cancellation checkpoint. The job SHALL be marked with status Cancelled.

#### Scenario: Stop in-progress job
- **WHEN** user clicks "Stop" on a job with status Processing
- **THEN** the job transitions to Cancelled status and the semaphore is released

#### Scenario: Cancellation during AI call
- **WHEN** the job's token is cancelled while a GptClient call is in flight
- **THEN** the call is abandoned at the next await checkpoint and the job is marked Cancelled

### Requirement: Cancelled status is distinct from Failed
The system SHALL use a Cancelled JobStatus value (not Failed) for jobs stopped by the user, so users can distinguish intentional cancellations from errors.

#### Scenario: Cancelled job visual state
- **WHEN** a job is cancelled by the user
- **THEN** the job card shows "Cancelled" status distinct from the error/failed state
