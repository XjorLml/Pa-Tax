# F09: Ratings, reviews and replies

Governing requirements: [PRD](../../docs/PRD.md) R13 and R14; BR02 ("Highest rated"), BR09, BR10, BR11; D05, D21 and D23 (provisional below); R11 (the reviewer's short name only). Contracts: [SDD](../../docs/SDD.md) render modes (the profile and the review page are static), authorization and ratings. [DESIGN](../../docs/DESIGN.md) "Ratings and reviews". Design origin: the prototype's `reviewsHTML`, `reviewPageHTML`, `ratingLine`, `starsHTML`, the card's `.rating` and the directory's sort links ([CLAUDE.md](../../CLAUDE.md)). Depends on [F07](../chat/spec.md) for BR09's "the accountant has sent a message". Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A client rates an accountant from 1 to 5 stars, with an optional written review, once the accountant has replied to them in chat. The profile and the directory show the average and the number of reviews, and the directory can be sorted by "Highest rated". The accountant posts one public reply to each review and can edit it.

**Authorization and decisions** (2026-11-17):
- The user said "implement next feature, give the list of considerations", then answered four questions and approved the plan.
- **Review form:** a static form posted to the page, not Interactive Server as the SDD had it. As with the profile editor (F05), a failed or invalid save shows the form again with everything kept, and it works without script. This closes [C04](../../docs/README.md#history-and-unresolved-conflicts) for reviews.
- **D21 provisionally decided:** a reviewer is shown by first name and last initial ("Liza F."), from the account's display name.
- **D23 provisionally decided, review part:** the prototype's values.
  - Stars: 1 to 5, required.
  - Text: optional, no minimum, at most 1,000 characters.
  - Reply: required, at most 600 characters.
- **BR10** adopted as proposed: one decimal with the count; "No reviews yet", never a zero score.
- **"Highest rated"** order included (D05, BR02).

**In scope:**
- The review page `/accountants/{id}/review`: post, update and delete; the states for visitors, accountants and clients without a reply.
- The profile's rating line, the "average from N reviews" fact, and the Reviews section with the accountant's reply form.
- The card's rating line and the directory's "Sort: Name (A–Z) · Highest rated" links.

**Exclusions:**
- "Report this review" (R15).
- The prototype's "My reviews" row on Account, and sample reviews in the development seed (the user left both out).
- Checking or moderating reviews (D09), email about reviews (R08), and a rate limit on reviews.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| RV01 / BR09 | A client whose conversation has no message from the accountant; an accountant; a visitor | "You can't review {first} yet" with the reason, "Only clients can write reviews" or "Sign in to write a review"; no form; a forged post saves nothing | Rule tests, Web.Tests |
| RV02 / R13 | An eligible client picks stars, optionally writes, and posts | Redirect to the profile with "Your review was posted."; the review shows "Liza F. (you)", the stars and the text; the average and count update on the profile and the card | Rule tests, Web.Tests, browser |
| RV03 / BR09 | The client opens the page again | "Edit your review" leads to the form filled in; "Update review" changes the one review; "Delete review" removes it and its reply | Rule tests, Web.Tests |
| RV04 / D23 | No stars, or text over 1,000 characters | "Choose a rating from 1 to 5 stars." or the length message; everything typed stays | Rule tests, Web.Tests |
| RV05 / R14, BR11 | The profile owner presses Reply or Edit reply, writes and posts | "Reply from {first}" under the review; editing starts from the saved reply; an empty reply says "Write a reply first."; nobody else can reply | Rule tests, Web.Tests, browser |
| RV06 / BR10 | A profile with no reviews; with reviews | "No reviews yet" (no stars, no zero); "4.5" with stars and "(2 reviews)", newest change first | Rule tests, Web.Tests |
| RV07 / BR02, D05 | "Highest rated" | Reviewed accountants by average, then count, then name and ID; unreviewed last; sort, search and page links keep one another | Rule tests, Web.Tests |
| RV08 / R11 | Any review | Only the short name, no picture and no link to the reviewer | Web.Tests |
| RV09 / DESIGN | Phone and desktop, light and dark | The prototype's star picker, review cards, reply box and rating lines | Browser |

## Deviations from the prototype

Recorded in [DESIGN](../../docs/DESIGN.md):
- The star picker uses real radio inputs drawn as the prototype's star buttons, so it works without script. The checked star and those before it light up through CSS.
- Toasts ("Your review was posted.") become a status line at the top of the profile's Reviews section, after the redirect.
- The owner's reply box opens through a link (`?reply={id}`) instead of a script toggle; Cancel is a link.
- Editing a review moves it to the top, because reviews are listed by their last change, as in the prototype.

## State and dependencies

- Table `reviews`: stars (checked 1–5), text, created and updated times, reply text and time. Unique per accountant profile and client account. It is deleted with the profile; deleting the client's account is refused until D07, as for chat.
- Eligibility reads F07's conversations and messages.
- The average and count are computed from the rows, never stored (SDD ratings).

## Evidence limits

Rule and web tests use the development database. The layout and the no-script star picker are checked in a browser. Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
