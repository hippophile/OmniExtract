# OmniExtract — Project Goal

## Mission

OmniExtract is a **universal document reader, analyzer, and OCR testing ground**.

The goal is to build a system that can accept any document in any format — PDF, Word, Excel, images, emails, scanned pages, spreadsheets — and extract structured, queryable information from it automatically using AI.

## What "Universal" Means

- **Any format**: PDF, DOCX, XLSX, PPTX, PNG/JPG (scanned), EML, CSV, TXT, ZIP archives
- **Any domain**: Finance, legal, HR, technical, medical, logistics — no hardcoded schemas
- **Any structure**: Tables, paragraphs, key-value pairs, mixed layouts, multi-language content
- **Structured output**: Always returns consistent JSON regardless of input chaos

## This App is a POC

The web interface exists purely as a testing cockpit:

1. Drop a document
2. Watch the extraction run
3. Inspect the raw structured output
4. Judge whether the AI understood the document correctly

No analytics. No dashboards. No metrics. Just: **input → extraction → raw result**.

## What We're Testing

- Can the AI correctly identify document type without being told?
- Can it extract all meaningful fields from unstructured text?
- Does OCR (image/scanned PDF) produce usable text for extraction?
- Does the output schema hold up across wildly different document types?
- What fails, and why?

## Stack

- **AI backend**: GitHub Copilot SDK (`GitHub.Copilot.SDK`) — no API keys, uses Copilot auth
- **Document parsing**: LibreOffice bridge (DOCX/XLSX/PPTX), PdfPig (PDF text), image-to-PNG pipeline
- **Web UI**: Blazor Server (.NET)
- **Storage**: JSON files on disk (simple, inspectable)

## North Star

Eventually: a pipeline where you drop *anything* — a photo of a handwritten note, a scanned invoice, a complex Excel model, a 200-page legal contract — and get back clean, structured, retrievable data.
