# F02 BD14 theme toggle in the header: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-23, about 09:10–09:40 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code), at the user's instruction of the same day ([F02 BD14](../../../specs/brand-design-refresh/spec.md#theme-toggle-in-the-header-2026-11-23-bd14)).
- Requirements and scenarios: DESIGN theme; F02 BD14 (replaces the footer switch of BD09/BD10); TEST-PLAN TP06.
- Source revision: `1565e69` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus uncommitted F11, BD14 and documentation changes.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Components/Shared/ThemeToggle.razor` (new; `ThemeSwitch.razor` removed) | `8dd59212282d` |
  | `src/TaxBuddy.Web/Components/Shared/SiteUser.razor` | `4fd1911cfbb1` |
  | `src/TaxBuddy.Web/Components/Shared/PhoneBar.razor` | `e8f3a0911fee` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `84e68380f1c7` |
  | `src/TaxBuddy.Web/Styles/app.css` | `ecbe2c3328d5` |
  | `tests/TaxBuddy.Web.Tests/ThemeSwitchTests.cs` | `d72e52e46ba5` |
  | `prototypes/index.html` | `08d11f66382d` |

- Environment: WSL2 with .NET SDK 11.0.100 and the development database (throwaway test schemas). Headless Edge 158.0.4370.33, driven from Windows Node v22.12.0.
- Running site: the user's own `dotnet run --launch-profile http` on port 5234 (a Debug build written at 09:20:26, after the last site source edit at about 09:17). The agent's own instance couldn't bind the port, and the user's was left running.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings; the new rules in `wwwroot/app.css` | "0 Warning(s) 0 Error(s)"; `theme-toggle` and `theme-to-light` present | Pass |
| `dotnet test TaxBuddy.slnx -c Release -m:1 --no-build` | All pass | 156 and 113 passed, 0 failed, 0 skipped (269 in all). The first run failed one new test, which looked for `<header class="sitebar">` while the layout's scoped CSS adds a `b-…` attribute; the test was corrected | Pass after a test fix |
| Edge via DevTools on the running site: `/` at 1440 px and `/`, a profile and `/terms` at 390 and 320 px, with the device light and dark | One visible toggle in the header: "Switch to dark theme" on light, "Switch to light theme" on dark; no overflow | As expected in every case; overflow 0 px; header screenshots viewed (desktop: Sign in with the moon beside it; phone: the icon at the end of the app bar) | Pass |
| Press the visible toggle on `/terms` (phone, light device), then press again | `data-theme` becomes `dark` on the same page, then `light` | `/terms`, `dark` with the sun shown; then `light` with the moon shown | Pass |
| Prototype: syntax check and `node tools/check_prototype.mjs` | Pass | `PASS: 202 prototype checks; Edg/158.0.4370.33` | Pass |
| Prototype screenshots: desktop light; a phone profile after pressing the toggle | Moon beside Sign in; after pressing, dark theme with the sun, focus kept | As expected; the "Theme" simulation control changed to Dark | Pass (visual) |

## Restoration and limits

- Cleanup: the screenshot folders in the Windows temporary folder were deleted. Pressing the toggle set a theme cookie only in the headless browser's throwaway profile, deleted afterwards. The user's running site was not stopped.
- Not run: keyboard and screen-reader use of the toggle; Safari, Firefox and Brave; real phones. The prototype checker has no BD14 checks of its own.
- Changed behaviour: there's no System button. A visitor follows the device until they press the icon; returning to System needs the cookie cleared. Recorded in the BD14 spec section.

Acceptance: pending the owner's walkthrough (G9).
