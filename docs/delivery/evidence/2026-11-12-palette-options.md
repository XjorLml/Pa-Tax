# Palette options prototype evidence

## Identity and attribution

- Date: 2026-11-12. Browser report timestamp: 10:27:53 Asia/Taipei (02:27:53 UTC).
- Observer: agent-executed automated checks and agent visual inspection. No owner acceptance or palette choice is claimed.
- Scope: [F02 BD08](../../../specs/brand-design-refresh/spec.md#palette-options-2026-11-12); DESIGN visual system. The user asked the agent to suggest other colour palettes, then instructed "add these choices in the index.html".
- Baseline: `1fccef362fe04050977c3f550997ac6637091b4b` on `develop` plus the uncommitted [Tax Buddy rename](2026-11-12-tax-buddy-rename.md) of the same day; this change is also uncommitted. The rename record describes the sources before this change, and its hashes apply to that earlier state only.
- Exact exercised source: SHA-256 values are in the retained [browser report](2026-11-12-palette-options/results.json). Prototype `index.html`: `934e39bbfa55cdc521c3707b32f47f717271fdff8b5f9d7ba168a6f9304e3ddd`; `taxbuddy.css`: `2dddc60aa13e09c30eebaf81042a65445604bfe83e654fff0ba1233cc4624ec8`; `tools/check_prototype.mjs`: `c471733ed6cfb1d0527f7410afb0dadaa7b3430d3e6927b9daa7d7f42798fde2`.
- Environment: Windows `10.0.26300`, x64 as returned by Node; Node 22.12.0; uv-managed Python 3.14.4; headless Edge reports `Edg/157.0.4341.46` through CDP.
- Fixtures: local `file:` prototype, fresh isolated Chromium profile, synthetic data. No real sign-in, account, database, email or deployment.

## Execution

| Procedure / command | Expected | Actual observation | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe" artifacts/palette-options` | The 142 earlier checks still pass with the default palette; the 36 new palette checks pass | Exit 0 on the first run; 178 checks passed, 0 failed | Pass, prototype boundary |
| Default palette (new check) | Without a valid `palette` setting the page uses Paper & Forest and the logo files | `data-palette` is `forest`; header logo is the SVG file; no token-coloured logo present | Pass |
| Each option in light and dark, desktop (16 new checks) | The option's tokens apply, the logo is drawn in the option's colours, six cards render, and the 17 semantic contrast pairs meet 4.5:1 for text and 3:1 for control edges | All pass; values in [contrast-palettes.json](2026-11-12-palette-options/contrast-palettes.json) and the table below | Pass for measured pairs |
| Each option on the 390 px phone frame (12 new checks) | No horizontal overflow, local images load, header icon takes the option's colour, primary differs from the current palette | All pass | Pass |
| Control behaviour (3 new checks) | Changing the Palette control switches in place; returning to Paper & Forest restores the current primary and file logos; with an earlier look the control is disabled and has no effect | All pass | Pass |
| Current-palette contrast | Unchanged from the rename run | 34 pairs pass; [light](2026-11-12-palette-options/contrast-light.json), [dark](2026-11-12-palette-options/contrast-dark.json); lowest light text/control 4.76/3.01, dark 5.43/4.06 | Pass |
| `node --check tools/check_prototype.mjs`; `node --check prototypes/service-workspace.js`; inline script extraction and `node --check` | No JavaScript syntax errors | All three exit 0 | Pass |
| Profile directory inspection | Isolated browser profile removed | No `browser-profile-*` directory remains | Pass |
| `uv run --no-project python tools/check_docs.py --ready`; `git diff --check` | Local documentation paths/anchors resolve; no whitespace errors | Both exit 0: 46 Markdown files, 476 local links, 0 errors; 53 external URLs not executed | Pass, documentation boundary |

Lowest measured contrast per option, from the browser's computed tokens:

| Option | Light text / control edges | Dark text / control edges |
|---|---|---|
| Ink & Ledger Blue | 4.67 / 3.06 | 5.71 / 4.17 |
| Lagoon Teal | 4.58 / 3.04 | 5.44 / 4.05 |
| Plum & Blush | 4.75 / 3.21 | 6.26 / 4.27 |
| Marigold & Charcoal | 5.01 / 3.47 | 6.09 / 4.38 |

Agent-reviewed screenshots: [Ink light](2026-11-12-palette-options/palette-ink-light.png), [Ink phone](2026-11-12-palette-options/palette-ink-phone-390-light.png), [Lagoon light](2026-11-12-palette-options/palette-lagoon-light.png), [Plum dark](2026-11-12-palette-options/palette-plum-dark.png), [Marigold light](2026-11-12-palette-options/palette-marigold-light.png), [Marigold dark](2026-11-12-palette-options/palette-marigold-dark.png). In each, the header logo, navigation underline, buttons, hero and chips follow the option. Other captures remain in ignored `artifacts/palette-options`.

## Observations and limits

- The options change colour tokens only. Danger, warning and success colours, type, spacing and layout are inherited from the Tax Buddy look.
- Marigold & Charcoal: in the dark theme the marigold link colour is close to the inherited amber warning colour, and in the light theme the warning background is close to the hero background. The 17 measured pairs do not cover that similarity. A warning hue would need to change if this option is chosen.
- Plum & Blush: the inherited error red is close in hue to plum. Error states keep their icon and wording.
- While an option is selected the logo is an inline copy of the logo paths coloured by tokens. The SVG files and favicon keep the current colours and were not regenerated per option.
- With the extra Palette control, the Theme control wraps to a second row of the prototype bar at 1440 px. The bar is not part of the site.
- Only the directory page was screenshot-reviewed per option. Other pages use the same tokens but were exercised with the default palette only.

The helper closed only its own Edge instance and removed its fresh profile. No real browser profile or device settings were changed, no dependencies were installed and no server was started.

Not exercised: other browsers; real phones; screen readers; colour-vision simulation; true browser zoom; production Tailwind/Blazor output; any application behaviour. No palette has been chosen: [DESIGN](../../DESIGN.md#visual-system) still names Paper & Forest as the current palette. Owner review and acceptance remain pending.
