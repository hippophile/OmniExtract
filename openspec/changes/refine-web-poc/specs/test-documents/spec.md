## ADDED Requirements

### Requirement: Test documents folder exists on desktop
A folder at `~/Desktop/OmniExtract_TestDocs/` SHALL exist and contain at least 7 test files covering diverse document types and extraction scenarios.

#### Scenario: Folder created
- **WHEN** the implementation task runs
- **THEN** the directory `~/Desktop/OmniExtract_TestDocs/` exists
- **THEN** it contains exactly the files listed in the test document manifest

### Requirement: Test documents cover diverse extraction scenarios
Each test document SHALL contain realistic (but clearly fake/test-labelled) content sufficient to exercise the extraction pipeline for its document type.

#### Scenario: Invoice extraction test
- **WHEN** `invoice.txt` is uploaded to OmniExtract
- **THEN** the extraction should identify vendor, line items, totals, tax, and invoice number

#### Scenario: Tabular data extraction test
- **WHEN** `data.csv` is uploaded to OmniExtract
- **THEN** the extraction should identify column headers, row count, and numeric value ranges

#### Scenario: Email extraction test
- **WHEN** `email.eml` is uploaded to OmniExtract
- **THEN** the extraction should identify sender, recipient, subject, date, and body summary

#### Scenario: Legal contract extraction test
- **WHEN** `contract.txt` is uploaded to OmniExtract
- **THEN** the extraction should identify parties, effective date, key clauses, and termination terms

#### Scenario: Receipt extraction test
- **WHEN** `receipt.txt` is uploaded to OmniExtract
- **THEN** the extraction should identify store name, items purchased, individual prices, and total

#### Scenario: Business report extraction test
- **WHEN** `report.txt` is uploaded to OmniExtract
- **THEN** the extraction should identify document sections, key figures, and summary statements

#### Scenario: Multilingual content extraction test
- **WHEN** `multilang.txt` is uploaded to OmniExtract
- **THEN** the extraction should detect multiple languages and extract content from each language block

### Requirement: Test documents are clearly marked as test data
Each test document SHALL include a header or comment indicating it contains synthetic/fake data, to prevent confusion with real PII or sensitive content.

#### Scenario: Test data marker present
- **WHEN** any test document is opened in a text editor
- **THEN** the first few lines contain a clear marker such as "TEST DATA — NOT REAL" or equivalent
