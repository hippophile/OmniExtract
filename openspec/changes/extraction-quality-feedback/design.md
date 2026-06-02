## Context

OmniExtract is a POC test cockpit for AI-driven document extraction. Results live in `ResultsRepository` as in-memory `ResultsEntry` objects. There is no persistence layer beyond the session. The web UI uses Blazor Server with component pages for the dashboard and result detail views.

Currently there is no mechanism to record whether an extraction was accurate. Adding lightweight thumbs-up/thumbs-down feedback addresses this.

## Goals / Non-Goals

**Goals:**
- Add a `Rating` enum (`Unrated`, `Good`, `Bad`) and property to `ResultsEntry`
- Expose a `Rate(string id, Rating rating)` method on `ResultsRepository`
- Render rating buttons on `ResultDetail.razor` with visual state (active/inactive)
- Display aggregate Good/Bad/Unrated counts on the results dashboard

**Non-Goals:**
- Persisting ratings to disk or a database (in-memory only, POC scope)
- Per-field granularity (rating is per-result, not per extracted field)
- User accounts or multi-user rating attribution
- Undo / rating history

## Decisions

**Decision: Rating as enum on `ResultsEntry`, not a separate store**
Simple and consistent with the existing flat in-memory model. A separate store would add indirection with no benefit at POC scale.

**Decision: `Unrated` as default (not `null`)**
Avoids nullable handling throughout the UI and makes intent explicit. `null` is ambiguous; `Unrated` is clear.

**Decision: `ResultsRepository.Rate()` mutates in place**
Single source of truth. Blazor Server components can call it directly and re-render. No need for events or observers at this scale.

**Decision: Stats on `Results.razor` dashboard, not `Home.razor`**
`Results.razor` already owns the result list; co-locating stats there avoids fetching data twice and keeps the home page as the upload entry point.

## Risks / Trade-offs

- [Risk] In-memory only — ratings lost on app restart → Mitigation: Acceptable for POC; noted in UI as session-scoped
- [Risk] Concurrent Blazor circuits could race on `Rate()` → Mitigation: `ResultsRepository` is a singleton; add a `lock` around mutation if needed (low risk for single-user POC)

## Migration Plan

No migration needed — purely additive change. Existing `ResultsEntry` objects default to `Unrated`. No breaking changes to existing pages or services.

## Open Questions

None — scope is clear and self-contained.
