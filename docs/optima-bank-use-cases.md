# OmniExtract — Optima Bank Greece: Use Case Reference

**Prepared for:** Optima Bank Greece  
**Document type:** POC Deployment Reference  
**Scope:** 5 high-impact banking workflows where OmniExtract replaces manual document processing with AI-powered structured extraction

---

## Introduction

OmniExtract is a universal document extraction engine that accepts any document format — PDF, scanned image, Word, Excel, email, archive — and returns consistent structured JSON output via AI analysis. It operates without hardcoded schemas: the AI identifies the document type, extracts all meaningful fields, and structures the result automatically.

This document describes five concrete deployment use cases for Optima Bank Greece, each covering the problem being solved, how OmniExtract addresses it, the document types involved, the structured fields produced, and the downstream systems that consume the output.

---

## Use Case 1: KYC Onboarding

### Problem

New customer onboarding at Optima Bank requires manual review and transcription of identity documents, utility bills, and company registration certificates. Compliance officers spend significant time re-keying data from scanned documents into the bank's KYC platform, introducing transcription errors and creating processing backlogs. Turnaround times of 2–5 business days are common for full KYC completion.

### Solution

OmniExtract accepts scanned or photographed identity documents and supporting materials via its extraction pipeline. The AI model identifies the document type (passport, national ID, utility bill, company certificate) without being configured in advance, extracts all structured fields, and returns a JSON object ready for automated ingestion into the KYC platform. OCR handles scanned and photographed documents via the image-to-text pipeline.

### Document Types

- Greek national identity card (Αστυνομική Ταυτότητα)
- Passport (EU and non-EU)
- Utility bill (electricity, water, telecom) for proof of address
- Company registration certificate (ΓΕΜΗ extract)
- Tax registration document (ΑΦΜ certificate)

### Output Fields

| Field | Description |
|---|---|
| `full_name` | Full legal name as printed on document |
| `document_type` | Detected document category (e.g., "passport", "national_id") |
| `document_number` | ID, passport, or registration number |
| `date_of_birth` | Date of birth (ISO 8601) |
| `nationality` | Country of citizenship |
| `issue_date` / `expiry_date` | Document validity period |
| `address` | Full postal address extracted from proof-of-address document |
| `tax_id` | ΑΦΜ number where present |
| `company_name` | Legal entity name for corporate onboarding |
| `company_registration_number` | ΓΕΜΗ or equivalent registry number |

### Integration Points

- **KYC / AML Platform** (e.g., Temenos Financial Crime Mitigation, NICE Actimize): extracted JSON fields map directly to KYC record creation APIs, eliminating manual data entry
- **Core Banking System** (e.g., Temenos Transact / T24): customer master record pre-populated from extraction output upon KYC approval
- **Document Management System**: original files stored alongside extracted metadata for audit trail

---

## Use Case 2: Loan Underwriting

### Problem

Credit analysts at Optima Bank manually review loan applications alongside supporting financial documents — payslips, tax returns, bank statements, and employment contracts — to assess repayment capacity. Collating data from multiple multi-page documents per applicant is time-consuming, error-prone, and creates underwriting bottlenecks during high-demand periods.

### Solution

OmniExtract processes the full set of applicant-submitted documents in a single batch (supporting ZIP archive input for multi-file submissions). The AI extracts financial data across all document types and aggregates it into a unified structured output. Analysts receive a pre-populated credit assessment data package instead of reviewing raw documents, reducing review time and improving consistency.

### Document Types

- Personal income tax return (Ε1 / ΑΑΔΕ declaration)
- Payslip (single employer or multiple)
- Bank statement (3–12 months)
- Employment contract or certification letter
- Self-employment income statement (Ε3 form)
- Loan application form (Optima Bank standard form or third-party originator form)

### Output Fields

| Field | Description |
|---|---|
| `applicant_name` | Full name of loan applicant |
| `employer_name` | Current employer as stated in payslip or contract |
| `gross_monthly_income` | Gross salary per month (EUR) |
| `net_monthly_income` | Net salary after tax and contributions (EUR) |
| `annual_taxable_income` | Declared taxable income from tax return (EUR) |
| `employment_type` | "salaried", "self_employed", "contractor" |
| `employment_start_date` | Start of current employment |
| `existing_loan_obligations` | Monthly debt repayments visible in bank statements (EUR) |
| `average_monthly_balance` | Average end-of-month balance across statement period (EUR) |
| `requested_loan_amount` | Amount applied for (EUR) |
| `loan_purpose` | Stated purpose from application form |

### Integration Points

- **Loan Origination System** (e.g., Nucleus Software FinnOne, Oracle FLEXCUBE Lending): extracted fields populate the credit assessment workflow automatically
- **Credit Scoring Engine**: income and obligation data fed directly into scoring model inputs
- **Internal CRM / Relationship Manager Portal**: pre-filled applicant summary reduces analyst prep time

---

## Use Case 3: Collateral Assessment

### Problem

Mortgage and secured lending at Optima Bank requires assessment of collateral — primarily Greek real estate. Engineers' valuation reports, property title deeds, and cadastral certificates arrive as scanned PDFs or physical documents and must be manually reviewed to extract property specifications, valuations, and encumbrances. This process is slow and introduces risk if key details are missed.

### Solution

OmniExtract processes property-related documents and extracts structured asset data including location identifiers, surface areas, valuations, and legal status. The AI handles Greek-language documents natively, extracting fields from both narrative sections and embedded tables. Output is delivered as structured JSON for automatic ingestion into the collateral management system.

### Document Types

- Property valuation report (RICS or certified Greek engineer report)
- Title deed (Συμβόλαιο) — notarial document
- Cadastral certificate (Κτηματολόγιο extract)
- Mortgage encumbrance certificate (Βαρύτητα)
- Building permit and technical specification sheet
- Property tax certificate (ΕΝΦΙΑ)

### Output Fields

| Field | Description |
|---|---|
| `property_address` | Full civic address of the collateral property |
| `cadastral_parcel_id` | National cadastral identifier (ΚΑΕΚ) |
| `property_type` | "residential_apartment", "house", "commercial", "land" |
| `surface_area_sqm` | Total floor area in square metres |
| `plot_area_sqm` | Land plot area where applicable |
| `construction_year` | Year of construction |
| `market_value_eur` | Estimated market value from valuation report (EUR) |
| `forced_sale_value_eur` | Distressed/forced sale value (EUR) |
| `encumbrances` | List of existing mortgages or liens on the property |
| `owner_name` | Legal owner(s) as stated in title deed |
| `ownership_share_pct` | Ownership percentage if co-owned |
| `valuation_date` | Date of most recent valuation |

### Integration Points

- **Collateral Management System** (e.g., Temenos Collateral, FIS IBS): structured property data loaded directly into collateral records
- **Loan Origination / Credit Decision Engine**: collateral value and LTV ratio computed automatically from extracted fields
- **Risk Management Platform**: portfolio-level collateral monitoring updated with extracted valuations

---

## Use Case 4: Supplier Contract Intelligence

### Problem

Optima Bank's procurement and legal teams manage hundreds of supplier contracts — IT vendor agreements, facility management contracts, consulting engagements — stored as unstructured PDFs across multiple repositories. Locating key commercial terms (payment conditions, renewal dates, SLA penalties, liability caps) requires manual document review, and expiry dates are frequently missed, resulting in unplanned auto-renewals and compliance gaps.

### Solution

OmniExtract processes supplier contracts and extracts key commercial and legal terms into structured JSON. Contracts are processed on ingestion and the extracted data is indexed for operational use. Renewal alerts and term summaries can be driven from the structured output without manual review of contract text.

### Document Types

- Master services agreement (MSA)
- Statement of work (SOW)
- Software licence agreement
- Non-disclosure agreement (NDA)
- Framework purchasing agreement
- Service level agreement (SLA) schedule

### Output Fields

| Field | Description |
|---|---|
| `supplier_name` | Legal name of the counterparty |
| `contract_reference` | Internal or supplier contract number |
| `contract_type` | "MSA", "SOW", "NDA", "licence", "framework" |
| `effective_date` | Date contract comes into force |
| `expiry_date` | Contract end date or review date |
| `auto_renewal` | Whether contract auto-renews; notice period if so |
| `total_contract_value_eur` | Total or annual contract value (EUR) |
| `payment_terms` | Payment schedule (e.g., "net 30", "quarterly in advance") |
| `sla_uptime_pct` | Committed uptime or availability percentage |
| `penalty_clause` | Description of financial penalties for SLA breach |
| `liability_cap_eur` | Maximum liability cap (EUR) |
| `governing_law` | Jurisdiction and governing law |
| `key_contacts` | Named relationship manager / account contacts |

### Integration Points

- **Contract Lifecycle Management (CLM) System** (e.g., Icertis, DocuSign CLM): structured fields populate contract records; renewal dates trigger automated alerts
- **Accounts Payable / ERP** (e.g., Oracle Financials, SAP): payment terms and invoice schedule synchronised from extracted contract data
- **Legal & Compliance Register**: liability caps and governing law tracked for regulatory exposure reporting

---

## Use Case 5: Regulatory Document Intake

### Problem

Optima Bank's compliance function receives a continuous stream of regulatory documents — Bank of Greece circulars, EBA guidelines, ECB supervisory letters, and FATF updates — requiring triage, classification, and extraction of actionable requirements. Manual classification and summarisation by compliance analysts creates delays in regulatory response and risks missing implementation deadlines.

### Solution

OmniExtract processes incoming regulatory documents and extracts structured metadata: issuing authority, document type, affected regulations, effective dates, and a list of identified obligations. The AI's domain-agnostic extraction handles the varied formats of supervisory communications (formal letters, technical standards, Q&A documents) without template configuration. Output feeds directly into the compliance obligations register.

### Document Types

- Bank of Greece (BoG) circular and decision (Πράξη Διοικητή)
- European Banking Authority (EBA) guideline or regulatory technical standard (RTS/ITS)
- ECB supervisory letter or SSM communication
- FATF mutual evaluation report or guidance
- European Commission regulation or directive (Official Journal publication)
- Internal compliance gap assessment report

### Output Fields

| Field | Description |
|---|---|
| `issuing_authority` | Regulatory body (e.g., "Bank of Greece", "EBA", "ECB") |
| `document_title` | Official title of the regulatory publication |
| `document_reference` | Reference number or OJ citation |
| `publication_date` | Date of issue |
| `effective_date` | Date from which requirements apply |
| `implementation_deadline` | Deadline for bank compliance where stated |
| `affected_regulations` | List of referenced regulations or directives (e.g., "CRR2", "AML4AMLD") |
| `document_type` | "circular", "guideline", "RTS", "letter", "directive" |
| `key_obligations` | Extracted list of specific compliance actions required |
| `impacted_functions` | Bank functions affected (e.g., "Risk", "AML", "Treasury") |
| `summary` | AI-generated plain-language summary of the document's requirements |

### Integration Points

- **Compliance Obligations Register / GRC Platform** (e.g., MetricStream, ServiceNow GRC): structured obligations and deadlines loaded automatically, triggering task assignment workflows
- **Document Management System**: classified regulatory documents indexed with extracted metadata for searchability and audit
- **Legal Entity Management System**: regulation-to-entity impact mapping updated based on `impacted_functions` and `affected_regulations` fields

---

## Summary

| Use Case | Document Volume | Primary Benefit | Key Integration |
|---|---|---|---|
| KYC Onboarding | High (per-customer) | Eliminate manual ID transcription | KYC platform, core banking |
| Loan Underwriting | High (per-application) | Accelerate credit data collation | Loan origination, credit scoring |
| Collateral Assessment | Medium (per-property) | Structured property data from PDFs | Collateral management, risk |
| Supplier Contract Intelligence | Medium (ongoing) | Automated contract term extraction | CLM, accounts payable |
| Regulatory Document Intake | Medium (continuous) | Triage and obligation extraction | GRC platform, document management |

All use cases operate on OmniExtract's standard extraction pipeline: document upload → AI analysis → structured JSON output. No per-use-case configuration or schema definition is required.
