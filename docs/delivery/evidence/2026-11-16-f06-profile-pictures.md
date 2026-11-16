# F06 profile pictures execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-16, about 14:10–15:20 +08:00.
- Observer: agent-observed (Claude Code). User's instruction: "User should able to edit their profile picture and when no picture povided make a default user avatar, plan for changes to the current plan and implement then up date the docs". The user decided: everyone; Supabase Storage; re-encode with SkiaSharp; initials as the default.
- Requirements/scenarios: PRD R24 (new), R11; [F06 PP01–PP08](../../../specs/profile-pictures/spec.md#acceptance-scenarios); [ADR 011](../../adr/011-profile-pictures-supabase-storage.md).
- Source revision: branch `feature/f02-f04-theme-switch-google-sign-in` at `2b5407c`, with F05 and BD10 staged, plus F06 unstaged. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Application/Pictures/PictureProcessor.cs` | `093ebc19d217` |
  | `src/TaxBuddy.Web/Application/Pictures/ProfilePictureService.cs` | `6933e57984df` |
  | `src/TaxBuddy.Web/Data/SupabasePictureStorage.cs` | `e082c9818b9d` |
  | `src/TaxBuddy.Web/Data/AccountPictureStore.cs` | `b9b9a5354e20` |
  | `src/TaxBuddy.Web/Data/TaxBuddyUser.cs` | `75be54f9404a` |
  | `src/TaxBuddy.Web/Data/TaxBuddyDbContext.cs` | `03398df8a719` |
  | `src/TaxBuddy.Web/Data/DirectoryStore.cs` | `1b7fea103627` |
  | `src/TaxBuddy.Web/Data/Migrations/20261116065218_AddProfilePicture.cs` | `b96c2ec9de26` |
  | `src/TaxBuddy.Web/Account/PictureEndpoints.cs` | `38a9f4193a49` |
  | `src/TaxBuddy.Web/Account/AccountSetup.cs` | `f027a0c392a7` |
  | `src/TaxBuddy.Web/Application/Directory/DirectoryModels.cs` | `8b0683a9c692` |
  | `src/TaxBuddy.Web/Application/Directory/DirectoryService.cs` | `94c6741813ba` |
  | `src/TaxBuddy.Web/Components/Shared/Avatar.razor` | `a804dde6a886` |
  | `src/TaxBuddy.Web/Components/Shared/SiteUser.razor` | `5f4851bb71c6` |
  | `src/TaxBuddy.Web/Components/Shared/AccountantCardItem.razor` | `752cbd27fa90` |
  | `src/TaxBuddy.Web/Components/Pages/AccountPage.razor` | `2cafed049ba0` |
  | `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `bcf7251d37d8` |
  | `src/TaxBuddy.Web/Components/Pages/ProfileEditorPage.razor` | `c80edaa35e47` |
  | `src/TaxBuddy.Web/Program.cs` | `fcba2dc6b823` |
  | `src/TaxBuddy.Web/appsettings.json` | `bf43d559415e` |
  | `src/TaxBuddy.Web/TaxBuddy.Web.csproj` | `c95304ee8b10` |
  | `src/TaxBuddy.Web/Styles/app.css` | `07565afb2db2` |
  | `tests/Shared/TestImages.cs` | `46bec0414d56` |
  | `tests/TaxBuddy.Tests/Application/PictureTests.cs` | `583553167aa6` |
  | `tests/TaxBuddy.Web.Tests/ProfilePictureTests.cs` | `08a9b429fd38` |
  | `tests/TaxBuddy.Web.Tests/Support/InMemoryPictureStorage.cs` | `3f5df47264a6` |
  | `tests/TaxBuddy.Web.Tests/Support/TaxBuddyAppFactory.cs` | `fe7ca2cbf639` |

- New packages: `SkiaSharp` 4.155.1 and `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.155.1. 4.156.1 was the newest; the release before it was pinned.
- Environment: WSL2 Ubuntu 26.04 with .NET SDK 11.0.100 and ICU; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; the Supabase development database. **No Supabase Storage key was configured**, so the tests use an in-memory store.
- Fixtures: synthetic images generated in code. These include a JPEG with an injected EXIF orientation block and a comment holding a fake location marker, and a PNG whose header claims 50 megapixels. Accounts use synthetic names and `example.test` emails.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` | 0 warnings, 0 errors | First run: 1 error, an obsolete `SKCanvas.DrawBitmap` treated as an error; replaced with `DrawImage` and sampling options. Then 0 and 0 | Fail, then pass |
| `dotnet ef migrations add AddProfilePicture`, then a review | Additive | One nullable column, `users.picture_object` (`character varying(200)`) | Pass |
| Picture rule tests (9), first run | Pass | 8 passed. The 50 MP test file had no data chunk, so it was refused as "not an image" before the pixel check, which proves nothing. The fixture now has a tiny data chunk, and the test reaches the pixel check | Fail, then pass |
| Whole suite, first run with the web tests | No failures | 1 failure in a test: the expected `Cache-Control` string was in a different order from .NET's normalised form. The assertion now checks the parts | Fail, then pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build` (final) | No failures, nothing skipped | TaxBuddy.Tests 86 passed; TaxBuddy.Web.Tests 82 passed; 0 failed, 0 skipped | Pass |
| `dotnet ef database update` | Applied | "Applying migration '20261116065218_AddProfilePicture'. Done." | Pass |
| Edge check ([results](2026-11-16-f06-profile-pictures/results.json)): the real app (`dotnet run`, Development, port 5240) started with a deliberately unreachable storage address (`Supabase__Url=http://127.0.0.1:9`), signed in through a temporary test host on 5301 | The picture section renders; a real upload through the form keeps the picture and says why | 4 of 4: form, label and no overflow at 390 and 1440 px, light and dark. A real 64 × 32 PNG uploaded through the file input redirected to `?picture=failed` with "We couldn't save your picture…". The app log has one "Picture upload failed (PictureStorageException)" and no unhandled exceptions | Pass |
| Log review | No personal data | HttpClient's own Information logs showed storage addresses (they contain account IDs); `System.Net.Http.HttpClient` was lowered to Warning in `appsettings.json` | Fixed |
| Screenshot review: Account with the failure (1440, light), Account (390, dark) | Prototype Account style | As expected | Pass |

What the tests cover:
- **Rule tests:**
  - a 600 × 300 PNG becomes a 400 × 400 JPEG from its centre;
  - the EXIF-oriented JPEG is turned upright, with no "Exif" or location marker left in the output;
  - empty, over-5-MB, non-image and 50 MP files are refused;
  - the service writes the new object, then points the account at it, then deletes the old one;
  - a storage failure keeps the current picture; a rejected file stores nothing; removal clears and deletes;
  - access rules: an accountant's picture is public, a client's is for that client only, an unknown one is null;
  - without storage, uploads are switched off.
- **Web tests:**
  - an accountant's upload shows in the header, on the Account page and on the public directory card, served as a 400 × 400 JPEG with an immutable public cache;
  - a client's picture is private (cache "private") and 404 for a visitor and for another client;
  - replacing leaves one object, and removing brings back the initials;
  - not-an-image, too-many-pixels and no-file messages; over 5 MB refused before reading;
  - no token gets 400; signed out goes to sign-in;
  - a storage failure keeps the picture;
  - without storage, the page says upload isn't set up.

## Restoration and limits

- The temporary hosts and checks were deleted; none are committed. The Edge profile and the temporary PNG were deleted.
- **Development database:** the browser checks created 2 synthetic accounts, which were deleted afterwards (`DELETE FROM users WHERE email = 'andrea.browser@example.test'`: 2 rows). Afterwards: 2 accounts, 0 `example.test` accounts, 0 pictures, 10 profiles. The tenth profile belongs to an accountant account created 2026-11-16 04:28Z, apparently the owner's own; it was not touched.

Not run: an accountant's picture on the card and profile with real storage (the round trip below used a client).
- Other browsers, phones and screen readers.

Acceptance: pending.

## Real Supabase Storage round trip

About 15:40–15:55 +08:00, after the owner stored `Supabase:Url` and `Supabase:SecretKey` in WSL user-secrets ("done").

**Key check by the agent**, printing no values:
- the key is present: 41 characters, starting `sb_secret_`, no stray spaces;
- Storage's bucket list answered with `profile-pictures` (private), which the owner had already created.

**Browser check** ([results](2026-11-16-f06-profile-pictures/supabase-results.json)):
- **Setup:** the real app (`dotnet run`, Development, with the user-secrets, so the Supabase client was active) on port 5240. A temporary test host on 5301 provided the sign-in, as a client. The test host also wrote the fixture: a synthetic 600 × 300 red|blue JPEG with EXIF orientation 6 and a fake location comment, uploaded through the Account form with Edge's file input.
- **Result:** 5 of 5 passed.
  - Saved, with "Picture updated."; the same `/pictures/…?v=…` address on Account and in the header.
  - The browser loaded it at 400 × 400.
  - The owner gets 200 `image/jpeg` with a private cache; a visitor gets 404.
  - "Remove picture" brought back the initials "AV".
- **Screenshot (390 px, dark):** red on top and blue below, so the EXIF orientation was applied.
- **App log:** no "Picture … failed" lines and no exceptions.
- **Afterwards:** Storage's object list showed no objects for the test account and nothing under `users/`.

**Clean-up:**
- the synthetic account was deleted (`DELETE FROM users WHERE email = 'andrea.browser@example.test'`: 1 row). Afterwards: 2 accounts, 0 `example.test`, 0 with pictures;
- the temporary host and the fixture file were deleted.
