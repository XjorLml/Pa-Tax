# Pencil on the profile picture (F06 PP09) execution evidence

## Identity and attribution

- **Date/time and timezone:** 2026-11-19, +08:00, between about 01:10 and 01:45.
- **Observer:** agent-observed (Claude Code).
- **User's instruction:** "is the profile pic change implemented in the prototype? improve the design and move it in the profile picture pencil recommend simple design for changing and updating profile picture".
- **Scope:** [PP09 scenario](../../../specs/profile-pictures/spec.md#pencil-on-the-picture-2026-11-19-pp09).
- **Source revision:** branch `feature/f02-f04-theme-switch-google-sign-in` at `55e0f12`, with F10, the BD11 alignment, the logo and the typeface work uncommitted. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `prototypes/index.html` | `9788e858387a` |
  | `tools/check_prototype.mjs` | `af20d6f69799` |
  | `src/TaxBuddy.Web/Components/Pages/AccountPage.razor` | `0a3f34f19e7b` |
  | `src/TaxBuddy.Web/Components/Pages/ProfileEditorPage.razor` | `b2b749599850` |
  | `src/TaxBuddy.Web/Styles/app.css` | `13abf6a9167f` |
  | `tests/TaxBuddy.Web.Tests/ProfilePictureTests.cs` | `857b9d456684` |

- **Environment:** WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; the Supabase development database for the tests, with the in-memory picture store.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Search the prototype for a picture upload before the change | Either a screen to follow, or none | None: no file input or picture control in `prototypes/index.html` | Finding |
| `node tools/check_prototype.mjs "<Edge>"` | All checks pass, with four new ones: the pencil and a closed panel; the pencil opens the panel; upload without a file says "Choose a photo first."; Close hides it | `PASS: 190 prototype checks` (186 before) | Pass |
| Prototype screenshot from the checker: Account as a client on a phone, panel open with the error banner | The pencil on the avatar; the panel under the name card | As expected. A toast from the previous check overlaps the panel's last line in the capture | Pass |
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings, 0 errors | 0 and 0 | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build -m:1` | No failures, nothing skipped | On the final sources, after the temporary test was deleted: 154 and 105 passed, 0 failed, 0 skipped. 105 includes the one new web test for PP09 | Pass |
| Rendered site pages ([folder](2026-11-19-f06-picture-pencil/)): Account closed at 1440 light; panel open after an upload at 1440 light; panel with an error at phone width dark; the editor at 1440 light | The pencil on the avatar; the panel closed until targeted; the banner, picker, hint, Remove and Close | As expected in all four. In the editor capture the avatar row is cut off at the bottom edge | Pass |

**How the site pages were rendered:** a temporary test signed in through the test host, saved the HTML of `/account`, `/account?picture=no-file`, `/account?picture=saved` and `/account/profile`, and was then deleted. The files were opened in Edge with the built `wwwroot/app.css`, with `#picture` added to show the panel as the page's target. The stored picture's address was replaced by a placeholder drawing, since no picture server was running.

## Picture design options, added later the same day

- **User's instruction:** "put the choices in the prototype so i can clearly visualize". Scope: [the options](../../../specs/profile-pictures/spec.md#picture-design-options-2026-11-19). Prototype only; no site source changed.
- **Sources after this change:** `prototypes/index.html` `ceebd31838ce`, `tools/check_prototype.mjs` `7081be8d7c81`. These replace the two prefixes in the table above for this section.

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs "<Edge>"` | All checks pass, with 11 new ones for the control and options A to E | `PASS: 201 prototype checks` | Pass |
| A, B and E with a real file: the checker puts a synthetic 2 × 2 PNG into the file input | A: "Picture updated", the picture in the avatar, "Remove picture". B: saved at once. E: a preview that waits for Save, then Save and Remove | As expected | Pass |
| C and D | C: the pencil opens the menu. D: the pencil opens the picture page | As expected. Neither was run with a file | Pass for what was run |
| One screenshot per option at phone width ([sheet](2026-11-19-f06-picture-pencil/prototype-picture-options-a-to-e.png)) | Each design as described | As expected. The sample picture is a plain blue square, and a toast from an earlier check overlaps the lower part of each capture | Pass |

Not run: the site build and tests, since no site source changed; desktop width and dark theme for options B to E; keyboard use of the menu in C.

## Restoration and limits

The temporary test and the saved pages were deleted. Test data lived in the throwaway schemas the tests create and remove.

Not exercised:
- **The running site in a signed-in browser:** clicking the pencil, choosing a file and uploading were not done in a browser; the earlier real-storage round trip of 2026-11-16 is not repeated here.
- **The prototype's upload with a real file:** the checker cannot choose a file, so "Picture updated", the picture in the avatar and "Remove picture" were not run.
- **Keyboard and screen reader:** focus order into the panel and the pencil's announcement were not tested; TP06 remains.
- **Phone width:** headless Edge would not go narrower than about 500 px for the site captures.

Acceptance decision: pending the owner's walkthrough (G9).
