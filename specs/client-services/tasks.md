# Client services tasks

Scope: [spec](spec.md). Approach: [plan](plan.md).

- [x] Inspect working tree and preserve F02 work; review relevant contracts/source.
- [x] Record user authorization, requirements, policy proposals and acceptance scenarios.
- [x] Update PRD, ADR, SDD, DESIGN and release/verification dependencies.
- [x] Implement private lists, request/accept, checklists, notes and shared progress in prototype.
- [x] Implement generated sample uploads/downloads, rejection, failure/retry and removal.
- [x] Verify role guards, drafts, offline behavior and asynchronous actor changes.
- [x] Run old/new browser checks, syntax and documentation checks; inspect responsive screenshots.
- [x] Record exact sources, environment, observations and cleanup in [new evidence](../../docs/delivery/evidence/2026-11-11-client-services.md); update STATUS.
- [x] Owner decides D24–D28 enough to build: the prototype's proposals adopted for now, plus D26/D28 for documents (2026-11-23). Prototype acceptance is still pending (G9).
- [x] Application stage 1: persistence and authorization for saved accountants, requests, checklists and notes; 8 web tests ([evidence](../../docs/delivery/evidence/2026-11-23-f03-stage1.md)).
- [x] Application stage 2: documents (R23) in a private Supabase Storage bucket, PDF, JPEG and PNG up to 10 MB, checked by content; 8 web tests and 24 unit tests ([evidence](../../docs/delivery/evidence/2026-11-23-f03-stage2-f15.md)).
- [x] Create the private `service-documents` bucket in the development project (2026-11-23, by SQL; [evidence](../../docs/delivery/evidence/2026-11-23-f03-stage2-f15.md#service-documents-bucket-created)).
- [x] The workspace row on Account, for both roles (CS13, 2026-11-24; [evidence](../../docs/delivery/evidence/2026-11-24-account-workspace-row.md)).
- [ ] Upload and download a real file through the site (needs `Supabase:Url` and `Supabase:SecretKey` in user-secrets; no automated check reaches Supabase).
- [ ] Owner decisions: the per-service count (working value 50), quotas, and the privacy impact assessment before real documents (legal issue 2.10).
- [ ] The signed-in workspace pages seen in a browser at phone and desktop widths.
