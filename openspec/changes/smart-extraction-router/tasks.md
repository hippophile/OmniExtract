## 1. Core Models

- [x] 1.1 Add `ExtractionStrategy` enum to `OmniExtract.Core/Models/` with values: `Heuristic`, `Vision`, `TextRich`, `Mixed`
- [x] 1.2 Add `Strategy` string property to `OutputMeta` with `[JsonPropertyName("strategy")]`
- [x] 1.3 Add `HeuristicFallbackThreshold` int property to `ProcessingSettings` with default value 3

## 2. Router

- [x] 2.1 Add `RouteDocument(ExtractionResult extracted, string ext)` method to `ExtractionService` returning `ExtractionStrategy`
- [x] 2.2 Implement Vision branch: `extracted.Images.Count > 0` → `Vision`
- [x] 2.3 Implement Heuristic branch: extension in `ForceStructuredExts` → `Heuristic`
- [x] 2.4 Implement TextRich branch: everything else → `TextRich`

## 3. ExtractAsync Wiring

- [x] 3.1 Replace binary `if images → vision else text` in `ExtractAsync` with `RouteDocument()` call
- [x] 3.2 Add `Vision` dispatch: call existing `ExtractVisionAsync`, set `meta.Strategy = "vision"`
- [x] 3.3 Add `Heuristic` dispatch: call `ExtractTextAsync` with heuristic classification, set `meta.Strategy = "heuristic"`
- [x] 3.4 Add `TextRich` dispatch: call `ExtractTextAsync` with `AdaptivePrompt` (B path), set `meta.Strategy = "text-rich"`

## 4. Heuristic Fallback

- [x] 4.1 After `Heuristic` extraction, count meaningful fields (exclude `current_datetime`, `raw_response`, `raw_error`)
- [x] 4.2 If count < `HeuristicFallbackThreshold`, re-run as `TextRich`
- [x] 4.3 Set `meta.Strategy = "heuristic→text-rich"` on escalation
- [x] 4.4 Return TextRich result when fallback triggers

## 5. Lab UI

- [x] 5.1 Add `Strategy` display to lab stats table alongside Classification label
- [x] 5.2 Update `appsettings.json` to include `HeuristicFallbackThreshold: 3`
