# Delivery status

**Authority:** current implementation, verification, and acceptance state. Behavior belongs in [PRD](../PRD.md); procedures in [TEST-PLAN](TEST-PLAN.md).

Last update: 2026-11-12. Repository HEAD: `1fccef362fe04050977c3f550997ac6637091b4b`, which holds the F02/F03 work; the 2026-11-12 work (rename to Tax Buddy, palette options, prototype review record) is uncommitted on top of it. Earlier evidence retains its own exact source identity and the names of its day. Current focus: client/service/progress/document prototype expansion and unresolved policies D24–D28. Release readiness: **Not assessed**.

## Feature state

No application source exists. Every row is at its starting state.

| Requirement / feature | Implementation | Automated verification | Build | Browser verification | Acceptance |
|---|---|---|---|---|---|
| R01–R03 / [F01 browse-accountants](../../specs/browse-accountants/spec.md) | Not started | Not run | Not run | Not run | Pending |
| R04 sign-in and registration | Not started (no spec) | Not run | Not run | Not run | Pending |
| R05 accountant profile editing | Not started (no spec) | Not run | Not run | Not run | Pending |
| R06, R07 chat | Not started (no spec) | Not run | Not run | Not run | Pending |
| R08 email notifications | Not started (no spec) | Not run | Not run | Not run | Pending |
| R09 account deletion | Not started (no spec) | Not run | Not run | Not run | Pending |
| R10, R11 connection loss and privacy | Not started; covered by each feature | Not run | Not run | Not run | Pending |
| R12 portfolio highlights | Not started (no spec) | Not run | Not run | Not run | Pending |
| R13, R14 ratings, reviews and replies | Not started (no spec) | Not run | Not run | Not run | Pending |
| R15 report and block | Not started (no spec) | Not run | Not run | Not run | Pending |
| R16–R18 legal, information and error pages | Not started (no spec) | Not run | Not run | Not run | Pending |
| R19–R23 / [F03 client services](../../specs/client-services/spec.md) | Application not started; documentation/prototype task separate below | Application tests not run | Not run | Application not run | Pending; policies D24–D28 open |

States:
- Implementation: not started, in progress, or implemented.
- Verification: not run, pass, fail, blocked, or not applicable with a reason.
- Acceptance: pending, accepted, or rejected, with decision evidence.

Use separate browser rows when outcomes differ. A blocked check requires a concrete reason and next action.

### Prototype work, separate from application implementation

| Work | Implementation | Verification | Acceptance |
|---|---|---|---|
| [F02 rename to Tax Buddy (BD07)](../../specs/brand-design-refresh/spec.md#rename-to-tax-buddy-2026-11-12) | Applied locally 2026-11-12: display name, redrawn wordmark, renamed stylesheet/logo files, look id, example domain, planned project names, current documentation | 142 Edge checks passed on the renamed sources, including 34 contrast pairs; syntax, SVG and documentation checks pass; header screenshots and logo render agent-reviewed ([evidence](evidence/2026-11-12-tax-buddy-rename.md)) | Name set by the user's instruction; name/trademark/domain availability unchecked (D01); owner walkthrough and release acceptance pending |
| [F02 palette options (BD08)](../../specs/brand-design-refresh/spec.md#palette-options-2026-11-12) | Applied locally 2026-11-12: four colour proposals selectable in the prototype beside the current palette; colours only | 178 Edge checks passed, including 36 new palette checks and 17 contrast pairs per option and theme; syntax and documentation checks pass; directory screenshots agent-reviewed ([evidence](evidence/2026-11-12-palette-options.md)) | No palette chosen; Paper & Forest stays current in DESIGN. Owner choice pending |
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

Use the [evidence template](evidence/TEMPLATE.md). A template/procedure is not a completed execution record. A previous pass does not automatically apply to changed source or another platform.

## Remaining gates and decisions

| Gate / decision | Owner | Missing evidence or dependency | Next action |
|---|---|---|---|
| G1 .NET 11 SDK on the development PC | lumabanls | Not installed (checked 2026-11-11) | Follow [SETUP first checkout](../SETUP.md#first-checkout) steps 1–3; record the versions |
| G2 Android build host | — | Closed 2026-11-11: not needed for a web application | — |
| G3 Android device for native checks | — | Closed 2026-11-11: replaced by the browser matrix in TEST-PLAN | — |
| G4 Windows sign-in (C01, PRD D02) | — | Closed 2026-11-11 by [ADR 006](../adr/006-web-sign-in.md) | — |
| G5 F01 blocking decisions (PRD D04, D05) | lumabanls | Profile fields; search fields, filters and order | Decide before building the data model |
| G6 Database and hosting (PRD D11) | lumabanls | Database decided: Supabase. No Supabase project exists yet. Host decided: Render. Neither a Supabase project nor a Render service exists yet ([ADR 007](../adr/007-supabase-brevo-container-host.md)) | Create a development Supabase project before F01; create the Render service before the first deployment |
| G7 PRD approval | lumabanls | PRD is Draft | Review the PRD and its open decisions; record approval with a date |
| G8 Review current changes | lumabanls | F02/F03 work is committed in `1fccef3`. The 2026-11-12 rename to Tax Buddy, the palette options and the prototype review record are uncommitted against it | Review the current diff; commit when ready |
| G9 Design prototype walkthrough | lumabanls | The visual direction is applied under the delegated design brief and the name Tax Buddy by the user's instruction ([DESIGN](../DESIGN.md#visual-system)); owner walkthrough, unresolved D03–D06/D18 behavior, and production accessibility are still pending | Walk through the refreshed [prototype](../../prototypes/README.md), choose a palette or keep Paper & Forest ([options](../../prototypes/DESIGN-DIRECTIONS.md#palette-options)), decide remaining behavior and record acceptance before implementing F01 |
| G10 Devices for browser checks (PRD D17) | lumabanls | Browsers decided, and Chrome, Edge, Firefox and Brave are installed on the PC. No Mac, iPhone or Android phone is recorded for Safari and phone checks | Record the devices in the TEST-PLAN matrix |
| G11 Email service (PRD D18) | lumabanls | Service decided: Brevo. No account, sender address or sender domain yet | Needed before R08 work, not before F01 |
| G12 Report handling (PRD D22) | lumabanls | Reports go to the owner by email and there is no admin page. No inbox, response time or removal steps are defined | Decide before anyone beyond people you know uses the site |
| G13 Legal text (PRD D10) | lumabanls | The privacy and terms pages hold an outline only | Have the text written or reviewed before real users sign in |
| G14 Private service workflow and file policies | lumabanls | R19–R23 categories authorized; private-save/request/accept confirmed; remaining lifecycle, completion/sign-off, file types/quotas, deletion/retention and private storage/scanning D24–D28 open | Review the [F03 proposal](../../specs/client-services/spec.md) and resolve policies before implementing real service/file intake |
| G15 Prototype and contract conflicts (C03, C04) | lumabanls | The prototype directory behaves interactively while the SDD and F01 plan define a static page with a GET form; the PRD keeps drafts through connection loss while the SDD restores only chat text ([conflict table](../README.md#history-and-unresolved-conflicts), [review record](evidence/2026-11-12-prototype-scope-review.md)) | Decide C03 before building F01 and C04 before the first interactive form; then update the SDD, PRD or prototype to match. The same record lists four decisions, including whether real file intake (R23) stays in release 1 |

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
