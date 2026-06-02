# OmniExtract

See [GOAL.md](./GOAL.md) for the project's purpose and north star before making any decisions.

## Stack

- Blazor Server (.NET) web UI — `OmniExtract.Web/`
- Core models and config — `OmniExtract.Core/`
- Document processing + AI client — `OmniExtract.App/Services/`
- AI: GitHub Copilot SDK (`GitHub.Copilot.SDK`) — `GptClient.cs` uses `CopilotClient`, no GITHUB_TOKEN or GitHub Models endpoint
- Storage: JSON flat files via `ResultsRepository`

## Key Conventions

- No fake/inflated numbers in the UI — only real data from `ResultsRepository`
- The web app is a POC test cockpit, not a product dashboard
- `appsettings.json` `OpenAI` section is legacy naming; the actual client is Copilot SDK
