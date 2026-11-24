# Profile pictures tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md). Decision: [ADR 011](../../docs/adr/011-profile-pictures-supabase-storage.md). Evidence: [2026-11-16 record](../../docs/delivery/evidence/2026-11-16-f06-profile-pictures.md).

## Definition

- [x] Review the PRD, SDD, DESIGN, F05 and the prototype. Profile photos were an explicit F05 exclusion and Supabase Storage was "not used"; recorded as a scope change.
- [x] Owner decisions on 2026-11-16: everyone; Supabase Storage; re-encode with SkiaSharp; initials as the default.
- [x] Spec PP01–PP08, plan, ADR 011.

## Implementation

- [x] Processor (SkiaSharp 4.155.1): type check, 5 MB and 40 MP limits, upright by EXIF orientation, centred square, 400 × 400 JPEG, no metadata.
- [x] Service with storage and account interfaces; Supabase Storage client (private bucket, server-side key, memory cache); a "not set up" stand-in.
- [x] `users.picture_object` with migration `AddProfilePicture`, applied to the development database on 2026-11-16.
- [x] Upload, remove and serve endpoints; picture claim refreshed after changes; `Avatar` everywhere; Account picture section; editor link.
- [x] HttpClient request logs lowered to Warning (SDD diagnostics: no storage addresses with account IDs in the logs).
- [x] PRD R24, SDD, DESIGN, SETUP, TEST-PLAN, the ADR index, the F05 spec, STATUS and the docs index updated.

## Verification

- [x] 9 rule tests (processor and service) and 9 web tests; the whole suite has 168 passed, 0 failed, 0 skipped; Release build with 0 warnings.
- [x] Browser: the Account picture section at 390 and 1440 px, light and dark; a real upload through the form with storage unreachable shows the failure and keeps the picture (4 of 4).
- [x] Real Supabase Storage round trip on 2026-11-16, after the owner stored the URL and secret key; the private bucket already existed. Upload, display, access rules and removal all passed (5 of 5), and the bucket was empty afterwards ([evidence](../../docs/delivery/evidence/2026-11-16-f06-profile-pictures.md#real-supabase-storage-round-trip)).
- [x] A picture shown on the styled site (Account and header) from real storage, upright from an EXIF-rotated photo.
- [ ] An accountant's picture on the directory card and public profile with real storage. The web tests cover it with the in-memory store; the owner's walkthrough covers it for real.
- [ ] Owner walkthrough (G9).

Pencil on the picture, 2026-11-19 (PP09):

- [x] Add the pencil, the picture panel and its states to the prototype's Account screen and editor; four new checker checks (190 in all).
- [x] Follow it on the site: the pencil link and the panel that opens as the page's target, on Account and in the editor. One new web test.
- [x] Update DESIGN, STATUS, the documentation index and the prototype README.
- [x] Release build, all tests and the prototype checker; review the rendered pages ([evidence](../../docs/delivery/evidence/2026-11-19-f06-picture-pencil.md)).
- [ ] See the pencil and panel on the running site in a signed-in browser, including a real upload. Not done: the pages were reviewed as rendered by the test host.
- [ ] Owner walkthrough (G9).

Picture design options, 2026-11-19:

- [x] Put options A to E in the prototype behind a "Picture design" control, with 11 more checker checks (201 in all) that choose a real file for A, B and E.
- [x] Owner chose option E on 2026-11-19 ("implement option e").

Pick, preview, save, 2026-11-19 (PP10):

- [x] Site: the script, the Save, Cancel and Remove controls under the name, the result banner under the name card, and the panel kept for script off.
- [x] Prototype: E is the default design; the checker follows (202 checks).
- [x] Update DESIGN, the SDD's Account row, STATUS, the documentation index and the prototype README.
- [x] Release build and all tests; the script run in Edge against the rendered Account page in six states ([evidence](../../docs/delivery/evidence/2026-11-19-f06-picture-option-e.md)).
- [x] Both roles checked against the prototype's `meCardHTML` (2026-11-24): a client's Account has the same pencil, preview controls and panel as an accountant's, with the client's hint; a web test covers it and the rendered page was seen in Edge ([evidence](../../docs/delivery/evidence/2026-11-24-account-workspace-row.md)). The editor's pencil is for accountants only, since clients have no profile editor.
- [ ] A real upload through the pencil on the running site in a signed-in browser, including on a phone, where the chooser offers the camera.
- [ ] Owner walkthrough (G9).
