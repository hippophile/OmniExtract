## ADDED Requirements

### Requirement: Pitch deck file exists at docs/pitch-deck.md
The system SHALL contain a file at `docs/pitch-deck.md` that is a complete stakeholder presentation for Optima Bank Greece.

#### Scenario: File is present and non-empty
- **WHEN** a user navigates to `docs/pitch-deck.md` in the repository
- **THEN** the file exists and contains at least 7 slide sections

### Requirement: Slide structure — problem slide
The pitch deck SHALL include a slide titled "The Problem" covering manual document processing pain points at banks.

#### Scenario: Problem slide contains key pain points
- **WHEN** the pitch deck is rendered
- **THEN** the problem slide contains bullet points addressing manual processing volume, error rates, and processing time at banks

### Requirement: Slide structure — solution slide
The pitch deck SHALL include a slide titled "The Solution" introducing OmniExtract as a universal document extraction platform.

#### Scenario: Solution slide covers core capabilities
- **WHEN** the pitch deck is rendered
- **THEN** the solution slide lists supported document formats, AI-powered extraction, and structured JSON output

### Requirement: Slide structure — live demo flow slide
The pitch deck SHALL include a slide describing the live demo flow step by step.

#### Scenario: Demo flow slide is actionable
- **WHEN** the pitch deck is rendered
- **THEN** the demo flow slide lists numbered steps that a presenter can follow during a live demo

### Requirement: Slide structure — bank use cases slides
The pitch deck SHALL include coverage of exactly 5 bank use cases, each with document type, extracted fields, and business outcome.

#### Scenario: Five use cases are present
- **WHEN** the pitch deck is rendered
- **THEN** five distinct banking use cases are described, each grounded in a real Optima Bank document type

### Requirement: Slide structure — technical architecture slide
The pitch deck SHALL include a simplified technical architecture slide suitable for a non-technical audience.

#### Scenario: Architecture slide avoids jargon overload
- **WHEN** the pitch deck is rendered
- **THEN** the architecture slide uses plain language and describes the pipeline in 4 or fewer steps

### Requirement: Slide structure — POC results slide
The pitch deck SHALL include a POC results slide with honest, qualitative or quantitative accuracy findings.

#### Scenario: Results slide contains no fabricated metrics
- **WHEN** the pitch deck is rendered
- **THEN** all claims in the results slide are grounded in observed POC behaviour, with no inflated numbers

### Requirement: Slide structure — next steps slide
The pitch deck SHALL include a next steps slide with actionable recommendations for Optima Bank stakeholders.

#### Scenario: Next steps slide contains at least 3 actions
- **WHEN** the pitch deck is rendered
- **THEN** the next steps slide lists at least 3 concrete, time-bound actions

### Requirement: Speaker notes on every slide
Every slide in the pitch deck SHALL have a speaker notes section formatted as a blockquote.

#### Scenario: Speaker notes present on all slides
- **WHEN** the pitch deck is rendered
- **THEN** every slide section contains a blockquote starting with "Speaker notes:"
