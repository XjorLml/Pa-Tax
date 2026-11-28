# Tax Buddy design prototype

**Interactive design artifact; product acceptance remains pending.** [index.html](index.html) shows release 1 pages in a phone or desktop browser. The user's design brief of 2026-11-11 delegates choosing the new name, logo and visual direction. That refresh replaced AccApp with Kuwenta and added original SVG branding, a paper/forest palette, editorial typography, clearer directory hierarchy and responsive refinements. On 2026-11-12 the user instructed renaming the app to Tax Buddy; the look is unchanged and the logo wordmark was redrawn ([F02](../specs/brand-design-refresh/spec.md)). On 2026-11-18 the user chose a new logo, a speech bubble holding a T, which replaced the ledger symbol and that wordmark ([BD12](../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)). On 2026-11-19 the Tax Buddy look took one typeface, Quicksand, in place of serif headings and a system sans body ([BD13](../specs/brand-design-refresh/spec.md#one-typeface-2026-11-19-bd13)); its font files are in `assets/fonts`, so the prototype still needs no network. [DESIGN](../docs/DESIGN.md#visual-system) owns the current visual contract.

The prototype was created on 2026-11-10 for the MAUI plan, then reworked for the web on 2026-11-11 ([ADR 005](../docs/adr/005-web-platform-blazor.md)). Earlier passes and their four visual directions remain in [DESIGN-DIRECTIONS](DESIGN-DIRECTIONS.md) as history. Tax Buddy is now the default; the earlier looks are comparison options.

**Origin of the design (owner decision, 2026-11-16):** every page of the real site follows its screen here: layout, components, wording and states. Where the site can't, because it works without script, a linked feature isn't built yet, or the owner decided otherwise, the deviation is recorded in [DESIGN](../docs/DESIGN.md) ([CLAUDE.md](../CLAUDE.md)). The site's pages were aligned on 2026-11-16 ([F02 BD10](../specs/brand-design-refresh/spec.md#application-aligned-to-the-prototype-2026-11-16-bd10)).

What it is not:
- It does not authorize building these pages. Scope stays in the [PRD](../docs/PRD.md).
- It is not accessibility evidence. TP06 in the [TEST-PLAN](../docs/delivery/TEST-PLAN.md) still runs on the real site.
- Its current look is applied under the delegated brief; unresolved behavior and release acceptance remain open. Earlier looks are historical alternatives.
- It is hand-written HTML and JavaScript, not Blazor. None of its code is meant to be reused in the site.

All people, companies and contact details in it are synthetic.

[brand-options.html](brand-options.html) preserves fourteen earlier name sketches. The current Tax Buddy logo, compact icons and favicon are in [assets](assets/README.md); the three logo proposals of 2026-11-18, one of which was adopted, are in [logo-directions](logo-directions/README.md). Tax Buddy is not one of the fourteen sketches; the closest is Buwis Buddy. Name/domain availability has not been checked; domain selection remains PRD D01.

## Open it

Double-click `index.html`, keeping `taxbuddy.css`, `service-workspace.js` and the `assets` folder beside it. Everything is local: no install, network or build. The initial frame follows the window width (desktop from 900 px), and can be overridden. The bar at the top is not part of the site. Its simulation controls start collapsed on small screens. It switches:
- the frame (phone browser or desktop browser);
- the look (Tax Buddy, or the earlier Modern, Ledger, Neighbourly and Clear directions);
- the palette of the Tax Buddy look: Paper & Forest, chosen by the user on 2026-11-13, or four proposals added on 2026-11-12 for comparison, kept as history ([DESIGN-DIRECTIONS](DESIGN-DIRECTIONS.md#palette-options));
- who is signed in (visitor, client, accountant, or a new accountant);
- the picture design: how the pencil on the Account avatar changes the profile picture. E (pick, preview, save) is the current design, chosen by the owner on 2026-11-19; A is what the site shows without script; B, C and D were not chosen and are kept for comparison ([F06](../specs/profile-pictures/spec.md#picture-design-options-2026-11-19));
- the workspace navigation: "Breadcrumb only" is the current design. "Side list" is a proposal of 2026-11-24, not adopted. Its parts:
  - from 960 px, a list of the viewer's clients (or accountants) and their services beside the client and service pages, like the conversation list beside a chat;
  - a breadcrumb that names the client and the service;
  - on phones, Back from a service goes to its client;
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
| `picture` | `e` (default, current), `a` (used without script), `b`, `c`, `d` (not chosen) |
| `wsnav` | `current` (default, breadcrumb only), `side` (side list, proposed 2026-11-24, not adopted) |
| `opt` | A design option per page, for example `directory-a` or `directory-a,chats-b`. Client pages: `directory`, `profile`, `chats`, `myacc`, `details`. Accountant pages: `editor`, `achats`, `clients`, `work`. Options `a`, `b`, `c`. Left out, every page shows the current design; for `details` that's B, chosen on 2026-11-25. See the design options for [clients](#design-options-for-the-clients-flow) and [accountants](#design-options-for-the-accountants-flow) |
| `embed` | `1` shows the simulated browser alone, without the bar or notes; used by `options.html` |
| `theme` | `light`, `dark` (default follows the system) |
| `offline` | `1` |
| `email` | `off` to start with email notifications turned off |
| `notes` | `0` to hide the notes panel |

Example: `index.html?device=desktop&look=taxbuddy&theme=dark&role=accountant#/chats`.

## Design options for the client's flow

[options.html](options.html) (2026-11-24, at the user's request) compares design options for the pages a client uses after signing in, in this order:
1. Directory
2. Accountant profile
3. Chats
4. My accountants

Open it directly or with **Design options** in the prototype bar. For each page it shows the current design and the options the user kept from a one-line list. Every option is the real prototype in a frame (`index.html?embed=1&opt=…`), signed in as the client Maria Santos, so it can be clicked.

How it's laid out:
- A step bar and Previous/Next move between the pages.
- Tabs pick Current or an option, and **Compare all** shows them side by side at a smaller size.
- Phone/Desktop and a theme switch change the frames.
- Each option has notes: the idea, what changes, the trade-off, how it works without script, and phones where they differ.
- The address keeps the page, option, frame and theme (`options.html?page=chats&option=a&frame=phone`), so a link can point to one option.

| Page | Options (none adopted) |
|---|---|
| Directory | A · Slim search, rows; B · Filter column; C · Welcome back |
| Accountant profile | A · Section links; B · Actions in the header |
| Chats | A · Details panel; B · Focused conversation; C · Sorted list |
| My accountants | A · Next steps first; B · All services list; C · List and details |

These are proposals. Without `opt`, the prototype shows the current design, which the site follows. When the owner chooses an option for a page:
- the choice is recorded in [DESIGN](../docs/DESIGN.md) and the feature spec;
- the prototype's default changes to it;
- the other options stay, marked "not chosen", as with the Picture design options.

None of them adds a feature: they rearrange data the pages already show. Directory C and Chats A show a client's own data in new places; for Directory C that's a public page, so it would be drawn only for that client and not cached publicly.

## Client details

On 2026-11-25 the user asked that clients can store and edit their basic information and profile information. Today a client has only the name and email from Google and an optional picture. The client's flow on `options.html` has a fifth step, **My details**. The user decided:
- **Fields:** a name (starting from the Google name), phone and city; a business name, type (sole proprietor, partnership, corporation, freelancer or self-employed, or not registered yet) and industry. The email stays the Google address and isn't shown. There is a note not to add a TIN or ID numbers.
- **Who sees them:** the accountants the client contacts, through a conversation or a service, as with the client's picture (R24). Never public (R11).
- **Where accountants see them:** an "About Maria" card on their page for that client, and inside a service request.

| Option | What it shows |
|---|---|
| **B · Edit on Account (chosen 2026-11-25, the default)** | A "Your details" card on Account, with Edit details opening the form in place (`/account?edit=details`), Save details and Cancel |
| A · My details page (not chosen) | A "My details" row on Account opens a page with the form, and a line naming the accountants who can see it (`opt=details-a`) |
| C · What accountants see (not chosen) | The page from A with a preview of the card accountants see (`opt=details-c`) |

**B is the default.** The user said "implement the edit on account in the prototype", so the prototype shows B without any `opt`. It also shows the accountant's side: `index.html?role=accountant#/clients/u-maria`, or `#/work/e2` for a request. The choice is recorded as [F02 BD15](../specs/brand-design-refresh/spec.md#client-details-on-account-2026-11-25-bd15), and the prototype checker covers it. **Jump to → Client details** opens the form.

**Built on the site, 2026-11-25.** The user then said "implement to site". It became [PRD R25](../docs/PRD.md#requirements) and [F17](../specs/client-details/spec.md), which resolved conflict C05. The prototype was aligned with the site:
- the name takes up to 100 characters;
- a phone needs at least 7 digits;
- the sharing line names only the accountants with a service.

## Design options for the accountant's flow

On 2026-11-25 the user asked for the same for accountants. **Flow → Accountant** in [options.html](options.html) (or `options.html?flow=accountant`) shows the accountant's pages, signed in as Andrea Villanueva. The steps follow an accountant's progress: get listed, get messages, get requests, do the work. The user kept every option proposed.

| Step | Page | Options |
|---|---|---|
| 1 | My profile (the editor) | **B · Preview beside the form (chosen 2026-11-25, the default)**; A · Section menu and C · Compact top, Save in view (not chosen) |
| 2 | Chats | A · Client panel; B · Waiting for your reply; C · All / Unread tabs |
| 3 | Clients | A · Requests first; B · Services by status; C · Client table |
| 4 | Service progress | A · Section links; B · Documents per task; C · Updates first |

The same rules apply as for the client's options:
- These are proposals, and the current design stays the default.
- Each option keeps to data and actions the accountant already has. Clients A answers requests with the same Accept and Decline as the service page. Service progress B uses the existing link between a document and a task.
- The accountant's Chats options use their own setting (`achats`). The client's Chats options don't apply to accountants.

**My profile: B is the default.** The user said "in the My profile implement Option B: Preview beside the form in the prototype". Without any `opt`, the editor now shows the directory card, as last saved, beside the form. On phones, and on desktop pages narrower than 860 px, the card is in a closed disclosure above the form. On the options page, B is "Current". "Before" shows the one long form the site still has (`opt=editor-current`). The choice is recorded as [F02 BD16](../specs/brand-design-refresh/spec.md#my-profile-preview-beside-the-form-2026-11-25-bd16), and the prototype checker covers it. The other three pages still wait for a choice.

**Built on the site, 2026-11-25.** The user then said "implement to site". It was built as [F05 PE09](../specs/profile-editing/spec.md), with the same markup and class names. The panel's label became an `h2` in both.

## Page titles options

On 2026-11-26 the user sent screenshots of Clients and a client's page and said: "create recommended designs for these, it shows redundant titles". Every workspace page stacks four labels:
- a breadcrumb that ends with the page itself (on Clients it's only "Clients");
- a small heading naming the page type ("Clients", "Client services", "Service progress");
- the eyebrow "Your private workspace";
- a large second title, which is a slogan on the lists and the name elsewhere.

On phones, the app bar repeats the page type above the large title. **Flow → Accountant → 5 Page titles** in [options.html](options.html) shows three fixes. Its **View** switch picks the page: Clients, a client, a service, My accountants, a client's service, or Request a service.

| Option | What it shows |
|---|---|
| Current | The four labels above |
| **A · Name as the title (recommended)** | A breadcrumb of the pages above only (none on the lists). Then the name as the only, large title, and the page's sentence, which says who can see the work. On phones, the app bar shows the name, and Back goes to the page above (`opt=heads-a`) |
| B · Compact header bar | Back to the page above, the name, and a pill such as "Private to you and Maria"; no sentence (`opt=heads-b`) |
| C · Trail as the title | "Clients / Maria Santos / Bookkeeping" on one line, with the name as the h1 and the pages above as muted links (`opt=heads-c`) |

All three drop the eyebrow and the slogans. The current design stays the default. It's a DESIGN change with no PRD effect: the site builds these headers in `WorkFrame` and `WorkIntro`, so a chosen option would be built there. On the site, the sections' headings would move up a level so none is skipped. The Side list proposal (Workspace navigation) keeps its own header.

## Accountant dashboard options

**Archived on 2026-11-26.** The same day the user said "archive dashboard design in the prototype for now", and no option was chosen.
- On [options.html](options.html), the Dashboard left the accountant's steps, which are back to four. It's listed under **Archived** in the header, with a note saying so.
- Every option, state and check still works, from there or with `opt=dash-a|b|c`.
- The two extra accountants (Francis Lim, Elaine Navarro) stay under **Signed in as**.

The user had asked for an accountant dashboard as design options. It's the first page after sign-in, and it answers "what needs me today?" and "how is my listing doing?". These were proposals only: the current pages stay the default, and the site and the PRD are unchanged.

| Option | What it shows | Where it lives |
|---|---|---|
| Current | No dashboard; the frame shows Clients, the nearest overview | Sign-in returns to the page it started from |
| A · "Needs you" list | One list, most urgent kind first: finishing the listing until listed; new requests with Accept, Decline and View request; clients who wrote last, with Reply; reviews without a reply. Then one line about the rest of the work | A new `/dashboard` (`opt=dash-a`) |
| B · Tiles and sections | Four tiles (new requests, unread messages, active services, rating), each jumping to its section. Then sections for the listing, requests, services by status, messages and reviews | A new `/dashboard` (`opt=dash-b`) |
| C · Overview on Clients | "Needs you" beside three small cards (listing, service counts, reviews), then the client cards. It replaces the Clients introduction and tiles | The top of Clients, `/workspace` (`opt=dash-c`) |

- **Navigation:**
  - A and B put Dashboard first in the site bar, current on `/dashboard`;
  - on phones, Dashboard takes the Accountants tab's place, because five tabs don't fit 320 px;
  - C changes nothing; Clients stays current.
- **States:** the options page's **State** switch shows each option as:
  - Andrea;
  - a busy accountant: Elaine Navarro, with three requests, three chats waiting and two reviews without a reply;
  - a new, unlisted accountant: Sam Aquino;
  - a listed accountant with no clients: Francis Lim;
  - a failed load (`wsdata=error`).

  In the full prototype, **Signed in as** has the two new accountants. **Workspace data → Load fails** shows the failed load.
- **Data:** only what the site has today: the listing checklist, requests, service statuses with their last update, unread and unanswered chats, and reviews with their replies.
- **Left out, because each needs a PRD decision:**
  - profile views and search appearances;
  - earnings or fees;
  - reply-time figures;
  - "new since your last visit";
  - due dates;
  - trends.
- **Overlap:** C settles the Clients options A–C if chosen; A and B leave Clients as it is.
- **Scope:** a dashboard isn't in the PRD. Choosing one adds a page and a navigation item, and changes where sign-in lands. That needs an owner decision recorded in the PRD or DESIGN, and in the SDD's navigation table.

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
| Profile picture: a pencil on the Account and editor avatars; on Account it opens the file chooser, previews the photo and saves it on Save; the picture is kept only in the page | R24; [F06](../specs/profile-pictures/spec.md) PP09, PP10 |
| Portfolio highlights on profiles and in the editor | R12, BR13 |
| Ratings on cards and profiles, a "Highest rated" sort, the review form, and the accountant's reply | R13, R14, BR09–BR11 |
| Report on profiles, reviews and chats; block and unblock in a chat | R15, BR12 |
| Agreement at first sign-in; privacy, terms, about, help, for accountants, emails-off, not-found and error pages | R16–R18, R08 |
| A simulated reply email for clients | R08 |

## Proposals waiting for decisions

The prototype shows one option for each of these PRD [open decisions](../docs/PRD.md#open-decisions). Dashed boxes mark text that depends on a decision. D04 and D05 were adopted provisionally on 2026-11-12 and are recorded in the PRD; their rows stay here to describe what the prototype shows.

| Decision | What the prototype proposes |
|---|---|
| D03 | One role per account, chosen at first sign-in |
| D04 | Adopted for now (2026-11-12). Profile fields: title, city, career summary, experience (role, company, start and end month), services from a fixed list with a short description, email, phone, website. To be listed, a profile needs a name, title, summary, email, at least one service, and consent to show contact details |
| D04 | Adopted for now (2026-11-12). Cards and profiles show years of experience, counted from the earliest start month the accountant enters |
| D05 | Adopted for now (2026-11-12). Text search on name, title, city and service; one service filter (a menu on phones, buttons on wide screens); A–Z order by default, with a "Highest rated" option. Pages of 6; page size is still open |
| D06 | One ongoing chat per client–accountant pair; 2,000-character messages |
| D21 | A review shows the client's first name and last initial |
| D23 | Up to six portfolio highlights, each with a title, type of client, the work and the result |
| D18 | At most one email per conversation until the accountant opens it; the email goes to the sign-in address and has no message text |
| D09 | A note on each profile saying Tax Buddy does not verify accountants; its wording needs review |
| Visual system | The Tax Buddy look (named Kuwenta until 2026-11-12) is applied under the delegated design brief; four earlier looks remain for comparison. Current tokens and stack mapping belong in [DESIGN](../docs/DESIGN.md#visual-system). Owner walkthrough and production accessibility remain pending |

After review, record each decision in the PRD or DESIGN (not here), then update the prototype or mark it superseded.

A 2026-11-12 agent read-through compared this prototype with the PRD, SDD and DESIGN ([review record](../docs/delivery/evidence/2026-11-12-prototype-scope-review.md)). It opened two conflicts, [C03 and C04](../docs/README.md#history-and-unresolved-conflicts): the directory here filters, sorts and pages in the page while the planned directory is a static page with a submitted form, and the offline messages shown inside forms here cannot be drawn by the planned server-connected pages. The prototype was not changed by that review.

C03 was resolved later the same day by the user's instruction, and the directory now behaves like the planned static page:
- typing changes nothing until Search is pressed (or Enter);
- on wide screens a service button submits the form at once, and on phones the service menu applies with Search;
- sort and page are links, and "Show more" is gone;
- search, service, sort and page all appear in the simulated address;
- cards no longer show a per-user "Chatting" tag.

The change is recorded with its [checks](../docs/delivery/evidence/2026-11-12-c03-directory.md). C04 remains open, so the directory's offline state with a retry button is unchanged.

## Checks run before this refresh

The observations below predate F02 and are preserved as history. They do not automatically apply to the refreshed artifact. On 2026-11-11, agent-observed, against the file after the earlier design pass:
- `node --check` on its script: no syntax errors.
- A scripted walk-through in headless Microsoft Edge 157 on Windows 11 drove the pages through the DOM, once per look. 132 checks passed in each of the four looks, covering every flow in the table above. The test script was kept outside the repository.
- Contrast was computed for every look and theme; all pairs pass WCAG AA ([table](DESIGN-DIRECTIONS.md#contrast)).
- Screenshots of the looks on phone and desktop frames, in light and dark themes and at narrow window widths, were reviewed.

Earlier the same day the web rework passed 56 checks, and the 2026-11-10 version passed 45 checks for the app flows of that plan.

Not checked: other browsers, real phones, screen readers, keyboard-only use beyond focus placement, and true browser zoom.

## Verify the current prototype

Use the dependency-free [browser checker](../tools/check_prototype.mjs) as documented in the [tools guide](../tools/README.md#check-the-prototype). The prior [F02 execution record](../docs/delivery/evidence/2026-11-11-kuwenta-refresh.md) records its 78 passing Edge prototype checks, 34 passing contrast pairs, source fingerprints and screenshot observations; these results describe that earlier artifact, under its then name Kuwenta. The checks rerun after the rename are in the [2026-11-12 record](../docs/delivery/evidence/2026-11-12-tax-buddy-rename.md), and the palette options have their own [record](../docs/delivery/evidence/2026-11-12-palette-options.md), as does the [directory change for C03](../docs/delivery/evidence/2026-11-12-c03-directory.md). [STATUS](../docs/delivery/STATUS.md) tracks the remaining acceptance. These checks use only synthetic data and cannot establish Blazor, database, provider or email behavior.

## Client services and progress prototype

The user authorized the documentation/prototype expansion on 2026-11-11 ([F03](../specs/client-services/spec.md), PRD R19–R23). Use **Jump to → Clients and services**, **My accountants**, or **Service progress and files**, or open:

- `index.html?role=accountant&device=desktop#/workspace`
- `index.html?role=client&device=phone#/workspace`
- `index.html?role=accountant#/work/e1`

The confirmed save/request/accept model is: client adds an accountant privately from their profile → requests an offered service → accountant opens Clients and accepts/declines → accountant adds/updates checklist tasks and shared notes → client reads progress. An accountant can add a service to an already accepted client relationship. Saving or chatting alone does not expose a client in the accountant list. The owner confirmed private saving followed by a service request and accountant acceptance. Accountant-added services, task editing and other D24/D25 details remain prototype proposals.

Open a service to attach a generated PDF/CSV/TXT sample to the service or one of its tasks. Both participants can download samples; proposed uploader-only removal requires confirmation. The selector includes oversized and unsupported samples to exercise rejection. Review controls simulate save/upload failure and list loading/empty/error. No native file picker, real document reading, external upload or storage service is used. Download produces a synthetic local fixture. The 5 MiB/type limits and removal behavior are proposals (D26–D28).

All service data and per-actor drafts live only in this loaded page and reset on reload. Upload selections survive simulated failure/navigation; switching actors interrupts pending upload. Changing personas demonstrates guard behavior, not security. Production authorization, storage, concurrency, retention and deletion remain unimplemented. Earlier F02 evidence above is historical for its fingerprinted source; new results belong to the F03 execution record linked from STATUS.

Current F03 observations: [142 passing Edge checks, source fingerprints and screenshots](../docs/delivery/evidence/2026-11-11-client-services.md). Existing flows were rerun; results do not establish production authorization or real file storage.
