## ADDED Requirements

### Requirement: Every successfully extracted document receives an agent recommendation
After extraction completes and before the result is saved, the system SHALL perform one additional AI call that reads the extracted output and returns a recommendation for which specialist agent should handle the document next. The recommendation SHALL be stored in `data["agent_recommendation"]` on the result.

#### Scenario: Financial document gets financial agent recommendation
- **WHEN** a document is extracted with `categories.domain = "finance"` and data keys suggest revenue, costs, or investment
- **THEN** `data["agent_recommendation"].recommended_agent` is `"FinancialAgent"`
- **AND** `reasoning` cites at least one specific signal from the document (e.g. field names, tags, or sensitivity)

#### Scenario: Document with fraud signals gets fraud agent recommendation
- **WHEN** a document contains signals such as inconsistent amounts, duplicate entries, or anomaly warnings
- **THEN** `data["agent_recommendation"].recommended_agent` is `"FraudAgent"`
- **AND** `domain` is `"fraud"`

#### Scenario: Legal document gets legal agent recommendation
- **WHEN** a document is classified as a contract, agreement, or legal brief
- **THEN** `data["agent_recommendation"].recommended_agent` is `"LegalAgent"`

#### Scenario: Sensitive/compliance document gets compliance recommendation
- **WHEN** `meta.sensitivity` is `"restricted"` or `"confidential"` and domain is not financial or legal
- **THEN** `data["agent_recommendation"].recommended_agent` is `"ComplianceAgent"`

#### Scenario: General business document falls back to business agent
- **WHEN** no strong domain signal is detected
- **THEN** `data["agent_recommendation"].recommended_agent` is `"BusinessAgent"`

### Requirement: Recommendation includes evidence-based reasoning
The recommendation SHALL include a `reasoning` field of 2–3 sentences that cites specific evidence from the extracted document — not generic descriptions of the agent.

#### Scenario: Reasoning references document content
- **WHEN** a recommendation is generated for a financial report
- **THEN** `reasoning` references specific signals such as document type, flagged fields, sensitivity level, or extracted tags
- **AND** `reasoning` does NOT contain generic text like "this agent handles financial documents"

### Requirement: Recommendation failure does not fail the job
If the recommendation AI call fails or returns unparseable JSON, the system SHALL log a warning and save the result without a recommendation. The job status SHALL remain `Done`.

#### Scenario: Recommendation API call fails
- **WHEN** the recommendation pass throws an exception
- **THEN** the result is saved to `ResultsRepository` without `data["agent_recommendation"]`
- **AND** `meta.warnings` contains `"Agent recommendation pass failed — result saved without recommendation."`
- **AND** job status is `Done`

### Requirement: Recommendation is displayed on the result detail page
The result detail page SHALL display a dedicated "Agent Recommendation" panel when `data["agent_recommendation"]` is present. The panel SHALL show the recommended agent name, domain, confidence, reasoning, and signals. It SHALL be read-only with no action buttons.

#### Scenario: Recommendation panel visible on result page
- **WHEN** a user views a result that has an agent recommendation
- **THEN** an "Agent Recommendation" panel is visible on the result detail page
- **AND** it shows: agent name, domain badge, confidence score, reasoning text, and signal tags

#### Scenario: No recommendation panel for failed jobs
- **WHEN** a user views a failed job result
- **THEN** no "Agent Recommendation" panel is shown
