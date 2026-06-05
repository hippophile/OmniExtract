## ADDED Requirements

### Requirement: Verdict card shows caveat when confidence is low
The verdict card on the result detail page SHALL display a low-confidence caveat badge in the header row when `Meta.Confidence < 0.75`. The caveat SHALL show the actual confidence percentage. The caveat SHALL be visually distinct (amber/warning tone) to set reader expectations before they read the summary. When confidence is 0.75 or above, no caveat is shown and the card renders as normal.

#### Scenario: Low confidence verdict shows caveat
- **WHEN** `Meta.Confidence` is below 0.75 and a verdict is present
- **THEN** the verdict card header displays a caveat badge reading "partial extraction · NN%"
- **THEN** the badge uses amber/warning styling distinct from the normal verdict header

#### Scenario: High confidence verdict shows no caveat
- **WHEN** `Meta.Confidence` is 0.75 or above
- **THEN** the verdict card renders without any caveat badge

#### Scenario: Zero confidence verdict shows caveat
- **WHEN** `Meta.Confidence` is 0.0 (e.g. failed or vision-only extraction)
- **THEN** the caveat badge is shown with "0%" confidence indicator
