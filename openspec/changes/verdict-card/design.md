## Context

Extraction currently ends with two post-processing passes: spec-first extraction (the main pass) and an agent recommendation pass. Both happen sequentially in the orchestrator. The result detail page renders extracted fields, tables, and the agent recommendation in a sidebar. There is no human-readable summary — a business user must read raw field values to understand the document.

The verdict pass sits naturally as a third post-extraction enrichment step. It receives the already-extracted `Data` dict (flat fields) plus document type and tags — not the original document text — keeping the input small and the call fast.

## Goals / Non-Goals

**Goals:**
- Generate a 2–3 sentence analyst-style summary of the document
- Extract 2–5 flagged action items (deadlines, missing signatures, risks, amounts needing approval)
- Store verdict in `Data["verdict"]` as structured JSON
- Render a visually distinct verdict card at the top of the result detail page
- Add "Generating verdict..." as a visible pipeline step

**Non-Goals:**
- Re-reading the original document text (uses extracted fields only)
- Replacing the extracted fields section (verdict is additive)
- Generating verdicts for failed extractions
- Customisable verdict templates per domain (keep it universal)

## Decisions

**1. Input: extracted fields, not raw text**
The verdict pass receives a compact JSON summary of the extracted data (document type, domain, tags, field names + values). This keeps the prompt small (~500 tokens), fast (~8–12s), and cheap. Alternative: re-send the full document text. Rejected — doubles cost, adds latency, and the fields already contain everything the model needs.

**2. Storage: `Data["verdict"]` dict key**
Consistent with how `agent_recommendation` is stored. No schema changes needed. The result detail page already handles special rendering for known keys. Alternative: a dedicated `Verdict` property on `UniversalOutput`. Rejected — over-engineering for a POC; the dict is sufficient.

**3. Verdict structure: summary string + action items array**
```json
{
  "summary": "2–3 sentence analyst brief",
  "action_items": [
    { "item": "Sign before 15 Jun 2026 to save £3,880", "priority": "high" },
    { "item": "DPA v3.1 requires countersignature", "priority": "high" }
  ],
  "flags": ["contains deadline", "requires signature"]
}
```
`priority` is `high | medium | low` — drives visual treatment (red/amber/green badges).

**4. Fail-silent: verdict is best-effort**
Same pattern as agent recommendation. If verdict pass fails or times out (45s), result is saved without it — no user-facing error. A warning is added to `meta.warnings`.

**5. UI: full-width card above extracted fields, below warnings**
Visually distinct from the rest of the page — dark background panel, large summary text, action items as a list with priority badges. Not a collapsible section — it's the main output, always visible.

## Risks / Trade-offs

- **Hallucination risk** — model may invent action items not present in the extracted fields. Mitigation: prompt explicitly instructs "only flag items present in the extracted data — do not infer."
- **Added latency** — ~10–15s extra per document. Mitigation: run verdict pass concurrently with agent recommendation pass (both are independent post-extraction).
- **Verdict quality degrades on sparse extractions** — if spec-first only extracted 3 fields, the verdict will be thin. Mitigation: if `Data` has fewer than 3 meaningful fields, skip verdict pass and add a warning.

## Migration Plan

No migration needed — `Data["verdict"]` is a new key. Old results simply won't have it; the UI renders the card conditionally.
