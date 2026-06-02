## Why

OmniExtract has no documentation: a developer cloning the repo has no entry point, no setup guide, and no explanation of how the system works. The codebase is non-trivial (CLI + Blazor web UI + multi-format parsing + AI pipeline) and requires documentation to be usable by anyone other than the original author.

## What Changes

- Create `README.md` at repo root — project overview, quick-start, supported formats, usage examples
- Create `docs/architecture.md` — pipeline diagram, component descriptions, data flow
- Create `docs/output-schema.md` — full JSON output schema with field descriptions and examples
- Create `docs/extending.md` — guide for adding new format parsers and extending the extraction pipeline

## Capabilities

### New Capabilities

- `project-readme`: Root-level README covering what OmniExtract is, prerequisites, how to run CLI and Web UI locally, supported formats, and usage examples
- `architecture-docs`: Developer guide covering the extraction pipeline, component responsibilities, and data flow from file input to JSON output
- `output-schema-docs`: Reference documentation for the `UniversalOutput` JSON schema with field-level descriptions, type info, and annotated examples
- `extension-guide`: Step-by-step guide for extending OmniExtract — adding new format parsers, adding new post-processors, and wiring in new AI models

### Modified Capabilities

## Impact

- No code changes — documentation only
- New files: `README.md`, `docs/architecture.md`, `docs/output-schema.md`, `docs/extending.md`
- Affects developer onboarding experience only
