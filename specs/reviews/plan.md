# Ratings, reviews and replies plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: built 2026-11-17, uncommitted.

## Affected modules

| Module | Change | Scenario |
|---|---|---|
| `Domain/Review.cs` | `Review` with `Change` and `SetReply`; `ReviewLimits`, `ReviewText`, `ReviewerName` | RV02–RV05, RV08 |
| `Application/Reviews` (`ReviewModels`, `ReviewService`) | Eligibility, form, save, delete, reply, the profile's section; validation with the prototype's words | RV01–RV06 |
| `Data/ReviewStore.cs`, `TaxBuddyDbContext.ConfigureReviews`, migration `AddReviews` | Table `reviews` with the unique index and check constraint | RV02, RV03 |
| `Application/Directory`, `Data/DirectoryStore.cs`, `Components/Shared/Display.cs` | `DirectorySort`; the rating order; counts on cards; `sort` in addresses | RV06, RV07 |
| `Components/Pages/ReviewPage.razor` | The static review form and its states | RV01–RV04 |
| `Components/Pages/AccountantProfilePage.razor` | Rating line, facts, Reviews section and the named `reply` form | RV02, RV05, RV06 |
| `Shared/Stars.razor`, `Shared/RatingLine.razor`, `AccountantCardItem.razor`, `DirectoryPage.razor`, `NotFound.razor` | Stars, the card line, the sort links, "This accountant is no longer available" | RV06, RV07 |
| `Styles/app.css` | The prototype's `.rating`, `.stars`, `.sortctl`, `.rev*` and `.starpick` rules through tokens | RV09 |

## Approach

Like the profile editor (F05), every form is a static named form:
- A successful post redirects, so a reload doesn't post again.
- A failed post shows the page again with the input kept.

The Application service resolves the listed profile and the acting account on every request. A forged post is refused there, even when no form was drawn. When no form is drawn, Blazor already refuses the post with 400.

A second "Post review" from another tab updates the one review. If two inserts race, the unique index decides and the loser updates.

"Highest rated" orders in SQL by:
1. whether the accountant has any reviews;
2. the average, descending;
3. the count, descending;
4. the name;
5. the ID.

Ordering by "has reviews" first keeps PostgreSQL from putting the empty averages first.

**Deviation from the approved plan:** the profile's reviews come from `ReviewService.ForProfileAsync`, not from fields added to `AccountantProfileView`. This keeps reviews in their own use case.

**Data:** additive migration. Rollback: `dotnet ef database update AddPortfolioHighlights`.

## Verification

- **Rule tests:**
  - the short name;
  - the average and its rounding;
  - validation;
  - the domain's limits;
  - eligibility;
  - one review per pair;
  - updating and deleting;
  - replies;
  - a failed save.
- **Data test:** the "Highest rated" order.
- **Web tests:**
  - the states;
  - posting, updating and deleting;
  - an invalid post;
  - forged posts;
  - replies;
  - the card;
  - the sort links.
- **Browser check:** 390 and 1440 px, light and dark.
- **Afterwards:** the docs and the evidence.
