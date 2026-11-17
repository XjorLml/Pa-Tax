# Ratings, reviews and replies tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md). Evidence: [2026-11-17 record](../../docs/delivery/evidence/2026-11-17-f09-reviews.md).

## Definition

- [x] Review the docs index, PRD (R13, R14, BR02, BR09–BR11, D05, D21, D23), SDD (render modes, authorization, ratings, navigation), DESIGN, STATUS and the prototype (`reviewsHTML`, `reviewPageHTML`, `ratingLine`, `starsHTML`, `cardHTML`, the sort links).
- [x] Conflict raised and decided by the user on 2026-11-17: the SDD made the review page Interactive Server, which loses drafts on a lost connection (C04). The user chose a static form.
- [x] D21, the review part of D23 and BR10 decided provisionally by the user's choices; "Highest rated" in; seed reviews and the Account row out.
- [x] Spec RV01–RV09 and plan.

## Implementation

- [x] `Review` with limits; table `reviews` (migration `AddReviews`, applied to the development database on 2026-11-17).
- [x] `ReviewService`: eligibility, form, save, delete, reply, the profile's section; `ReviewStore`.
- [x] Review page; the profile's rating line, facts, Reviews section and reply form; card rating line; sort links; not-found text.
- [x] Styles ported from the prototype; Tailwind rebuilt.
- [x] Web test classes limited to four at a time (database connection slots).
- [x] PRD R13, R14, BR10, D05, D21, D23; SDD; DESIGN; TEST-PLAN TP26; SETUP; STATUS; the docs index; README.

## Verification

- [x] 19 rule tests, 1 data test, 7 web tests, 5 sort-parsing cases; whole suite 235 passed, 0 failed, 0 skipped with `-m:1`; Release build with 0 warnings.
- [x] Browser: 12 of 12, including a review posted with page script switched off; screenshots reviewed.
- [ ] Owner walkthrough (G9).
