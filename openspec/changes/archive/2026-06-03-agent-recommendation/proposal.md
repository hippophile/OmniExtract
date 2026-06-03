## Why

OmniExtract extracts data but stops there — the user must decide what to do with it. For a document intelligence pipeline with specialist agents (financial analysis, legal review, fraud detection), the extracted result already contains enough signal to make a smart routing recommendation. Adding an AI-generated "next agent" recommendation turns OmniExtract into the intake layer of a broader pipeline, not just a standalone extractor.

## What Changes

- **Recommendation pass**: After extraction completes, a lightweight AI call reads the extracted `data`, `meta`, `tags`, and `categories` and returns a structured recommendation: which agent to use next, domain classification, and 2–3 sentence reasoning citing specific evidence from the document.
- **Domains covered**: `financial`, `legal`, `fraud`, `sensitive/compliance`, `business-general`. OmniExtract auto-detects — user does not select.
- **Result detail page**: New "Agent Recommendation" panel displays agent name, domain, and reasoning. Read-only — no action buttons in this POC.
- **Storage**: Recommendation stored in the result's `data` under well-known key `"agent_recommendation"` so it persists across sessions.

## Capabilities

### New Capabilities
- `agent-recommendation`: Post-extraction AI pass that classifies the document domain and recommends the appropriate specialist agent with evidence-based reasoning.

### Modified Capabilities
*(none)*

## Impact

- `OmniExtract.App/Services/ExtractionService.cs` — add `RecommendationPassAsync` method and `RecommendationPrompt` constant
- `OmniExtract.Web/Services/ExtractionOrchestrator.cs` — call recommendation pass after extraction completes
- `OmniExtract.Web/Components/Pages/Results.razor` — render recommendation panel on result detail page
- `OmniExtract.Web/wwwroot/app.css` — recommendation panel styles
