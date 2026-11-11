# AccApp design prototype

**Proposal for review, not an approved design.** [index.html](index.html) is a clickable prototype of the release 1 pages, shown in a phone browser or a desktop browser. It exists so the owner can review flows and decide open questions before any application code is written. It was created on 2026-11-10 for the MAUI plan and reworked on 2026-11-11 for the web platform ([ADR 005](../docs/adr/005-web-platform-blazor.md)). Design passes the same day added four visual directions, a website layout based on reference sites, and a set of user-experience changes; [DESIGN-DIRECTIONS](DESIGN-DIRECTIONS.md) explains them and recommends one.

What it is not:
- It does not authorize building these pages. Scope stays in the [PRD](../docs/PRD.md).
- It is not accessibility evidence. TP06 in the [TEST-PLAN](../docs/delivery/TEST-PLAN.md) still runs on the real site.
- Its four looks are proposals. The [DESIGN](../docs/DESIGN.md) visual system is not chosen yet.
- It is hand-written HTML and JavaScript, not Blazor. None of its code is meant to be reused in the site.

All people, companies and contact details in it are synthetic.

[brand-options.html](brand-options.html) is a separate page of fourteen proposed site names with logo sketches, six Taglish and eight Filipino (added 2026-11-11), for PRD decision D01. None is chosen, and no trademark, business-name or domain check has been made.

## Open it

Double-click `index.html`. It is one self-contained file: no install, no network, no build. The bar at the top is not part of the site. It switches:
- the frame (phone browser or desktop browser);
- the look (Modern, Ledger, Neighbourly or Clear);
- who is signed in (visitor, client, accountant, or a new accountant);
- online or offline;
- the theme;
- simulated failures;
- the notes panel, which lists the requirement IDs and open decisions for the current page.

The simulated browser bar shows the proposed address of each page, including the search and filter.

Optional URL settings, useful for sharing a specific state:

| Setting | Values |
|---|---|
| `device` | `desktop` (default is the phone browser) |
| `look` | `modern` (default), `ledger`, `neighbourly`, `clear` |
| `role` | `client`, `accountant`, `newAccountant` (default is a visitor) |
| `theme` | `light`, `dark` (default follows the system) |
| `offline` | `1` |
| `email` | `off` to start with email notifications turned off |
| `notes` | `0` to hide the notes panel |

Example: `index.html?device=desktop&look=neighbourly&role=accountant#/chats`.

## What it covers

| Pages | Requirements |
|---|---|
| Directory with a visitor introduction, search, service filter, paging, empty, no-match and load-failure states | R01, R02, R10, BR02; [F01](../specs/browse-accountants/spec.md) FT01–FT03, FT06, FT07 |
| Accountant profile, removed-profile link | R03, R11, BR01, BR03; F01 FT04, FT05 |
| Visitor types a message, signs in, and the text is kept; sign-in with role choice | R04, R06; [ADR 006](../docs/adr/006-web-sign-in.md) |
| Chat with send states: sending, sent, failed and retried, timed out then confirmed, connection lost | R06, R07, R10, BR04; TP03, TP08, TP09 |
| Accountant chats with unread marks; a simulated notification email; turning emails off | R07, R08, BR08 |
| Accountant profile editor: listing checklist, listed or not listed, format errors, unsaved-changes guard | R05, BR01, R11 |
| Account page and two-step account deletion | R04, R08, R09, BR06 |
| Portfolio highlights on profiles and in the editor | R12, BR13 |
| Ratings on cards and profiles, a "Highest rated" sort, the review form, and the accountant's reply | R13, R14, BR09–BR11 |
| Report on profiles, reviews and chats; block and unblock in a chat | R15, BR12 |
| Agreement at first sign-in; privacy, terms, about, help, for accountants, emails-off, not-found and error pages | R16–R18, R08 |
| A simulated reply email for clients | R08 |

## Proposals waiting for decisions

The prototype shows one option for each of these PRD [open decisions](../docs/PRD.md#open-decisions). Dashed boxes mark text that depends on a decision.

| Decision | What the prototype proposes |
|---|---|
| D03 | One role per account, chosen at first sign-in |
| D04 | Profile fields: title, city, career summary, experience (role, company, start and end month), services from a fixed list with a short description, email, phone, website. To be listed, a profile needs a name, title, summary, email, at least one service, and consent to show contact details |
| D04 | Cards and profiles show years of experience, counted from the earliest start month the accountant enters |
| D05 | Text search on name, title, city and service; one service filter (a menu on phones, buttons on wide screens); A–Z order by default, with a "Highest rated" option |
| D06 | One ongoing chat per client–accountant pair; 2,000-character messages |
| D21 | A review shows the client's first name and last initial |
| D23 | Up to six portfolio highlights, each with a title, type of client, the work and the result |
| D18 | At most one email per conversation until the accountant opens it; the email goes to the sign-in address and has no message text |
| D09 | A note on each profile saying AccApp does not verify accountants yet; its wording needs review |
| Visual system | Four looks to choose from, each with light and dark themes: Modern (recommended, and the default), Ledger, Neighbourly and Clear. Bottom navigation on phones; on wide screens a website header, a card grid and a footer. Details in [DESIGN-DIRECTIONS](DESIGN-DIRECTIONS.md) |

After review, record each decision in the PRD or DESIGN (not here), then update the prototype or mark it superseded.

## Checks run

On 2026-11-11, agent-observed, against this file after the design pass:
- `node --check` on its script: no syntax errors.
- A scripted walk-through in headless Microsoft Edge 157 on Windows 11 drove the pages through the DOM, once per look. 132 checks passed in each of the four looks, covering every flow in the table above. The test script was kept outside the repository.
- Contrast was computed for every look and theme; all pairs pass WCAG AA ([table](DESIGN-DIRECTIONS.md#contrast)).
- Screenshots of the looks on phone and desktop frames, in light and dark themes and at narrow window widths, were reviewed.

Earlier the same day the web rework passed 56 checks, and the 2026-11-10 version passed 45 checks for the app flows of that plan.

Not checked: other browsers, real phones, screen readers, keyboard-only use beyond focus placement, and true browser zoom.
