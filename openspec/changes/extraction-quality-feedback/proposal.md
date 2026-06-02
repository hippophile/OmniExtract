## Why

Extraction results have no quality signal — the user cannot indicate whether the AI got it right or wrong. Without feedback data, there is no way to measure extraction accuracy across runs or file types.

## What Changes

- Add a `Rating` field (`null` | `Good` | `Bad`) to `ResultsEntry` in the repository
- Render thumbs-up / thumbs-down buttons on the result detail page (`ResultDetail.razor`)
- Persist the rating back into the in-memory store via `ResultsRepository.Rate(id, rating)`
- Show aggregate accuracy stats (good count, bad count, rated %) on the dashboard (`Results.razor` or `Home.razor`)

## Capabilities

### New Capabilities
- `result-rating`: Allows the user to rate an extraction result as Good or Bad from the detail page; rating is stored on the `ResultsEntry` and persisted in memory for the session.
- `accuracy-stats`: Aggregates Good/Bad counts across all rated results and surfaces them on the dashboard.

### Modified Capabilities

## Impact

- `OmniExtract.Web/Services/ResultsRepository.cs` — add `Rating` property to `ResultsEntry`, add `Rate()` method
- `OmniExtract.Web/Components/Pages/ResultDetail.razor` — add thumbs-up/thumbs-down UI with Blazor event handlers
- `OmniExtract.Web/Components/Pages/Results.razor` or `Home.razor` — add accuracy stats summary widget
- No new dependencies; no breaking API changes
