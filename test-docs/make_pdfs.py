from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.lib import colors
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, HRFlowable
from reportlab.lib.enums import TA_LEFT, TA_CENTER, TA_JUSTIFY
import os

OUT = "/home/zazu/Desktop/OmniExtract_TestDocs"

# ── CONTRACT PDF ─────────────────────────────────────────────────────────────

def make_contract():
    doc = SimpleDocTemplate(
        os.path.join(OUT, "service-agreement.pdf"),
        pagesize=A4,
        leftMargin=2.5*cm, rightMargin=2.5*cm,
        topMargin=2.5*cm, bottomMargin=2.5*cm
    )
    styles = getSampleStyleSheet()
    body  = ParagraphStyle("body",  parent=styles["Normal"], fontSize=10, leading=15, spaceAfter=8, alignment=TA_JUSTIFY)
    h1    = ParagraphStyle("h1",    parent=styles["Heading1"], fontSize=13, spaceAfter=8, spaceBefore=14)
    center= ParagraphStyle("center",parent=styles["Normal"], fontSize=10, alignment=TA_CENTER, spaceAfter=4)

    story = []

    story.append(Paragraph("PROFESSIONAL SERVICES AGREEMENT", ParagraphStyle("t", parent=styles["Title"], fontSize=15, alignment=TA_CENTER, spaceAfter=2)))
    # Dirty: ref buried mid-sentence, not labelled clearly
    story.append(Paragraph("(ref. PSA-2026-0047 — supersedes draft dated 14 Apr 2026)", center))
    story.append(Spacer(1, 0.3*cm))
    story.append(HRFlowable(width="100%", thickness=1, color=colors.grey))
    story.append(Spacer(1, 0.3*cm))

    # Dirty: parties section is a wall of text, not split out cleanly
    story.append(Paragraph("PARTIES", h1))
    story.append(Paragraph(
        "This agreement (the \"Agreement\") is made on <b>1st June 2026</b> between "
        "Vertex Consulting Group Ltd (Co. No. 09284731), 14 Harbour Exchange Sq., London E14 9GE "
        "(\"Vertex\", \"Service Provider\", \"Consultant\" — used interchangeably herein) AND "
        "Acme Corporation S.A. (ΑΦΜ: EL800123456), 12 Kifisias Ave, Athens 115 26 (the \"Client\"). "
        "Where context permits, references to Vertex include its authorised subcontractors.", body))

    story.append(Paragraph("1.  SCOPE", h1))
    # Dirty: scope mixes deliverables and personnel in same paragraph, unclear separation
    story.append(Paragraph(
        "Vertex will provide financial systems integration consulting re: Client's ERP migration to "
        "SAP S/4HANA incl. infrastructure analysis, migration roadmap, and up to 4 on-site workshops "
        "(Athens). Lead: Dr. Andreas Papadopoulos (Principal, ERP). Min. 2 senior consultants assigned. "
        "Subcontracting to pre-approved partners permitted w/ 5 business days notice to Client.", body))

    story.append(Paragraph("2.  TERM & RENEWAL", h1))
    # Dirty: dates buried in prose
    story.append(Paragraph(
        "Commences Effective Date, expires <b>30 Nov 2026</b> unless extended. Extension by mutual "
        "written agreement ≥30 days before expiry. If neither party serves notice, agreement lapses — "
        "there is NO automatic renewal. Note: Milestone M4 (see Clause 3) must be delivered before "
        "expiry regardless of any extension discussions.", body))

    story.append(Paragraph("3.  FEES", h1))
    story.append(Paragraph("Payment schedule as follows (all amounts excl. VAT):", body))

    # Dirty: inconsistent number formatting, one cell missing, one amount ambiguous
    fee_data = [
        ["#", "Deliverable", "Amount", "Due"],
        ["M1", "Infra audit report", "EUR 18,500", "30/06/26"],
        ["M2", "Migration roadmap", "24.000 EUR", "31/08/26"],   # European decimal notation
        ["M3", "Workshops x4", "€16,000", "31/10/26"],
        ["M4", "Final sign-off / integration cert.", "11,500", "30/11/26"],  # missing currency symbol
        ["", "TOTAL (excl. VAT)", "€70,000", ""],
    ]
    t = Table(fee_data, colWidths=[1.5*cm, 7*cm, 3*cm, 3*cm])
    t.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (-1,0), colors.HexColor("#333333")),
        ("TEXTCOLOR",  (0,0), (-1,0), colors.white),
        ("FONTNAME",   (0,0), (-1,0), "Helvetica-Bold"),
        ("FONTSIZE",   (0,0), (-1,-1), 9),
        ("ROWBACKGROUNDS", (0,1), (-1,-2), [colors.HexColor("#f5f5f5"), colors.white]),
        ("BACKGROUND", (0,-1), (-1,-1), colors.HexColor("#e0e0e0")),
        ("FONTNAME",   (0,-1), (-1,-1), "Helvetica-Bold"),
        ("GRID",       (0,0), (-1,-1), 0.5, colors.HexColor("#bbbbbb")),
        ("PADDING",    (0,0), (-1,-1), 5),
    ]))
    story.append(t)
    story.append(Spacer(1, 0.2*cm))

    # Dirty: payment terms split across two paragraphs with slightly different language
    story.append(Paragraph(
        "Each milestone payable net-30 from Client's written sign-off. Overdue amounts bear interest "
        "@ 4% p.a. above BoE base rate (compounded monthly).", body))
    story.append(Paragraph(
        "Note: M2 and M3 fees may be invoiced together at Vertex's discretion if deliverables are "
        "accepted within the same calendar month. All fees are subject to UK VAT at prevailing rate "
        "where applicable; Greek VAT rules apply to Client's local entity.", body))

    story.append(Paragraph("4.  IP / OWNERSHIP", h1))
    story.append(Paragraph(
        "Work product created specifically for Client vests in Client upon receipt of full payment. "
        "Vertex retains all Background IP (methodologies, tools, frameworks, know-how). "
        "Client granted perpetual royalty-free licence to Background IP embedded in deliverables. "
        "For avoidance of doubt: generic consulting templates remain Vertex property even if "
        "customised for this engagement.", body))

    story.append(Paragraph("5.  CONFIDENTIALITY", h1))
    story.append(Paragraph(
        "Standard mutual NDA obligations apply. Survives termination for 3 years. "
        "Carve-outs: publicly available info, info received from third parties w/o restriction, "
        "info independently developed. Either party may disclose to regulators as required by law.", body))

    story.append(Paragraph("6.  LIABILITY CAP", h1))
    # Dirty: liability cap expressed ambiguously
    story.append(Paragraph(
        "Vertex's aggregate liability capped at 100% of fees paid in the 12 months preceding the "
        "claim event (max. €70,000 for this agreement). Neither party liable for indirect, "
        "consequential or punitive losses. Client's liability to Vertex not separately capped — "
        "standard contract law applies.", body))

    story.append(Paragraph("7.  GOVERNING LAW / DISPUTES", h1))
    story.append(Paragraph(
        "England & Wales law. Disputes → LCIA arbitration, seat London. "
        "Notwithstanding the foregoing, either party may seek injunctive relief in any court of "
        "competent jurisdiction without this being deemed a waiver of the arbitration clause.", body))

    story.append(Paragraph("8.  TERMINATION", h1))
    # Dirty: notice periods mentioned twice with slightly different framing
    story.append(Paragraph(
        "For convenience: 60 days written notice by either party. For material breach: 14-day "
        "cure period after written notice; if uncured, immediate termination. On termination, "
        "Client pays pro-rata fees for work completed; no refund for fees already paid. "
        "Vertex may suspend (not terminate) for non-payment after 45 days overdue, "
        "with 7 days prior written notice.", body))

    story.append(Spacer(1, 0.5*cm))
    story.append(HRFlowable(width="100%", thickness=1, color=colors.grey))
    story.append(Spacer(1, 0.3*cm))

    # Dirty: signature block mixed into prose
    story.append(Paragraph(
        "Executed by the duly authorised representatives of each party as of the date first written above. "
        "FOR VERTEX: Marcus Reid, CEO — signature: ________________  date: ________ "
        "FOR CLIENT: Filip Paschalakis, CFO — signature: ________________  date: ________", body))

    doc.build(story)
    print("service-agreement.pdf → done")


# ── FINANCIAL REPORT PDF ─────────────────────────────────────────────────────

def make_financial_report():
    doc = SimpleDocTemplate(
        os.path.join(OUT, "annual-financial-report.pdf"),
        pagesize=A4,
        leftMargin=2.5*cm, rightMargin=2.5*cm,
        topMargin=2.5*cm, bottomMargin=2.5*cm
    )
    styles = getSampleStyleSheet()
    body  = ParagraphStyle("body",  parent=styles["Normal"], fontSize=10, leading=15, spaceAfter=8, alignment=TA_JUSTIFY)
    h1    = ParagraphStyle("h1",    parent=styles["Heading1"], fontSize=13, spaceAfter=8, spaceBefore=14)
    small = ParagraphStyle("small", parent=styles["Normal"], fontSize=8, leading=12, spaceAfter=4, textColor=colors.HexColor("#555555"))
    center= ParagraphStyle("center",parent=styles["Normal"], fontSize=10, alignment=TA_CENTER, spaceAfter=4)

    story = []

    story.append(Paragraph("ACME CORPORATION S.A.", ParagraphStyle("co", parent=styles["Title"], fontSize=17, alignment=TA_CENTER, spaceAfter=2)))
    story.append(Paragraph("Annual Financial Report — FY ended 31 December 2025", center))
    # Dirty: audit note buried, not prominent
    story.append(Paragraph("Audited (PwC Athens, opinion signed 28/02/2026) | EUR | IFRS basis | Prepared: Finance Dept.", ParagraphStyle("s", parent=styles["Normal"], fontSize=8, alignment=TA_CENTER, spaceAfter=2, textColor=colors.grey)))
    story.append(Spacer(1, 0.3*cm))
    story.append(HRFlowable(width="100%", thickness=1.5, color=colors.HexColor("#222222")))
    story.append(Spacer(1, 0.3*cm))

    story.append(Paragraph("CFO COMMENTARY", h1))
    # Dirty: numbers embedded inconsistently in prose, some % formatted differently
    story.append(Paragraph(
        "FY2025 was a year of solid operational recovery following the restructuring initiated Q3 2024. "
        "Group revenue grew 14.3% YoY to €42.6m (FY2024: €37.27m), led by Technology Solutions (+22.1%) "
        "and partial Logistics recovery (+8.4%). Domestic (Greek) market: 58% of revenue; international EU: 42%.", body))
    story.append(Paragraph(
        "EBITDA: <b>€7.84m</b> (margin 18.4%), up from €5.21m / 14.0% in 2024. Full-year impact of cost "
        "programme delivered ~€2.1m annualised savings. D&A: €1.92m → EBIT €5.92m. "
        "Net profit after tax: <b>€3,870,000</b> (margin: 9.1%, vs 6.1% prior year).", body))
    # Dirty: dividend info split — some detail here, more in table, creates confusion
    story.append(Paragraph(
        "Board proposes dividend of €0.18/share (total: approx. €1.26m) subject to AGM approval "
        "(15 Apr 2026). EPS: €0.553 (FY2024: €0.327). YE cash: €6.14m. Net debt: €8.92m "
        "(ND/EBITDA: 1.14x — covenant: 2.5x). Total assets: €38.4m.", body))

    story.append(Paragraph("KEY METRICS — FY2025 vs FY2024", h1))
    story.append(Paragraph("All figures in EUR unless stated. Figures may not sum due to rounding.", small))

    # Dirty: inconsistent formatting across rows — some EUR symbol, some not, some M notation
    kpi_data = [
        ["Metric", "FY2025", "FY2024", "Δ"],
        ["Revenue (total)", "€42,600,000", "37.27M", "+14.3%"],
        ["Gross profit", "€18,330,000", "€15,240,000", "+20.3%"],
        ["Gross margin", "43.0%", "40.9%", "+2.1pp"],
        ["EBITDA", "7,840,000", "€5,210,000", "+50.5%"],    # missing € on FY2025
        ["EBITDA margin", "18.4%", "14.0 %", "+4.4pp"],
        ["EBIT", "€5.92m", "€3,680,000", "+60.9%"],          # M notation vs full number
        ["Net profit (AT)", "€3,870,000", "€2,290,000", "+69.0%"],
        ["Net margin", "9.1%", "6.1%", "+3.0pp"],
        ["EPS", "0.553", "€0.327", "+69.1%"],                # missing € on FY2025
        ["Total assets", "€38.4m", "34,100,000", "+12.6%"],
        ["Net debt", "€8,920,000", "€11.45m", "-22.1%"],
        ["ND / EBITDA", "1.14x", "2.20x", "—"],
        ["Cash & equivalents", "€6,140,000", "3,820,000", "+60.7%"],  # missing €
        ["Headcount (FTE)", "312", "287", "+25"],
        ["Dividend / share (proposed)", "€0.18", "nil", "n/a"],
    ]
    t = Table(kpi_data, colWidths=[6.5*cm, 3.5*cm, 3.5*cm, 2*cm])
    t.setStyle(TableStyle([
        ("BACKGROUND",     (0,0), (-1,0), colors.HexColor("#222222")),
        ("TEXTCOLOR",      (0,0), (-1,0), colors.white),
        ("FONTNAME",       (0,0), (-1,0), "Helvetica-Bold"),
        ("FONTSIZE",       (0,0), (-1,-1), 9),
        ("ROWBACKGROUNDS", (0,1), (-1,-1), [colors.HexColor("#f5f5f5"), colors.white]),
        ("GRID",           (0,0), (-1,-1), 0.4, colors.HexColor("#cccccc")),
        ("ALIGN",          (1,0), (-1,-1), "RIGHT"),
        ("PADDING",        (0,0), (-1,-1), 5),
        ("FONTNAME",       (0,1), (0,-1), "Helvetica-Bold"),
    ]))
    story.append(t)
    story.append(Paragraph("* FY2024 figures restated for IFRS 16 adoption. See Note 3.", small))
    story.append(Spacer(1, 0.3*cm))

    story.append(Paragraph("REVENUE BY DIVISION", h1))
    # Dirty: division names slightly different from how they appear in CFO commentary
    story.append(Paragraph(
        "Tech Solutions remains primary growth engine. Logistics recovering. Advisory stable. "
        "Breakdown below (note: 'Other' includes inter-segment eliminations):", body))

    div_data = [
        ["Segment", "FY2025", "FY2024", "Growth", "Mix"],
        ["Technology Solutions", "19,800,000", "€16,220,000", "+22.1%", "46.5%"],
        ["Logistics & Supply Chain", "€11.3m", "€10,425,000", "+8.4%", "26.5%"],   # abbreviated + full mix
        ["Consulting / Advisory", "€8,600,000", "7.83M", "+9.8%", "20.2%"],
        ["Other / eliminations", "2,900,000", "€2,795,000", "+3.8%", "6.8%"],
        ["GROUP TOTAL", "€42,600,000", "€37,270,000", "+14.3%", "100%"],
    ]
    dt = Table(div_data, colWidths=[5.5*cm, 3.5*cm, 3*cm, 2*cm, 2*cm])
    dt.setStyle(TableStyle([
        ("BACKGROUND",     (0,0), (-1,0), colors.HexColor("#222222")),
        ("TEXTCOLOR",      (0,0), (-1,0), colors.white),
        ("FONTNAME",       (0,0), (-1,0), "Helvetica-Bold"),
        ("FONTNAME",       (0,-1), (-1,-1), "Helvetica-Bold"),
        ("BACKGROUND",     (0,-1), (-1,-1), colors.HexColor("#e0e0e0")),
        ("FONTSIZE",       (0,0), (-1,-1), 9),
        ("ROWBACKGROUNDS", (0,1), (-1,-2), [colors.HexColor("#f5f5f5"), colors.white]),
        ("GRID",           (0,0), (-1,-1), 0.4, colors.HexColor("#cccccc")),
        ("ALIGN",          (1,0), (-1,-1), "RIGHT"),
        ("PADDING",        (0,0), (-1,-1), 5),
    ]))
    story.append(dt)
    story.append(Spacer(1, 0.3*cm))

    story.append(Paragraph("OUTLOOK FY2026", h1))
    # Dirty: guidance range expressed two different ways
    story.append(Paragraph(
        "Management targets revenue of €47–49m (+10–14% YoY). EBITDA margin guided 17–19%. "
        "CapEx budget: €2.8m (data centre + ERP). ERP migration (NexusSoft → SAP, commenced Jun 2026) "
        "expected to deliver €3.2m efficiency savings over 18 months. "
        "No material acquisitions planned. Headcount growth ~5%. "
        "The above constitutes forward-looking statements and is subject to risks and uncertainties.", body))

    doc.build(story)
    print("annual-financial-report.pdf → done")


# ── EML ──────────────────────────────────────────────────────────────────────

def make_eml():
    content = """\
From: James Whitfield <j.whitfield@nexussoft.com>
To: Filip Paschalakis <filip@acmecorp.com>
CC: legal@nexussoft.com, procurement@acmecorp.com
Subject: RE: RE: Licence renewal 2026/27 - NexusSoft Ent Suite [NSL-2024-00812]
Date: Thu, 22 May 2026 09:14:37 +0000
MIME-Version: 1.0
Content-Type: text/plain; charset="utf-8"

Filip,

Following our call on Tuesday, pls find below the formal renewal proposal. Apologies for the delay — had to get sign-off from Legal on the DPA changes (new sub-processor list, see attached).

--- RENEWAL PROPOSAL SUMMARY ---

Current agreement: NSL-2024-00812, Enterprise Site Licence (named-user, 250 seats), expires 30 Jun 2026.

Proposed:
- Type: Enterprise Site Licence — UNLIMITED users (upgrade, named-user model retired Dec 2026)
- Period: 01/07/2026 – 30/06/2027
- Fee: £48,500 + VAT (i.e. £58,200 inc. VAT @ 20%)
- Early-bird (sign by 15 Jun): £44,620 + VAT [8% disc.]
- Payment: Net 30 from invoice
- Support: Priority tier, 4hr SLA, 24/5

Included at no extra cost this cycle:
  * Upgrade path to v5.0 (release: Aug 2026)
  * 1x on-site training day (up to 20 pax, to be scheduled w/in 90 days of renewal)
  * Dedicated CSM: Sarah Okafor <s.okafor@nexussoft.com>

Re the named-user → unlimited upgrade: this offer is valid for THIS renewal only. From FY2027/28 unlimited will be priced at £57,000 base.

--- DATA PROCESSING ---

Revised DPA attached (v3.1, replaces v2.4 signed Mar 2024). Key changes: updated sub-processor list (3 new processors added: AWS Frankfurt, Cloudflare, Intercom), updated retention schedules, and Art. 28 GDPR compliance language per ICO guidance published Jan 2026. Must be countersigned before or simultaneously with licence agreement.

--- NEXT STEPS ---

1. Review attached Licence Agreement + DPA v3.1
2. Countersign both, return to legal@nexussoft.com
3. Deadline for early-bird: 15 Jun 2026

Governing law: England & Wales. Disputes: courts of E&W (exclusive jurisdiction).
Termination: 90 days written notice; no pro-rata refund on early termination.

Shout if any questions. My mobile is +44 7700 900 312 if easier.

Cheers,
James

---
James Whitfield | Sr. Account Executive, Enterprise
NexusSoft Ltd | 22 Bishopsgate, London EC2N 4AY
D: +44 207 946 0312 | M: +44 7700 900 312

-----Original Message-----
From: Filip Paschalakis <filip@acmecorp.com>
Sent: Monday, 19 May 2026 16:42
To: James Whitfield <j.whitfield@nexussoft.com>
Subject: RE: Licence renewal 2026/27 - NexusSoft Ent Suite

James — yes, fine to proceed. Can you send over the formal proposal + updated DPA? We'll need both for internal sign-off (Finance + Legal). Ideally by end of week.

F.
"""
    path = os.path.join(OUT, "licence-renewal.eml")
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("licence-renewal.eml → done")


if __name__ == "__main__":
    make_eml()
    make_contract()
    make_financial_report()
