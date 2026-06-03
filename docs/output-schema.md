# OmniExtract — Output Schema

All extractions produce a `UniversalOutput` JSON object. The schema is fixed; the content is AI-determined per document.

---

## Top-level Fields

| Field | Type | Description |
|---|---|---|
| `meta` | object | Metadata about the extraction run |
| `tags` | string[] | Flat list of descriptive tags |
| `categories` | object | Domain classification and sensitivity |
| `data` | object | Dynamic key-value fields extracted from the document |
| `tables` | array | Tabular data extracted from the document |

---

## `meta` — OutputMeta

| Field | Type | Description |
|---|---|---|
| `source_file` | string | Base filename of the processed document |
| `document_type` | string | AI-identified document type (e.g. `"Invoice"`, `"Employment Contract"`) |
| `language` | string[] | ISO 639-1 language codes detected (e.g. `["en", "el"]`) |
| `confidence` | number | AI confidence in extraction quality, 0.0–1.0 |
| `extraction_method` | string | How the document was processed — see values below |
| `warnings` | string[] | Non-fatal issues encountered during extraction |

**`extraction_method` values:**

| Value | Meaning |
|---|---|
| `text` | Document text was successfully extracted and sent to the AI |
| `vision` | Document was rendered to images and sent to AI vision model |
| `failed` | Extraction pipeline failed; `warnings` contains the error |

---

## `categories` — OutputCategories

| Field | Type | Description |
|---|---|---|
| `domain` | string | Primary domain: `finance`, `legal`, `hr`, `technical`, `general` |
| `subdomain` | string | More specific domain (e.g. `"payroll"`, `"contract"`, `"invoice"`) |
| `sensitivity` | string | Sensitivity level: `public`, `internal`, `confidential`, `restricted` |

---

## `data` — Dynamic Fields

`data` is a `Dictionary<string, object?>` containing all key-value fields the AI extracted from the document. Keys are snake_case strings; values are strings, numbers, booleans, arrays, or nested objects depending on what the document contains.

**The set of keys is entirely document-driven** — there is no fixed schema. The AI extracts whatever structured information is present and names fields descriptively.

**Example — Invoice:**
```json
{
  "invoice_number": "INV-2024-0042",
  "invoice_date": "2024-03-15",
  "due_date": "2024-04-14",
  "vendor_name": "Acme Supplies Ltd",
  "vendor_vat": "GB123456789",
  "client_name": "Optima Bank S.A.",
  "subtotal": 4200.00,
  "vat_rate": 0.24,
  "vat_amount": 1008.00,
  "total_due": 5208.00,
  "currency": "EUR",
  "payment_terms": "Net 30"
}
```

**Example — Employment Contract:**
```json
{
  "employee_name": "Maria Papadopoulou",
  "employer_name": "Optima Bank S.A.",
  "position": "Senior Credit Analyst",
  "start_date": "2024-04-01",
  "gross_annual_salary": 48000,
  "notice_period_days": 60,
  "governing_law": "Greek Labour Law"
}
```

**Example — Email:**
```json
{
  "from": "noreply@supplier.com",
  "to": "procurement@optimabank.gr",
  "subject": "Q1 2024 Invoice Attached",
  "date": "2024-03-15",
  "body_summary": "Please find attached our Q1 invoice for IT support services.",
  "attachments": ["INV-2024-Q1.pdf"]
}
```

---

## `tables` — Tabular Data

`tables` is a 3D array: **list of tables → list of rows → list of cell strings**.

```
tables[tableIndex][rowIndex][columnIndex]
```

The first row of each table is always the header row.

**Example — a single table with 2 header columns and 3 data rows:**
```json
{
  "tables": [
    [
      ["Item", "Amount (EUR)"],
      ["IT Support Services Q1", "3500.00"],
      ["Hardware Maintenance", "600.00"],
      ["Software Licences", "100.00"]
    ]
  ]
}
```

**Access pattern (C#):**
```csharp
var firstTable = output.Tables[0];
var headers = firstTable[0];          // ["Item", "Amount (EUR)"]
var firstDataRow = firstTable[1];     // ["IT Support Services Q1", "3500.00"]
var cellValue = firstTable[2][1];     // "600.00"
```

---

## Full Annotated Example

A realistic extraction result for a scanned Greek invoice:

```json
{
  "meta": {
    "source_file": "invoice-march-2024.pdf",
    "document_type": "Invoice",
    "language": ["el", "en"],
    "confidence": 0.91,
    "extraction_method": "vision",
    "warnings": []
  },
  "tags": [
    "invoice",
    "finance",
    "2024",
    "greek",
    "vat",
    "acme-supplies",
    "optima-bank",
    "scanned"
  ],
  "categories": {
    "domain": "finance",
    "subdomain": "invoice",
    "sensitivity": "confidential"
  },
  "data": {
    "invoice_number": "ΤΔΑ-2024-0042",
    "invoice_date": "2024-03-15",
    "due_date": "2024-04-14",
    "vendor_name": "Acme Supplies ΑΕ",
    "vendor_afm": "123456789",
    "vendor_address": "Λεωφόρος Αθηνών 100, Αθήνα 10441",
    "client_name": "Optima Bank Α.Ε.",
    "client_afm": "999888777",
    "subtotal": 4200.00,
    "vat_rate": 0.24,
    "vat_amount": 1008.00,
    "total_due": 5208.00,
    "currency": "EUR",
    "payment_method": "Bank transfer",
    "payment_terms": "30 days"
  },
  "tables": [
    [
      ["Περιγραφή", "Ποσότητα", "Τιμή Μονάδας", "Σύνολο"],
      ["IT Support Services - Q1 2024", "1", "3500.00", "3500.00"],
      ["Hardware Maintenance", "2", "250.00", "500.00"],
      ["Software Licences (annual)", "1", "200.00", "200.00"]
    ]
  ]
}
```
