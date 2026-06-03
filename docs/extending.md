# OmniExtract — Extension Guide

---

## Adding a New Format Parser

Format parsing lives entirely in `OmniExtract.App/Services/DocumentProcessor.cs`. Adding support for a new file extension requires changes in two places within that file.

### Step 1 — Register the extension as natively handled

Add the extension to the `NativeExtensions` hash set so the file bypasses the LibreOffice fallback path:

```csharp
private static readonly HashSet<string> NativeExtensions = new(StringComparer.OrdinalIgnoreCase)
{
    // existing entries ...
    ".yourext",   // add here
};
```

If you *want* LibreOffice to handle the format (i.e. convert it to PDF first), skip this step.

### Step 2 — Add a case to `ExtractNative`

Add a branch to the `ExtractNative` switch expression:

```csharp
private ExtractionResult ExtractNative(string filePath, string ext) => ext switch
{
    // existing cases ...
    ".yourext" => ExtractYourFormat(filePath),
    _ => new ExtractionResult { Error = $"Unsupported: {ext}" }
};
```

### Step 3 — Implement the extraction method

Write a method that returns an `ExtractionResult`. See the contract below.

```csharp
private static ExtractionResult ExtractYourFormat(string filePath)
{
    // Parse the file and produce either:
    // (a) extracted text, or
    // (b) a list of base64-encoded PNG images for vision extraction

    var text = /* read and convert the file to plain text */;
    return new ExtractionResult { Text = text };
}
```

---

## `ExtractionResult` Contract

`ExtractionResult` is the internal handoff from `DocumentProcessor` to `ExtractionService`:

```csharp
public class ExtractionResult
{
    public string Text { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();  // base64-encoded PNGs
    public string? Error { get; set; }
    public bool IsGibberish { get; set; }
}
```

**Return rules:**

| Scenario | What to set |
|---|---|
| Text content extracted successfully | `Text` = extracted text, leave `Images` empty |
| Document is image-only or scanned | `Images` = list of base64-encoded PNG strings (one per page/image), leave `Text` empty |
| Both text and images available | Prefer `Text` if it's valid; fall back to `Images` for scanned documents |
| Extraction failed | `Error` = error message; `ExtractionService` wraps this in a warning and returns an empty `UniversalOutput` |
| Text was extracted but is garbage | Set `IsGibberish = true`; `ExtractionService` will treat this like an error |

**Image format:** Each string in `Images` must be a base64-encoded PNG. The `ResizeForVision` helper in `DocumentProcessor` handles resizing to stay within the vision API's size limits — call it if producing images:

```csharp
private ExtractionResult ExtractMyImageFormat(string filePath) =>
    new() { Images = [ResizeForVision(File.ReadAllBytes(filePath))] };
```

---

## Modifying Extraction Behaviour (System Prompt)

The AI extraction is driven by a system prompt defined as a constant in `ExtractionService.cs`:

```
OmniExtract.App/Services/ExtractionService.cs
→ private const string SystemPrompt = """ ... """;
```

The prompt instructs the AI to:
1. Identify the document type
2. Extract all fields, values, tables, and entities
3. Generate descriptive tags
4. Assign domain and sensitivity categories
5. Score confidence

**To change what gets extracted:**
- Edit the `SystemPrompt` constant directly
- The prompt is included in every AI call; changes take effect immediately on next run
- Avoid removing the JSON schema block at the bottom of the prompt — the parser (`ParseResponse`) expects that exact structure

**To add per-document-type instructions:**
- Append instructions to the prompt (e.g. "For invoices, always extract the VAT number as `vendor_vat`")
- Or inject document-type-specific context into the user message in `ExtractTextAsync` before calling `_gpt.CallAsync`

---

## Changing AI Models

Models are configured in `appsettings.json` under the `OpenAI` section:

```json
{
  "OpenAI": {
    "Model": "gpt-4.1",
    "VisionModel": "gpt-4.1"
  }
}
```

| Setting | Default | Purpose |
|---|---|---|
| `Model` | `gpt-4.1` | Text extraction (documents with readable text) |
| `VisionModel` | `gpt-4.1` | Vision extraction (scanned PDFs, images) |

**Important — Copilot model availability:**

OmniExtract uses `GitHub.Copilot.SDK` which routes through GitHub Copilot. The available models depend on what Copilot exposes in your account/plan at runtime — not all OpenAI model identifiers are available. If a model name is unavailable, the SDK will throw at call time. Check Copilot's current model list via `gh copilot` or the GitHub Copilot settings page.

**To use different models per extraction type**, set `Model` and `VisionModel` to different values. For example, if a cheaper model is available and sufficient for text documents:

```json
{
  "OpenAI": {
    "Model": "gpt-4o-mini",
    "VisionModel": "gpt-4.1"
  }
}
```

**Other tuneable settings** (`Processing` section in `appsettings.json`):

| Setting | Default | Description |
|---|---|---|
| `ApiConcurrency` | `2` | Max concurrent AI calls |
| `VisionChunkSize` | `6` | Pages per vision API call |
| `VisionMaxDimension` | `1024` | Max image dimension before resizing |
| `PdfDpi` | `150` | DPI for PDF→image rendering |
| `MaxOutputTokens` | `4096` | Max tokens in AI response |
| `ModelContextLimit` | `128000` | Token limit for chunking pre-flight check |
| `TokenBuffer` | `8000` | Safety margin added to pre-flight token estimate |
