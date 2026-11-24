# F03: Client relationships, service progress and private documents

Status: scope expansion authorized for documentation and prototype, 2026-11-11. **Application stage 1 built 2026-11-23** (saved accountants, requests, accept and decline, checklists, notes; [evidence](../../docs/delivery/evidence/2026-11-23-f03-stage1.md)). **Application stage 2, documents (R23), built the same day** ([evidence](../../docs/delivery/evidence/2026-11-23-f03-stage2-f15.md); see [stage 2 in the application](#stage-2-in-the-application-2026-11-23)). The user adopted the prototype's D24–D25 proposals for now and chose D26/D28 for documents: PDF and images up to 10 MB in a private Supabase Storage bucket. The user requested accountant client lists, per-client service checklists, a client's added-accountant list and files for service progress, then instructed execution of the expanded brief. [PRD](../../docs/PRD.md) R19–R23 own the behavior; [SDD](../../docs/SDD.md#private-service-workspaces) owns contracts; [DESIGN](../../docs/DESIGN.md#service-workspace-presentation) owns presentation. [Plan](plan.md), [tasks](tasks.md).

## Scope and baseline

Extend the current HTML prototype (named Kuwenta at this baseline; renamed Tax Buddy on 2026-11-12); no application source exists. Preserve all pre-existing uncommitted F02 work and its attributed evidence. Baseline commit `668a9a9e122787f2c86f6f6b4a63c398cc78d282` plus that F02 working tree; the F02 report identifies its exact UI sources. No deployment or production file storage is authorized by this prototype task.

The feature categories are authorized. Unresolved policy stays in PRD D24–D28. The owner confirmed private save → service request → accountant acceptance in the async clarification on 2026-11-11 (selected “Save privately, then request a service” in the question describing acceptance before a shared relationship). The prototype follows that model. Saving is not a contract or a public follow. Accountants manage service tasks and shared notes; both participants attach generated sample documents. Client confirmation of completion, service fees, bookings, real uploads, progress emails and chat attachments are excluded from this change.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Boundary |
|---|---|---|---|
| CS01 / R19 | Accountant opens Clients | Only requesting/engaged clients for that accountant appear, grouped with their services; no public client directory | Prototype role simulation; later server integration |
| CS02 / R20 | Client adds an accountant from a profile | Added once to My accountants; no client relationship is created just by saving; empty and unavailable states are clear | Prototype |
| CS03 / R21 | Client requests a service; accountant accepts or declines | Pending request is distinct from active work; actions are limited to the relevant roles; acceptance creates a starting checklist | Prototype; confirmed save/request/accept model, other D24 policies open |
| CS04 / R21–R22 | Accountant changes tasks and posts a shared note | Saved statuses and notes appear to the client with author/time; progress is completed tasks / total tasks; zero tasks is not completion | Prototype |
| CS05 / R22 | Client or unrelated user opens service work | Client sees read-only progress for their own work; unrelated IDs show unavailable; no participant names/files leak | Prototype guard checks, not security proof |
| CS06 / R23 | Either participant selects a synthetic sample file and task | Uploading → available; correct service/task association; other participant can download a generated synthetic fixture | Local demo, no real input files |
| CS07 / R10, R23 | Unsupported/oversize sample, failed upload, offline or role switch during upload | Clear error; selection remains for retry; nothing queued; no duplicate, false success or insertion as a different actor | Controlled simulation |
| CS08 / R23 | Uploader removes an own file | Confirmation, then removal in this demo; counterpart cannot remove it. Production retention/deletion remain D27 | Prototype |
| CS09 / DESIGN | Desktop, phone, dark theme and keyboard | List, checklist, upload states and actions fit; controls labelled, progress textual, status announcements present | Recorded browser/layout checks |
| CS10 / preservation | Run existing regression checker and documentation check | Old flows preserved; current evidence is separate from F02; links and traceability resolve | Automated checks and diff |

## Stage 2 in the application (2026-11-23)

At the user's request "implement Service documents and Microsoft sign-in", with D26/D28 as decided (PDF, JPEG and PNG, checked by content, up to 10 MB, a private Supabase Storage bucket, no virus scanning) and the D27 proposal (uploader-only removal) adopted with the rest of the prototype's proposals. Contract: [SDD](../../docs/SDD.md#private-service-workspaces); presentation and deviations: [DESIGN](../../docs/DESIGN.md#service-workspace-presentation).

| ID / requirement | Given / when | Observable result in the application | Verification |
|---|---|---|---|
| CS06a / R23, BR17 | A participant of an accepted service uploads a PDF to a task | "Document attached."; both see it with uploader, time, task and size; the other downloads it as an attachment with its name; the history says "Attached … to …"; the service card counts it | Web test |
| CS07a / R10, R23, D26 | A non-PDF named `.pdf`, a PNG signature without an image, a PDF with no end marker, over 10 MB, an empty file, or another service's task | A message for each; the task choice kept and "Retry upload"; nothing stored | Web test, unit tests |
| CS07b / BR18 | The same upload form sent twice | One document, one history line | Web test |
| CS07c / BR18 | Storage refuses the write | "The document could not be uploaded…", nothing listed, a log line with the error type only | Web test |
| CS05a / BR15 | A stranger, another accountant, a visitor, a guessed ID, a file under another service, or an upload before acceptance | 404 (visitors sign in); nothing stored; "Documents can be added after acceptance." | Web test |
| CS08a / R23, D27 | The uploader removes their document; the other participant tries | A confirmation step, then "Document removed." and a history line; the other sees no Remove and a forged post changes nothing | Web test |
| CS11 / R09, D27 | A participant deletes their account | Their documents and bytes go, with a history line; the other's documents stay | Web test |
| CS12 / D28 | No document storage configured | "Document upload isn't set up on this site yet." | Web test |

Working value: 50 documents per service (D26 counts and quotas are open). Not done: a real Supabase bucket round trip (the `service-documents` bucket exists since 2026-11-23, but this machine has no Supabase API key for the site), the signed-in pages in a browser, and the privacy impact assessment the legal notes recommend before real documents (issue 2.10).

## Account row (2026-11-24)

The user asked whether the prototype's Account screen was completely implemented, and then: "yes add the clients row, keep my reviews out". The prototype's `meHTML` opens Account's menu with the workspace row, and DESIGN lists Account among the workspace's ways in, but stage 1 added only the site bar and phone tabs. "My reviews" stays out ([F09](../reviews/spec.md)).

| ID / requirement | Given / when | Observable result in the application | Verification |
|---|---|---|---|
| CS13 / R19, R20, DESIGN | A signed-in accountant or client opens Account | The first row links to `/workspace`: "Clients and service work" or "My accountants and service progress", with "Private checklists, shared updates and service documents" and a chevron; visitors don't see it; no "My reviews" row | Web test; rendered page in Edge ([evidence](../../docs/delivery/evidence/2026-11-24-account-workspace-row.md)) |

## Prototype alignment (2026-11-24)

After a [comparison](../../docs/delivery/evidence/2026-11-24-prototype-site-comparison.md), the user said "yes fix the first two" (its bugs and lapsed items, then the layout problems). Presentation: [DESIGN](../../docs/DESIGN.md#service-workspace-presentation).

| ID / requirement | Given / when | Observable result in the application | Verification |
|---|---|---|---|
| CS14 / DESIGN | A signed-in user opens any workspace page on a wide screen | The full page width, a breadcrumb starting with "Clients" or "My accountants", and the page's h1; the site bar marks Clients or My accountants, including on Request a service | Web test |
| CS15 / R20 | Adding or removing an accountant fails | "We couldn't change your list. Please try again." as a red alert, on the profile and in My accountants | Web test |
| CS16 / R22 | A history entry from today, yesterday or earlier | "10:42 AM", "Yesterday · 10:42 AM", "Nov 16 · 10:42 AM"; the time is never shown twice | Unit test |

## Decisions and evidence limits

The private-save/request/accept model is confirmed. Other lifecycle, editing/completion and demo file policies remain proposals. Demo files: generated PDF/CSV/TXT, 5 MiB maximum simulated size; unsupported/oversize samples exist to exercise rejection. Actual storage provider, scanning, limits, retention, deletion and relationship termination must be decided before real documents are accepted. See [ADR 009](../../docs/adr/009-private-service-workspaces.md).

Prototype state is in memory and clears on reload. Per-actor drafts survive simulated failures/navigation within the page; no real document is selected, read or transmitted. Browser checks cannot establish server authorization, storage isolation or durable progress. Owner review and release acceptance remain pending.
