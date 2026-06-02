## Context

This change adds a static documentation artifact (`docs/optima-bank-use-cases.md`) to the OmniExtract repository. No code changes are involved. The document serves as a structured reference for demonstrating OmniExtract's value to Optima Bank Greece across 5 key banking workflows.

OmniExtract is a Blazor Server/.NET POC that uses GitHub Copilot SDK to extract structured data from documents (PDFs, images, Office files) via AI. The core pipeline: file upload → AI extraction → JSON result stored via `ResultsRepository`.

## Goals / Non-Goals

**Goals:**
- Produce `docs/optima-bank-use-cases.md` with 5 fully detailed use cases
- Each use case covers: problem, OmniExtract solution, document types, output fields, integration points
- Document is suitable for stakeholder presentation

**Non-Goals:**
- No new code, services, or API endpoints
- No changes to existing extraction pipeline
- Not a product spec or SLA document

## Decisions

**Single markdown file over multiple files**: All 5 use cases in one document for easy sharing and presentation. Stakeholders receive one URL/file.

**`docs/` directory**: Keeps documentation separate from source. Consistent with common repo conventions. No `docs/` folder exists yet — creating it as part of this change.

**Structured per-use-case format**: Uniform sections (Problem, Solution, Documents, Output Fields, Integration Points) enable quick scanning and comparison across use cases.

## Risks / Trade-offs

- [Content accuracy] Integration points reference generic core banking systems (Temenos, Flexcube, etc.) — actual Optima Bank systems unknown → Mitigation: use common Greek banking system references; mark as illustrative examples
- [Maintenance] Document may drift from actual OmniExtract capabilities → Mitigation: clearly scope as a POC/proof-of-concept reference
