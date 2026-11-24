# Account: workspace row and picture pencil for both roles: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-24, about 11:50–12:30 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user asked whether the prototype's Account screen was completely implemented. After the finding, they said: "yes add the clients row, keep my reviews out also consider the edit profile picture for both account and client based on the prototype".
- Requirement and scenario: PRD R19, R20, R24. [F03](../../../specs/client-services/spec.md#account-row-2026-11-24) CS13; [F06](../../../specs/profile-pictures/spec.md) PP09–PP10 for the client role. Covered by DESIGN's Account and workspace navigation rows and the prototype's `meHTML` and `meCardHTML`.
- Finding before the change: Account had no workspace row, although F03 was built on 2026-11-23 and DESIGN lists Account among the workspace's ways in. The SDD named only the site bar and the phone tabs. The picture controls on Account were already the same for both roles; only an accountant's pencil was covered by a test.
- Source revision: `ff4f0df` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus earlier uncommitted work and this change.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Components/Pages/AccountPage.razor` | `d329f9f7c9b4` |
  | `tests/TaxBuddy.Web.Tests/ClientServicesTests.cs` | `769c963c84fd` |
  | `tests/TaxBuddy.Web.Tests/ProfilePictureTests.cs` | `9efdbd0983c4` |

- Environment: WSL2 with .NET SDK 11.0.100; the Supabase development database (throwaway schemas) for the tests; Windows Edge 158.0.4370.33, headless.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings | 0 Warning(s), 0 Error(s) | Pass |
| `dotnet test --filter ClientServicesTests\|ProfilePictureTests` | All pass, including the two new tests | 21 passed | Pass |
| New test `Account_opens_the_workspace_by_role_as_its_first_row` | Accountant: "Clients and service work" with the detail line, before "My profile and portfolio", no "My reviews". Client: "My accountants and service progress". Both link to `/workspace`. A visitor sees the visitor card and no row | Passed | Pass |
| New test `A_client_changes_their_picture_on_Account_with_the_same_pencil_as_an_accountant` | A client's Account has the picture area, the pencil, the hidden Save/Cancel block, the panel and the client's hint. After an upload: "Picture updated.", the picture, and the hidden "Remove picture" link | Passed | Pass |
| `dotnet test TaxBuddy.slnx -c Release -m:1` | All pass | 354 passed (188 unit, 166 web); 0 failed, 0 skipped | Pass |
| Rendered Account pages in Edge. A temporary test signed in an accountant and a client through the test host and saved `/account`; it was then deleted. The HTML was served locally with the built `app.css`, the scoped styles, the fonts and the logos, in the dark theme. Captured at 1280 × 900 and in a 390 px frame (headless Edge won't size its window that narrow) | The row first, with the list icon, the detail line and the chevron, in the prototype's style; the pencil on the avatar for both roles | As expected for both roles at both widths. The page script wasn't loaded, so this is the without-script view | Pass |

## Follow-up the same day: long addresses and the site shell row

The user said "yes fix both" to the two items found above.

- **Long email addresses:** `.mi-main small` now has `overflow-wrap: anywhere`. The prototype had the same style and the same overflow, since its sample address is short. The fix went into the prototype's style first, then into `src/TaxBuddy.Web/Styles/app.css`, so the two stay the same and no deviation is recorded.
- **DESIGN's site shell row** said Clients and "For accountants" would join the navigation when built. Both joined on 2026-11-23 (F03, F13). The row now describes the site bar and the phone tabs as built, which match the prototype's `desktopShell` and `tabbarHTML`.

| File | Hash (SHA-256, first 12) |
|---|---|
| `prototypes/index.html` | `0209225981cd` |
| `src/TaxBuddy.Web/Styles/app.css` | `0a329911f0eb` |

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings; the rule in the built `wwwroot/app.css` | 0 Warning(s), 0 Error(s); `.mi-main small{…overflow-wrap:anywhere…}` present | Pass |
| The same rendered Account pages with the new stylesheet, in a 390 px frame and at 1280 × 900, dark | The address wraps inside its column and "Turn off" sits clear of it; desktop unchanged | As expected for both roles; the desktop capture is the same as before | Pass |
| `node tools/check_prototype.mjs "<Edge>"`, Windows Node 22.12.0, on a copy of `prototypes/` and `tools/` in the Windows temp folder | All pass | `PASS: 202 prototype checks; Edg/158.0.4370.33` | Pass |

The test suite wasn't rerun for these two changes: a stylesheet rule and a documentation row, with no markup changed.

## Restoration and limits

- Cleanup: the temporary test file was deleted before the full run, and the local server on port 5311 was stopped (it no longer answers). Edge's temporary profile was removed. The checker's Windows copy and its artifacts were deleted. The screenshots stay in the session's scratch folder, outside the repository.
- Fixed the same day (above): on a 390 px page a long email address with no break points (the tests' 32-character addresses) ran under the "Turn off" button in the Email notifications row. This was there before the workspace row was added; short addresses fit.
- Not run: the signed-in pages in a real signed-in browser, a phone browser, the light theme, and the picture script on the client's page. The script doesn't depend on the role.
