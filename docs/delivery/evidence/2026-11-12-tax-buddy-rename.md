# Tax Buddy rename evidence

## Identity and attribution

- Date: 2026-11-12. Browser report timestamp: 09:26:03 Asia/Taipei (01:26:03 UTC).
- Observer: agent-executed automated checks and agent visual inspection. No owner acceptance is claimed.
- Scope: [F02 BD07](../../../specs/brand-design-refresh/spec.md#rename-to-tax-buddy-2026-11-12); PRD D01. The user's instruction in the session of 2026-11-12 was "rename the app name to tax buddy".
- Baseline: `1fccef362fe04050977c3f550997ac6637091b4b` on `develop`, clean working tree before the change; the rename is uncommitted. Earlier records remain historical and their passes were not reused.
- Exact exercised source: SHA-256 values for the prototype, stylesheet, service module, checker and all five SVGs are in the retained [browser report](2026-11-12-tax-buddy-rename/results.json). Prototype `index.html`: `469a5309687b226853bff384edbaff34874aa4aa929825deca50f146d6f95b45`; `taxbuddy.css`: `5a65cc833b446440711ea50a45766489e39796a37c0ef8eb640c29be4cbbe985`.
- Environment: Windows `10.0.26300`, x64 as returned by Node; Node 22.12.0; uv-managed Python 3.14.4; headless Edge reports `Edg/157.0.4341.46` through CDP.
- Fixtures: local `file:` prototype, fresh isolated Chromium profile, synthetic directory, message, review and service data, example contact addresses. No real sign-in, account, database, email or deployment.

## Execution

| Procedure / command | Expected | Actual observation | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe" artifacts/tax-buddy-rename` | Existing F02 and F03 flows still work under the new name, look id and file names; page title contains "Tax Buddy"; no visible "AccApp" or "Kuwenta" | Exit 0 on the first run; 142 checks passed, 0 failed | Pass, prototype boundary |
| Semantic contrast computed from actual browser tokens | Text at least 4.5:1; input edges/focus at least 3:1 | 34 pairs across light/dark themes passed; [light values](2026-11-12-tax-buddy-rename/contrast-light.json), [dark values](2026-11-12-tax-buddy-rename/contrast-dark.json). Lowest light text/control 4.76/3.01; dark 5.43/4.06. The palette was not changed | Pass for measured pairs |
| `node --check tools/check_prototype.mjs`; `node --check prototypes/service-workspace.js`; inline script extraction and `node --check` | No JavaScript syntax errors | All three exit 0 | Pass |
| Python standard-library XML parsing and SVG source inspection | All five SVG assets parse, with no font, text or external references | Five parsed; wordmark and symbol are vector paths only | Pass |
| Headless Edge render of both logo files at 700 px and 175 px wide | The redrawn wordmark reads "tax buddy" on light and dark surfaces without clipping | [Rendered logos](2026-11-12-tax-buddy-rename/logo-light-dark.png) reviewed; all nine letters legible, the descender of "y" inside the canvas | Pass, agent-reviewed |
| Profile directory inspection | Isolated browser profile removed | No `browser-profile-*` directory remains in the artifact folder | Pass |
| `uv run --no-project python tools/check_docs.py --ready`; `git diff --check` | Local documentation paths/anchors resolve after the file renames; no whitespace errors | Both exit 0; details under Documentation checks | Pass |

The 142 checks are the existing F02 and F03 checks. Two were edited for the rename: the first now requires "Tax Buddy" in the title and the `taxbuddy` look, and the stale-name check now rejects both "AccApp" and "Kuwenta" in the visible page. The comparison-look loop and the source-hash list use the new identifiers. The contrast pair groups are two of the 142 checks, not 34 additional browser assertions.

Agent-reviewed screenshots: [desktop light](2026-11-12-tax-buddy-rename/directory-desktop-light.png) and [desktop dark](2026-11-12-tax-buddy-rename/directory-desktop-1440-dark.png) show the new wordmark in the header, "Tax Buddy" in the simulated tab and prototype bar, and `taxbuddy.example` in the address pill; [phone 320 px](2026-11-12-tax-buddy-rename/directory-phone-320-light.png) shows the icon with the "Tax Buddy" text heading on one line. Other captures remain in ignored `artifacts/tax-buddy-rename`.

## Documentation checks

`uv run --no-project python tools/check_docs.py --ready` exited 0: 45 Markdown files, 450 local links, 0 errors; 53 external URLs and application routes not executed; 81 placeholders, all in reusable templates. `git diff --check` exited 0. A search of tracked text files for the old name found it only in name-history statements, the F02 record of its original scope, the preserved 2026-11-11 evidence, the unrelated "Kuwentas klaras" name sketch, and the checker's stale-name assertion. Outcome: pass, documentation boundary.

## Changes and preservation

Renamed with history kept: `prototypes/kuwenta.css` to `prototypes/taxbuddy.css`; `prototypes/assets/kuwenta-{logo-light,logo-dark,icon,icon-dark}.svg` to `taxbuddy-*`. The logo canvas widened from 254 to 280 units and the header/footer logo from 159 to 175 CSS px wide; the symbol, favicon and palette are unchanged. Current documents, the prototype and planned project/configuration names (`TaxBuddy.*`, `ConnectionStrings:TaxBuddy`) use the new name.

Not changed: ADRs 001–009; evidence records dated 2026-11-10 and 2026-11-11 and their retained files, including the `2026-11-11-kuwenta` folder and the old file names inside their source-hash lists; the earlier name sketches in `brand-options.html` apart from its notice of the current identity; the earlier AccApp studies in DESIGN-DIRECTIONS.

Recorded tension: the 2026-11-11 refresh preferred a name not limited to tax. Tax Buddy is tax-specific, while PRD scope still includes bookkeeping, payroll and other services. This is noted in [DESIGN-DIRECTIONS](../../../prototypes/DESIGN-DIRECTIONS.md#current-design-review) and left to the owner.

## Restoration and limits

The helper closed only its own Edge instance and removed its fresh profile. A second throwaway profile used for the logo render was in the session scratch directory and was deleted. No real browser profile or device settings were changed, no dependencies were installed and no server was started.

Not exercised: other browsers; real phones; screen readers; a full keyboard walkthrough; true browser zoom; real network loss; production Tailwind/Blazor output; server authorization, persistence, OAuth, email, hosting or deployment. Name, trademark and domain availability for "Tax Buddy" were not checked and remain open in PRD D01. The application does not exist, so no .NET build or application tests were run. Owner walkthrough and acceptance remain pending.
