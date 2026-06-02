## Why

The web UI was built as an analytics dashboard but OmniExtract is a POC test cockpit — the gap between what it looks like and what it does creates confusion. Fake KPI numbers, GitHub token warnings, and verbose pages on first load obscure the actual loop: drop document → extract → inspect raw result.

## What Changes

- **Remove** KPI stat cards with hardcoded/inflated numbers from the dashboard
- **Remove** bar charts (document type and domain breakdowns) from the dashboard
- **Add** minimal dashboard: prominent extract CTA + real recent extractions list (≤5 items)
- **Remove** GITHUB_TOKEN warning banner from the upload page (backend uses Copilot SDK, no token needed)
- **Remove** "GPT-4.1 via GitHub Models" text from sidebar footer
- **Remove** `ApiKeyEnvVar` from `appsettings.json`
- **Add** collapsible format chips on upload page (hidden by default behind "Supported formats" toggle)
- **Add** collapsible JSON/structured output sections on result detail page (collapsed by default, summary line shown)
- **Add** `~/Desktop/OmniExtract_TestDocs/` folder with 7 realistic test documents covering diverse extraction scenarios

## Capabilities

### New Capabilities
- `minimal-dashboard`: Replaces analytics dashboard with a POC-focused entry point — extract CTA + recent extractions list
- `collapsible-ui`: Collapsible sections on upload page (format chips) and result detail page (structured output blocks)
- `test-documents`: Curated set of test files on the desktop for manual extraction testing

### Modified Capabilities
- none

## Impact

- `OmniExtract.Web/Components/Pages/Home.razor` — full redesign
- `OmniExtract.Web/Components/Pages/Upload.razor` — remove token warning, collapse format chips
- `OmniExtract.Web/Components/Pages/ResultDetail.razor` — collapsible output sections
- `OmniExtract.Web/Components/Layout/MainLayout.razor` — sidebar footer text
- `OmniExtract.Web/appsettings.json` — remove `ApiKeyEnvVar`
- `~/Desktop/OmniExtract_TestDocs/` — new folder, no codebase impact
