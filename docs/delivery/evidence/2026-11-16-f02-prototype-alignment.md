# F02 BD10 application aligned to the prototype: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-16, about 13:40–15:10 +08:00.
- Observer: agent-observed (Claude Code), at the user's instruction "update the current implementation based on the prototype, that should be the origin of the design. add to claude that review doucmentation first before implementing".
- Requirements/scenarios/procedures: [F02 BD10](../../../specs/brand-design-refresh/spec.md#application-aligned-to-the-prototype-2026-11-16-bd10); DESIGN; [CLAUDE.md](../../../CLAUDE.md).
- Source revision and working-tree changes: branch `feature/f02-f04-theme-switch-google-sign-in` at `af9d3ea`, with F02 and F04 staged and F05 plus BD10 unstaged. Leading 12 hex digits of SHA-256 at the end of the work:

  | File | SHA-256 (prefix) |
  |---|---|
  | `CLAUDE.md` (new) | `d5956e2eaa05` |
  | `src/TaxBuddy.Web/Styles/app.css` | `de3d6487271e` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `0cf75224bdfb` |
  | `src/TaxBuddy.Web/Components/Shared/SiteUser.razor` (new) | `c28a4ca08eaf` |
  | `src/TaxBuddy.Web/Components/Shared/PhoneBar.razor` (new) | `1d7180fa9b59` |
  | `src/TaxBuddy.Web/Components/Shared/PhoneTabs.razor` (new) | `763815cd63e2` |
  | `src/TaxBuddy.Web/Components/Shared/DeskTitle.razor` (new) | `bb227c417f9b` |
  | `src/TaxBuddy.Web/Components/Shared/BrandIcon.razor` (new) | `76a1a9adeb0f` |
  | `src/TaxBuddy.Web/Components/Shared/DraftLegalNotice.razor` | `4f4cdb96ae14` |
  | `src/TaxBuddy.Web/Components/Shared/Icon.razor` | `91a23f90af64` |
  | `src/TaxBuddy.Web/Components/Shared/Display.cs` | `4d742d69e384` |
  | `src/TaxBuddy.Web/Components/Pages/AccountPage.razor` (new) | `7ec51ced1394` |
  | `src/TaxBuddy.Web/Components/Pages/SignInPage.razor` | `174bdd04c5b1` |
  | `src/TaxBuddy.Web/Components/Pages/RegisterPage.razor` | `51514288c303` |
  | `src/TaxBuddy.Web/Components/Pages/ProfileEditorPage.razor` | `45504a802bec` |
  | `src/TaxBuddy.Web/Components/Pages/TermsPage.razor` | `2922b553ab89` |
  | `src/TaxBuddy.Web/Components/Pages/PrivacyPage.razor` | `e4f3b7612657` |
  | `src/TaxBuddy.Web/Components/Pages/DirectoryPage.razor` | `0a60ae935821` |
  | `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `33d247029acb` |
  | `src/TaxBuddy.Web/Components/Pages/NotFound.razor` | `c63ebc8235f1` |
  | `src/TaxBuddy.Web/Components/Pages/Error.razor` | `c995e38edc86` |
  | `src/TaxBuddy.Web/Components/Pages/ChatComingSoon.razor` | `b663393a8865` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileForm.cs` | `26f99eded2cf` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileValidator.cs` | `706126b9e419` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileEditorService.cs` | `d1659951d41d` |
  | `src/TaxBuddy.Web/Account/AccountEndpoints.cs` | `f8fb334d0891` |
  | `tests/TaxBuddy.Tests/Application/ProfileValidatorTests.cs` | `49512c300fde` |
  | `tests/TaxBuddy.Web.Tests/ProfileEditorTests.cs` | `a7e9efa371b1` |
  | `tests/TaxBuddy.Web.Tests/SignInTests.cs` | `363dde9d6f94` |

  Also: `wwwroot/images/taxbuddy-icon-dark.svg` was copied unchanged from `prototypes/assets`, and `Shared/AccountMenu.razor` was removed.
- Environment: WSL2 Ubuntu 26.04 with .NET SDK 11.0.100 and ICU; Tailwind 4.3.3 through Windows Node 24.16.0; headless Microsoft Edge 157.0.4341.60 on Windows; the Supabase development database.
- Preconditions/fixtures: synthetic names and `example.test` emails only.
- Reference: the prototype's render functions (`desktopShell`, `phoneShell`, `tabbarHTML`, `signinHTML`, `meHTML`, `editorParts`, `fld`, `listingSteps`, `checklistInner`, `validateFormat`, `infoPage`, `footerHTML`) and the base styles in `index.html` plus `taxbuddy.css`, read before the changes.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` | 0 warnings, 0 errors | 0 and 0, after each round | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build` after the changes (tests updated to the prototype's texts, ids and service rows) | No failures | First run: 10 web tests failed because of test-side markers (a trailing space in a marker; attribute order of NavLink). After the fix: TaxBuddy.Tests 77 passed, TaxBuddy.Web.Tests 72 passed; 0 failed, 0 skipped | Pass |
| Tailwind through Windows Node | New rules in `wwwroot/app.css` | Generated after each CSS change | Pass |
| Comparison script (agent-written): the prototype (`file:` URL with `device=phone` or `desktop`, `notes=0`, and `role` where needed) against the real app (`dotnet run`, Development, port 5240) at 390 and 1440 px. The app was signed in through a temporary, uncommitted test host on port 5301 that shares the session | App flows work; no overflow; screens match apart from the recorded deviations | Registration as an accountant landed on `/account/profile`; adding a service, adding experience and saving reached `?saved=1`. 16 app views, none with horizontal overflow ([app-views.json](2026-11-16-f02-prototype-alignment/app-views.json)). No exceptions in the app log | Pass |
| Screenshot review, prototype against app: sign-in (1440), the new editor (1440), Account (390), then after fixes the editor with errors (1440) and registration (390) | Same layout, wording and states | Matches, apart from the recorded deviations. Found and fixed: a missing space before "(needed to be listed)"; body links weren't underlined; the page title wasn't in the prototype's heading band; the sign-in heading was too large; the end month sat lower when the start month showed an error. The last fix (`.grid2` alignment, CSS only) came after the final screenshots and was not re-captured | Pass, with a note |

## Restoration and limits

- The temporary host and cleanup test were deleted; none are committed. Edge profiles were deleted and the app processes stopped.
- **Development database:** the comparison runs created two synthetic accounts (`andrea.browser@example.test`). They were deleted (`DELETE FROM users WHERE email = 'andrea.browser@example.test'`: 2 rows). Afterwards: 2 accounts (not the agent's), 0 owned profiles, 9 seed profiles, 0 `example.test` accounts.

Not compared in detail:
- Dark mode, apart from earlier checks.
- Other browsers and phones.

Deviations from the prototype are recorded in DESIGN and in F02 BD10. Acceptance decision: pending (owner walkthrough).

## F01 pages compared and aligned, later the same day

About 15:20–16:00 +08:00, at the user's instruction "proceed to the to do". The user had committed F02 and F04 as `2b5407c` in the meantime.

**What the comparison found** (prototype `#/directory` and `#/profile/a1` against the app's `/` and the same synthetic profile, at 390 and 1440 px):
- **Hero:** a smaller heading; no arched illustration or caption.
- **Service buttons:** pill-shaped and wrapping to two lines.
- **Trust line:** different wording.
- **Cards:** card style, avatar tone, tag shape, no "Read their story".
- **Profile:** no breadcrumb or summary chips; the contact panel order, the absent accent border and credentials note; the pricing note wording.

**Fixed** by porting the prototype's `heroHTML`, `cardHTML` and `profileBodyHTML` markup and the Tax Buddy look's styles. File hashes afterwards:

| File | SHA-256 (prefix) |
|---|---|
| `src/TaxBuddy.Web/Components/Pages/DirectoryPage.razor` | `6fbcffdee954` |
| `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `56b03d2152ba` |
| `src/TaxBuddy.Web/Components/Shared/AccountantCardItem.razor` | `162e1d213ce9` |
| `src/TaxBuddy.Web/Components/Shared/Icon.razor` (info icon) | `6b37573ed235` |
| `src/TaxBuddy.Web/Styles/app.css` (final, including the phone fixes) | `52efd96de03d` |

**Checks:**
- Release build: 0 warnings.
- `dotnet test`: 149 passed, 0 skipped. F01's content tests needed no change.
- Recapture at 390 and 1440 px: no overflow, no app exceptions; screenshots match the prototype apart from the recorded deviations.
- Afterwards two phone-only CSS fixes (breadcrumb hidden, contact card spacing) were not re-captured.

**Deviations recorded in DESIGN:** no rating, sort, highlights or report link (R12, R13, R15); contact details stay links; the credentials note has no reference to reviews.
