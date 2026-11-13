# Tax Buddy UI and UX standards

**Authority:** visual and interaction standards. [PRD](PRD.md) owns feature scope. [SDD](SDD.md) owns technical implementation. Status: visual direction applied to the prototype under the user's delegated design brief, 2026-11-11 (then named Kuwenta); renamed Tax Buddy by the user's instruction, 2026-11-12. Palette chosen by the user, 2026-11-13: Paper & Forest. Applied to the application's directory and profile pages in F01, 2026-11-13. Product and release acceptance remain pending.

## Scope notice

Applies to the release 1 pages in the [SDD navigation table](SDD.md#navigation-and-screen-state):
- directory
- profile
- sign-in
- conversation list
- chat thread
- my profile editor
- account settings
- private client/added-accountant lists, service requests, checklists, progress and service documents ([F03](../specs/client-services/spec.md))

It is one responsive website, used in phone and desktop browsers.

The clickable [design prototype](../prototypes/README.md) uses Tax Buddy as its default direction. The user authorized executing a brief that delegates choosing the name, creating the logo and improving the prototype, and on 2026-11-12 instructed renaming the result from Kuwenta to Tax Buddy; [F02](../specs/brand-design-refresh/spec.md) records the scope and observable outcomes. The four earlier directions remain available for comparison and are preserved in [DESIGN-DIRECTIONS](../prototypes/DESIGN-DIRECTIONS.md). Behavior depending on D03–D06 and D18 remains a proposal. A prototype is not evidence of production accessibility or authorization to expand the feature scope.

## Visual system

| Role | Value / semantic resource | Rationale and verification |
|---|---|---|
| Brand identity | Tax Buddy: a ledger with a conversation corner and an original geometric wordmark. [Assets and usage](../prototypes/assets/README.md). Domain remains PRD D01 | Original vector paths, no downloaded font or artwork. Symbol applied under the delegated brief; name set by the user on 2026-11-12 and the wordmark redrawn to match. No name/domain clearance or release approval claimed |
| Color | Warm paper and forest green; exact semantic values below, implemented in [taxbuddy.css](../prototypes/taxbuddy.css) | Error and unread states retain text/icons; observed contrast belongs in the dated evidence |
| Typography | Georgia/Times serif headings; Segoe UI/system sans body. Body 15 px; supporting text 12–14 px; section headings 24–30 px; hero 31–52 px | System fonts, no downloads. Logo paths do not depend on installed fonts. Browser zoom and screen-reader acceptance remain TP06 |
| Layout | Spacing 4/8/12/16/24/32/48 px; 1040 px content including 32 px side padding; two desktop card columns. Container breakpoints 760/480 px, compact phone chrome at 420 px. Buttons normally 44 px; phone controls at least 44 px | Narrow widths are observations, not closure of D15. Fixed device frames are prototype controls, not a production layout requirement |
| Motion | Pressed, loading and sending feedback only | Respect the browser's reduced-motion setting |

Centralize the variables in one stylesheet. Decided on 2026-11-11: Tailwind CSS for styling, with these variables as its theme and no component library ([SDD](SDD.md#technology-stack)). Document exceptions instead of allowing each page to invent colors, spacing, and status symbols.

### Tax Buddy palette

| Semantic token | Light | Dark |
|---|---|---|
| `app-bg` | `#F7F7F2` | `#111C18` |
| `surface` | `#FFFFFF` | `#192720` |
| `surface-2` | `#ECEFE8` | `#24352B` |
| `text` | `#1E332C` | `#EEF1E7` |
| `muted` | `#556259` | `#B3C0B3` |
| `primary`, `link`, `focus` | `#245C46` | `#B7D8A4` |
| `on-primary` | `#FFFFFF` | `#162D20` |
| `primary-soft` / `on-soft` | `#E5EEE4` / `#254C36` | `#2B402C` / `#D8EACD` |
| `border` (decorative) / `ctl` (input edge) | `#D7DED4` / `#7C8B80` | `#3A4D3F` / `#7E9383` |
| `danger` / `danger-soft` | `#A32925` / `#FCEBE9` | `#FFB4A9` / `#40251F` |
| `warn` / `warn-soft` | `#745010` / `#FAEED4` | `#ECCC8D` / `#3D321C` |
| `hero` / `hero-art` | `#E9EDE1` / `#D9E1CD` | `#223328` / `#304631` |

Palette proposals, 2026-11-12: the prototype's Palette control also offers Ink & Ledger Blue, Lagoon Teal, Plum & Blush and Marigold & Charcoal for comparison ([F02 BD08](../specs/brand-design-refresh/spec.md#palette-options-2026-11-12), [DESIGN-DIRECTIONS](../prototypes/DESIGN-DIRECTIONS.md#palette-options)). On 2026-11-13 the user chose Paper & Forest ("for pallete go for paper and forest"), so the table above is the palette. The four proposals stay in the prototype stylesheet as history and are not used by the application.

### Stack and component mapping

The prototype uses plain CSS as a buildless design artifact. At scaffold, map the semantic roles above to Tailwind theme values and reusable Razor components: SiteHeader, BrandMark, DirectorySearch, AccountantCard, ProfileSummary, ContactPanel, MessageComposer, StatusBanner and SiteFooter. Component names are planning labels, not existing source. Keep public directory/profile HTML suitable for static rendering and the chat/editor boundary as defined in SDD; adding JavaScript interactivity here does not change those application contracts. Reuse SVG assets from `wwwroot` without an image service or component library.

As built in F01, 2026-11-13 ([evidence](delivery/evidence/2026-11-13-f01-browse-accountants.md)):
- **Tokens:** the palette table above is the only colour source, in `src/TaxBuddy.Web/Styles/app.css`. Tailwind 4's own colour palette is switched off, so a class such as `bg-surface` or `text-muted` must name a semantic role. Dark mode follows the browser's `prefers-color-scheme` setting.
- **Breakpoints:** 480 px (`sm`) and 760 px (`md`), as in the layout row above, plus 1040 px for the content width.
- **Razor components:** `BrandLogo` (picks the light or dark logo file without script), `Icon` (the prototype's line icons, hidden from screen readers), `SeoHead` (title, description, canonical address), `StateMessage` (empty, no match, unavailable and error states), `AccountantCardItem` (one link per card, covering the whole card) and `Pager`. The layout supplies the header, skip link and footer.
- **The directory form's source order matters:** text box, service menu, Search, then service buttons. Comments in `Styles/app.css` and the SDD explain why.
- **Grid columns on the hero use `minmax(0, 1fr)`,** so the search form fits at 320 px.

Directory pages lead with a short introduction and labelled search, followed by comparable cards. The desktop introduction uses an original decorative ledger illustration rather than fabricated statistics or portraits. Cards show initials, career title, rating/count, location/experience, services, summary and profile action in a consistent order. Profiles keep a clearly separated contact panel; mobile keeps its contact action below the scrolling content. Serif display type is limited to brand/headings; controls, forms, messages and dense facts stay in the body face.

## Screen and component states

| Component / workflow | States to design | Observable behavior |
|---|---|---|
| Directory list and search (R01, R02) | Ready, empty directory, no match, load error | A static page with a plain search form ([SDD](SDD.md#navigation-and-screen-state), C03 resolved 2026-11-12): search and the service menu apply when the form is submitted, and on wide screens each service button submits it at once. Sort and page are links. Search, service, sort and page live in the address, so Back and shared links keep them. No in-page loading state, and the prototype's brief placeholder stands in for the page load. A full page load (first visit, reload, Search) shows the browser's own indicator. Links inside the site use Blazor's enhanced navigation and currently show no indicator; see [F01 polish P1](../specs/browse-accountants/tasks.md#polish-follow-ups). Cards look the same to every viewer. Empty and "no match" use different messages. A failed load is an error with a retry, never an empty list |
| Profile (R03) | Ready, not available (removed or malformed link), error | Contact details and services are readable without sign-in. A "Message" action is shown to everyone; visitors are taken to sign-in first and returned afterwards |
| Sign-in (R04) | Choose provider, redirecting, cancelled, failed, choose role | Cancelling at the provider returns to the previous page |
| Profile editor (R05) | Pristine, draft, validation error, saving, failed, saved, changed elsewhere | Edits are never cleared by a failure. Leaving with unsaved changes asks for confirmation |
| Conversation list (R07) | Loading, ready, empty, error, signed out | Unread conversations have a badge with text, not color only |
| Chat thread (R06, R07) | Loading, ready, sending, sent, failed (retry), outcome unknown (checking), connection lost | Unsent text stays in the input field. Each message shows its send state until confirmed. A failed message offers retry |
| Email notifications (R08) | On, off | The account page shows which address gets emails and lets the accountant turn them off. Every email has a link to turn them off |
| Connection (interactive pages) | Connected, reconnecting, could not reconnect | A visible notice while reconnecting; typed text kept (R10). If reconnecting fails, a reload button |
| Account deletion (R09) | Confirm, deleting, failed, deleted | Two-step confirmation that names what will be removed. Failure keeps the user signed in |
| Ratings and reviews (R13, R14) | No reviews, some reviews, can review, cannot review (with the reason), writing, saved, replying | Stars always come with a number and a count, never stars alone. The review form explains what will be public. An accountant's own profile shows Reply on each review |
| Portfolio highlights (R12) | None, some, editing | Shown as cards on the profile and as a count on directory cards. The editor warns against naming clients |
| Report and block (R15) | Choose a reason, sent, cancelled; blocked by me, blocked by them | Report sits on profiles, reviews and chats. Block asks for confirmation and can be undone |
| Information pages (R16–R18) | Privacy, terms, about, help, for accountants, emails off, not found, error | Linked from the footer on wide screens and from the home and account pages on phones |

Link business rules to the PRD instead of redefining them. Keep keyboard focus where the user was when a list updates. On phones, the on-screen keyboard must not hide the chat send button.

## Service workspace presentation

[PRD](PRD.md) R19–R23 / [F03](../specs/client-services/spec.md) add private work pages in the existing Tax Buddy visual system. Keep the public directory separate from private progress; no public client names, service files, completion statistics or verification badges.

| Component | States / presentation |
|---|---|
| Workspace navigation | Signed-in accountant: Clients; signed-in client: My accountants. Available in desktop navigation, phone tabs and Account. Visitors get sign-in; unrelated deep links get generic unavailable |
| Private lists | Accountant groups services by client with request counts. Client groups own services under added accountants, with profile/chat/request links. Distinct loading, empty, load-error/retry and unavailable states |
| Request/acceptance | Explicit pending, active and declined states; acceptance is not payment, booking or a verification claim. Client-name sharing is explained before request submission. Confirmed private-save/request/accept model and remaining D24 proposals are distinguished in notes/context |
| Checklist | Named tasks with visible statuses; accountant can stage checkbox/status edits and explicitly save. Client sees read-only saved values. Progress shows completed/total plus percentage, never color alone; no tasks means checklist not set |
| Shared updates | Notes/activity include author and Asia/Manila display time; newest first; no private/internal accountant notes are implied. Failed saves keep draft and say they failed |
| Service documents | Original display name, uploader, time, whole-service/task association, download and uploader-only proposed removal. Service documents do not appear in public profiles or chat. Uploading/available/failed/retry and invalid type/size are visually distinct |
| Upload prototype | Explicit generated-sample selector; no native picker or real file intake. Demo PDF/CSV/TXT and 5 MiB limits are marked proposed. A busy upload cannot be submitted again; account changes interrupt it |

Reuse existing semantic tokens, paper surfaces, serif section headings, sans controls, 44 px phone targets and visible focus. Desktop service detail uses a main checklist/documents column and a narrower update-history column; phone/narrow layouts stack them. Labels remain visible and no task/filename forces horizontal scrolling. Announce saves and upload failure/completion with existing live regions. Service status/progress derives from the PRD's proposed rules; do not create a second business-rule source here.

## Accessibility, responsive layout, and localization

- Use semantic HTML first: real links, buttons, headings, lists and form labels. Add ARIA only where HTML has no element for the job.
- Provide a visible focus indicator, a logical focus order, and announcements for status changes (new message, send failed, reconnecting).
- Test with a screen reader on desktop and on a phone, keyboard only, and at the zoom target in D15. Include light, dark and system themes, portrait and landscape, and the smallest supported width.
- Desktop keyboard: Enter sends a message, and Shift+Enter adds a new line (Proposed).
- English only in release 1. Keep UI text out of logic; use typed values, not display labels, in code.
- Times are displayed in Asia/Manila time (PRD BR07). RTL is out of scope.

## Verification and exceptions

Procedures: [TEST-PLAN](delivery/TEST-PLAN.md) TP06. Observed results: [STATUS](delivery/STATUS.md). The user delegated the current prototype's visual choice; owner walkthrough, unresolved behavior decisions and production accessibility remain STATUS gate G9 and TP06. Valid HTML and a passing build alone do not establish compliance.
