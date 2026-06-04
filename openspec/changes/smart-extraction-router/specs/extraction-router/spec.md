## ADDED Requirements

### Requirement: Strategy enum defines extraction paths
The system SHALL define an `ExtractionStrategy` enum in `OmniExtract.Core` with values: `Heuristic`, `Vision`, `TextRich`, `Mixed`.

#### Scenario: Enum available to web layer
- **WHEN** the web project references `OmniExtract.Core`
- **THEN** `ExtractionStrategy` is accessible for display without circular dependencies

### Requirement: RouteDocument selects strategy from document signals
The system SHALL expose a `RouteDocument(ExtractionResult extracted, string ext)` method that inspects three signals in priority order: images present, known tabular extension, everything else.

#### Scenario: Image file routes to Vision
- **WHEN** `extracted.Images.Count > 0`
- **THEN** `RouteDocument` returns `ExtractionStrategy.Vision`

#### Scenario: Known tabular extension routes to Heuristic
- **WHEN** extension is `.xlsx`, `.xls`, `.csv`, or `.tsv` AND no images present
- **THEN** `RouteDocument` returns `ExtractionStrategy.Heuristic`

#### Scenario: Text document routes to TextRich
- **WHEN** document has extractable text AND extension is not a known tabular format
- **THEN** `RouteDocument` returns `ExtractionStrategy.TextRich`

#### Scenario: Scanned PDF with no text routes to Vision
- **WHEN** `extracted.Text` is empty or whitespace AND `extracted.Images.Count > 0`
- **THEN** `RouteDocument` returns `ExtractionStrategy.Vision`

### Requirement: ExtractAsync uses router
The system SHALL replace the binary `if images → vision else text` branch in `ExtractAsync` with a call to `RouteDocument` and dispatch to the appropriate extraction method.

#### Scenario: Strategy is logged and stored
- **WHEN** extraction completes via any strategy
- **THEN** `OutputMeta.Strategy` contains the strategy name as a string (e.g. `"heuristic"`, `"vision"`, `"text-rich"`)

### Requirement: Strategy visible in OutputMeta
`OutputMeta` SHALL include a `Strategy` string property serialized as `"strategy"` in JSON output.

#### Scenario: Strategy persists to result file
- **WHEN** a result is saved to disk
- **THEN** the JSON result file includes `"strategy"` in the `meta` block
