## 1. Recommendation Prompt + Pass

- [x] 1.1 Add `RecommendationPrompt` constant to `ExtractionService` — instructs GPT to classify domain and recommend one of: `FinancialAgent`, `LegalAgent`, `FraudAgent`, `ComplianceAgent`, `BusinessAgent`; return JSON with `recommended_agent`, `domain`, `confidence`, `reasoning`, `signals[]`; cite specific evidence; temp=0
- [x] 1.2 Add `RecommendationPassAsync(UniversalOutput result, CancellationToken ct)` method to `ExtractionService` that: serialises a compact summary (document_type, domain, sensitivity, tags, data keys only, warnings), calls `_gpt.CallAsync` with `RecommendationPrompt`, parses response as `Dictionary<string, object?>`, returns it or `null` on failure (logs warning)

## 2. Orchestrator Wiring

- [x] 2.1 In `ExtractionOrchestrator.ProcessJobAsync`, after `_extractionService.ExtractAsync` returns and before `_resultsRepository.Add`, call `RecommendationPassAsync`
- [x] 2.2 If recommendation is non-null, set `result.Data["agent_recommendation"] = recommendation`
- [x] 2.3 If recommendation is null, add warning to `result.Meta.Warnings`: `"Agent recommendation pass failed — result saved without recommendation."`
- [x] 2.4 Do the same in `ProcessArchiveJobAsync` after the merged result is built

## 3. Result Detail UI

- [x] 3.1 In `Results.razor`, detect if `result.Data` contains key `"agent_recommendation"` and extract the nested dict
- [x] 3.2 Render an "Agent Recommendation" panel showing: agent name (bold), domain badge, confidence as percentage, reasoning paragraph, signals as chips
- [x] 3.3 Panel only renders when recommendation data is present — hidden for failed jobs and results without the key

## 4. CSS

- [x] 4.1 Add `.recommendation-panel` card style in `app.css` — distinct colour from existing panels (e.g. soft blue/indigo border)
- [x] 4.2 Add `.domain-badge` styles for each domain: financial (green), legal (blue), fraud (red), sensitive (orange), business (grey)
- [x] 4.3 Add `.signal-chip` style for signal tags in the recommendation panel
