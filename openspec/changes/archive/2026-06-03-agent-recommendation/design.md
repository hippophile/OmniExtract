## Context

After `ExtractionService.ExtractAsync` returns a `UniversalOutput`, the orchestrator saves it to `ResultsRepository`. The recommendation pass slots in between those two steps — it receives the completed output, makes one API call, and enriches `data["agent_recommendation"]` before the result is saved.

The recommendation must be grounded in the actual extracted content, not just the document type. A financial report with fraud signals should recommend `FraudAgent`, not `FinancialAgent`. This requires the prompt to read the full extracted payload.

## Goals / Non-Goals

**Goals:**
- One additional API call per document, post-extraction, that produces a domain classification and agent recommendation with cited reasoning.
- Recommendation persists in the stored result.
- Rendered as a read-only panel on the result detail page.

**Non-Goals:**
- Actually triggering or calling any downstream agent.
- User-selectable agent routing.
- Multi-agent recommendations (one agent only, best fit).
- Recommendation for failed jobs (no extraction data to reason from).

## Decisions

### 1. Input to the recommendation prompt

Send a compact summary of the extraction result, not the full payload:
```
document_type, domain, sensitivity, tags, confidence,
top 20 data keys (keys only, not values — to avoid PII in prompt),
warnings, first 500 chars of data values (truncated)
```

This keeps the prompt small (~1–2K tokens), avoids sending sensitive values to the API unnecessarily, and still gives enough signal for domain classification.

**Alternative considered**: Send full `data` JSON. Rejected — large docs produce large data payloads; values may contain PII; keys alone are sufficient for routing.

### 2. Recommendation prompt output schema

```json
{
  "recommended_agent": "FinancialAgent | LegalAgent | FraudAgent | ComplianceAgent | BusinessAgent",
  "domain": "financial | legal | fraud | sensitive | business-general",
  "confidence": 0.0–1.0,
  "reasoning": "2–3 sentences citing specific evidence from the document",
  "signals": ["signal1", "signal2"]
}
```

Stored as `data["agent_recommendation"]` on the `UniversalOutput`.

### 3. Where the pass runs — orchestrator, not extraction service

The recommendation is a post-processing concern, not part of the core extraction pipeline. It runs in `ExtractionOrchestrator` after `_extractionService.ExtractAsync` returns, before `_resultsRepository.Add`. This keeps `ExtractionService` focused on extraction.

**Alternative considered**: Run inside `ExtractionService.ExtractAsync`. Rejected — recommendation is pipeline-level routing logic, not document extraction logic.

### 4. Failure handling

If the recommendation pass throws or returns unparseable JSON, log a warning and proceed without it — the result is still saved normally. Recommendation is best-effort enrichment.

### 5. Temperature

Use temperature=0. Recommendations should be deterministic and consistent — two runs of the same document should always recommend the same agent.

## Risks / Trade-offs

- **Extra API call per document**: Adds ~4–10s latency per job. Acceptable for POC — recommendation is genuinely useful and this is an intake layer, not a real-time pipeline.
- **Keys-only input misses context**: A document with field `"anomaly"` signals fraud, but field `"total"` could be anything. Mitigated by also including `tags`, `domain`, `sensitivity`, and `document_type` which carry strong signals.
- **Single agent recommendation**: Some documents are genuinely cross-domain (a legal contract with financial terms). For POC, best-fit single recommendation is sufficient.
