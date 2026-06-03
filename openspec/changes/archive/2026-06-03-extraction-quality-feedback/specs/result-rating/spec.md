## ADDED Requirements

### Requirement: Result has a rating field
Each `ResultsEntry` SHALL have a `Rating` property of type `ExtractionRating` enum with values `Unrated`, `Good`, and `Bad`. New entries SHALL default to `Unrated`.

#### Scenario: Default rating on new result
- **WHEN** a new `ResultsEntry` is created
- **THEN** its `Rating` SHALL be `Unrated`

### Requirement: User can rate a result Good or Bad
The system SHALL allow the user to set the rating of any result to `Good` or `Bad` from the result detail page. `ResultsRepository` SHALL expose a `Rate(string id, ExtractionRating rating)` method that updates the entry in place.

#### Scenario: Rate Good
- **WHEN** the user clicks the thumbs-up button on the result detail page
- **THEN** the result's `Rating` SHALL be updated to `Good` in `ResultsRepository`
- **THEN** the thumbs-up button SHALL appear visually active

#### Scenario: Rate Bad
- **WHEN** the user clicks the thumbs-down button on the result detail page
- **THEN** the result's `Rating` SHALL be updated to `Bad` in `ResultsRepository`
- **THEN** the thumbs-down button SHALL appear visually active

#### Scenario: Re-rating toggles active state
- **WHEN** the user clicks the already-active rating button
- **THEN** the rating SHALL revert to `Unrated`
- **THEN** neither button SHALL appear active

#### Scenario: Rating persists within session navigation
- **WHEN** a result is rated and the user navigates away and back
- **THEN** the previously set rating SHALL still be shown as active
