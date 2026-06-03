## ADDED Requirements

### Requirement: Use case document exists at docs/optima-bank-use-cases.md
The repository SHALL contain a markdown file at `docs/optima-bank-use-cases.md` documenting 5 concrete OmniExtract deployment use cases for Optima Bank Greece.

#### Scenario: Document is present in repo
- **WHEN** a user navigates to `docs/optima-bank-use-cases.md` in the repository
- **THEN** the file exists and contains 5 named use case sections

### Requirement: Each use case contains a Problem statement
Each of the 5 use cases SHALL include a clearly written Problem section describing the manual or inefficient process it replaces.

#### Scenario: Problem section present
- **WHEN** a reader opens any use case section
- **THEN** a Problem subsection is present with at least 2 sentences describing the challenge

### Requirement: Each use case describes how OmniExtract solves the problem
Each use case SHALL include a Solution section explaining which OmniExtract capabilities address the stated problem.

#### Scenario: Solution section present
- **WHEN** a reader opens any use case section
- **THEN** a Solution subsection describes the extraction pipeline and AI model role

### Requirement: Each use case lists example document types
Each use case SHALL enumerate at least 3 example document types that OmniExtract would process in that scenario.

#### Scenario: Document types listed
- **WHEN** a reader reviews a use case
- **THEN** a Document Types subsection lists specific document formats (e.g., passport, loan application PDF, property valuation report)

### Requirement: Each use case specifies expected output fields
Each use case SHALL include an Output Fields section listing the structured data fields that OmniExtract extracts and returns as JSON.

#### Scenario: Output fields enumerated
- **WHEN** a reader reviews a use case
- **THEN** an Output Fields subsection lists specific field names with brief descriptions (e.g., `full_name`, `property_value_eur`)

### Requirement: Each use case identifies core banking integration points
Each use case SHALL include an Integration Points section naming the downstream systems or APIs where extracted data would flow.

#### Scenario: Integration points named
- **WHEN** a reader reviews a use case
- **THEN** an Integration Points subsection names at least 2 target systems or APIs (e.g., KYC platform, loan origination system, ERP)

### Requirement: Document covers all 5 specified use cases
The document SHALL cover exactly these use cases: KYC onboarding, loan underwriting, collateral assessment, supplier contract intelligence, and regulatory document intake.

#### Scenario: All 5 use cases present
- **WHEN** a reader scans the document headings
- **THEN** all 5 use case titles appear as top-level sections
