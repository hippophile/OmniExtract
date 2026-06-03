# OmniExtract — Pitch Deck
**Optima Bank Greece · Document Intelligence POC**

---

## Slide 1 — OmniExtract: Universal Document Intelligence

**Turning any document into structured data — automatically.**

*Presenter: [Name]*
*Date: [Date]*
*Prepared for: Optima Bank Greece*

> **Speaker notes:** Welcome and introduce yourself and OmniExtract. Set the tone: this is a working proof of concept, not a slide-ware demo. Everything shown today runs live. The goal of this session is to explore how OmniExtract fits into Optima Bank's document processing workflows and agree on concrete next steps.

---

## Slide 2 — The Problem

**Banks process thousands of documents every day. Almost entirely by hand.**

- **Volume**: Loan applications, KYC packets, contracts, statements, invoices — every customer interaction generates documents
- **Manual effort**: Compliance officers, analysts, and operations staff spend hours re-keying data from PDFs and scans into core banking systems
- **Error rate**: Manual transcription introduces errors that propagate downstream into credit decisions, KYC records, and regulatory filings
- **Speed**: Processing times of 2–5 business days for KYC; multi-day delays in loan underwriting during peak periods
- **Cost**: Every manually reviewed document is analyst time that cannot be spent on higher-value work
- **Compliance risk**: Missed renewal dates, incomplete obligation extraction, and inconsistent classification create regulatory exposure

**The documents are already digital. The problem is turning them into usable data.**

> **Speaker notes:** Ground this in Optima Bank's reality. Ask the audience: how many documents does your operations team process per day? Where are the biggest backlogs? The goal is to make them feel the pain before presenting the solution. Avoid overstating — let them supply their own numbers.

---

## Slide 3 — The Solution

**OmniExtract: one pipeline that reads any document and returns clean, structured data.**

- **Any format**: PDF, Word, Excel, PowerPoint, scanned images, emails, CSV, ZIP archives — no per-format configuration
- **Any document type**: Invoices, contracts, identity documents, bank statements, regulatory notices — no templates required
- **AI-powered**: GPT-4.1 identifies the document type automatically and extracts every meaningful field
- **Structured output**: Every document returns consistent JSON — ready for ingestion into any downstream system
- **Adaptive classification**: Documents classified as structured (data-heavy) or narrative (prose-heavy) before extraction; appropriate AI strategy applied automatically
- **Specialist routing**: After extraction, an agent recommendation identifies the best downstream specialist for each document

**Drop a document. Get structured data. No configuration, no schemas, no per-type rules.**

> **Speaker notes:** Emphasise the "no configuration" message — this is the key differentiator. Banks typically spend months configuring OCR templates for each document type. OmniExtract requires none of that. The AI understands document structure without being told what to look for.

---

## Slide 4 — Live Demo Flow

**What you're about to see — step by step:**

1. **Open the OmniExtract web interface** — running locally on this machine (no cloud dependency for this demo)
2. **Upload a document** — drag and drop any file: PDF, scanned image, Word document, or spreadsheet
3. **Watch the pipeline run** — the queue shows real-time processing stages: Parse → Extract → AI Analysis → Agent Recommendation
4. **Inspect the result** — structured JSON output: document type, confidence score, all extracted fields, tags, and classification
5. **See the agent recommendation** — which specialist agent should handle this document next (Financial, Legal, Fraud, Compliance, or Business)
6. **Try Deep Analysis mode** — for long-form documents, enable the toggle to get a distilled intelligence brief: findings, risks, key facts, one-page summary
7. **Try the Test Lab** — upload one document, run all three extraction strategies in parallel, and compare results side by side

> **Speaker notes:** Before starting the demo, have test documents ready: one structured document (e.g., an invoice or loan application form), one narrative document (e.g., a regulatory circular or employment contract), and one scanned image. Walk through the flow slowly — let the audience read the output. If a document returns unexpected results, treat it as useful signal rather than a failure.

---

## Slide 5 — Use Case 1: KYC Onboarding

**Eliminating manual identity document transcription**

| | |
|---|---|
| **Documents** | Greek national ID, passport, utility bill, company certificate (ΓΕΜΗ), ΑΦΜ certificate |
| **Current state** | Compliance officers manually transcribe data from scanned documents into the KYC platform (2–5 day turnaround) |
| **With OmniExtract** | Scanned or photographed documents uploaded in batch; AI extracts all fields and returns structured JSON for automated KYC record creation |

**Fields extracted automatically:**
`full_name` · `document_number` · `date_of_birth` · `nationality` · `issue_date` · `expiry_date` · `address` · `tax_id` · `company_registration_number`

**Business outcome:** Eliminate manual re-keying, reduce KYC completion time, and create a consistent audit trail with extraction confidence scores.

> **Speaker notes:** KYC is typically the highest-volume, most labour-intensive use case for onboarding teams. Emphasise that OmniExtract handles Greek-language documents natively — no translation step required. The confidence score on each extraction tells the compliance officer when to apply closer human review, rather than reviewing everything.

---

## Slide 6 — Use Case 2: Loan Underwriting

**Accelerating credit data collation from multi-document applicant packs**

| | |
|---|---|
| **Documents** | Payslips, tax returns (Ε1), bank statements, employment contracts, loan application form |
| **Current state** | Credit analysts manually collate financial data across multiple documents per applicant before populating the loan origination system |
| **With OmniExtract** | Full applicant document pack (ZIP supported) processed in one batch; unified structured output delivered to the loan origination system |

**Fields extracted automatically:**
`applicant_name` · `gross_monthly_income` · `net_monthly_income` · `annual_taxable_income` · `employment_type` · `existing_loan_obligations` · `average_monthly_balance` · `requested_loan_amount`

**Business outcome:** Reduce analyst document review time, improve consistency of credit data inputs, and eliminate the bottleneck that causes underwriting delays during high-demand periods.

> **Speaker notes:** This use case benefits from OmniExtract's ZIP archive support — an entire applicant pack can be uploaded as a single file and processed as a batch. Highlight the income and obligation fields: these are exactly what the credit scoring engine needs, and they arrive pre-structured rather than requiring manual extraction.

---

## Slide 7 — Use Case 3: Collateral Assessment

**Structured property data from valuation reports and title deeds**

| | |
|---|---|
| **Documents** | Property valuation report, title deed (Συμβόλαιο), cadastral certificate (Κτηματολόγιο), ΕΝΦΙΑ tax certificate, encumbrance certificate |
| **Current state** | Risk analysts manually extract property specifications, valuations, and encumbrances from multi-page scanned PDFs — with risk of missing key terms |
| **With OmniExtract** | Greek-language property documents processed with full field extraction; encumbrances, valuation figures, and identifiers extracted into structured JSON |

**Fields extracted automatically:**
`property_address` · `cadastral_parcel_id` · `surface_area_sqm` · `market_value_eur` · `forced_sale_value_eur` · `encumbrances` · `owner_name` · `valuation_date`

**Business outcome:** Structured collateral data available immediately on document upload; LTV ratios and portfolio monitoring updated automatically without manual data entry.

> **Speaker notes:** The collateral use case is particularly compelling for mortgage lending. Emphasise that the AI handles Greek-language documents — notarial deeds and cadastral certificates — without any language configuration. The encumbrances field is especially high-value: missing a prior mortgage on a collateral asset is a serious underwriting risk.

---

## Slide 8 — Use Case 4: Supplier Contract Intelligence

**Extracting commercial terms from hundreds of supplier agreements**

| | |
|---|---|
| **Documents** | Master services agreements, SOWs, NDAs, software licence agreements, SLA schedules |
| **Current state** | Procurement and legal teams manually review contracts to locate renewal dates, payment terms, and SLA penalties — expiry dates frequently missed |
| **With OmniExtract** | Contracts processed on ingestion; key commercial terms extracted and available for integration into CLM and accounts payable systems |

**Fields extracted automatically:**
`supplier_name` · `contract_type` · `effective_date` · `expiry_date` · `auto_renewal` · `total_contract_value_eur` · `payment_terms` · `sla_uptime_pct` · `penalty_clause` · `liability_cap_eur`

**Business outcome:** No missed renewal dates; commercial terms available for automated alerting; legal exposure tracking from structured liability cap data.

> **Speaker notes:** This use case resonates strongly with CFOs and legal teams. The "missed auto-renewal" scenario is universally relatable — ask if they've experienced it. The narrative extraction mode is particularly effective here: contracts are prose-heavy documents, and OmniExtract's adaptive classification routes them to the narrative prompt automatically, extracting sections, key terms, and conclusions rather than just flat fields.

---

## Slide 9 — Use Case 5: Regulatory Document Intake

**Triaging and extracting obligations from supervisory communications**

| | |
|---|---|
| **Documents** | Bank of Greece circulars, EBA guidelines, ECB supervisory letters, FATF updates, EC regulations |
| **Current state** | Compliance analysts manually classify, summarise, and extract obligations from regulatory publications — creating delays in response and risk of missed deadlines |
| **With OmniExtract** | Regulatory documents processed on receipt; issuing authority, effective dates, obligations, and impacted functions extracted and delivered to the GRC platform |

**Fields extracted automatically:**
`issuing_authority` · `document_reference` · `publication_date` · `effective_date` · `implementation_deadline` · `affected_regulations` · `key_obligations` · `impacted_functions` · `summary`

**Business outcome:** Faster regulatory triage, automated obligation tracking, and consistent classification of every incoming supervisory communication without analyst bottlenecks.

> **Speaker notes:** This use case is often overlooked but has significant compliance value. Banks receive hundreds of regulatory documents per year; missing an implementation deadline carries real regulatory consequences. The Deep Analysis mode is ideal here: a Bank of Greece circular can be distilled into a one-page brief with findings prioritised by certainty level, saving analysts significant reading time.

---

## Slide 10 — Technical Architecture

**How it works — from document to structured data in four steps:**

```
┌─────────────────────────────────────────────────────────────────┐
│  STEP 1: UPLOAD                                                   │
│  Any file dropped into the web interface or CLI                   │
│  PDF · Word · Excel · Image · Email · CSV · ZIP                  │
└──────────────────────┬──────────────────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  STEP 2: PARSE                                                    │
│  Format detected → text extracted or pages converted to images   │
│  LibreOffice (Office formats) · PdfPig (PDF) · OCR (images)     │
└──────────────────────┬──────────────────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  STEP 3: AI EXTRACTION                                            │
│  Document classified → appropriate prompt selected               │
│  GPT-4.1 extracts all fields, tables, tags, and categories      │
│  Large documents chunked and merged automatically                │
└──────────────────────┬──────────────────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  STEP 4: STRUCTURED OUTPUT                                        │
│  JSON result: meta · tags · categories · data fields · tables   │
│  Agent recommendation attached · stored for review and export   │
└─────────────────────────────────────────────────────────────────┘
```

**No cloud dependency** · **No per-format templates** · **No hardcoded schemas**

> **Speaker notes:** Keep the technical explanation high-level for this audience. The key message is: four steps, no configuration, works for any document. If there are technical stakeholders in the room, you can go deeper on the chunking strategy or the adaptive classification — but lead with the business outcome, not the implementation detail. The GitHub Copilot SDK is the AI backend; it uses Optima Bank's existing Copilot subscription, requiring no separate API keys.

---

## Slide 11 — POC Results

**What we've observed in testing — honestly reported**

### What works well
- **Structured documents** (invoices, forms, spreadsheets, CSV): high-quality field extraction with correct identification of document type and field names in the majority of tested documents
- **Multi-language support**: Greek and English documents processed without language configuration — field names and values extracted correctly from both
- **Large documents**: Chunking and synthesis pass handles multi-hundred-page PDFs; extraction quality degrades gracefully rather than failing
- **Archive processing**: ZIP files containing multiple documents processed as a batch with per-member extraction

### Where results vary
- **Scanned images**: OCR quality depends on scan resolution and handwriting clarity — printed documents perform better than handwritten ones
- **Complex tables**: Deeply nested or merged-cell tables are sometimes flattened or partially extracted
- **Very short documents** (< 200 characters): Classifier may default to structured mode; output is shallow but correct for the available content

### Honest assessment
This is a proof of concept. Accuracy varies by document type and quality. The extraction confidence score on each result tells you how certain the AI is — treat low-confidence results as candidates for human review rather than automatic processing.

> **Speaker notes:** Do not inflate these results. The audience will test OmniExtract themselves after this meeting — if you've overstated accuracy, you'll lose credibility immediately. Instead, frame the variability as a known, manageable property: the confidence score exists precisely to flag which results need review. No human process is 100% accurate either — the question is whether OmniExtract's accuracy, combined with selective human review, is faster and cheaper than full manual processing.

---

## Slide 12 — Next Steps

**Proposed actions for Optima Bank stakeholders**

### Immediate (within 2 weeks)
1. **Pilot with real documents**: Optima Bank provides a sample set of 20–50 documents across 2–3 use cases; we run OmniExtract and share extraction results for accuracy review
2. **Identify integration targets**: Agree which downstream system (KYC platform, loan origination, or GRC) would receive the first structured JSON output in a live integration

### Short-term (within 4–6 weeks)
3. **API integration prototype**: Connect OmniExtract's JSON output to one target system via a lightweight REST wrapper; demonstrate end-to-end flow from document upload to system record creation
4. **Accuracy baseline**: Run a structured accuracy assessment on the pilot document set — measure field-level extraction accuracy against ground truth for agreed use cases

### Decision point (6 weeks)
5. **Production readiness review**: Based on pilot accuracy and integration results, assess whether OmniExtract meets Optima Bank's threshold for production deployment in the chosen use case

**What we need from you today:** Nominate a technical and a business contact; agree on the pilot document set and first integration target.

> **Speaker notes:** End with a direct ask — don't leave the room without agreeing on who does what next. The pilot is low-risk: 20–50 documents, a few days of work, clear output. The integration prototype is where the real evaluation happens. If you get pushback on the timeline, the minimum viable next step is just the pilot document set — you can run that yourself and share results by email within a week.

---

*End of deck.*

---

*Generated by OmniExtract · docs/pitch-deck.md · © 2026*
