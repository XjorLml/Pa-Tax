# Brand and prototype refresh plan (Kuwenta, renamed Tax Buddy)

Status: authorized, local prototype work. Scenarios: [spec](spec.md); execution: [tasks](tasks.md).

Rename, 2026-11-12 (BD07): `prototypes/kuwenta.css` is now `prototypes/taxbuddy.css`, the logo files are `prototypes/assets/taxbuddy-*.svg`, and the look id is `taxbuddy`. Affected sources: the prototype page, stylesheet, service module and checker; the two logo wordmarks; current authorities and planned project names in SDD, SETUP, RELEASE and the F01/F03 plans. The plan below is the original 2026-11-11 record and keeps the names of that day.

Palette options, 2026-11-12 (BD08): `prototypes/taxbuddy.css` gains four `[data-palette]` token blocks, light and dark, that override colours only. `prototypes/index.html` gains a Palette control, a `palette` URL setting, and an inline token-coloured copy of the logo used while an option is selected. `tools/check_prototype.mjs` gains the matching checks. The default path, logo files and DESIGN's current palette are unchanged.

Theme switch in the application, 2026-11-16 (BD09):

| Source | Change |
|---|---|
| `Services/ThemePreference.cs` (new) | The cookie `taxbuddy-theme` (`light` or `dark`; absent means System; one year; HttpOnly, SameSite=Lax, Secure on HTTPS). Also the `POST /preferences/theme` endpoint, which requires an antiforgery token through `[FromForm]` and returns only to local addresses, and the return-address rule |
| `Program.cs` | `UseAntiforgery()` (SDD forms rule) and the endpoint |
| `Components/App.razor` | Writes `data-theme` on `<html>` from the cookie; leaves it out for System |
| `Components/Shared/ThemeSwitch.razor` (new), `Layout/MainLayout.razor`, `Icon.razor` | The header form: three submit buttons with `aria-pressed` in a labelled group, sun, moon and monitor icons, and the current address as the return address (the original address on not-found and error pages) |
| `Components/Shared/BrandLogo.razor` | Writes both logo files; CSS shows the one for the theme. The `<picture>` media query could only follow the browser |
| `Styles/app.css` | Dark tokens for the browser setting (when not overridden) and for a chosen dark; logo and switch rules; a matching Tailwind `dark:` variant |
| `tests/TaxBuddy.Web.Tests/ThemeSwitchTests.cs` (new) | Integration tests that need no database, plus a check that the two dark token blocks are identical |

No database, migration, package or new client is involved. The endpoint belongs to the one web app, like the planned account endpoints, so it is not an API ([AGENTS](../../AGENTS.md)).

Application aligned to the prototype, 2026-11-16 (BD10):
- Each page was rebuilt from its prototype render function, with the prototype's base styles and `taxbuddy.css` ported into `Styles/app.css`.
- New tokens: `ok`, `ok-soft`, `t1` to `t3`, `primary-strong`, radii, `elev`.
- Shell pieces are placed by pages through Blazor sections: `PhoneBar`, `PhoneTabs` and `DeskTitle` in the layout's outlets; `SiteUser` in the site bar.
- The editor's services became rows: the form model, validator and messages follow `editorParts` and `validateFormat`.
- `CLAUDE.md` records the instruction that the prototype is the design origin.
- Verification: the tests updated and passing; a comparison script screenshots the prototype and the app at 390 and 1440 px.

## Affected modules and contracts

| Source | Change | Scenario |
|---|---|---|
| `prototypes/index.html`, `prototypes/kuwenta.css` | New default direction, visual hierarchy, responsive shell, existing interactions | BD01–BD04 |
| `prototypes/assets/` | Light/dark SVG logos, icon and favicon; usage/provenance | BD05 |
| DESIGN, PRD D01, current naming references | Apply current brand and document tokens and stack mapping | BD01, BD06 |
| Prototype README, design directions, STATUS, dated evidence | Record changes and actual verification without inheriting previous passes | BD06 |

## Approach and dependencies

1. Reuse the prototype's semantic HTML, route renderer, state, initials avatars and simulated data. Add a Kuwenta look while preserving the earlier comparison directions.
2. Use CSS variables for semantic colors, spacing, type and shape. Document their later Tailwind mapping; no framework or component library is introduced. Public content remains suitable for static Blazor rendering, while chat/forms retain the interactive-server boundary in SDD.
3. Create code-native SVG assets rather than bitmap logos. Bundle every asset locally.
4. Update only current identity references; preserve historical decision/evidence records and label older design proposals.
5. Verify JavaScript syntax, local assets, meaningful flows, responsive layout, color contrast and documentation. Use an isolated browser profile and synthetic data; clean up owned browser processes.

Logo replaced, 2026-11-18 ([BD12](spec.md#logo-replaced-2026-11-18-bd12)): `prototypes/logo-directions/generate.py` writes the adopted files into `prototypes/assets`; they are copied to the site's `wwwroot/images` under the same names, so only the prototype's inline copy and the `BrandLogo` size change. Verified with the prototype checker, a Release build, the tests and screenshots of both.

One typeface, 2026-11-19 ([BD13](spec.md#one-typeface-2026-11-19-bd13)): fonts were already set through two tokens in each stylesheet, so both now name Quicksand, served from local files; a few weights tuned for a serif were raised. No markup changes.

No schema migration or application build applies to the 2026-11-11 and 2026-11-12 work. Domain selection, external name clearance, real phones, screen readers and application acceptance remain separate work. Authority for visual tokens stays in [DESIGN](../../docs/DESIGN.md); prototype code is a disposable implementation of that contract.
