## Context

Both features are pure UI reads — no new AI calls, no backend changes. The verdict JSON is already stored in `result.Data["verdict"]` (a `Dictionary<string, object?>`). Extraction confidence is already in `result.Meta.Confidence` (0.0–1.0). The results list page (`Results.razor`) iterates `ResultsEntry` objects from `ResultsRepository`. The detail page (`ResultDetail.razor`) already renders the verdict card. Both pages use the shared `ToJsonString()` helper and `ParseJsonObjectFlat()` for reading verdict data.

## Goals / Non-Goals

**Goals:**
- Surface the one-line verdict summary on each results list card — no click required
- Show a visible low-confidence caveat on the verdict card when `Meta.Confidence < 0.75`
- Graceful degradation — old results without verdicts render exactly as before

**Non-Goals:**
- Changing how the verdict is generated or stored
- Showing full action items on the list page (summary only)
- Configurable confidence threshold (hardcoded at 0.75 for now)

## Decisions

**1. Summary on results list: read `Data["verdict"]` per entry**
Each `ResultsEntry` in the list already exposes `Output.Data`. Reading `Data["verdict"]` and extracting `summary` is identical to what `ResultDetail.razor` already does. Alternative: add a dedicated `VerdictSummary` property to `ResultsEntry`. Rejected — over-engineering for a POC; the dict is sufficient and consistent.

**2. Confidence threshold: 0.75 hardcoded**
Below 75% the extraction is materially incomplete. Above it, the verdict is trustworthy enough to present without qualification. Alternative: make it configurable in `appsettings.json`. Deferred — no evidence of need yet.

**3. Caveat badge placement: inside the verdict card header row**
The caveat should sit next to the "VERDICT BRIEF" kicker — same line, right-aligned. This makes it visible without disrupting the summary text. Alternative: below the summary. Rejected — it belongs at the top where it sets expectations before the user reads the summary.

**4. List card summary: truncate at ~120 chars**
The results list is dense. The summary must not reflow the card layout. Truncate with ellipsis at 120 characters. Alternative: full summary. Rejected — would make list cards variable-height and visually noisy.

## Risks / Trade-offs

- **Old results without verdict** — `Data["verdict"]` will be absent; `TryGetValue` handles this cleanly, no rendering change
- **Summary too long for list** — mitigated by 120-char truncation
- **Confidence of 0.0 on vision results** — some documents extract at 0.0 confidence legitimately (pure image scan); caveat will show; acceptable since the warning is accurate

## Migration Plan

No migration needed. Both features are additive reads on existing stored data.
