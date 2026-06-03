## Context

OmniExtract is a working POC with four projects (Core, App, Web, plus CLI entry point) and no documentation. The codebase uses an unconventional AI backend (GitHub Copilot SDK instead of direct OpenAI), a multi-stage extraction pipeline (native parsers → LibreOffice fallback → vision fallback), and a Blazor Server web UI. None of this is documented anywhere. Target reader: a .NET developer who has just cloned the repo.

## Goals / Non-Goals

**Goals:**
- Root `README.md` that gets a developer running within 5 minutes
- `docs/architecture.md` that explains the pipeline and component graph clearly enough to debug or extend it
- `docs/output-schema.md` that fully documents the `UniversalOutput` JSON structure with examples
- `docs/extending.md` that explains how to add a new format parser or swap the AI backend

**Non-Goals:**
- API reference auto-generation (no XML doc comments in codebase)
- User-facing product documentation
- Deployment/infrastructure guides (this is a POC, not a deployed service)
- Changelog or release notes

## Decisions

### Decision: Four discrete files over a single monolithic README

A single README would become unwieldy (architecture + schema + extension guide). Splitting into `docs/` keeps the root README scannable while linking out to deeper references.

*Alternative considered*: Inline everything in README. Rejected — README would exceed 500 lines and mix audience concerns (quick-start vs. deep architecture).

### Decision: Mermaid diagrams for pipeline flow in architecture.md

The extraction pipeline has multiple conditional branches (native → LibreOffice fallback → vision fallback). A flowchart communicates this better than prose. GitHub renders Mermaid natively.

*Alternative considered*: ASCII art. Rejected — harder to maintain, less readable.

### Decision: Annotated JSON example in output-schema.md rather than JSON Schema (jsonschema.org format)

The `data` field is `Dictionary<string, object?>` — its keys are document-specific and cannot be formally schematized. An annotated example is more useful than a partial JSON Schema.

*Alternative considered*: Full JSON Schema. Rejected — `data` field is unconstrained; a partial schema would be misleading.

### Decision: docs/ subfolder, not separate wiki

Keeps documentation co-located with code and version-controlled alongside it.

## Risks / Trade-offs

- [Risk: docs drift as code evolves] → Mitigation: Architecture doc references specific file paths and class names so drift is obvious; no attempt to auto-sync.
- [Risk: Copilot SDK setup is non-obvious] → Mitigation: README prerequisite section explicitly states Copilot CLI requirement and links to setup.
- [Risk: LibreOffice dependency undocumented] → Mitigation: README and architecture doc both call out LibreOffice as optional with install command.
