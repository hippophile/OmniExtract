## Context

OmniExtract is a POC document extraction platform built on the GitHub Copilot SDK. The pitch deck is a single Markdown file targeting Optima Bank Greece stakeholders — non-technical decision-makers and IT leads — who need to understand the business value, see a demo flow, and evaluate next steps for adoption.

The file lives at `docs/pitch-deck.md`. No tooling, no build step, no dependencies — just a well-structured Markdown document that can be rendered in any Markdown viewer or converted to slides with tools like Marp, Slidev, or Pandoc.

## Goals / Non-Goals

**Goals:**
- Single `docs/pitch-deck.md` file, slide-by-slide structure
- Each slide: `## Slide N — Title`, bullet points, `> Speaker notes:` block
- Cover all 7 required topic areas: problem, solution, live demo flow, 5 bank use cases, architecture, POC results, next steps
- Tone: concise, business-first, no jargon overload
- Self-contained — readable without the running app

**Non-Goals:**
- No actual slide renderer or HTML output
- No changes to application code
- No Marp/Slidev frontmatter (plain Markdown only, conversion optional)
- Not a full business case or RFP response

## Decisions

**Decision: Plain Markdown, no slide framework**
Rationale: Stakeholders may view this in GitHub, VS Code, or a simple viewer. Keeping it pure Markdown maximises portability. Conversion to PowerPoint or PDF is a later, optional step.

**Decision: Speaker notes as blockquotes**
Format: each slide ends with a `> **Speaker notes:** ...` blockquote. This is visually distinct from bullets, renders cleanly in Markdown, and is easy to strip when exporting.

**Decision: 5 bank use cases are the core value section**
The use cases slide (or slides) should be the most detailed — this is the "aha" moment for a bank stakeholder. Each use case gets its own named sub-section with document type, extracted fields, and business outcome.

**Decision: POC results use honest data**
Per CLAUDE.md: no fake/inflated numbers. Results section references real extraction behaviour observed during testing, framed qualitatively if quantitative metrics aren't available.

## Risks / Trade-offs

- [Risk: Markdown rendering varies] → Keep formatting simple; avoid HTML tags or complex tables
- [Risk: Speaker notes may feel sparse] → Prioritise quality over length; 2-3 sentences per slide is enough
- [Risk: Use cases may be too generic] → Ground each use case in a specific Optima Bank document type (e.g., loan application, KYC packet)

## Open Questions

*(none — scope is fully defined)*
