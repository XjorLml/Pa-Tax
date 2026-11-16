# Profile pictures implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Decision: [ADR 011](../../docs/adr/011-profile-pictures-supabase-storage.md). Status: as built 2026-11-16, uncommitted.

## Changes to the current plan

- **F05:** "profile photos" leaves the exclusions. The editor gets a "Change picture" link to Account.
- **SDD:** Supabase Storage is now used, for pictures only.
- **Account page (BD10):** gains picture controls in the prototype's style.
- **Avatars:** one `Avatar` component replaces the hand-written ones in the header, Account, cards and the profile page.

## Affected modules

| Module | Change | Scenario |
|---|---|---|
| `Application/Pictures/PictureProcessor.cs` (new) | Validates and re-encodes with SkiaSharp: type check, 5 MB and 40 MP limits, centre square, 400 × 400 JPEG at quality 85 | PP02, PP03 |
| `Application/Pictures/IPictureStorage.cs`, `ProfilePictureService.cs` (new) | Upload (process, store a new object, point the account at it, delete the old one), remove, read with access rules | PP01, PP04, PP05, PP07 |
| `Data/SupabasePictureStorage.cs` (new) | Storage REST calls (put, get, delete) with the secret key; a small memory cache for reads; not registered without configuration | PP01, PP07 |
| `Data/TaxBuddyUser.cs`, `TaxBuddyDbContext.cs`, migration `AddProfilePicture` | `picture_object` on `users` (nullable, 200 characters) | PP01 |
| `Account/PictureEndpoints.cs` (new) | `POST /account/picture` (multipart, antiforgery, 5 MB request limit), `POST /account/picture/remove`, `GET /pictures/{accountId}` | PP01, PP04–PP06 |
| `Account/AccountSetup.cs` | A `taxbuddy:picture` claim; the session is refreshed after a change | PP01 |
| `Data/DirectoryStore.cs`, `Application/Directory` models | Cards and profiles carry the owner's picture address | PP01 |
| `Components/Shared/Avatar.razor` (new); header, Account, cards, profile, editor | Picture or initials | PP01, PP08 |
| `Components/Pages/AccountPage.razor` | The picture form and its messages | PP01, PP03, PP04, PP07 |
| Tests | Processor rule tests; web tests with an in-memory store replacing Supabase | All |

## Approach

The picture form is a plain multipart form posted to an endpoint, as the theme switch and sign-out are. A successful post redirects to `/account?picture=…`, so a reload doesn't repeat it. Processing happens before anything is stored. A storage failure leaves the account unchanged.

Supabase setup, once by the owner:
1. In the Supabase dashboard, open **Project Settings → API keys** and copy a **secret** key (`sb_secret_…`), or the legacy `service_role` key.
2. Store it, and the project URL, in WSL user-secrets:
   ```bash
   dotnet user-secrets set --id taxbuddy-web "Supabase:Url" "https://<project-id>.supabase.co"
   read -rsp 'Secret key: ' V; echo; dotnet user-secrets set --id taxbuddy-web "Supabase:SecretKey" "$V"; unset V
   ```
3. Create the private bucket: in the dashboard under **Storage → New bucket**, named `profile-pictures`, public off. Or ask the agent to create it through the API.

**Data:** additive migration `AddProfilePicture`. Rollback: `dotnet ef database update AddProfileOwner`.

## Verification

1. Processor rule tests with generated images: a PNG, a JPEG with an injected EXIF block, a non-image, oversized dimensions, a non-square image.
2. Web tests with an in-memory `IPictureStorage` for PP01 and PP03–PP07.
3. Release build; full suite; the migration applied; a browser check of Account at phone and desktop widths.
4. Real Supabase round trip once the key is stored.
5. PRD R24, SDD, DESIGN, SETUP, TEST-PLAN, STATUS, the F05 spec, the ADR index and the docs index.

## Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Decoding untrusted images | Memory exhaustion, or library bugs | 5 MB request limit; dimensions read before decoding; 40 MP cap; SkiaSharp kept current |
| Inappropriate public pictures | Harm before moderation exists | Reporting arrives with R15; the owner can remove a picture (D22) |
| Storage outage | Uploads fail | The old picture is kept; avatars fall back to initials if a read fails |
