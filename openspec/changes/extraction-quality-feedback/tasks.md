## 1. Data Model

- [ ] 1.1 Add `ExtractionRating` enum (`Unrated`, `Good`, `Bad`) to `OmniExtract.Web/Services/ResultsRepository.cs`
- [ ] 1.2 Add `Rating` property of type `ExtractionRating` (default `Unrated`) to `ResultsEntry`
- [ ] 1.3 Add `Rate(string id, ExtractionRating rating)` method to `ResultsRepository` that sets the entry's rating (toggles back to `Unrated` if same value passed)

## 2. Result Detail UI

- [ ] 2.1 Inject `ResultsRepository` into `ResultDetail.razor` (if not already injected)
- [ ] 2.2 Add thumbs-up and thumbs-down buttons to `ResultDetail.razor` below the extraction output
- [ ] 2.3 Wire button click handlers to call `ResultsRepository.Rate()` with the appropriate `ExtractionRating` value
- [ ] 2.4 Apply active/inactive CSS classes to each button based on current `entry.Rating` value
- [ ] 2.5 Add minimal CSS for the rating buttons (active highlight for Good = green, Bad = red; neutral for inactive)

## 3. Dashboard Accuracy Stats

- [ ] 3.1 In `Results.razor`, compute Good count, Bad count, and rated total from `ResultsRepository.GetAll()`
- [ ] 3.2 Render the accuracy stats widget only when rated count > 0
- [ ] 3.3 Display Good count, Bad count, and accuracy percentage (Good / rated * 100, rounded to 1 decimal)
- [ ] 3.4 Style the widget to fit the existing dashboard layout (small summary bar or card)
