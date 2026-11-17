# Portfolio highlights plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: built 2026-11-17, uncommitted.

## Affected modules

| Module | Change | Scenario |
|---|---|---|
| `Domain/AccountantProfile.cs` | `PortfolioHighlight` owned entries; `ReplaceHighlights`; limits in `ProfileLimits` | PH01, PH03 |
| `Data/TaxBuddyDbContext.cs`, migration `AddPortfolioHighlights` | Table `accountant_highlights` | PH01 |
| `Application/Profiles` (`ProfileForm`, `ProfileValidator`), `Data/ProfileStore.cs` | Highlight rows as typed; the prototype's messages; saved with the profile | PH02–PH04 |
| `Application/Directory` (`AccountantCard`, `CardRow`, `AccountantProfileView`), `Data/DirectoryStore.cs` | Highlight count on cards; highlights on the profile | PH01, PH06 |
| `Components/Pages/ProfileEditorPage.razor` | `portfolioFieldset`: rows with Add and Remove commands, as for experience | PH02, PH04, PH05 |
| `Components/Pages/AccountantProfilePage.razor`, `Shared/AccountantCardItem.razor`, `Styles/app.css` | The `.pf` section and the card fact | PH01, PH06, PH07 |
| `Data/Seed/development-profiles.json`, `DevelopmentSeed.cs` | The prototype's sample highlights | PH07 |

## Approach

The same shape as experience rows in F05:
- The form posts rows as typed.
- "Add highlight" and "Remove" are submit commands that redraw the form without saving.
- Validation skips fully blank rows and reports per field, with the prototype's wording.
- The domain refuses more than six highlights or over-long text, as a last check.

**Data:** additive migration. Rollback: `dotnet ef database update AddChat`.

## Verification

Rule tests (domain limits, validator), a data round trip, web tests (editor add, remove and save; profile section; card count; empty state), a browser check at 390 and 1440 px in light and dark, then the docs and evidence.
