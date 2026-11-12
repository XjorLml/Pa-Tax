# Tax Buddy design prototype

**Interactive design artifact; product acceptance remains pending.** [index.html](index.html) shows release 1 pages in a phone or desktop browser. The user's design brief of 2026-11-11 delegates choosing the new name, logo and visual direction. That refresh replaced AccApp with Kuwenta and added original SVG branding, a paper/forest palette, editorial typography, clearer directory hierarchy and responsive refinements. On 2026-11-12 the user instructed renaming the app to Tax Buddy; the look is unchanged and the logo wordmark was redrawn ([F02](../specs/brand-design-refresh/spec.md)). [DESIGN](../docs/DESIGN.md#visual-system) owns the current visual contract.

The prototype was created on 2026-11-10 for the MAUI plan, then reworked for the web on 2026-11-11 ([ADR 005](../docs/adr/005-web-platform-blazor.md)). Earlier passes and their four visual directions remain in [DESIGN-DIRECTIONS](DESIGN-DIRECTIONS.md) as history. Tax Buddy is now the default; the earlier looks are comparison options.

What it is not:
- It does not authorize building these pages. Scope stays in the [PRD](../docs/PRD.md).
- It is not accessibility evidence. TP06 in the [TEST-PLAN](../docs/delivery/TEST-PLAN.md) still runs on the real site.
- Its current look is applied under the delegated brief; unresolved behavior and release acceptance remain open. Earlier looks are historical alternatives.
- It is hand-written HTML and JavaScript, not Blazor. None of its code is meant to be reused in the site.

All people, companies and contact details in it are synthetic.

[brand-options.html](brand-options.html) preserves fourteen earlier name sketches. The current Tax Buddy logo, compact icons and favicon are in [assets](assets/README.md). Tax Buddy is not one of the fourteen sketches; the closest is Buwis Buddy. Name/domain availability has not been checked; domain selection remains PRD D01.

## Open it

Double-click `index.html`, keeping `taxbuddy.css`, `service-workspace.js` and the `assets` folder beside it. Everything is local: no install, network or build. The initial frame follows the window width (desktop from 900 px), and can be overridden. The bar at the top is not part of the site. Its simulation controls start collapsed on small screens. It switches:
- the frame (phone browser or desktop browser);
- the look (Tax Buddy, or the earlier Modern, Ledger, Neighbourly and Clear directions);
- the palette of the Tax Buddy look: Paper & Forest (current), or four proposals added on 2026-11-12 for comparison ([DESIGN-DIRECTIONS](DESIGN-DIRECTIONS.md#palette-options)). No proposal is chosen;
- who is signed in (visitor, client, accountant, or a new accountant);
- online or offline;
- the theme;
- simulated failures;
- the notes panel, which lists the requirement IDs and open decisions for the current page;
- workspace data, work-save failures and service-upload failures (review controls only).

The simulated browser bar shows the proposed address of each page, including the search and filter.

Optional URL settings, useful for sharing a specific state:

| Setting | Values |
|---|---|
| `device` | `desktop`, `phone` (default follows initial window width) |
| `look` | `taxbuddy` (default), `modern`, `ledger`, `neighbourly`, `clear` |
| `palette` | `forest` (default, current), `ink`, `lagoon`, `plum`, `marigold`; applies to the `taxbuddy` look only |
| `role` | `client`, `accountant`, `newAccountant` (default is a visitor) |
| `theme` | `light`, `dark` (default follows the system) |
| `offline` | `1` |
| `email` | `off` to start with email notifications turned off |
| `notes` | `0` to hide the notes panel |

Example: `index.html?device=desktop&look=taxbuddy&theme=dark&role=accountant#/chats`.

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
| D09 | A note on each profile saying Tax Buddy does not verify accountants; its wording needs review |
| Visual system | The Tax Buddy look (named Kuwenta until 2026-11-12) is applied under the delegated design brief; four earlier looks remain for comparison. Current tokens and stack mapping belong in [DESIGN](../docs/DESIGN.md#visual-system). Owner walkthrough and production accessibility remain pending |

After review, record each decision in the PRD or DESIGN (not here), then update the prototype or mark it superseded.

A 2026-11-12 agent read-through compared this prototype with the PRD, SDD and DESIGN ([review record](../docs/delivery/evidence/2026-11-12-prototype-scope-review.md)). It opened two conflicts, [C03 and C04](../docs/README.md#history-and-unresolved-conflicts): the directory here filters, sorts and pages in the page while the planned directory is a static page with a submitted form, and the offline messages shown inside forms here cannot be drawn by the planned server-connected pages. The prototype was not changed.

## Checks run before this refresh

The observations below predate F02 and are preserved as history. They do not automatically apply to the refreshed artifact. On 2026-11-11, agent-observed, against the file after the earlier design pass:
- `node --check` on its script: no syntax errors.
- A scripted walk-through in headless Microsoft Edge 157 on Windows 11 drove the pages through the DOM, once per look. 132 checks passed in each of the four looks, covering every flow in the table above. The test script was kept outside the repository.
- Contrast was computed for every look and theme; all pairs pass WCAG AA ([table](DESIGN-DIRECTIONS.md#contrast)).
- Screenshots of the looks on phone and desktop frames, in light and dark themes and at narrow window widths, were reviewed.

Earlier the same day the web rework passed 56 checks, and the 2026-11-10 version passed 45 checks for the app flows of that plan.

Not checked: other browsers, real phones, screen readers, keyboard-only use beyond focus placement, and true browser zoom.

## Verify the current prototype

Use the dependency-free [browser checker](../tools/check_prototype.mjs) as documented in the [tools guide](../tools/README.md#check-the-prototype). The prior [F02 execution record](../docs/delivery/evidence/2026-11-11-kuwenta-refresh.md) records its 78 passing Edge prototype checks, 34 passing contrast pairs, source fingerprints and screenshot observations; these results describe that earlier artifact, under its then name Kuwenta. The checks rerun after the rename are in the [2026-11-12 record](../docs/delivery/evidence/2026-11-12-tax-buddy-rename.md), and the palette options have their own [record](../docs/delivery/evidence/2026-11-12-palette-options.md). [STATUS](../docs/delivery/STATUS.md) tracks the remaining acceptance. These checks use only synthetic data and cannot establish Blazor, database, provider or email behavior.

## Client services and progress prototype

The user authorized the documentation/prototype expansion on 2026-11-11 ([F03](../specs/client-services/spec.md), PRD R19–R23). Use **Jump to → Clients and services**, **My accountants**, or **Service progress and files**, or open:

- `index.html?role=accountant&device=desktop#/workspace`
- `index.html?role=client&device=phone#/workspace`
- `index.html?role=accountant#/work/e1`

The confirmed save/request/accept model is: client adds an accountant privately from their profile → requests an offered service → accountant opens Clients and accepts/declines → accountant adds/updates checklist tasks and shared notes → client reads progress. An accountant can add a service to an already accepted client relationship. Saving or chatting alone does not expose a client in the accountant list. The owner confirmed private saving followed by a service request and accountant acceptance. Accountant-added services, task editing and other D24/D25 details remain prototype proposals.

Open a service to attach a generated PDF/CSV/TXT sample to the service or one of its tasks. Both participants can download samples; proposed uploader-only removal requires confirmation. The selector includes oversized and unsupported samples to exercise rejection. Review controls simulate save/upload failure and list loading/empty/error. No native file picker, real document reading, external upload or storage service is used. Download produces a synthetic local fixture. The 5 MiB/type limits and removal behavior are proposals (D26–D28).

All service data and per-actor drafts live only in this loaded page and reset on reload. Upload selections survive simulated failure/navigation; switching actors interrupts pending upload. Changing personas demonstrates guard behavior, not security. Production authorization, storage, concurrency, retention and deletion remain unimplemented. Earlier F02 evidence above is historical for its fingerprinted source; new results belong to the F03 execution record linked from STATUS.

Current F03 observations: [142 passing Edge checks, source fingerprints and screenshots](../docs/delivery/evidence/2026-11-11-client-services.md). Existing flows were rerun; results do not establish production authorization or real file storage.
