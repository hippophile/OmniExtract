using OmniExtract.Core.Models;

namespace OmniExtract.Web.Services;

public static class MockDataService
{
    public static List<(string FileName, UniversalOutput Output)> GetMockResults() =>
    [
        ("invoice_meridian_q4_2024.pdf",          Invoice()),
        ("employment_contract_papadopoulos.docx",  EmploymentContract()),
        ("q3_financial_report_hellas.xlsx",        FinancialStatement()),
        ("board_meeting_minutes_nov2024.pdf",       MeetingMinutes()),
        ("it_security_policy_v3.pdf",              TechPolicy()),
        ("purchase_order_27841.pdf",               PurchaseOrder()),
        ("lease_piraeus_office_2025.pdf",          LeaseAgreement()),
    ];

    private static UniversalOutput Invoice() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "Invoice",
            Language = ["en"],
            Confidence = 0.97,
            ExtractionMethod = "text",
            Warnings = []
        },
        Tags = ["invoice", "finance", "Q4-2024", "EUR", "commercial", "Meridian Software", "accounts-payable"],
        Categories = new OutputCategories { Domain = "finance", Subdomain = "accounts-payable", Sensitivity = "confidential" },
        Data = new Dictionary<string, object?>
        {
            ["invoice_number"]   = "INV-2024-4721",
            ["vendor"]           = "Meridian Software Solutions Ltd",
            ["client"]           = "Hellas Dynamics S.A.",
            ["issue_date"]       = "2024-11-15",
            ["due_date"]         = "2024-12-15",
            ["subtotal"]         = "€18,400.00",
            ["vat_rate"]         = "24%",
            ["vat_amount"]       = "€4,416.00",
            ["total_amount"]     = "€22,816.00",
            ["currency"]         = "EUR",
            ["payment_terms"]    = "Net 30",
            ["iban"]             = "GR16 0110 1250 0000 0001 2300 695",
            ["description"]      = "Software licensing and support services Q4 2024",
            ["purchase_order_ref"] = "PO-2024-18823"
        },
        Tables =
        [
            [
                ["Description", "Qty", "Unit Price", "Total"],
                ["Enterprise License (Annual)", "1", "€12,000.00", "€12,000.00"],
                ["API Integration Pack", "2", "€2,400.00", "€4,800.00"],
                ["Priority Support (Q4)", "1", "€1,600.00", "€1,600.00"]
            ]
        ],
    };

    private static UniversalOutput EmploymentContract() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "Employment Contract",
            Language = ["el", "en"],
            Confidence = 0.94,
            ExtractionMethod = "text",
            Warnings = []
        },
        Tags = ["employment", "contract", "hr", "2024", "full-time", "Athens", "legal"],
        Categories = new OutputCategories { Domain = "hr", Subdomain = "employment-agreements", Sensitivity = "restricted" },
        Data = new Dictionary<string, object?>
        {
            ["employee_name"]        = "Papadopoulos, Aliki Ioanna",
            ["employee_id"]          = "EMP-2024-0183",
            ["position"]             = "Senior Data Analyst",
            ["department"]           = "Business Intelligence",
            ["start_date"]           = "2025-01-06",
            ["contract_type"]        = "Permanent — Full Time",
            ["gross_annual_salary"]  = "€52,400",
            ["probation_period"]     = "6 months",
            ["working_hours"]        = "40 hrs/week",
            ["location"]             = "Athens, Attica (Hybrid)",
            ["reporting_to"]         = "Head of BI, Konstantinos Mavros",
            ["notice_period"]        = "3 months",
            ["non_compete_clause"]   = "12 months post-employment",
            ["signing_date"]         = "2024-12-12"
        },
        Tables = [],
    };

    private static UniversalOutput FinancialStatement() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "Income Statement",
            Language = ["en"],
            Confidence = 0.98,
            ExtractionMethod = "text",
            Warnings = []
        },
        Tags = ["finance", "income-statement", "Q3-2024", "consolidated", "IFRS", "public"],
        Categories = new OutputCategories { Domain = "finance", Subdomain = "financial-reporting", Sensitivity = "public" },
        Data = new Dictionary<string, object?>
        {
            ["company"]            = "Hellas Dynamics S.A.",
            ["period"]             = "Q3 2024 (July — September)",
            ["total_revenue"]      = "€14,381,200",
            ["operating_expenses"] = "€9,847,500",
            ["ebitda"]             = "€4,533,700",
            ["ebitda_margin"]      = "31.5%",
            ["net_profit"]         = "€2,891,400",
            ["net_margin"]         = "20.1%",
            ["eps"]                = "€0.47",
            ["yoy_revenue_growth"] = "+18.3%",
            ["reporting_standard"] = "IFRS",
            ["auditor"]            = "Deloitte Greece",
            ["currency"]           = "EUR (thousands)"
        },
        Tables =
        [
            [
                ["Line Item", "Q3 2024", "Q3 2023", "Change"],
                ["Revenue", "€14,381k", "€12,156k", "+18.3%"],
                ["Cost of Sales", "€6,214k", "€5,340k", "+16.4%"],
                ["Gross Profit", "€8,167k", "€6,816k", "+19.8%"],
                ["Operating Expenses", "€3,633k", "€3,142k", "+15.6%"],
                ["EBITDA", "€4,534k", "€3,674k", "+23.4%"],
                ["Depreciation", "€712k", "€621k", "+14.7%"],
                ["EBIT", "€3,822k", "€3,053k", "+25.2%"],
                ["Finance Costs", "€198k", "€234k", "-15.4%"],
                ["Net Profit", "€2,891k", "€2,189k", "+32.1%"]
            ]
        ],
    };

    private static UniversalOutput MeetingMinutes() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "Board Meeting Minutes",
            Language = ["en"],
            Confidence = 0.89,
            ExtractionMethod = "text",
            Warnings = ["Section 4 partially illegible (scanned document)"]
        },
        Tags = ["board-meeting", "minutes", "governance", "November-2024", "strategy"],
        Categories = new OutputCategories { Domain = "general", Subdomain = "corporate-governance", Sensitivity = "restricted" },
        Data = new Dictionary<string, object?>
        {
            ["meeting_date"]           = "2024-11-28",
            ["meeting_type"]           = "Ordinary Board Meeting",
            ["venue"]                  = "Boardroom A, 12 Kifissias Ave, Marousi",
            ["chair"]                  = "Nikolaos Andreou (Chairman)",
            ["secretary"]              = "Eleni Christodoulou",
            ["quorum"]                 = "8 of 9 members present",
            ["agenda_items"]           = "5",
            ["resolution_count"]       = "3",
            ["next_meeting"]           = "2025-01-30",
            ["minutes_approved_by"]    = "Board Secretary"
        },
        Tables =
        [
            [
                ["Attendee", "Role", "Present"],
                ["Nikolaos Andreou", "Chairman", "Yes"],
                ["Dimitra Vasileiou", "CEO", "Yes"],
                ["Petros Lekkas", "CFO", "Yes"],
                ["Anastasia Roumeliotou", "Non-Exec Director", "Yes"],
                ["Giorgos Hatzigeorgiou", "Non-Exec Director", "Yes"],
                ["Ioannis Papadimitriou", "Legal Advisor", "Yes"],
                ["Stavros Michalakis", "Non-Exec Director", "Yes"],
                ["Maria Tzoumaka", "Risk Director", "Yes"],
                ["Vassilis Kontogiannis", "Non-Exec Director", "No (apology sent)"]
            ]
        ],
    };

    private static UniversalOutput TechPolicy() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "IT Security Policy",
            Language = ["en"],
            Confidence = 0.93,
            ExtractionMethod = "text",
            Warnings = []
        },
        Tags = ["it-security", "policy", "technical", "v3.2", "ISO-27001", "compliance", "internal"],
        Categories = new OutputCategories { Domain = "technical", Subdomain = "it-security", Sensitivity = "internal" },
        Data = new Dictionary<string, object?>
        {
            ["document_title"]           = "Information Security Policy v3.2",
            ["version"]                  = "3.2",
            ["effective_date"]           = "2024-09-01",
            ["review_date"]              = "2025-09-01",
            ["owner"]                    = "CISO — Thanos Alexiou",
            ["approved_by"]              = "CTO — Nikos Diamantis",
            ["scope"]                    = "All employees, contractors, and systems",
            ["framework"]                = "ISO/IEC 27001:2022",
            ["classification"]           = "Internal",
            ["policy_sections"]          = "12",
            ["last_audit_date"]          = "2024-08-14",
            ["next_audit_date"]          = "2025-02-14",
            ["incident_response_contact"] = "security@hellasdynamics.gr"
        },
        Tables = [],
    };

    private static UniversalOutput PurchaseOrder() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "Purchase Order",
            Language = ["en"],
            Confidence = 0.96,
            ExtractionMethod = "text",
            Warnings = []
        },
        Tags = ["purchase-order", "procurement", "finance", "2024", "hardware", "EUR"],
        Categories = new OutputCategories { Domain = "finance", Subdomain = "procurement", Sensitivity = "internal" },
        Data = new Dictionary<string, object?>
        {
            ["po_number"]          = "PO-2024-27841",
            ["issued_by"]          = "Hellas Dynamics S.A.",
            ["supplier"]           = "TechVault Distribution GmbH",
            ["delivery_address"]   = "12 Kifissias Ave, Marousi, 151 24 Athens",
            ["issue_date"]         = "2024-11-04",
            ["expected_delivery"]  = "2024-11-22",
            ["payment_terms"]      = "Net 45",
            ["subtotal"]           = "€31,740.00",
            ["vat"]                = "€7,617.60",
            ["total"]              = "€39,357.60",
            ["approved_by"]        = "Petros Lekkas, CFO",
            ["cost_centre"]        = "IT Infrastructure — CC-4420"
        },
        Tables =
        [
            [
                ["Item", "SKU", "Qty", "Unit Price", "Total"],
                ["Dell PowerEdge R750 Server", "DEL-R750-64G", "2", "€8,400.00", "€16,800.00"],
                ["Cisco Catalyst 9300 Switch", "CIS-C9300-48P", "3", "€2,840.00", "€8,520.00"],
                ["APC Smart-UPS 3000VA", "APC-SMT3000I", "4", "€1,605.00", "€6,420.00"]
            ]
        ],
    };

    private static UniversalOutput LeaseAgreement() => new()
    {
        Meta = new OutputMeta
        {
            DocumentType = "Commercial Lease Agreement",
            Language = ["el", "en"],
            Confidence = 0.91,
            ExtractionMethod = "text",
            Warnings = ["Exhibits A–C referenced but not included in scanned document"]
        },
        Tags = ["lease", "legal", "real-estate", "2025", "Piraeus", "commercial", "confidential"],
        Categories = new OutputCategories { Domain = "legal", Subdomain = "real-estate", Sensitivity = "confidential" },
        Data = new Dictionary<string, object?>
        {
            ["lessor"]             = "Piraeus Property Holdings S.A.",
            ["lessee"]             = "Hellas Dynamics S.A.",
            ["property_address"]   = "3 Akti Miaouli, Piraeus, 185 35",
            ["floor_area_sqm"]     = "840",
            ["floor"]              = "4th",
            ["lease_start"]        = "2025-02-01",
            ["lease_duration"]     = "5 years",
            ["lease_end"]          = "2030-01-31",
            ["monthly_rent"]       = "€9,200",
            ["annual_rent"]        = "€110,400",
            ["rent_escalation"]    = "2.5% per annum (CPI-linked)",
            ["security_deposit"]   = "€27,600 (3 months)",
            ["purpose"]            = "Office — IT Operations Centre",
            ["break_clause"]       = "At month 30 with 6 months' notice",
            ["signed_date"]        = "2024-12-20"
        },
        Tables = [],
    };
}
