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
