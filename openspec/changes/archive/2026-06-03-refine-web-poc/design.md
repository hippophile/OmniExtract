## Context

OmniExtract is a Blazor Server app with three pages: Dashboard (`/`), Upload (`/upload`), and Results (`/results`, `/results/{id}`). The current dashboard was built with analytics-style KPI cards and charts that show hardcoded/inflated numbers, making it look like a product rather than a test cockpit. The backend already uses `GitHub.Copilot.SDK` — no `GITHUB_TOKEN` is needed anywhere in the UI or config.

## Goals / Non-Goals

**Goals:**
- Dashboard communicates "this is a test tool" — not a product with KPIs
- First-load verbosity reduced: no walls of format chips, no collapsed-by-default JSON walls
- All references to GITHUB_TOKEN and GitHub Models removed from UI and config
- Test documents exist on disk for manual testing sessions

**Non-Goals:**
- Changing any extraction logic, models, or backend services
- Adding persistence, user accounts, or sharing features
- Automated tests for the test documents

## Decisions

**Decision: Dashboard becomes a two-zone layout (CTA + recent list), not a stats page**
- Rationale: The only workflows are "extract something new" and "review what I just extracted". KPIs add noise without supporting either workflow.
- Alternative considered: Keep KPIs but make them real (remove fake inflation). Rejected — even real counts don't help the POC loop.

**Decision: Collapsible sections use pure Blazor state (`bool` toggles), no JS**
- Rationale: Blazor Server already owns the render cycle; CSS `display:none` toggled by `@onclick` is zero-dependency and works with the existing CSS system.
- Alternative considered: `<details>`/`<summary>` HTML elements. Viable but harder to style consistently with the existing design system.

**Decision: Format chips collapsed behind a toggle on Upload page**
- Rationale: The drop zone is the primary action; the format list is reference info. Showing 10 format chips by default clutters the first impression.
- Collapsed label: "Supported formats ▾" / "Hide ▴"

**Decision: Result detail output sections collapsed by default, showing a one-line summary**
- Summary line format: `{section name} · {field count} fields` or just the section name if field count isn't available.
- User can expand each section independently.

**Decision: Remove `ApiKeyEnvVar` from appsettings.json entirely**
- The `OpenAI` config section is legacy naming. The actual client (`GptClient.cs`) uses `CopilotClient` and reads no env vars. Removing `ApiKeyEnvVar` prevents confusion for anyone reading the config.

**Decision: Test documents are plain text / CSV / EML — no binary files**
- Rationale: Binary DOCX/PDF require external tools to generate programmatically. Plain text files are sufficient to test the extraction prompt pipeline end-to-end, which is the goal.

## Risks / Trade-offs

- [Dashboard loses "recent extractions" when result store is empty] → Show an empty state with a prompt to extract the first document
- [Collapsed sections may hide important output from first-time users] → Section headers are always visible with a clear expand affordance; nothing is hidden behind a tab or secondary page
- [Test documents with realistic-looking fake data could be mistaken for real PII] → Files will include a header comment marking them as test data

## Migration Plan

1. UI changes are non-breaking — no data model or API changes
2. Removing `ApiKeyEnvVar` from appsettings.json is safe — nothing reads it at runtime
3. Test documents folder is additive — no existing files affected
4. No rollback complexity — all changes are file edits
