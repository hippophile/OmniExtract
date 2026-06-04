## Context

Currently `ExtractAsync` in `ExtractionService.cs` makes a single binary decision: if `extracted.Images.Count > 0` → vision, else → text. There is no concept of strategy — every text document goes through the same classify→extract path regardless of whether it's a `.xlsx` spreadsheet (which needs no classification) or a 20-page legal contract (which needs domain-aware extraction).

The `ClassifyDocument()` heuristic exists but is only used by lab approach C. The main pipeline ignores it entirely and always fires API calls. The result: known tabular formats waste tokens on classification, and text-rich PDFs get misrouted when the heuristic is used standalone.

The lab (A/B/C) has validated today that:
- Heuristic is reliable for known extensions (`.xlsx`, `.csv`) — 0 API calls, correct
- B (domain-aware, 1 call) is best for text-rich documents
- Confidence-gated fallback is needed when heuristic yields < 3 meaningful fields

## Goals / Non-Goals

**Goals:**
- Single `RouteDocument()` function that selects strategy before any AI call
- Strategy is explicit, logged, and stored in `OutputMeta`
- Heuristic path for known tabular formats (fast, 0 classify calls)
- Vision path for images and scanned PDFs (existing, unchanged)
- TextRich path (B's AdaptivePrompt) for prose/narrative documents
- Confidence-gated fallback: heuristic → TextRich if field count < 3
- Lab shows strategy badge per approach

**Non-Goals:**
- Mixed strategy (text + tables in same document) — future work
- Changing the vision path — already works well
- Per-page routing for multi-section PDFs — future work

## Decisions

### 1. Strategy enum lives in Core, router lives in ExtractionService
Putting `ExtractionStrategy` in `OmniExtract.Core` makes it available to the web layer (for display) without circular dependencies. The routing logic stays in `ExtractionService` where all extraction knowledge lives.

*Alternative considered:* New `RouterService` class — rejected, over-engineering for what is essentially a switch statement plus one fallback check.

### 2. Router uses three signals in priority order
```
1. Images present?           → Vision
2. Known tabular extension?  → Heuristic
3. Everything else           → TextRich
```
Priority order matters: a `.xlsx` that somehow has images still goes Vision. A `.pdf` with no extractable text goes Vision. A `.pdf` with text goes TextRich.

*Known tabular extensions:* `.xlsx`, `.xls`, `.csv`, `.tsv` — same as `ForceStructuredExts` already in `ClassifyDocument`.

### 3. Fallback threshold: < 3 meaningful fields
"Meaningful" = `Data.Count` excluding `current_datetime` and other injected metadata keys. If heuristic extraction returns fewer than 3 real fields, re-run as TextRich.

*Alternative considered:* Use confidence score — rejected, self-reported confidence is unreliable (proven today: 98% confidence on wrong output).

### 4. Strategy stored in OutputMeta as a string
Adding `Strategy` as `string` (not enum) to `OutputMeta` avoids serialization complexity in the JSON flat-file storage. Values: `"heuristic"`, `"vision"`, `"text-rich"`, `"heuristic→text-rich"` (for escalated fallback).

## Risks / Trade-offs

- **Fallback doubles latency for misrouted docs** → Acceptable: only triggers when heuristic fails, which is the current broken state anyway. Net improvement.
- **TextRich path costs more tokens than heuristic** → By design. The router only escalates when heuristic produced nothing useful.
- **Threshold of 3 fields is arbitrary** → Start at 3, adjust based on real document testing. Configurable via `ProcessingSettings` if needed.

## Migration Plan

1. Add `ExtractionStrategy` enum and `Strategy` property to `OutputMeta` — no breaking change, new optional field
2. Add `RouteDocument()` to `ExtractionService` — internal, no API surface change
3. Swap the binary `if images → else text` in `ExtractAsync` for router call — transparent to callers
4. Add fallback logic after heuristic extraction
5. Update lab UI to show strategy badge — cosmetic only

No rollback needed — changes are additive. Existing behavior preserved as the `TextRich` path.
