## ADDED Requirements

### Requirement: Dashboard shows aggregate accuracy stats
The results dashboard (`Results.razor`) SHALL display a summary of extraction accuracy containing: total rated count, Good count, Bad count, and accuracy percentage (Good / rated * 100). The widget SHALL only appear when at least one result has been rated.

#### Scenario: No rated results
- **WHEN** all results are `Unrated`
- **THEN** the accuracy stats widget SHALL NOT be rendered on the dashboard

#### Scenario: Mix of rated and unrated results
- **WHEN** some results are rated (`Good` or `Bad`) and others are `Unrated`
- **THEN** the dashboard SHALL display Good count, Bad count, and accuracy % computed from rated entries only
- **THEN** unrated entries SHALL NOT affect the counts

#### Scenario: All results rated Good
- **WHEN** all results have `Rating = Good`
- **THEN** accuracy percentage SHALL display as 100%

#### Scenario: Stats update after rating
- **WHEN** the user rates a result from the detail page and returns to the dashboard
- **THEN** the accuracy stats SHALL reflect the updated counts
