## ADDED Requirements

### Requirement: Finished jobs can be deleted from queue view
The system SHALL allow a user to delete any Done, Failed, or Cancelled job from the queue view. Deletion SHALL only remove the job from the in-memory list; the corresponding result in ResultsRepository SHALL remain intact.

#### Scenario: Delete completed job
- **WHEN** user clicks "Delete" on a job with status Done, Failed, or Cancelled
- **THEN** the job card is removed from the queue view immediately
- **AND** the result is still accessible on the Results page

#### Scenario: Delete failed job
- **WHEN** user clicks "Delete" on a job with status Failed
- **THEN** the job card is removed from the queue view
- **AND** the failed result entry in ResultsRepository is preserved
