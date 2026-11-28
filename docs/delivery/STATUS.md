# Delivery status

**Authority:** current implementation, verification, and acceptance state. Behavior belongs in [PRD](../PRD.md); procedures in [TEST-PLAN](TEST-PLAN.md).

Last update: 2026-11-16. Repository HEAD: `af9d3ea60af1cda9ad022322904b76a74e3900cb` on `main` ("feat(f01): browse accountants directory and profile pages"). It holds everything that was uncommitted on 2026-11-13: the C03 resolution, the provisional D04/D05 decisions, [ADR 010](../adr/010-single-project-per-page-render-modes.md), the Supabase setup records, and the F01 source and tests. Uncommitted on top of it: the 2026-11-16 F02 work (application theme switch, name survey, record updates) and F04 Google sign-in (both staged on branch `feature/f02-f04-theme-switch-google-sign-in`), and F05 profile editing (unstaged), plus an untracked `.vscode/` that is not part of this work.

Earlier evidence retains its own exact source identity and the names of its day. Current focus: F01 and F02 acceptance (owner walkthrough and browser matrix), the D01 name decision, then the next feature. Release readiness: **Not assessed**.

## Feature state

Application source exists since 2026-11-13 (F01, committed in `af9d3ea`), the F02 theme switch, F04 Google sign-in, F05 profile editing and F06 profile pictures since 2026-11-16 (committed up to `37c5ae7`), F07 chat since 2026-11-16 and F08 portfolio highlights since 2026-11-17 (committed in `7bdf42e` and `11466b4`), F09 ratings, reviews and replies since 2026-11-17 (committed in `55e0f12`), and F10 report and block since 2026-11-18 (uncommitted). The other rows are at their starting state.

| Requirement / feature | Implementation | Automated verification | Build | Browser verification | Acceptance |
|---|---|---|---|---|---|
| R01–R03 / [F01 browse-accountants](../../specs/browse-accountants/spec.md) | Implemented 2026-11-13, committed in `af9d3ea` | Pass: 62 tests, 0 skipped, including data tests on PostgreSQL ([evidence](evidence/2026-11-13-f01-browse-accountants.md)) | Pass: Release, 0 warnings | Partial: Edge 157 emulated at 320–1440 px, light and dark, 0 layout problems. Chrome, Firefox, Brave, Safari, real phones, screen reader and zoom not run | Pending |
| DESIGN theme / [F02 BD09 theme switch](../../specs/brand-design-refresh/spec.md#theme-switch-in-the-application-2026-11-16) | Implemented 2026-11-16 (uncommitted) | Pass: 20 new tests; the whole suite has 82 passed, 0 failed, 0 skipped, including the database tests on the development database ([evidence](evidence/2026-11-16-f02-theme-switch.md)) | Pass: Release, 0 warnings (Windows and WSL) | Partial: Edge 157 headless, 320–1440 px, both browser settings, 181 checks pass. Other browsers, real phones, screen reader and zoom not run | Pending (G9) |
| R24 / [F06 profile pictures](../../specs/profile-pictures/spec.md) | Implemented 2026-11-16 (uncommitted); the owner chose everyone, Supabase Storage, SkiaSharp and the initials default ([ADR 011](../adr/011-profile-pictures-supabase-storage.md)) | Pass: 18 new tests; whole suite 168 passed, 0 skipped ([evidence](evidence/2026-11-16-f06-profile-pictures.md)) | Pass: Release, 0 warnings | Partial: the Account picture section in Edge at 390 and 1440 px, light and dark; storage failure keeps the picture; **real Supabase Storage round trip passed** (5 of 5). Accountant card and profile with real storage, other browsers and phones not run | Pending |
| R04 sign-in and registration / [F04 Google sign-in](../../specs/google-sign-in/spec.md) | Google part implemented 2026-11-16 (uncommitted). Microsoft part implemented 2026-11-23 ([F15](../../specs/microsoft-sign-in/spec.md), uncommitted) and archived the same day by the owner: hidden behind `Authentication:Microsoft:Enabled` (off), so R04 is Google only. D03 decided (one role per account) | Pass: 22 new tests; whole suite 104 passed, 0 skipped ([evidence](evidence/2026-11-16-f04-google-sign-in.md)) | Pass: Release, 0 warnings | Partial: Edge 157 headless, 320–1440 px, both themes, 152 checks. Real Google round trip (GS09) passed 2026-11-16: the owner reported it and the agent checked the account in the database. Returning sign-in and sign-out with real Google, other browsers and phones not run. Microsoft (archived): web tests only, including that it stays hidden ([evidence](evidence/2026-11-23-f03-stage2-f15.md)); MS07 dropped | Pending |
| R05 / [F05 accountant profile editing](../../specs/profile-editing/spec.md) | Implemented 2026-11-16 (uncommitted). D04 limits decided; the editor is a static form (C04 for this page) | Pass: 44 new tests; whole suite 148 passed, 0 skipped ([evidence](evidence/2026-11-16-f05-profile-editing.md)) | Pass: Release, 0 warnings | Partial: Edge 157 headless, 320–1440 px, both themes, 42 checks; one defect found and fixed. Real Google accountant, other browsers and phones not run | Pending |
| R06, R07 / [F07 chat](../../specs/chat/spec.md) | Implemented 2026-11-16 (committed in `7bdf42e`). D06 decided: one conversation per pair, 2,000 characters. Email (R08) separate | Pass: 22 new tests; whole suite 190 passed, 0 skipped with `-m:1` ([evidence](evidence/2026-11-16-f07-chat.md)) | Pass: Release, 0 warnings; Tailwind built | Partial: two Edge sessions against the real app, 14 of 14 (draft through reload and sign-in, live reply, unread marks, server restart). Phones, other browsers, two tabs and an expired session not run | Pending |
| R08 / [F14 email notifications](../../specs/email-notifications/spec.md) | Implemented 2026-11-23 (uncommitted) with a development sender; migration `AddEmailNotifications` applied to the development database. Real sending waits for Brevo (G11) | Pass: 8 web and 8 unit tests; whole suite 301 passed ([evidence](evidence/2026-11-23-f14-email.md)) | Pass: Release, 0 warnings | Not run: the Account row and off page need a signed-in session; no real email | Pending (G9, G11) |
| R09 / [F12 account deletion](../../specs/account-deletion/spec.md) | Implemented 2026-11-23 (uncommitted). Two static steps; the account is made nameless; chats stay read-only for the other person (D07); migration `AddAccountDeletion` applied to the development database | Pass: 9 new tests; whole suite 285 passed ([evidence](evidence/2026-11-23-f12-f13.md)) | Pass: Release, 0 warnings | Partial: the visitor view in Edge, no overflow. The signed-in steps need a real Google sign-in | Pending (G9) |
| R10, R11 connection loss and privacy | Covered by each feature. R10 on chat built in F07: drafts in the tab, a non-modal connection banner, nothing sent later | Pass for chat (F07) | Pass | Partial: server restart in Edge (F07) | Pending |
| R12 / [F08 portfolio highlights](../../specs/portfolio-highlights/spec.md) | Implemented 2026-11-17 (committed in `11466b4`). D23 provisional: up to 6, working field limits | Pass: 7 new tests; whole suite 197 passed, 0 skipped with `-m:1` ([evidence](evidence/2026-11-17-f08-portfolio-highlights.md)) | Pass: Release, 0 warnings; Tailwind built | Partial: Edge, profile, card and editor at 390 and 1440 px, light and dark (7 of 7). Other browsers and phones not run | Pending |
| R13, R14 / [F09 ratings, reviews and replies](../../specs/reviews/spec.md) | Implemented 2026-11-17 (committed in `55e0f12`). Static review form (C04 closed for reviews); D21 and D23's review part provisional; "Highest rated" sort | Pass: 38 new tests; whole suite 235 passed, 0 skipped with `-m:1` ([evidence](evidence/2026-11-17-f09-reviews.md)) | Pass: Release, 0 warnings; Tailwind built | Partial: Edge, client and owner flows at 390 and 1440 px, light and dark, and a post without script (12 of 12). Other browsers and phones not run | Pending |
| R15 / [F10 report and block](../../specs/report-block/spec.md) | Implemented 2026-11-18 (uncommitted). D22 partly decided: reports stored and read in Supabase; one static report page; block covers messaging and reviews (BR19); one report per item, 10 a day | Pass: 23 new tests; whole suite 258 passed, 0 skipped with `-m:1` ([evidence](evidence/2026-11-18-f10-report-block.md)) | Pass: Release, 0 warnings; Tailwind built | Partial: Edge, two sessions with a live block and unblock, reports at 390 and 1440 px, light and dark, one without script (15 of 15). Other browsers and phones not run | Pending |
| R16–R18 legal, information and error pages | Legal pages: F04, then F11. Not-found and error pages from F01. About, Help and contact, and For accountants: [F13](../../specs/information-pages/spec.md), 2026-11-23 (uncommitted) | Pass: 6 new F13 tests ([evidence](evidence/2026-11-23-f12-f13.md)) | Pass | Partial: Edge at 320–1440 px, light and dark | Pending (G9) |
| R16 / [F11 legal text and privacy prompts](../../specs/legal-pages/spec.md) | Implemented 2026-11-21 (uncommitted). It covers: the owner-approved text, version `2026-11-21`, in the long-document layout; agreeing again at `/account/agree`; the new registration wording; the chat, editor, profile and report prompts | Pass: 9 new tests; whole suite 268 passed, 0 skipped ([evidence](evidence/2026-11-21-f11-legal-pages.md)) | Pass: Release, 0 warnings | Partial: Edge 158 via DevTools, `/terms` and `/privacy` at 320–1440 px, light and dark, no overflow. The agree page and chat hint weren't seen on the running site (they need a signed-in session) | Pending (G9); text not reviewed by a lawyer (G13) |
| R19–R23 / [F03 client services](../../specs/client-services/spec.md) | Stage 1 implemented 2026-11-23 (uncommitted): saved accountants, requests, accept and decline, checklists, notes and the two workspace lists; migration `AddClientServices` applied to the development database. Stage 2, documents (R23), implemented the same day (uncommitted): migration `AddServiceDocuments` applied; the private `service-documents` bucket created in the development project the same day | Pass: 8 new tests; whole suite 309 passed ([evidence](evidence/2026-11-23-f03-stage1.md)). Stage 2: 8 web and 24 unit tests ([evidence](evidence/2026-11-23-f03-stage2-f15.md)) | Pass: Release, 0 warnings | Not run: the signed-in pages need Google sign-in; no real Supabase Storage round trip for documents | Pending (G9) |
| R25 / [F17 client details](../../specs/client-details/spec.md) | Implemented 2026-11-25 (uncommitted): a client's details on Account, edited in place; the accountant of a service sees them on the client page and in requests; migration `AddClientDetails` applied to the development database | Pass: 23 new tests (17 unit, 6 web); whole suite 400 passed, 0 skipped with `-m:1` ([evidence](evidence/2026-11-25-f17-client-details.md)) | Pass: Release, 0 warnings; Tailwind built | Partial: test-host pages in Edge at 1280 px (light and dark) and 390 px; not the running site with a real sign-in | Pending: the owner's try-out and approval of privacy version `2026-11-25` |

States:
- Implementation: not started, in progress, or implemented.
- Verification: not run, pass, fail, blocked, or not applicable with a reason.
- Acceptance: pending, accepted, or rejected, with decision evidence.

Use separate browser rows when outcomes differ. A blocked check requires a concrete reason and next action.

### Prototype work, separate from application implementation

| Work | Implementation | Verification | Acceptance |
|---|---|---|---|
| [Client details on Account (BD15)](../../specs/brand-design-refresh/spec.md#client-details-on-account-2026-11-25-bd15) | Added 2026-11-25 at the user's request, uncommitted. Clients store and edit their name, phone and city and their business name, type and industry, shared with the accountants they contact; accountants see them on their client page and in service requests. The user chose B (Edit on Account), now the prototype's default; A and C stay on the options page as not chosen | 38 click checks; 212 prototype checks, 10 of them new; only the four intended pages differ from `06e9be6` ([evidence](evidence/2026-11-25-prototype-client-details.md)) | Built on the site as [F17](../../specs/client-details/spec.md) on 2026-11-25 (R25; C05 resolved); the prototype aligned with it the same day |
| [My profile: preview beside the form (BD16)](../../specs/brand-design-refresh/spec.md#my-profile-preview-beside-the-form-2026-11-25-bd16) | Applied 2026-11-25 at the user's choice of option B, uncommitted. The accountant's editor shows their directory card, as last saved, beside the form, or in a disclosure above it on phones and below 860 px. It's the prototype's default; the one long form stays viewable as `opt=editor-current` | 224 prototype checks, 12 of them new; 43 click checks ([evidence](evidence/2026-11-25-prototype-editor-preview.md)) | Built on the site the same day as [F05 PE09](../../specs/profile-editing/spec.md) ([evidence](evidence/2026-11-25-f05-editor-preview.md)); owner walkthrough pending |
| [Page titles on the workspace pages](../../prototypes/README.md#page-titles-options) | Added 2026-11-26 at the user's request, uncommitted. Step 5 of the accountant's flow on `options.html`, with a View switch over every workspace page:<br>• A: the name as the title, recommended;<br>• B: a compact header bar;<br>• C: the trail as the title.<br>All three replace the breadcrumb, page-type heading, eyebrow and second title. The current design stays the default | 279 prototype checks (20 new); 18 click checks ([evidence](evidence/2026-11-26-prototype-page-titles.md)) | Proposals, not adopted: waiting for the owner's choice. A DESIGN change; the site's `WorkFrame` and `WorkIntro` would follow |
| [Accountant dashboard options](../../prototypes/README.md#accountant-dashboard-options) | Added 2026-11-26 at the user's request, uncommitted. A fifth accountant step on `options.html`, with a State switch:<br>• A: a "Needs you" list at `/dashboard`;<br>• B: tiles and sections at `/dashboard`;<br>• C: an overview at the top of Clients.<br>Only existing data. Two personas and Elaine Navarro's busy data were added for the states. The current pages stay the default | 259 prototype checks (35 new); 11 click checks on the options page ([evidence](evidence/2026-11-26-prototype-dashboard-options.md)) | Archived the same day at the user's request ("archive dashboard design in the prototype for now"); no option chosen. Listed under Archived on `options.html`; still works |
| [Design options for the accountant's flow](../../prototypes/README.md#design-options-for-the-accountants-flow) | Added 2026-11-25 at the user's request, uncommitted. `options.html` gained a Client / Accountant flow switch, with options for My profile (A–C), Chats (A–C), Clients (A–C) and Service progress (A–C). The accountant's Chats options have their own setting. The current design stays the default | 202 prototype checks pass; 21 click checks; 16 default pages identical to `06e9be6`; every option viewed on desktop and phone ([evidence](evidence/2026-11-25-prototype-accountant-options.md)) | My profile: B chosen on 2026-11-25 (BD16, the row above). Chats, Clients and Service progress: waiting for the owner's choice |
| [Design options for the client's flow, and a workspace side list](../../prototypes/README.md#design-options-for-the-clients-flow) | Added 2026-11-24 at the user's request; committed in `06e9be6`. `prototypes/options.html` shows options for the directory (A–C), accountant profile (A, B), chats (A–C) and My accountants (A–C), each as the real prototype in a frame. A "Workspace navigation" switch shows the side-list proposal. The current design stays the default | 202 prototype checks pass on the unchanged default; every option viewed on desktop and phone in Edge; 14 default pages identical to `5237dab`; 26 DOM checks ([evidence](evidence/2026-11-24-prototype-design-options.md)) | Proposals, not adopted: waiting for the owner's choice per page. DESIGN, the specs and the site are unchanged |
| [F02 theme toggle in the header (BD14)](../../specs/brand-design-refresh/spec.md#theme-toggle-in-the-header-2026-11-23-bd14) | Applied 2026-11-23 at the user's instruction, in the prototype and the site, uncommitted. One moon/sun icon after Sign in or the account, and at the end of the phone app bar; the footer switch is removed | 269 tests, 202 prototype checks, and Edge on the running site and the prototype ([evidence](evidence/2026-11-23-f02-theme-toggle.md)) | Owner walkthrough pending (G9) |
| [Legal decisions (D10) in the prototype](../legal/drafting-notes.md#decisions-made-on-2026-11-21) | Applied 2026-11-21 at the user's choice ("Prototype now, site after"), uncommitted. Changes: the long-document layout for Privacy and Terms (summaries, not the full draft text); the registration wording; a chat hint about TINs and tax returns; a licence-number hint in the editor; the PRC pointer on the profile note; 10 working days on the report page; "Deleted user" and the D07 deletion notes; the guide-panel notes | 202 Edge checks pass, syntax checked, and screenshots viewed ([evidence](evidence/2026-11-21-legal-decisions-prototype.md)). The existing checks don't assert the new elements | Owner walkthrough pending (G9); the site follows in a separate feature |
| [C03 directory contract](../README.md#history-and-unresolved-conflicts) | Applied locally 2026-11-12: the prototype directory is a submitted form with sort and page links, all in the address, and no per-user card tag | 186 Edge checks passed, including 9 new directory checks; syntax and documentation checks pass; desktop, page-2 and 320 px screenshots agent-reviewed ([evidence](evidence/2026-11-12-c03-directory.md)) | Resolved by the user's instruction; owner walkthrough pending (G9) |
| [F02 application aligned to the prototype (BD10)](../../specs/brand-design-refresh/spec.md#application-aligned-to-the-prototype-2026-11-16-bd10) | Applied 2026-11-16 at the user's instruction (the prototype is the design origin, recorded in `CLAUDE.md`): the site shell, sign-in and role, a new Account page, the profile editor and the legal pages follow their prototype screens; deviations recorded in DESIGN | 149 tests pass; screenshots compared at 390 and 1440 px ([evidence](evidence/2026-11-16-f02-prototype-alignment.md)) | F01's directory and profile also compared and aligned the same day. Owner walkthrough pending (G9) |
| [F06 pick, preview, save (PP10)](../../specs/profile-pictures/spec.md#pick-preview-save-2026-11-19-pp10) | Applied 2026-11-19 at the user's instruction ("implement option e"), uncommitted: on Account the pencil opens the file chooser, the photo is previewed, and Save stores it; PP09's panel remains for script off. E is the prototype's default design | 202 Edge prototype checks pass; Release build with 0 warnings; 154 and 105 tests pass; the script run in Edge against the rendered page in six states ([evidence](evidence/2026-11-19-f06-picture-option-e.md)) | Not yet used on the running site in a signed-in browser, or on a phone. Owner walkthrough pending (G9) |
| [F06 pencil on the picture (PP09)](../../specs/profile-pictures/spec.md#pencil-on-the-picture-2026-11-19-pp09) | Applied 2026-11-19 at the user's instruction, uncommitted: a pencil on the avatar opens a small picture panel on Account, in the prototype (new) and the site; the editor's avatar has the same pencil | 190 Edge prototype checks pass, 4 of them new; Release build with 0 warnings; 154 and 105 tests pass, 1 of them new; rendered pages reviewed ([evidence](evidence/2026-11-19-f06-picture-pencil.md)) | Not yet seen on the running site in a signed-in browser. Owner walkthrough pending (G9) |
| [F02 one typeface (BD13)](../../specs/brand-design-refresh/spec.md#one-typeface-2026-11-19-bd13) | Applied 2026-11-19 at the user's instruction, uncommitted: Quicksand for headings, body and controls in the prototype and the site, from local font files | 186 Edge prototype checks pass; Release build with 0 warnings; 154 and 104 tests pass; screenshots of both reviewed ([evidence](evidence/2026-11-19-f02-typeface.md)) | Owner walkthrough pending (G9) |
| [F02 logo replaced (BD12)](../../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12) | Applied 2026-11-18 at the user's instruction, uncommitted: a speech bubble holding a T and a two-tone wordmark replace the ledger symbol in the prototype's five logo files and inline copy, and in the site's images and `BrandLogo` | 186 Edge prototype checks pass; Release build with 0 warnings; 154 application tests pass; of 104 web tests, 34 passed and 70 could not connect because the development database had no free connections (`53300`); screenshots of the prototype and the running site reviewed ([evidence](evidence/2026-11-18-f02-logo.md)) | Follow-up 2026-11-19: outlined wordmark, the directory illustration redrawn, the WSL Tailwind build fixed, and all 258 tests pass ([evidence](evidence/2026-11-19-f02-logo-outlines-hero.md)). Web survey of similar marks and domains done; not clearance. Owner walkthrough pending (G9) |
| [F02 rename to Tax Buddy (BD07)](../../specs/brand-design-refresh/spec.md#rename-to-tax-buddy-2026-11-12) | Applied locally 2026-11-12: display name, redrawn wordmark, renamed stylesheet/logo files, look id, example domain, planned project names, current documentation | 142 Edge checks passed on the renamed sources, including 34 contrast pairs; syntax, SVG and documentation checks pass; header screenshots and logo render agent-reviewed ([evidence](evidence/2026-11-12-tax-buddy-rename.md)) | Name set by the user's instruction. Preliminary name survey 2026-11-16 ([record](evidence/2026-11-16-name-availability.md)): conflicting active TaxBuddy tax products in the Philippines and India and a live US TAXBUDDIES mark; not clearance. D01 decision, owner walkthrough and release acceptance pending |
| [F02 palette options (BD08)](../../specs/brand-design-refresh/spec.md#palette-options-2026-11-12) | Applied locally 2026-11-12: four colour proposals selectable in the prototype beside the current palette; colours only | 178 Edge checks passed, including 36 new palette checks and 17 contrast pairs per option and theme; syntax and documentation checks pass; directory screenshots agent-reviewed ([evidence](evidence/2026-11-12-palette-options.md)) | 2026-11-13: the user chose Paper & Forest; the four proposals stay in the prototype as history ([DESIGN](../DESIGN.md#visual-system)) |
| [F02 brand/design refresh, as Kuwenta](../../specs/brand-design-refresh/spec.md) | Applied locally: name, original SVG logos/favicon, light/dark direction, directory/profile/shell refinements, current documentation | Historical observation for the fingerprinted F02 artifact: 78 Edge prototype checks, 34 contrast pairs, syntax/SVG/docs checks passed; screenshots agent-reviewed ([evidence](evidence/2026-11-11-kuwenta-refresh.md)). Not automatically transferred to F03 | User delegated the design choice; owner walkthrough and product/release acceptance pending. No application behavior is established |
| [F03 client services/progress/documents](../../specs/client-services/spec.md) | Docs and local prototype updated: private lists, request/acceptance, per-client services, checklist/progress/history and generated sample file lifecycle | 142 Edge checks passed, including rerun of prior flows and 34 contrast pairs; syntax, documentation, identifier/table and source-hash checks pass; responsive screenshots agent-reviewed ([evidence](evidence/2026-11-11-client-services.md)) | Owner confirmed private-save/request/accept model; remaining D24–D28 policies and overall prototype/release acceptance pending. Real uploads/storage and server behavior unimplemented |

## Evidence register

| Evidence record | Date | Source / artifact | Environment | Attribution | Result / limits |
|---|---|---|---|---|---|
| [Documentation setup checks](evidence/2026-11-10-documentation-setup.md) | 2026-11-10 | `b9b50f3` plus uncommitted documentation changes | Windows 11 Pro 10.0.26300; uv-managed CPython 3.14.4 | Agent-observed | Documentation checks passed for the MAUI plan of that day. Host inventory recorded. Nothing about app behavior is established |
| [Web platform change checks](evidence/2026-11-11-web-platform-change.md) | 2026-11-11 | `b9b50f3` plus uncommitted documentation and prototype changes | Same PC; headless Microsoft Edge for the prototype | Agent-observed | Documentation checks pass after the platform change; prototype walk-through passes. No application exists, so nothing about app behavior is established |
| [Kuwenta brand and prototype checks](evidence/2026-11-11-kuwenta-refresh.md) | 2026-11-11 | `668a9a9` plus F02 changes; source SHA-256 values retained with report | Windows 10.0.26300 x64, Node 24.16.0, headless Edge 157.0.4341.42 | Agent-observed | 78 prototype checks, 34 semantic contrast pairs, syntax/SVG/docs checks pass. Screen readers, other browsers and application acceptance remain pending |
| [Client services prototype checks](evidence/2026-11-11-client-services.md) | 2026-11-11 | Preserved F02 working tree plus F03; exact HTML/CSS/module/checker/asset hashes in report | Windows 10.0.26300 x64, Node 24.16.0, Edge 157.0.4341.42 | Agent-observed | 142 checks and static checks pass; screenshots reviewed; generated fixtures only. TP19–TP22 production boundaries untested |
| [Tax Buddy rename checks](evidence/2026-11-12-tax-buddy-rename.md) | 2026-11-12 | `1fccef3` plus the uncommitted rename; source SHA-256 values retained with report | Windows 10.0.26300 x64, Node 22.12.0, headless Edge 157.0.4341.46 | Agent-observed | 142 prototype checks, 34 contrast pairs, syntax/SVG/docs checks pass on the renamed sources. Name availability, screen readers, other browsers and application acceptance remain pending |
| [Palette options prototype checks](evidence/2026-11-12-palette-options.md) | 2026-11-12 | `1fccef3` plus the uncommitted rename and palette options; source SHA-256 values retained with report | Windows 10.0.26300 x64, Node 22.12.0, headless Edge 157.0.4341.46 | Agent-observed | 178 prototype checks pass; each of four options passes 17 contrast pairs in light and dark. No palette chosen; other pages not reviewed per option |
| [Prototype scope and stack-fit review](evidence/2026-11-12-prototype-scope-review.md) | 2026-11-12 | `1fccef3` plus the uncommitted rename and palette options; reviewed source SHA-256 values in the record | Not applicable: read-through of documents and prototype source, nothing executed | Agent-observed | Two open conflicts recorded (C03 directory interaction, C04 drafts on connection loss) and a list of costly or prototype-only items. Recommendations only; no decision, scope change or browser check. Some stack statements are from the agent's knowledge and unverified |
| [F02 theme switch](evidence/2026-11-16-f02-theme-switch.md) | 2026-11-16 | `af9d3ea` plus uncommitted BD09 source; per-file SHA-256 in the record | Windows 10.0.26300 (build, Tailwind); WSL2 Ubuntu 26.04 with SDK 11.0.100 (build, tests, app; first in invariant-globalization mode, then rerun with ICU, same results); headless Edge 157.0.4341.60 | Agent-observed | Release build 0 warnings; 82 tests pass with 0 skipped, once the user restored the database secret; 181 browser checks pass; screenshots reviewed. On Windows, Application Control blocked the new binaries from running. Other browsers and acceptance pending |
| [F06 profile pictures](evidence/2026-11-16-f06-profile-pictures.md) | 2026-11-16 | `2b5407c` plus staged F05/BD10 and unstaged F06; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; development database; headless Edge 157.0.4341.60; storage in memory for tests | Agent-observed | 168 tests pass, 0 skipped; migration `AddProfilePicture` applied; Account picture section checked in Edge; a real upload with storage unreachable keeps the picture. Real Supabase Storage round trip passed later the same day |
| [F02 prototype alignment](evidence/2026-11-16-f02-prototype-alignment.md) | 2026-11-16 | `af9d3ea` plus staged F02/F04 and unstaged F05 and BD10; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; development database; headless Edge 157.0.4341.60 | Agent-observed | Pages rebuilt from the prototype. 149 tests pass, 0 skipped; 16 app views with no overflow; screenshots compared with the prototype and gaps fixed. Synthetic accounts deleted. F01 pages compared and aligned too. Acceptance pending |
| [F05 profile editing](evidence/2026-11-16-f05-profile-editing.md) | 2026-11-16 | `af9d3ea` plus staged F02/F04 and unstaged F05; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100, ICU; development database; headless Edge 157.0.4341.60 | Agent-observed | Release build 0 warnings; 148 tests pass, 0 skipped; migration `AddProfileOwner` applied; 42 browser checks pass on the real app. Two product defects found and fixed. Synthetic browser-check accounts deleted. Real Google accountant, other browsers and acceptance pending |
| [F07 chat](evidence/2026-11-16-f07-chat.md) | 2026-11-16 | `37c5ae7` plus uncommitted F07; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; headless Edge 157.0.4341.60; Supabase development database | Agent-observed (Claude Code) | 190 tests and 14 of 14 browser checks pass; real phones, other browsers, two tabs and an expired session not run |
| [F08 portfolio highlights](evidence/2026-11-17-f08-portfolio-highlights.md) | 2026-11-17 | `37c5ae7` plus uncommitted F07 and F08; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; headless Edge 157.0.4341.60; Supabase development database | Agent-observed (Claude Code) | 197 tests and 7 of 7 browser checks pass; the seed was not rerun (it would delete the owner's profile) |
| [F09 ratings, reviews and replies](evidence/2026-11-17-f09-reviews.md) | 2026-11-17 | `11466b4` plus uncommitted F09; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; headless Edge 157.0.4341.60; Supabase development database | Agent-observed (Claude Code) | 235 tests and 12 of 12 browser checks pass; the database's connection slots ran out once and the web test classes were limited to four at a time |
| [F10 report and block](evidence/2026-11-18-f10-report-block.md) | 2026-11-18 | `55e0f12` plus uncommitted F10; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; headless Edge 157.0.4341.60; Supabase development database | Agent-observed (Claude Code) | 258 tests and 15 of 15 browser checks pass (three runs: a check's timing and a 390 px alignment were fixed) |
| [Prototype alignment check (BD11)](evidence/2026-11-18-prototype-alignment.md) | 2026-11-18 | `55e0f12` plus uncommitted F10 and BD11; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100; headless Edge 157.0.4341.60; Supabase development database | Agent-observed (Claude Code; three read-only audit subagents) | About 40 unrecorded differences from the prototype fixed; 258 tests and 16 of 16 browser checks pass; one CSS fix after the run, not re-shot |
| [F04 Google sign-in](evidence/2026-11-16-f04-google-sign-in.md) | 2026-11-16 | `af9d3ea` plus uncommitted F02 BD09 and F04; per-file SHA-256 in the record | WSL2 Ubuntu 26.04, SDK 11.0.100, ICU; development database; headless Edge 157.0.4341.60 | Agent-observed | Release build 0 warnings; 104 tests pass, 0 skipped; migration `AddAccounts` applied; 152 browser checks pass. Real Google round trip, registration page in a browser, other browsers and acceptance pending |
| [Name availability survey](evidence/2026-11-16-name-availability.md) | 2026-11-16 | Not applicable (web research) | Web search and page fetches | Agent-observed (subagent) | Conflicting uses found in the Philippines, India and the US; IPOPHL, WIPO and registrars not searched directly. Not clearance; no decision |
| [F01 browse accountants](evidence/2026-11-13-f01-browse-accountants.md) | 2026-11-13 | `5ceee37` plus uncommitted F01 source; 62 files with combined SHA-256 `eae0d6ae…` | WSL2 Ubuntu 24.04.1, .NET SDK 11.0.100; Supabase development project; headless Edge 157 on Windows | Agent-observed | Release build with 0 warnings; 62 tests pass with 0 skipped; 32 emulated browser views with 0 layout problems after one fix. Two defects found by the tests were fixed. Browser matrix, screen reader and acceptance pending |
| [Supabase development database connection](evidence/2026-11-13-supabase-dev-connection.md) | 2026-11-13 | No application source; a throwaway check program (Npgsql 10.0.3, .NET SDK 11.0.100) | WSL2 Ubuntu 24.04.1, no IPv6; Supabase free project in `ap-southeast-1` | User created the project and stored the secret; agent-observed check and host correction | Connects through the session pooler with encryption required; PostgreSQL 17.11; empty database. Data API setting and EF Core behaviour not checked |
| [C03 directory and provisional D04/D05](evidence/2026-11-12-c03-directory.md) | 2026-11-12 | `5ceee37` plus this uncommitted change; source SHA-256 values retained with report (WSL checkout, LF line endings) | WSL2 Ubuntu 24.04.1 on Windows 10.0.26300; Windows Node 22.12.0 through WSL interop; headless Edge 157.0.4341.46 | Agent-observed | 186 prototype checks pass after one failing run was fixed (320 px phone overflow). Decisions are the user's; no acceptance. Behaviour without JavaScript is an application check |

Use the [evidence template](evidence/TEMPLATE.md). A template/procedure is not a completed execution record. A previous pass does not automatically apply to changed source or another platform.
| [Logo replacement checks](evidence/2026-11-18-f02-logo.md) | 2026-11-18 | `55e0f12` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; Windows Node 24.16.0; headless Edge 157.0.4341.60 | Agent-observed | 186 prototype checks, Release build and 154 application tests pass. 70 of 104 web tests could not reach the development database (no free connections), so the web suite is not a pass. Screenshots reviewed in both themes |
| [Logo outlines, illustration, build and survey](evidence/2026-11-19-f02-logo-outlines-hero.md) | 2026-11-19 | `55e0f12` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; web search and RDAP | Agent-observed | 186 prototype checks; Release build with the Tailwind step, 0 warnings; 154 and 104 tests pass, 0 skipped; screenshots reviewed. The survey is web search and domain records only: no trademark register, no image search |
| [One typeface checks](evidence/2026-11-19-f02-typeface.md) | 2026-11-19 | `55e0f12` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60 | Agent-observed | 186 prototype checks, Release build, 154 and 104 tests pass; screenshots reviewed on desktop light and phone dark. Not checked: real phones, zoom, other browsers, the fallback when the font fails to load |
| [Pencil on the picture checks](evidence/2026-11-19-f06-picture-pencil.md) | 2026-11-19 | `55e0f12` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60 | Agent-observed | 190 prototype checks; Release build; 154 and 105 tests pass. The site's pages were reviewed as rendered by the test host and opened as files, not in a signed-in browser on the running site; no real upload was made |
| [Pick, preview, save checks](evidence/2026-11-19-f06-picture-option-e.md) | 2026-11-19 | `55e0f12` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60 | Agent-observed | 202 prototype checks; Release build; 154 and 105 tests pass. The script was run against the page as rendered by the test host, with a file put into the input by script; the site confirmed it serves and registers the script. No signed-in browser session and no real upload |
| [Legal decisions in the prototype](evidence/2026-11-21-legal-decisions-prototype.md) | 2026-11-21 | `1565e69` plus uncommitted work; source SHA-256 prefixes in the record | WSL2; Windows Node 22.12.0; headless Edge 158.0.4370.33 | Agent-observed | 202 prototype checks pass; screenshots of the legal pages, the cookie table and the chat hint viewed. The new elements have no automated checks; the site is unchanged |
| [F11 legal text and privacy prompts](evidence/2026-11-21-f11-legal-pages.md) | 2026-11-21 | `1565e69` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas); Windows Node 22.12.0; Edge 158.0.4370.33 | Agent-observed | Release build; 268 tests pass; 202 prototype checks; `/terms` and `/privacy` viewed at 320–1440 px in both themes with no overflow. The agree page and chat hint weren't seen on the running site |
| [F03 client services, stage 1](evidence/2026-11-23-f03-stage1.md) | 2026-11-23 | `1565e69` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas; migration applied) | Agent-observed | 309 tests (8 new). Signed-in pages not seen in a browser; documents not built |
| [F14 email notifications](evidence/2026-11-23-f14-email.md) | 2026-11-23 | `1565e69` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas; migration applied) | Agent-observed | 301 tests (16 new for email). No real email (no Brevo account); no browser check of the signed-in email row |
| [F12 account deletion and F13 information pages](evidence/2026-11-23-f12-f13.md) | 2026-11-23 | `1565e69` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas; migration applied); Windows Node 22.12.0; Edge 158.0.4370.33 | Agent-observed | 285 tests (one earlier run had 10 unexplained setup failures, not reproduced); 202 prototype checks; new pages in Edge with no overflow. Signed-in deletion not seen in a browser |
| [F02 theme toggle in the header](evidence/2026-11-23-f02-theme-toggle.md) | 2026-11-23 | `1565e69` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; the user's running site (Debug); Windows Node 22.12.0; Edge 158.0.4370.33 | Agent-observed | 269 tests; 202 prototype checks; toggle seen and pressed at 320–1440 px with the device light and dark. No keyboard, screen-reader or other-browser checks |
| [Account workspace row and picture pencil for both roles](evidence/2026-11-24-account-workspace-row.md) | 2026-11-24 | `ff4f0df` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas); headless Edge 158.0.4370.33 | Agent-observed | Release build; 354 tests (2 new). Rendered Account pages for both roles viewed at 1280 and 390 px, dark theme, without script, before and after the long-address fix; 202 prototype checks after it. No signed-in browser session |
| [Prototype and site compared, per user and page](evidence/2026-11-24-prototype-site-comparison.md) | 2026-11-24 | `ff4f0df` plus uncommitted work; prototype hash in the record | Code reading only | Agent-observed (five read-only reviewers, spot-checked) | Unrecorded differences listed per page and user, plus documents out of date. Nothing run in a browser; no decisions taken |
| [Prototype alignment fixes](evidence/2026-11-24-prototype-alignment-fixes.md) | 2026-11-24 | `ff4f0df` plus uncommitted work; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas); headless Edge 158.0.4370.33 | Agent-observed | Release build; 374 tests (new unit and web checks for each fix). Seven rendered pages viewed at 1280 and 390 px, dark theme. No signed-in browser session against the running site |
| [Prototype design options and workspace side list](evidence/2026-11-24-prototype-design-options.md) | 2026-11-24 (checker 2026-11-25) | `06e9be6`; prototype SHA-256 prefixes in the record | Headless Edge 158.0.4370.33 from WSL2; Windows Node 22.12.0 | Agent-observed | 202 prototype checks on the default design; options and side list checked by screenshots and DOM reads only. Screenshots not kept; no real phones, screen readers or contrast calculations |
| [Prototype design options for the accountant's flow](evidence/2026-11-25-prototype-accountant-options.md) | 2026-11-25 | Uncommitted work on `06e9be6`; prototype SHA-256 prefixes in the record | Headless Edge 158.0.4370.33 from WSL2; Windows Node 22.12.0 | Agent-observed | 202 prototype checks; 21 click checks; 16 default pages identical to `06e9be6`. No real phones, screen readers or contrast calculations; one or two actions clicked per option |
| [Prototype client details](evidence/2026-11-25-prototype-client-details.md) | 2026-11-25 | Uncommitted work on `06e9be6`; prototype SHA-256 prefixes in the record | Headless Edge 158.0.4370.33 from WSL2; Windows Node 22.12.0 | Agent-observed | 33 click checks (12 new) and 202 prototype checks; 14 default pages identical to `06e9be6`. Outside the PRD; no real phones, screen readers, dark theme or contrast calculations |
| [F17 client details and the picture-upload setup](evidence/2026-11-25-f17-client-details.md) | 2026-11-25 | Uncommitted work on `06e9be6`; source SHA-256 prefixes in the record | WSL2 with .NET SDK 11.0.100; development database (throwaway schemas; migration applied); headless Edge 158.0.4370.33; Windows Node 22.12.0 | Agent-observed | Release build; 400 tests (23 new); test-host pages viewed in Edge. Picture setup fixed in user-secrets and checked against Supabase Storage, not by an upload. No real sign-in |
| [Prototype My profile with the preview beside the form](evidence/2026-11-25-prototype-editor-preview.md) | 2026-11-25 | Uncommitted work on `06e9be6`; prototype SHA-256 prefixes in the record | Headless Edge 158.0.4370.33 from WSL2; Windows Node 22.12.0 | Agent-observed | 224 prototype checks (12 new); 43 click checks. No page-by-page comparison with the previous prototype; no real phones, screen readers or contrast calculations; nothing on the site |
| [Prototype accountant dashboard options](evidence/2026-11-26-prototype-dashboard-options.md) | 2026-11-26 | Uncommitted work on `06e9be6`; prototype SHA-256 prefixes in the record | Headless Edge 158.0.4370.33 from WSL2; Windows Node 22.12.0 | Agent-observed | 259 prototype checks (35 new) and 11 options-page click checks; screenshots viewed; three faults fixed. No DOM comparison of the default pages with the previous prototype (no committed baseline); no real phones, screen readers or contrast calculations |
| [Prototype page titles on the workspace pages](evidence/2026-11-26-prototype-page-titles.md) | 2026-11-26 | Uncommitted work on `06e9be6`; prototype SHA-256 prefixes in the record | Headless Edge 158.0.4370.33 from WSL2; Windows Node 22.12.0 | Agent-observed | 279 prototype checks (20 new) and 18 options-page click checks; screenshots viewed; one wording fix. No DOM comparison with the previous prototype; no real phones, screen readers or contrast calculations |

## Remaining gates and decisions

| Gate / decision | Owner | Missing evidence or dependency | Next action |
|---|---|---|---|
| G1 .NET 11 SDK on the development PC | lumabanls | Not installed on Windows (checked 2026-11-11). Installed 2026-11-12 in WSL2 Ubuntu 24.04.1 on the same PC: SDK 11.0.100 with runtimes 11.0.0, user-level in `~/.dotnet` beside SDK 10.0.404, through Microsoft's `dotnet-install.sh`. A throwaway Blazor Web App targeting `net11.0` built with 0 warnings and 0 errors. The development certificate in WSL is not trusted yet. Closed for WSL 2026-11-13: [SETUP](../SETUP.md#reproducibility-record) records the WSL toolchain, `global.json` pins 11.0.100, and F01 builds and tests with it | 2026-11-16: the WSL distribution is now Ubuntu 26.04 without the earlier SDK, secrets, Linux Node or `libicu`. At the user's choice, SDK 11.0.100 was reinstalled in `~/.dotnet`, and the user installed `libicu78`. Windows has SDK 11.0.100, but Application Control blocks newly built binaries ([SETUP troubleshooting](../SETUP.md#troubleshooting-and-maintenance)). `~/.bashrc` now puts `~/.dotnet` on `PATH`, and the user restored the database user-secret in WSL; the full suite passes there. Re-record SETUP's host row. Trust the development certificate before using the HTTPS profile |
| G2 Android build host | — | Closed 2026-11-11: not needed for a web application | — |
| G3 Android device for native checks | — | Closed 2026-11-11: replaced by the browser matrix in TEST-PLAN | — |
| G4 Windows sign-in (C01, PRD D02) | — | Closed 2026-11-11 by [ADR 006](../adr/006-web-sign-in.md) | — |
| G5 F01 blocking decisions (PRD D04, D05) | lumabanls | Adopted provisionally 2026-11-12 by the user's instruction: the prototype's proposals ([PRD](../PRD.md#open-decisions)). Field length limits decided 2026-11-16 with R05 (F01's values, plus at most 10 experience entries). Page size remains a working value (6) | Decide the directory page size (D05) |
| G6 Database and hosting (PRD D11) | lumabanls | Development database ready 2026-11-13: the user created a Supabase project in Singapore (`ap-southeast-1`, PostgreSQL 17.11), and the agent connected to it from WSL through the session pooler ([evidence](evidence/2026-11-13-supabase-dev-connection.md)). F01 applied its migration and synthetic seed there; Supabase's automatic RLS covers its tables. Still open: the user's screenshot showed the Data API toggle off with Save still active, and saving was not confirmed; no test-site project yet. Host decided as Render, and the user is considering Oracle Cloud instead; no host service exists ([ADR 007](../adr/007-supabase-brevo-container-host.md)) | Confirm the Data API setting. Delete the earlier Sydney project if it still exists. Create the test-site project and the host service before the first deployment |
| G7 PRD approval | lumabanls | PRD is Draft | Review the PRD and its open decisions; record approval with a date |
| G8 Review current changes | lumabanls | F01 and its records were committed in `af9d3ea` on 2026-11-13. The 2026-11-16 F02 work (theme switch, name survey and record updates) is uncommitted against it | Review the current diff and commit |
| G9 Design prototype walkthrough | lumabanls | The visual direction is applied under the delegated design brief and the name Tax Buddy by the user's instruction ([DESIGN](../DESIGN.md#visual-system)); 2026-11-13: the user chose the Paper & Forest palette and instructed starting F01. A recorded walkthrough of the whole prototype, unresolved D03, D06 and D18 behaviour, and production accessibility are still pending | Walk through the F01 pages and the F02 theme switch in the running app (SETUP), including whether the switch's own header row on phones is acceptable, and the rest of the [prototype](../../prototypes/README.md); record acceptance |
| G10 Devices for browser checks (PRD D17) | lumabanls | Browsers decided, and Chrome, Edge, Firefox and Brave are installed on the PC. No Mac, iPhone or Android phone is recorded for Safari and phone checks | Record the devices in the TEST-PLAN matrix |
| G11 Email service (PRD D18) | lumabanls | Service decided: Brevo. Email is built (F14, 2026-11-23) with a development sender, and D18 frequency is decided. Still missing: a Brevo account, sender address and sender domain | Create the Brevo account, verify a sender domain, then store `Email:Brevo:ApiKey`, `Email:SenderAddress` and `Email:SiteAddress` (user-secrets locally, the host's secret store when deployed) |
| G12 Report handling (PRD D22) | lumabanls | Partly decided 2026-11-18 (F10): reports are stored in the `reports` table and read in Supabase's Table Editor ([RELEASE](../RELEASE.md#reading-user-reports-r15-d22)); the email to the owner waits for Brevo (G11). Response time decided 2026-11-21: 10 working days. No removal steps are defined | Decide the removal steps before anyone beyond people you know uses the site |
| G13 Legal text (PRD D10) | lumabanls | Since 2026-11-16 `/terms` and `/privacy` exist as clearly marked drafts (F04) built from the outline; registration stores the agreed version (`draft-2026-11-16`). Since 2026-11-21 a [drafting prompt](../legal/terms-privacy-drafting-prompt.md) holds the fact sheet and instructions, and drafts of the [terms](../legal/terms-of-use.md) and [privacy policy](../legal/privacy-policy.md) exist with [drafting notes](../legal/drafting-notes.md). The user decided 16 policy questions the same day (PRD D07, D09, D19; D10, D14, D22 partly), and the prototype has the approved changes. Later that day the user gave the operator details. There's no lawyer, so the owner approved the text as version `2026-11-21` without a legal review, and [F11](../../specs/legal-pages/spec.md) put it on the site with agreeing again for older accounts. On 2026-11-23 version `2026-11-23.2` switched on saved accountants, service work and documents as built (Microsoft's blocks stay off: archived); **it awaits the owner's approval**. On 2026-11-25 version `2026-11-25` added a client's details (F17), also awaiting approval, and every signed-in person agrees again on their next visit | Before the public launch: a lawyer's review is still advised (the owner's risk if skipped; issues in the [drafting notes](../legal/drafting-notes.md#2-issues-for-counsel)); register the business name with the DTI before using "xjor.inc"; build account deletion (R09) so erasure requests don't depend on email |
| G14 Private service workflow and file policies | lumabanls | R19–R23 categories authorized; private-save/request/accept confirmed; D24–D25 prototype proposals adopted and D26/D28 decided on 2026-11-23; both F03 stages built that day. Still open: the per-service document count (working value 50) and quotas, retention of service work and relationship ending (D27), and a privacy impact assessment before real documents (legal issue 2.10) | Decide the open items, create the `service-documents` bucket, and walk through the service pages ([RELEASE](../RELEASE.md#private-service-documents-gate)) |
| G15 Prototype and contract conflicts (C03, C04, C05) | lumabanls | C03 resolved 2026-11-12: the prototype directory now follows the SDD's static contract ([evidence](evidence/2026-11-12-c03-directory.md)). C04 is open: the PRD keeps drafts through connection loss while the SDD restores only chat text ([conflict table](../README.md#history-and-unresolved-conflicts), [review record](evidence/2026-11-12-prototype-scope-review.md)). C05 (prototype client details without a requirement) was resolved on 2026-11-25 by PRD R25 and F17 | Decide C04 before the first interactive form; then update the SDD, PRD or prototype to match. The review record's other open decisions are whether real file intake (R23) stays in release 1 and whether the checklist uses one status control |
| G16 Google OAuth client (F04 GS09) | lumabanls | Development part closed 2026-11-16: the owner created the client (Testing mode, redirect `http://localhost:5234/signin-google`), stored its ID and secret in WSL user-secrets, and signed in for real ([evidence](evidence/2026-11-16-f04-google-sign-in.md)) | Before deployment: add the deployed site's `https://<host>/signin-google`, put the ID and secret in the host's secret store, and persist data-protection keys outside the app (SDD keys and secrets). Before real users: publish the consent screen or list the testers |
| G17 Supabase Storage for profile pictures (F06) | lumabanls | Development part closed 2026-11-16: the owner stored `Supabase:Url` and `Supabase:SecretKey`, the private `profile-pictures` bucket exists, and the real round trip passed ([evidence](evidence/2026-11-16-f06-profile-pictures.md#real-supabase-storage-round-trip)) | Before deployment: put the key in the host's secret store and use a separate bucket or project for the test site |
| G18 Single server for live chat (F07) | lumabanls | Live updates use an in-process notifier, so the site must run as one instance with WebSockets. Since 2026-11-24 the host is one OCI VM running one `web` container behind Caddy, which passes WebSockets through ([ADR 012](../adr/012-oci-compute-docker-host.md)) | Keep one container; a second instance needs a shared notifier and session affinity first |

## Recent delivery changes

2026-11-10:
- Repository created from maui-starter-kit `8069179`, and its documents filled for a MAUI app on Android and Windows.
- A clickable design prototype was added as a proposal for review.

2026-11-11:
- The owner changed the platform to a .NET web application ([ADR 005](../adr/005-web-platform-blazor.md), [ADR 006](../adr/006-web-sign-in.md)).
- PRD, SDD, SETUP, DESIGN, DEVELOPMENT, RELEASE, TEST-PLAN, REFERENCES, the F01 spec and the prototype were reworked for the web. `docs/API.md` was removed. ADRs 001–004 were marked superseded.
- R08 changed from push notifications to email.
- A design pass on the prototype added three visual directions and user-experience changes, all as proposals ([DESIGN-DIRECTIONS](../../prototypes/DESIGN-DIRECTIONS.md)).
- Stack decisions: Supabase for the database, Brevo for email, and Chrome, Edge, Firefox, Brave and Safari as supported browsers. Vercel was ruled out as the host because it cannot keep the Blazor app's live connections open ([ADR 007](../adr/007-supabase-brevo-container-host.md)).
- The owner considered Next.js, chose to stay with C#, and approved the technology stack table in the [SDD](../SDD.md#technology-stack), including Tailwind CSS, xUnit and Render.
- A second design pass studied reference websites and added a Modern look and a website layout to the prototype, as proposals.
- The owner set a new product direction: a portfolio site with client ratings and no approval of accountants ([ADR 008](../adr/008-portfolio-ratings-no-approval.md)). Requirements R12–R18 were added, and the prototype gained portfolio highlights, ratings and reviews, report and block, legal and information pages, and emails for clients.

The earlier web-platform documentation work established no application verification ([evidence](evidence/2026-11-11-web-platform-change.md)). The later Kuwenta refresh applied the delegated brand/design choice and verified only the local prototype ([evidence](evidence/2026-11-11-kuwenta-refresh.md)); application verification and acceptance remain unchanged.

The subsequent F03 expansion added PRD R19–R23, ADR 009, private workspace contracts and clickable client/service/progress/document screens. The owner confirmed private saving followed by service request and accountant acceptance. Remaining lifecycle/edit/file policies stay explicit; [new prototype observations](evidence/2026-11-11-client-services.md) establish no production file or server behavior.

2026-11-12:
- The user renamed the app from Kuwenta to Tax Buddy (PRD D01, [F02 BD07](../../specs/brand-design-refresh/spec.md#rename-to-tax-buddy-2026-11-12)). The prototype, its stylesheet and logo files, current documents and planned project names were updated, and the wordmark was redrawn. The prototype checks were rerun on the renamed sources ([evidence](evidence/2026-11-12-tax-buddy-rename.md)). Name, trademark and domain availability were not checked.
- At the user's request, four colour palette proposals were added to the prototype's new Palette control beside the current Paper & Forest palette ([F02 BD08](../../specs/brand-design-refresh/spec.md#palette-options-2026-11-12), [evidence](evidence/2026-11-12-palette-options.md)). No palette was chosen and DESIGN's current palette is unchanged.
- At the user's request, the agent reviewed the prototype against the PRD, SDD and DESIGN for extra or costly features and stack fit, by reading only ([record](evidence/2026-11-12-prototype-scope-review.md)). Two conflicts were opened (C03, C04; gate G15). The prototype, PRD, SDD and DESIGN were not changed, and nothing was decided.
- Later the same day the user instructed resolving C03 as recommended and adopting the prototype's D04 and D05 proposals for now. The prototype directory became a submitted form with sort and page links, and the PRD, SDD, DESIGN and F01 documents were updated. The prototype checks were rerun ([evidence](evidence/2026-11-12-c03-directory.md)). F01's remaining prerequisites are recording the toolchain in SETUP (G1; SDK 11.0.100 was installed in WSL the same day), a Supabase development project (G6) and the prototype walkthrough (G9).

2026-11-13:
- On the user's instruction, with a pasted architecture note, the technical architecture was reviewed and updated ([ADR 010](../adr/010-single-project-per-page-render-modes.md)):
  - business rules and data access move into the one web project;
  - render modes are listed per page, and review submission is Interactive Server;
  - public pages are built to be indexable, while switching indexing on stays PRD D19.
- Two proposed parts fill gaps the note left: signed-in actions on static pages are plain named forms, and the new-chat page stays open to visitors for R06.
- The SDD, TEST-PLAN, SETUP, DEVELOPMENT, REFERENCES, the PRD's D19 row, the ADR indexes and the F01 plan, spec and tasks were updated. Documentation only: nothing was built or run.
- The user created the development Supabase project in Singapore and stored its connection string in user-secrets. The agent's check connected after correcting a placeholder host name that had come from its own instructions ([evidence](evidence/2026-11-13-supabase-dev-connection.md)). SETUP now records the region, version, storage location, format and troubleshooting. Gate G6's development part is done.
- The user approved ADR 010's two proposals, chose the Paper & Forest palette and instructed starting F01. F01 was implemented:
  - one web project with Domain, Application and Data folders, and two test projects;
  - EF Core 10 with the Npgsql provider, one migration and a synthetic development seed;
  - static directory and profile pages styled with Tailwind on DESIGN's tokens.
- Results: Release build with 0 warnings; 62 tests pass; emulated Edge checks pass after one layout fix ([evidence](evidence/2026-11-13-f01-browse-accountants.md)). The tests found two defects in error statuses, both fixed. SETUP, SDD, DESIGN, ADR 010, the PRD's D05 row and the F01 documents were updated. Nothing was committed.
- Three polish follow-ups were recorded for later, at the user's request ([F01 tasks](../../specs/browse-accountants/tasks.md#polish-follow-ups)): P1 feedback when following links inside the site, P2 shorter database timeouts, and P3 naming the environment in the start-up error. Not started.

2026-11-16:
- The user asked to "implement f02" and chose three items: a theme switch, a name availability check, and closing the F02 records.
- Theme switch ([F02 BD09](../../specs/brand-design-refresh/spec.md#theme-switch-in-the-application-2026-11-16)): the header now has Light / Dark / System. The choice is a cookie posted through a form with antiforgery, and the server applies it in the first response, without script.
  - Results: Release build with 0 warnings; 82 tests pass, with 0 skipped once the database secret was restored; 181 headless Edge checks pass ([evidence](evidence/2026-11-16-f02-theme-switch.md)).
  - The first test run found a defect in the new tests themselves, which was fixed.
  - On Windows, Application Control blocked the new binaries from running, so tests and the app ran in WSL after the SDK was reinstalled there.
  - DESIGN, SDD, PRD D10, SETUP and the F02 documents were updated.
- Name survey ([record](evidence/2026-11-16-name-availability.md)): conflicting active TaxBuddy tax products in the Philippines and India, and a live US TAXBUDDIES mark. Not clearance; D01 stays open with the owner.
- The F02 palette task was closed, since Paper & Forest was chosen on 2026-11-13. Nothing was committed.
- The user decided PRD D03: one role per account. They asked to focus on Google sign-in and chose draft legal pages. [F04](../../specs/google-sign-in/spec.md) was specified and built: Google sign-in, registration (role and agreement), sign-out, draft terms and privacy pages, and the header account menu.
  - Results: Release build with 0 warnings; 104 tests pass with 0 skipped; 152 headless Edge checks pass ([evidence](evidence/2026-11-16-f04-google-sign-in.md)). Six defects in the new test code were found and fixed; none in the product code.
  - Migration `AddAccounts` (four account tables) was applied to the development database.
  - Later the same day the owner created the Google OAuth client and signed in for real as a client (GS09). The account was checked in the database. Gate G16's development part is closed.
  - PRD, SDD, DESIGN, SETUP, TEST-PLAN and the F01 spec (FT01 clarification) were updated. Nothing was committed.
- The user asked for R05 next. They chose a static form for the profile editor (C04 for this page) and F01's limits as D04, plus at most 10 experience entries. [F05](../../specs/profile-editing/spec.md) was specified and built:
  - `/account/profile` for an accountant's own profile; "My profile" in the navigation; registration as an accountant from the home page lands on the editor;
  - saving an unfinished profile unlisted, with what's missing named; a stale save from another tab refused with the edits kept.
  - Results: Release build with 0 warnings; 148 tests pass with 0 skipped; 42 headless Edge checks pass on the real app ([evidence](evidence/2026-11-16-f05-profile-editing.md)).
  - Two product defects were found and fixed: a render before the form loaded, and a null experience list on a new profile. The second was found by the browser check and now has a regression test.
  - Migration `AddProfileOwner` was applied to the development database. The synthetic browser-check accounts were deleted.
  - Recorded deviation: no leave confirmation or disabled Save, since both need script.
  - PRD D04 and R05, the SDD, DESIGN, the C04 entry, TEST-PLAN and the docs index were updated. F05 is not staged or committed.
- The user made the prototype the origin of the design and asked that documentation be reviewed before implementing. Both are recorded in a new `CLAUDE.md` (which imports AGENTS.md), and in AGENTS.md, the prototype README, DESIGN and the agent's memory.
- [F02 BD10](../../specs/brand-design-refresh/spec.md#application-aligned-to-the-prototype-2026-11-16-bd10) rebuilt the built pages from their prototype screens:
  - the site bar with avatar and name, and on phones an app bar and tab bar;
  - sign-in and the role choice buttons;
  - a new static Account page with Sign out;
  - the profile editor with a listing checklist, service rows and the prototype's messages;
  - the legal pages in panels.
- The theme switch moved to the footer. Recorded deviations: the switch itself, static forms instead of script-only effects, and links to unbuilt features left out.
- Results: 149 tests pass, with 0 skipped; screenshots compared with the prototype ([evidence](evidence/2026-11-16-f02-prototype-alignment.md)). Synthetic accounts deleted.
- Not staged or committed.
- Later: the user committed F02 and F04 as `2b5407c`. F01's directory and profile pages were compared with the prototype side by side and aligned (hero, service buttons, cards, profile layout, contact panel, phone Message bar). 149 tests pass ([evidence](evidence/2026-11-16-f02-prototype-alignment.md#f01-pages-compared-and-aligned-later-the-same-day)).
- The user asked for editable profile pictures with a default avatar. The decisions were everyone, Supabase Storage (private bucket, served by the site), SkiaSharp re-encoding and initials as the default ([ADR 011](../adr/011-profile-pictures-supabase-storage.md)). PRD R24 was added and the F05 exclusion lifted.
- [F06](../../specs/profile-pictures/spec.md) built:
  - upload and remove on Account, with a link from the editor;
  - processing to a 400 × 400 JPEG with metadata removed;
  - `/pictures/{id}` with access rules;
  - the `Avatar` component everywhere;
  - migration `AddProfilePicture`.
- Results: 168 tests pass ([evidence](evidence/2026-11-16-f06-profile-pictures.md)). Real storage waits for the owner's key and bucket (new gate G17). HttpClient logs were lowered to Warning to keep account IDs out of the logs.
- Not staged or committed.
- Later: the owner stored the Supabase key; the private bucket already existed. A real round trip through the app passed: upload, an upright 400 × 400 picture on Account and in the header, a private client picture, removal, and an empty bucket afterwards. Gate G17's development part is closed.
- The user asked for chat (F07) next, with the agent's recommendations: D06 decided as one conversation per client and accountant and 2,000 characters; email (R08) left for its own feature after Brevo.
- [F07](../../specs/chat/spec.md) built:
  - `/chats`, `/chats/{id}` and `/chats/new/{accountantId}`, replacing the "coming soon" page;
  - live updates through an in-process notifier;
  - unread marks in the list and the navigation;
  - drafts and message IDs kept in the tab, so retries store one message;
  - 20 messages a minute;
  - client pictures visible to their accountants;
  - the connection notice as the prototype's banner;
  - migration `AddChat`, applied to the development database.
- Results: 190 tests pass with `-m:1`, and 14 of 14 browser checks pass ([evidence](evidence/2026-11-16-f07-chat.md)). Running both test projects in parallel can exhaust the development database's connections (SETUP troubleshooting). New gate G18: one server instance for live chat.
- Not staged or committed.
- 2026-11-17: the user asked for portfolio highlights (R12). D23 was adopted provisionally from the prototype: up to 6, with working field limits.
- [F08](../../specs/portfolio-highlights/spec.md) built:
  - the editor fieldset;
  - the profile section between Services and Experience;
  - "N highlights" on cards;
  - sample highlights in the seed;
  - migration `AddPortfolioHighlights`, applied to the development database.
- Results: 197 tests pass with `-m:1`, and 7 of 7 browser checks pass ([evidence](evidence/2026-11-17-f08-portfolio-highlights.md)).
- Not staged or committed.
- 2026-11-17: the user asked for the next feature with a list of considerations. The agent proposed R13 and R14 and asked four questions. The user chose:
  - a static review form instead of the SDD's Interactive Server, which closes C04 for reviews;
  - D21 first name and last initial;
  - the prototype's review text values (D23);
  - the "Highest rated" sort, without seed reviews or the Account row.
- [F09](../../specs/reviews/spec.md) built:
  - the review page with post, update and delete, and the states for visitors, accountants and clients without a reply;
  - the profile's rating line and Reviews section;
  - the owner's reply form;
  - the card's rating line;
  - the "Highest rated" sort;
  - migration `AddReviews`, applied to the development database.
- Results: 235 tests pass with `-m:1`, and 12 of 12 browser checks pass ([evidence](evidence/2026-11-17-f09-reviews.md)). Web test classes now run four at a time, after the development database ran out of connection slots (SETUP troubleshooting).
- Not staged or committed.
- 2026-11-18: the user asked to implement R15 with recommendations and choices for the open decisions. The user chose:
  - reports stored in the database and read in Supabase, with email later (D22, partly decided; gate G12 updated);
  - one static report page;
  - blocking covers messaging and reviews (new BR19);
  - one report per item and 10 a day.
- [F10](../../specs/report-block/spec.md) built:
  - the report page;
  - "Report this profile", "Report this review" and the chat's Report;
  - Block with confirmation, the blocked notes, Unblock and live updates;
  - sends refused while blocked, including through "Message";
  - the blocked client's review state;
  - migration `AddReportsAndBlocks`, applied to the development database.
- Results: 258 tests pass with `-m:1`, and 15 of 15 browser checks pass ([evidence](evidence/2026-11-18-f10-report-block.md)).
- Not staged or committed.
- 2026-11-18: the user asked to check the implementation against the prototype, follow it, and update the documentation ([F02 BD11](../../specs/brand-design-refresh/spec.md#prototype-alignment-check-2026-11-18-bd11)).
  - **Audit:** three read-only audits compared every built page with its render function.
  - **Changed to match the prototype:**
    - the state blocks, and the not-found and error copy;
    - the full-width site bar;
    - the directory hero for signed-in users, and "How it works" for visitors;
    - the profile's Message action, which now follows the viewer;
    - phone layouts for the profile, info pages and review page;
    - the editor's Save bar on phones;
    - chat details;
    - many small copy and spacing changes.
  - **Kept and recorded in DESIGN:** the user's own decisions and the deviations that the static pages or unbuilt features require.
  - **Stale lines fixed:** in DESIGN and in the F02 and F07 specs.
  - **Results:** 258 tests and 16 of 16 browser checks pass ([evidence](evidence/2026-11-18-prototype-alignment.md)).
  - Not staged or committed.
- 2026-11-18: the user chose a new logo and asked for it across the app ([F02 BD12](../../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)).
  - **Chosen:** direction C of three proposals, reworked three times at the user's request: a round speech bubble with one square corner holding a solid T, and a two-tone "Tax Buddy" wordmark.
  - **Prototype:** the five files in `prototypes/assets`, the inline copy for the palette options, the logo size (161 × 30) and a theme-aware symbol in the phone app bar.
  - **Site:** the same five files in `wwwroot/images`, and the `BrandLogo` size. No page markup changed.
  - **Documents:** DESIGN, PRD D01, the F02 spec, plan and tasks, the documentation index, and the asset, prototype and logo-direction READMEs.
  - **Results:** 186 prototype checks, the Release build and 154 application tests pass. 70 of 104 web tests failed to connect to the development database, which had no free connections; they are to be rerun ([evidence](evidence/2026-11-18-f02-logo.md)).
  - **Open:** stroke outlines, an originality and trademark check, and D01.
  - Not staged or committed.
- 2026-11-19: the user said "do this all" of three items left open after the logo change ([F02 BD12](../../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)).
  - **Illustration:** the directory's decorative ledger now carries the logo's bubble, in the prototype and the site.
  - **Outlines:** the wordmark is filled outlines in the logo files and in the prototype's inline copy.
  - **Build:** the Tailwind step runs in WSL through the Windows `node.exe` when there is no Linux Node; `-p:SkipTailwind=true` is no longer needed here.
  - **Survey:** similar marks and domain records, by web search and RDAP. Not clearance; D01 stays with the owner.
  - **Results:** 186 prototype checks, the Release build and all 258 tests pass ([evidence](evidence/2026-11-19-f02-logo-outlines-hero.md)).
  - Not staged or committed.
- 2026-11-19: the user asked for one font style matching the logo ([F02 BD13](../../specs/brand-design-refresh/spec.md#one-typeface-2026-11-19-bd13)).
  - **Choice:** Quicksand, the closest of ten open-licence families to the logo's drawn letters.
  - **Applied:** both font tokens in the prototype's Tax Buddy look and in the site; body 500, headings 700. No markup changes.
  - **Files:** two font files and the licence in `prototypes/assets/fonts` and `wwwroot/fonts`. DESIGN's "system fonts, no downloads" rule is replaced.
  - **Results:** 186 prototype checks, the Release build and all 258 tests pass ([evidence](evidence/2026-11-19-f02-typeface.md)).
  - Not staged or committed.
- 2026-11-19: the user asked whether the prototype had the profile picture change, and for a simpler design behind a pencil on the picture ([F06 PP09](../../specs/profile-pictures/spec.md#pencil-on-the-picture-2026-11-19-pp09)).
  - **Finding:** the prototype had none; only the site did.
  - **Prototype:** a pencil on the Account and editor avatars; a picture panel with upload, remove and close; four new checks.
  - **Site:** the same, as a plain link and a panel that needs no script. Forms, addresses and messages unchanged.
  - **Results:** 190 prototype checks, the Release build and all 259 tests pass ([evidence](evidence/2026-11-19-f06-picture-pencil.md)).
  - **Open:** a look at the running site in a signed-in browser with a real upload.
  - Not staged or committed.
- 2026-11-19: the user asked to see the picture design choices in the prototype ([F06 options](../../specs/profile-pictures/spec.md#picture-design-options-2026-11-19)).
  - **Prototype only:** a "Picture design" control with options A (current) to E; the site is unchanged.
  - **Results:** 201 prototype checks pass, 11 of them new; A, B and E were run with a real file chosen, and all five were seen in screenshots ([evidence](evidence/2026-11-19-f06-picture-pencil.md#picture-design-options-added-later-the-same-day)).
  - **Open:** the owner's choice.
- 2026-11-19: the user chose picture design E ([F06 PP10](../../specs/profile-pictures/spec.md#pick-preview-save-2026-11-19-pp10)).
  - **Site:** the pencil opens the file chooser; preview, Save and Cancel under the name; "Remove picture" as a link; results in a banner under the name card. One new script file; the server side is unchanged. The panel stays for script off.
  - **Prototype:** E is the default design; A, B, C and D stay selectable.
  - **Results:** 202 prototype checks, the Release build and all 259 tests pass ([evidence](evidence/2026-11-19-f06-picture-option-e.md)).
  - **Found, not changed:** the sign-in page's failure banner has the class `inline`, which Tailwind also defines as `display: inline`; the same class broke this work's banner until it was removed there.
  - Not staged or committed.
- 2026-11-21: the user asked for a prompt to draft professional terms of use and a privacy policy (G13, PRD D10).
  - **Added:** a [drafting prompt](../legal/terms-privacy-drafting-prompt.md). It holds a fact sheet compiled from the PRD, SDD, ADRs, SETUP, RELEASE, the F04–F10 specs and the draft pages; Philippine law to consider; placeholders for the operator's details and open decisions; and the sections to produce. No legal text was written, and the site is unchanged.
  - **Fixed, at the user's go-ahead:** stale lines left from F06 (R24, ADR 011). The PRD still excluded profile photos, and SDD lines 7 and 130 said Supabase stores no files. ADR 007 and ADR 010 now point to ADR 011, which amends them.
  - **Updated the same day:** the user described the site as like social media, where accountants post their professional credentials publicly and anyone can join. The prompt now treats Tax Buddy that way and covers self-published credentials, licence details as sensitive personal information, public content, impersonation, scraping and takedown requests. Feeds, posts, follows, likes and comments remain excluded (PRD).
  - **Drafted the same day:** at the user's request ("do the prompt"), the agent ran the prompt. The results are the [Terms of Use](../legal/terms-of-use.md), the [Privacy Policy](../legal/privacy-policy.md) and the [drafting notes](../legal/drafting-notes.md), which hold the placeholders, issues for counsel, suggested product changes, coverage and sources. Four legal points were checked against secondary web sources: RA 11967, NPC Circular 2023-04, RA 9298 and RA 8792 Section 30. Not legal advice; `/terms` and `/privacy` on the site are unchanged.
  - **Decided the same day:** the user asked whether the drafts were in the prototype (they weren't), then answered 16 questions in this session ([list](../legal/drafting-notes.md#decisions-made-on-2026-11-21)).
    - PRD D07, D09 and D19 were decided, and D10, D14 and D22 partly. The drafts, PRD, SDD, DESIGN, RELEASE, TEST-PLAN TP14 and the ADR index were updated.
    - **Prototype:** the approved changes and a long-document layout for the legal pages. 202 checks pass ([evidence](evidence/2026-11-21-legal-decisions-prototype.md)).
    - **Site:** a follow-up feature, at the user's choice.
  - **Built the same day ([F11](../../specs/legal-pages/spec.md)):** the user gave the operator details (Lemnuel Lumaban, Manila, contact email; no DTI registration, so "xjor.inc" isn't used yet) and decided D27 partly (uploaded documents go with the account). They asked for simple report steps, now in [RELEASE](../RELEASE.md#acting-on-a-report-d22), said there is no lawyer, and asked to proceed.
    - **Text:** the owner approved it as version `2026-11-21`; the agent filled the minor placeholders with the recommended defaults, listed in the drafting notes.
    - **Site:** full legal pages, `/account/agree` for older agreements, the new registration wording, and the chat, editor, profile and report prompts.
    - **Prototype:** a new "Updated terms" screen. A missing fake address found by a screenshot was fixed.
    - **Results:** 268 tests pass (9 new), Release with 0 warnings, 202 prototype checks, legal pages viewed in Edge ([evidence](evidence/2026-11-21-f11-legal-pages.md)).
  - Not staged or committed.

2026-11-23:
- The user asked how Tax Buddy handles someone disconnecting it in their Google account. Answered from the code: nothing happens. No Google tokens are stored, sessions continue, and the account and profile remain. The suggested next step is account deletion (R09). No change was made.
- [F02 BD14](../../specs/brand-design-refresh/spec.md#theme-toggle-in-the-header-2026-11-23-bd14): at the user's instruction, the theme control moved from the footer to the header, as one moon/sun icon beside Sign in or the account, and at the end of the phone app bar, in the prototype and the site.
  - There's no System button; the device setting applies until the icon is pressed.
  - **Results:** 269 tests, 202 prototype checks, and Edge on the running site ([evidence](evidence/2026-11-23-f02-theme-toggle.md)).
  - Not staged or committed.
- The user asked for the missing features, then to "proceed on your suggested order": account deletion, then the information pages, then email once Brevo is set up, then client services once D24–D28 are decided.
  - [F12 account deletion](../../specs/account-deletion/spec.md):
    - **On the site:** a red "Delete account" item on Account, then two static steps.
    - **What happens:** the account is made nameless ("Deleted user"); a profile goes with its reviews, and a client's reviews go. Chats stay read-only for the other person, and a chat goes once both people are deleted. The picture is removed. Other sessions end within a minute: the security check was 30 minutes, now 1.
    - **Data:** migration `AddAccountDeletion`, applied to the development database.
    - **Legal text:** version `2026-11-23`, because "email us until deletion is available" is gone.
  - [F13 information pages](../../specs/information-pages/spec.md): About, Help and contact, and For accountants, linked from the footer, Account and (for visitors) the site bar.
  - **Results:** 285 tests, 202 prototype checks, and Edge ([evidence](evidence/2026-11-23-f12-f13.md)).
  - **Waiting on the user:** a Brevo account and sender domain for email (R08, G11), and decisions D24–D28 for client services (F03).
  - Not staged or committed.
- The user then answered four questions:
  - build email now with a development sender;
  - D18: one email per conversation until it is opened;
  - adopt the prototype's proposals for D24–D25 for now;
  - documents (D26, D28): PDF and images up to 10 MB, in Supabase Storage.
- [F14 email notifications](../../specs/email-notifications/spec.md):
  - **What's built:** queued with each message; sent after 2 minutes unless read; retries; a signed turn-off link; the Account row; `/email/off`.
  - **Data:** migration `AddEmailNotifications`, applied to the development database.
  - **Legal text:** emails and Brevo added to version `2026-11-23` (still unpublished).
  - **Results:** 301 tests pass ([evidence](evidence/2026-11-23-f14-email.md)).
  - Not staged or committed.
- [F03 client services, stage 1](../../specs/client-services/spec.md):
  - **What's built:** saved accountants ("Add accountant" on profiles), service requests, accept and decline, the accountant adding a service for an existing client, checklists with four statuses and safe concurrent saves, shared notes, and My accountants / Clients in the navigation.
  - **Pages:** all static forms, so C04 closes for service work.
  - **Data:** migration `AddClientServices`, applied to the development database.
  - **Results:** 309 tests pass ([evidence](evidence/2026-11-23-f03-stage1.md)).
  - **Next:** stage 2, documents (R23), with the D26/D28 choices: PDF and images up to 10 MB in Supabase Storage.
  - Not staged or committed.
- [F03 client services, stage 2: documents](../../specs/client-services/spec.md#stage-2-in-the-application-2026-11-23) and [F15 Microsoft sign-in](../../specs/microsoft-sign-in/spec.md), at the user's request "implement Service documents and Microsoft sign-in":
  - **Documents:** PDF, JPEG and PNG up to 10 MB, checked by content, attached to the whole service or a task by either participant of accepted work, downloaded only by the two of them, removed only by the uploader after a confirmation, and deleted with the uploader's account. No virus scanning (D28).
  - **Microsoft:** built, then archived the same day at the user's instruction ("google signin is enough … just hide it"): off behind `Authentication:Microsoft:Enabled`, so the site shows Google only. The code and its tests stay.
  - **Data:** migration `AddServiceDocuments`, applied to the development database. Storage: the private bucket `service-documents`, created in the development project on 2026-11-23 (10 MB limit; PDF, JPEG and PNG only).
  - **Legal text:** version `2026-11-23.2` describes saved accountants, service work and documents, not Microsoft; it awaits the owner's approval (G13).
  - **Results:** 348 tests pass (188 unit, 160 web), 0 warnings; after the Microsoft archive 349 (161 web, with the new hidden-while-archived test), though two test classes then failed to drop their throwaway schemas ([evidence](evidence/2026-11-23-f03-stage2-f15.md#microsoft-sign-in-archived-the-same-day)).
  - **Also:** Help now has the prototype's service, documents and email questions.
  - **Waiting on the user:** `Supabase:Url` and `Supabase:SecretKey` in user-secrets on this machine, so the site can reach the bucket; the open G14 items; approval of the legal text.
  - Not staged or committed.
- 2026-11-24, at the user's request "add google logo on the sigin button": Google's "G" on "Continue with Google" ([evidence](evidence/2026-11-24-google-mark.md)); a recorded deviation from the prototype in DESIGN. The database was unreachable from the first network that day; on another network all 349 tests passed, two classes only on a rerun after a fixture-start failure whose cause wasn't captured. Committed on 2026-11-24.
- 2026-11-24, the user: "im changing the deployment to releasing to oic via dockcer", then chose a Compute VM with Docker Compose, Singapore, and an sslip.io address ([ADR 012](../adr/012-oci-compute-docker-host.md), replacing Render):
  - **Built:** the [Dockerfile](../../Dockerfile), [deploy/oci](../../deploy/oci/compose.yaml) with Caddy, kept data-protection keys and `/health` in the app ([F16](../../specs/deployment-oci/spec.md)).
  - **Checked:** 352 tests pass (188 unit, 164 web, 3 of them new); the published files ran locally with the container's settings; the migration bundle ran against the development database ([evidence](evidence/2026-11-24-oci-docker.md)).
  - **Not done:** building the image (no Docker on this PC) and deploying (no VM yet).
  - **Privacy text:** the hosting provider is now Oracle Cloud Infrastructure, Singapore, in pending version `2026-11-23.2`.
  - Committed on 2026-11-24.
- 2026-11-24, after the user asked whether the prototype's Account screen was completely implemented: "yes add the clients row, keep my reviews out also consider the edit profile picture for both account and client based on the prototype".
  - **Built:** Account's first row now opens the workspace: "Clients and service work" for an accountant, "My accountants and service progress" for a client ([F03 CS13](../../specs/client-services/spec.md#account-row-2026-11-24)). "My reviews" stays out.
  - **Checked:** picture editing on Account was already the same for both roles; a new test now covers the client's pencil. 354 tests pass (188 unit, 166 web, 2 of them new). Both roles' rendered Account pages were viewed in Edge at desktop and phone widths ([evidence](evidence/2026-11-24-account-workspace-row.md)).
  - **Then fixed, at the user's "yes fix both":** a long email address with no break points ran under "Turn off" on phones. It now wraps, in the prototype and the site. DESIGN's site shell row was rewritten to describe the navigation as built. The prototype checker passes (202 checks) ([evidence](evidence/2026-11-24-account-workspace-row.md#follow-up-the-same-day-long-addresses-and-the-site-shell-row)).
  - Committed on 2026-11-24.
- 2026-11-24, the user: "check prototype vs site implementation difference per user and page". A code comparison of every page for visitors, clients and accountants ([record](evidence/2026-11-24-prototype-site-comparison.md)). It lists unrecorded differences and two bugs:
  - **Bugs:** today's service-history times are printed twice; a failed add or remove shows as success.
  - **Stale:** four "left out until built" deviations whose feature is now built.
  - **Documents out of date:** a list.
  - **Waiting on the user:** which differences to fix in the site and which to record as deviations. Nothing changed in the code.
- 2026-11-24, the user: "yes fix the first two" ([evidence](evidence/2026-11-24-prototype-alignment-fixes.md)).
  - **Fixed:** the two bugs (history times, the green failure line) and the four lapsed items ("See how listing works", the chat email note, the deletion list, and the deleted-account note's documents).
  - **Layout:** the workspace pages use the prototype's full-width frame with a breadcrumb and h1. The site bar marks the prototype's item on every page. Not-available states keep their frames: chat panes, the review panel, the workspace frame. Request a service answers 404 like other workspace pages.
  - **Checked:** 374 tests pass (206 unit, 168 web). The rendered pages were viewed in Edge at desktop and phone widths.
  - **Still open:** the comparison's other differences.
  - Committed on 2026-11-24.
- 2026-11-24, the user asked for navigation recommendations ("like sidebar panel for the private pages"), then said "show in the prototype". Later: "proceed with that prompt, profile as step 2", for a page of design options for the client's flow after sign-in ([evidence](evidence/2026-11-24-prototype-design-options.md)).
  - **Recommended:** no sidebar across the whole site, because the private area has only three or four sections. Instead, a list beside the workspace pages and a breadcrumb that names the client.
  - **Built in the prototype:**
    - the "Workspace navigation" switch, with that side-list proposal;
    - `prototypes/options.html`, with options for the directory (A–C), profile (A, B), chats (A–C) and My accountants (A–C), from the user's picks of a one-line list.
  - The current design stays the default; DESIGN, the specs and the site are unchanged.
  - **Checked:** 202 prototype checks pass on the unchanged default. Every option was viewed on desktop and phone in Edge. 14 default pages match `5237dab`.
  - **Waiting on the user:** a choice per page. Each choice then goes into DESIGN and the feature spec, and becomes the prototype's default.
  - Committed in `06e9be6` and pushed on 2026-11-25.
- 2026-11-25, the user: "now update prototype design recommendations for accountant pages" ([evidence](evidence/2026-11-25-prototype-accountant-options.md)). They kept all twelve options proposed.
  - **Built in the prototype:** a Client / Accountant flow switch on `options.html`. The accountant's steps are My profile, Chats, Clients and Service progress, with options A–C each. The current design stays the default; DESIGN, the specs and the site are unchanged.
  - **Checked:** 202 prototype checks pass, 21 click checks pass, and 16 default pages match `06e9be6`. Every option was viewed on desktop and phone in Edge.
  - **Found, not fixed:** on a service still awaiting acceptance, the accountant's own checklist uses the client's wording ("Updated by your accountant").
  - **Waiting on the user:** a choice per page, for both flows.
- 2026-11-25, the user: "on the client side i want users to store and edit thier basic information and their profile information suggest design recommendations and show it the prototype" ([evidence](evidence/2026-11-25-prototype-client-details.md)).
  - **The user chose:**
    - the fields: name, phone and city; business name, type and industry;
    - who sees them: the accountants the client contacts;
    - the designs: A, B and C;
    - where accountants see them: their client page and service requests.
  - **Built in the prototype:** a fifth client step, My details, on `options.html`, with the accountant's side behind the same options. The current design stays the default.
  - **Checked:** 33 click checks and 202 prototype checks pass; 14 default pages match `06e9be6`.
  - **Not in the PRD:** clients have only the Google name and email and a picture today. Building this needs a PRD requirement, a privacy-policy update and a data plan. DESIGN, the specs, the PRD and the site are unchanged.
  - **Waiting on the user:** a design choice and the scope decision.
- 2026-11-25, the user: "implement the edit on account in the prototype" ([evidence](evidence/2026-11-25-prototype-client-details.md#follow-up-the-same-day-edit-on-account-is-the-prototypes-default)).
  - **Done:** design B is the prototype's default for clients: a "Your details" card on Account, edited in place, with focus and a saved line. A and C are marked not chosen. The prototype checker covers it.
  - **Recorded:** [F02 BD15](../../specs/brand-design-refresh/spec.md#client-details-on-account-2026-11-25-bd15) and conflict [C05](../README.md#history-and-unresolved-conflicts), because the PRD and the privacy policy don't cover these details. DESIGN says the site leaves the card out.
  - **Checked:** 212 prototype checks and 38 click checks pass; only the four intended pages differ from `06e9be6`.
  - **Waiting on the user:** C05. Either approve a requirement and privacy-policy update so the site can build it, or take it out of the prototype.
- 2026-11-25, the user: "implement to site and also how to resolve profile change/upload in the build?" ([evidence](evidence/2026-11-25-f17-client-details.md)).
  - **Picture upload:** the user-secrets had an empty `Supabase:Url` and the secret key stored as an entry's name. Fixed: both buckets answer as private. The key was shown in the session, so the owner should rotate it ([SETUP](../SETUP.md)).
  - **Built [F17](../../specs/client-details/spec.md) (PRD R25):**
    - a client's "Your details" card on Account, edited in place;
    - the accountant of a service sees "About Maria" on the client page and in requests;
    - migration `AddClientDetails`, applied to the development database;
    - account deletion clears the details.
  - **Legal:** the privacy policy names the details. Version `2026-11-25` awaits the owner's approval, and signed-in people agree again.
  - **Checked:** 400 tests pass (23 new); Release build with no warnings; pages viewed in Edge; the prototype, aligned with the site, passes its 212 checks.
  - **Waiting on the user:** rotate the Supabase key, approve version `2026-11-25`, and try both on the running site.
- 2026-11-25, the user: a loading and busy-feedback standard, adapted from another web app's audit of about 60 loading defects. Documentation only.
  - **Written, all Proposed:**
    - DESIGN [Loading and busy feedback](../DESIGN.md#loading-and-busy-feedback): states, the indicator table, timings, shape, refreshes, writes, the spinner with its computed contrast, failures, live connections and component labels;
    - DEVELOPMENT's review checks;
    - an SDD note on streaming and prerendered state;
    - TP28, PRD D29–D34 and gate G19;
    - REFERENCES rows for the Blazor pages read.
  - **Adapted to the repository:** the request described a repository without source, where the account, editor and workspace pages were interactive. In fact F01–F17 are built, and only the chat panel is interactive. So no streamed skeletons are proposed for static pages (ADR 010 and the 404/503 statuses), and the prototype's spinner sizes and button words stay until D34.
  - **Recorded, not changed:** the SDD's render-mode rows for the workspace pages still say Interactive Server.
  - **Checked:**
    - the documentation checker (`--ready`) passes: 132 files, 1,438 local links, 0 errors;
    - the API names against the .NET 11.0.0 reference assemblies, SDK 11.0.100's `blazor.web.js` and the docs' source.
  - **Not checked:** the contrast ratios were computed from the palette values, not measured. Nothing was built, and nothing was checked in a browser.
  - **Waiting on the user:** D29–D34.
- 2026-11-25, the user: "go with your recommendations for D29 to D34" ([PRD](../PRD.md#open-decisions)).
  - **Recorded as decided:** the agent had listed the options without a pick for each, so it stated its recommendations after the instruction:
    - D29: 300 ms and 400 ms, as working values;
    - D30: skeletons pulse, as in the prototype;
    - D31: static forms run no script, and the server makes a repeated post change nothing;
    - D32: no page streams;
    - D33: a thin bar along the top of the window during moves between pages ([F01 P1](../../specs/browse-accountants/tasks.md#polish-follow-ups) option A);
    - D34: the prototype's spinner sizes, button words and connection notice, and no session screen of its own.
  - **Updated:** PRD, DESIGN, DEVELOPMENT, the SDD (its submit-button sentence now covers interactive pages only), TP28 and F01 P1.
  - **Found while checking D31:** `WorkService.AddTaskAsync` and `PostNoteAsync` have no repeat guard, so a double-clicked "Add task" or shared note is stored twice (BR18, TP20). Recorded in G19; not fixed.
  - **Not done:** nothing was built or checked in a browser.
- 2026-11-25, the user: "in the My profile implement Option B: Preview beside the form in the prototype" ([evidence](evidence/2026-11-25-prototype-editor-preview.md)).
  - **Done:** B is the prototype's default for the accountant's editor ([F02 BD16](../../specs/brand-design-refresh/spec.md#my-profile-preview-beside-the-form-2026-11-25-bd16)). It shows the directory card, as last saved, beside the form. On phones and below 860 px it's a disclosure above the form.
  - **Options page:** B is Current. "Before" shows the one long form (`opt=editor-current`), and A and C are marked not chosen.
  - **Recorded:** DESIGN and F05 say the site still shows the long form.
  - **Checked:** 224 prototype checks (12 new) and 43 click checks pass.
  - **Waiting on the user:** whether to build it on the site, and choices for Chats, Clients and Service progress.
- 2026-11-26, the user: an accountant dashboard as design options in the prototype ("what needs me today?" and "how is my listing doing?"), with existing data only ([evidence](evidence/2026-11-26-prototype-dashboard-options.md)).
  - **Built in the prototype:** step 5 of the accountant's flow on `options.html`:
    - A: a "Needs you" list at `/dashboard`;
    - B: tiles and sections at `/dashboard`;
    - C: an overview at the top of Clients.
  - **Navigation:** A and B add Dashboard first in the site bar, and in place of Accountants in the phone tabs; C changes nothing.
  - **States:** a State switch shows Andrea, a busy accountant, a new one, a listed one with no clients, and a failed load.
  - **Left out:** profile views, earnings, reply times, "new since your last visit", due dates and trends. Each needs a PRD decision.
  - **Unchanged:** the current pages stay the default; the site, the PRD and DESIGN are unchanged.
  - **Checked:** 259 prototype checks (35 new) and 11 click checks pass. Screenshots were viewed in light and dark on desktop and phone; three faults were found and fixed.
  - **Waiting on the user:** a choice. A dashboard isn't in the PRD, so building one needs a scope decision and an SDD navigation row. C would also settle the Clients options.
- 2026-11-26, the user asked "is this the best homepage you can recommend for the accountant users?", then said "archive dashboard design in the prototype for now" ([evidence](evidence/2026-11-26-prototype-dashboard-options.md#follow-up-the-same-day-archived)).
  - **Recommended in the conversation, not built:** no separate dashboard. Instead, make Clients the accountant's home:
    - signed-in accountants land there;
    - a short "Needs you" strip with requests and chats waiting;
    - a listing banner until they're listed.
  - **Done:** the Dashboard left the accountant's steps on `options.html`, which are back to four. It's listed under Archived with a note; no option was chosen, and the designs still work.
  - **Checked:** 259 prototype checks and 14 options-page click checks pass.
- 2026-11-26, the user, with screenshots of Clients and a client's page: "create recommended designs for these, it shows redundant titles" ([evidence](evidence/2026-11-26-prototype-page-titles.md)).
  - **The problem:** every workspace page stacks a breadcrumb that ends with the page, a page-type heading, the "Your private workspace" eyebrow and a second large title. Phones repeat the title under the app bar.
  - **Built in the prototype:** step 5 of the accountant's flow on `options.html`, "Page titles", with a View switch over every workspace page:
    - A: the name as the only title, under a breadcrumb of the pages above (recommended);
    - B: Back, the name and a privacy pill in one bar;
    - C: "Clients / Maria Santos / Bookkeeping" as one title line.
  - **Unchanged:** the current design stays the default; the site, DESIGN and the PRD are unchanged.
  - **Checked:** 279 prototype checks (20 new) and 18 click checks pass. Screenshots were viewed on desktop and phone.
  - **Waiting on the user:** a choice. The site's `WorkFrame` and `WorkIntro` would then follow, with the sections' headings moved up a level.
