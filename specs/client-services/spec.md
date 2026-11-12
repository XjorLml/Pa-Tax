# F03: Client relationships, service progress and private documents

Status: scope expansion authorized for documentation and prototype, 2026-11-11. The user requested accountant client lists, per-client service checklists, a client's added-accountant list and files for service progress, then instructed execution of the expanded brief. [PRD](../../docs/PRD.md) R19–R23 own the behavior; [SDD](../../docs/SDD.md#private-service-workspaces) owns contracts; [DESIGN](../../docs/DESIGN.md#service-workspace-presentation) owns presentation. [Plan](plan.md), [tasks](tasks.md).

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

## Decisions and evidence limits

The private-save/request/accept model is confirmed. Other lifecycle, editing/completion and demo file policies remain proposals. Demo files: generated PDF/CSV/TXT, 5 MiB maximum simulated size; unsupported/oversize samples exist to exercise rejection. Actual storage provider, scanning, limits, retention, deletion and relationship termination must be decided before real documents are accepted. See [ADR 009](../../docs/adr/009-private-service-workspaces.md).

Prototype state is in memory and clears on reload. Per-actor drafts survive simulated failures/navigation within the page; no real document is selected, read or transmitted. Browser checks cannot establish server authorization, storage isolation or durable progress. Owner review and release acceptance remain pending.
