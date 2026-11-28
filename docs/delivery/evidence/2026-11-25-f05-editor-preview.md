# F05 PE09: the profile editor's card beside the form, on the site: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-25, about 20:00–21:00 Asia/Manila (UTC+8), with the full test suite rerun on 2026-11-26 after the session was interrupted and its first result lost.
- Observer: agent-observed (Claude Code). After the prototype change ([evidence](2026-11-25-prototype-editor-preview.md)), the user said: "implement to site".
- Requirement and scenario: [F05 PE09](../../../specs/profile-editing/spec.md) and [F02 BD16](../../../specs/brand-design-refresh/spec.md#my-profile-preview-beside-the-form-2026-11-25-bd16); R05 and R01 (the directory card). No PRD change: the preview shows the directory's existing card, as last saved.
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`. Another session was editing the same working tree at the same time (repeated-post guards, G19). Its files were not changed here.
- Artifact (SHA-256, first 12 characters), as tested:

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Application/Directory/DirectoryService.cs` | `cc89a292cc7e` |
  | `src/TaxBuddy.Web/Application/Directory/DirectoryModels.cs` | `7d52296594cf` |
  | `src/TaxBuddy.Web/Data/DirectoryStore.cs` | `474287851783` |
  | `src/TaxBuddy.Web/Components/Pages/ProfileEditorPage.razor` | `632323bfc8bf` |
  | `src/TaxBuddy.Web/Components/Shared/DeskTitle.razor` | `558578e27c5b` |
  | `src/TaxBuddy.Web/Styles/app.css` | `7df930fccebd` |
  | `tests/TaxBuddy.Tests/Application/DirectoryServiceTests.cs` | `e6b02d6d12de` |
  | `tests/TaxBuddy.Web.Tests/ProfileEditorTests.cs` | `667ad3850dfd` |
  | `prototypes/index.html` (panel label as `h2`) | `a5680d05f1a1` |

- Environment: WSL2 with .NET SDK 11.0.100; the Supabase development database (throwaway schemas for the tests; no migration); Windows Edge 158.0.4370.33, headless; Windows Node 22.12.0.

## What was built

- **Data:**
  - `IDirectoryStore.FindCardAsync` reads one listed profile's card row, in the same projection that directory pages use. That projection is now shared: the card's fields, review totals and the owner's picture.
  - `DirectoryService.GetCardAsync` maps the row to the same `AccountantCard`.
- **Page:**
  - the editor draws the card with the directory's `AccountantCardItem`: in a side panel headed "Your card in the directory", and in a closed "Preview your directory card" disclosure;
  - the stylesheet shows the panel from 860 px and the disclosure below that;
  - before listing, both say where the card will appear;
  - if the card can't be read, the panel says so and the form still works;
  - `DeskTitle` takes a `Class`, so the heading band lines up with the wider page.
- **Prototype:** the panel's label became an `h2`, to match.

## Execution

| Procedure | Expected result | Actual observation | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` (with Tailwind) | 0 warnings | 0 Warning(s), 0 Error(s); the new rules are in `wwwroot/app.css` | Pass |
| Unit tests, including the new `One_card_reads_like_a_directory_card_and_is_null_when_not_listed` | All pass | 224 passed | Pass |
| The editor's, directory's and profile page's web tests, including the new PE09 test | All pass | 30 passed. PE09 checks four things:<br>• before listing, the not-listed text twice and no card;<br>• after a save, two cards (the disclosure's and the panel's) with name, title, city, service and "No reviews yet", identical to the card on the directory page;<br>• after a refused save, the form has "Pasig City" and both cards still show "Makati City";<br>• the heading band has `editor-wide` | Pass |
| `dotnet test TaxBuddy.slnx -c Release -m:1`, 2026-11-26 | All pass | Unit tests: 224 passed. Web tests: 199 passed, **6 failed**, all in the other session's unfinished work:<br>• 5 in its new `RepeatedPostTests`;<br>• 1 in its changed `ServiceDocumentsTests`.<br>The editor's failure in that file is the second post answering 409 Conflict: F05's stale-save check (PE05), which its new test expects to change. This change doesn't touch saving | Pass for this change; 6 failures belong to the other work |
| Rendered pages from the test host. A temporary test signed in an accountant and saved the editor's HTML before listing, after a save and after a refused save; the test was then deleted. The pages were served locally with the built styles, fonts and images, then viewed in Edge | Matches the prototype's design B | All as designed:<br>• 1280 px, light and dark: the panel right of the form, sticking 16 px from the top when scrolled;<br>• 1280 px, not listed and refused;<br>• 820 px: the closed disclosure above the form;<br>• 390 px phone (emulated), disclosure open and closed: no horizontal overflow, summary 44 px tall | Pass |
| 320 px phone (emulated) | No horizontal overflow | 27 px of overflow from a saved experience row's End month field, with the disclosure open or closed. It doesn't occur without an experience row. Its styles (`.grid2`) are unchanged from `06e9be6`, so it predates this change | Found, not fixed |
| `node tools/check_prototype.mjs` after the label change | All pass | `PASS: 224 prototype checks` | Pass |

## Restoration and limits

- **Cleanup:**
  - the temporary test was deleted;
  - the local file server was stopped;
  - the Windows temp copies and Edge profiles were removed.
- **Screenshots:** kept in the session's scratch folder, which was cleared when the session was interrupted, so none remain.
- **Not run:**
  - the running site with a real Google sign-in;
  - a card-read failure while the editor itself loads (a code path only);
  - keyboard and screen-reader checks, real phones, or contrast calculations (the panel reuses existing tokens).
