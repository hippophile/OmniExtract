## ADDED Requirements

### Requirement: Extension guide explains how to add a new format parser
The extension guide SHALL provide a step-by-step walkthrough for adding support for a new file format: where to add the extension to `NativeExtensions`, how to implement a new `Extract<Format>` method in `DocumentProcessor`, and how to wire it into the `ExtractNative` switch expression.

#### Scenario: Developer adds a new format
- **WHEN** a developer follows the extension guide to add a new format
- **THEN** they have modified exactly the right files and the new format is handled by the pipeline

### Requirement: Extension guide explains the ExtractionResult contract
The extension guide SHALL document the `ExtractionResult` model — explaining that a parser MUST populate either `Text` (for text-extractable formats) or `Images` (list of base64 PNG strings, for visual formats), and that populating `Error` signals a failed extraction.

#### Scenario: New parser returns correct output type
- **WHEN** a developer implements a new parser
- **THEN** they understand whether to return text or images based on the format

### Requirement: Extension guide explains how to modify the system prompt
The extension guide SHALL explain where the AI system prompt lives (`ExtractionService.SystemPrompt`), how it controls extraction behavior, and what modifications would affect all document types vs. format-specific extraction.

#### Scenario: Developer customizes extraction behavior
- **WHEN** a developer wants the AI to extract a new type of field consistently
- **THEN** they know to modify the system prompt and understand the impact

### Requirement: Extension guide explains how to swap or add an AI model
The extension guide SHALL explain how to change the text or vision model via `appsettings.json` (`OpenAI.Model` and `OpenAI.VisionModel`) and note that the model must be available via GitHub Copilot.

#### Scenario: Developer changes the AI model
- **WHEN** a developer updates `appsettings.json` with a different model name
- **THEN** the extension guide has told them what constraints apply (Copilot availability)
