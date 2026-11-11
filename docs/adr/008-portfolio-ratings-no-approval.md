# ADR 008: A portfolio site with client ratings and no approval of accountants

Status: Accepted (product direction, who may rate, rating format, report and block without an admin page, the added pages); Proposed (reviewer name display, report handling steps, portfolio limits). Decision date: 2026-11-11. Decision owner/evidence: lumabanls, message of 2026-11-11 ("more of a social media/portfolio for accountants without needing the admin approval but more of a rating side to identify their performance") and answers the same day ("Portfolio highlights", "Clients they've chatted with", "Stars + review + reply", "Report and block only").

## Context

AccApp began as a directory with chat ([PRD](../PRD.md#requirements) R01–R11). Accountants list themselves with no approval (BR01), so nothing on the site vouched for them. A review of what the site still needed raised three gaps: no trust signal, no way to report abuse, and no legal or help pages.

The owner chose to make the profile a portfolio and to use client ratings as the trust signal, without adding admin approval.

## Decision

1. An accountant's profile is a portfolio: services, experience and portfolio highlights (R12). Posts, follows, likes and comments are not part of release 1.
2. Clients rate accountants from 1 to 5 stars with an optional written review (R13). Only a client the accountant has replied to in chat can review, once per accountant, and can change or delete it (BR09).
3. The accountant can post one public reply to each review, and cannot remove or hide a review (R14, BR11).
4. Accountants are still not approved or verified.
5. Anyone signed in can report a profile, a review or a conversation, and block someone from messaging them (R15). There is no admin page: reports are sent to the owner and handled by hand.
6. Release 1 also gets privacy and terms pages agreed to at first sign-in, About, Help and For accountants pages, not-found and error pages, and notification emails for clients as well as accountants (R16–R18, R08).

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Ratings from clients the accountant has replied to (chosen) | Ties a review to a real exchange; needs no payment or booking data | A reply is a low bar. An accountant could message themselves from a second account and review themselves |
| Ratings from any signed-in client | More reviews sooner | Easy to fake praise or attack a competitor. Rejected by the owner |
| Admin approval or licence checks | The strongest trust signal | Rejected by the owner: the site should not depend on an approver |
| Stars, review and one reply (chosen) | Readers get context, and the accountant gets a fair answer | Written text can be unfair or defamatory, and needs a way to be reported |
| A small admin page for reports | Reported content can be removed in a few clicks | Not chosen by the owner for release 1 |
| Posts, follows, likes | More of a social network | More content to police. Not chosen for release 1 |

## Consequences and verification

- The requirement "client details are never public" changes: a review shows its author's first name and last initial (Proposed, PRD D21).
- Review rules are enforced on the server and covered by tests (TP16): who may review, one per pair, and that an accountant cannot review themselves.
- With no admin page, removing a fake profile or an abusive review means changing the database by hand. Who receives reports, how fast they act, and the removal steps must be written down before strangers use the site (PRD D22, STATUS G12).
- Known weaknesses to revisit before a public launch (PRD D09):
  - self-reviews through a second account;
  - a new accountant has no ratings and sorts below rated ones when sorted by rating;
  - a single unfair review weighs heavily while counts are small.
- The privacy and terms pages hold an outline only, and need legal review (PRD D10).
- Accept the proposed parts when the ratings feature is specified and its tests pass.

## Provenance and supersession

Original decision, recorded the day it was made. Supersedes nothing. It closes PRD decision D08 and partly decides D09.
