## 1. Core Model — AnalysisMode

- [x] 1.1 Add `AnalysisMode` enum (`Standard`, `DeepAnalysis`) to `OmniExtract.Web/Models/ProcessingJob.cs`
- [x] 1.2 Add `AnalysisMode AnalysisMode` property to `ProcessingJob` (default `Standard`)

## 2. Document Classifier

- [x] 2.1 Add `ClassifyDocument(string text, string ext)` static method to `ExtractionService` returning `"structured"` or `"narrative"` using heuristic scorer (extension weight + numeric pattern count + delimiter density + sentence count + avg word length)
- [x] 2.2 Force `structured` for extensions: `.csv`, `.tsv`, `.xlsx`, `.xls`, `.ods`, `.json`, `.jsonl`, `.xml`, `.yaml`, `.yml` regardless of prose score
- [x] 2.3 Store classification in `meta.ExtractionMethod` as `text/structured` or `text/narrative`

## 3. Prompts

- [x] 3.1 Rename existing `SystemPrompt` to `StructuredPrompt` (no content change)
- [x] 3.2 Add `NarrativePrompt` constant: instructs GPT to return `{title, author, date, summary, sections:[{heading, key_points[], summary}], conclusions, key_entities[], word_count}`, temp=0, objective viewer
- [x] 3.3 Add `DeepAnalysisPrompt` constant: instructs GPT to distil document into `{document_type, domain, one_page_summary, distilled_findings:[{finding, certainty, source_section}], risks:[], key_facts:[], flags:[]}`, temp=0.3

## 4. Extraction Dispatch

- [x] 4.1 In `ExtractTextAsync`, call `ClassifyDocument` on the first chunk before the API loop
- [x] 4.2 Select `systemMsg` based on classification: `StructuredPrompt` for structured, `NarrativePrompt` for narrative (Standard mode)
- [x] 4.3 Accept `AnalysisMode` parameter in `ExtractTextAsync` — if `DeepAnalysis` and classification is `narrative`, run an additional `DeepAnalysisPrompt` pass after the synthesis pass and replace `merged.Data` with deep analysis output
- [x] 4.4 If `DeepAnalysis` mode but classification is `structured`, add warning: `"Deep Analysis skipped — document classified as structured."`
- [x] 4.5 Pass `temperature: 0.3` to `_gpt.CallAsync` for the deep analysis pass (standard passes remain `0`)

## 5. Orchestrator + Enqueue

- [x] 5.1 Add `AnalysisMode mode` parameter to `ExtractionOrchestrator.EnqueueAsync`
- [x] 5.2 Assign `job.AnalysisMode = mode` in `EnqueueAsync`
- [x] 5.3 Pass `job.AnalysisMode` through to `_extractionService.ExtractAsync`
- [x] 5.4 Add `AnalysisMode` parameter to `ExtractionService.ExtractAsync` and thread it to `ExtractTextAsync`

## 6. Upload UI

- [x] 6.1 Add `_deepAnalysis` bool field to Upload.razor code block (default `false`)
- [x] 6.2 Add "Deep Analysis" checkbox toggle below the drop zone with label and brief description
- [x] 6.3 Pass `_deepAnalysis ? AnalysisMode.DeepAnalysis : AnalysisMode.Standard` to `Orchestrator.EnqueueAsync`
- [x] 6.4 Show a "Deep Analysis" badge on job cards where `job.AnalysisMode == DeepAnalysis`

## 7. Results UI

- [x] 7.1 In `Results.razor`, detect if `result.Data` contains key `"sections"` → render narrative layout (title, summary, sections with key points, conclusions)
- [x] 7.2 Detect if `result.Data` contains key `"distilled_findings"` → render deep analysis layout (one-page summary panel, findings list with certainty badges, risks, flags)
- [x] 7.3 Add CSS for narrative and deep analysis result panels in `app.css`
