# Tax Buddy logo directions

**Direction C was adopted on 2026-11-18; A and B are kept as history.** Three logo directions drawn by the agent on 2026-11-18 from the owner's logo brief of the same date. The owner reworked direction C and then said "update the docs then implement this to the entire app" ([F02 BD12](../../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)). The adopted files are in [assets](../assets/README.md), and [DESIGN](../../docs/DESIGN.md#visual-system) describes the identity. Name and domain clearance remain PRD D01 and have not been performed. No trademark register was searched for these marks; a web survey of 2026-11-19 covers direction C only ([record](../../docs/delivery/evidence/2026-11-19-f02-logo-outlines-hero.md)).

Original code-native vector artwork: no third-party artwork, icons, downloaded fonts or traced logos. The wordmark is drawn, not typeset, so no SVG depends on an installed font. In directions A and B its letters are stroked centre lines. In direction C, the adopted one, they are filled outlines since 2026-11-19: overlapping closed shapes, not yet merged into single contours.

| Direction | Idea |
|---|---|
| [A: the speaking page](a-speaking-page/logo-horizontal.svg) | A ledger page with a folded corner that grows a speech tail. Closest to the current symbol |
| [B: the open spread](b-open-spread/logo-horizontal.svg) | Two facing speech bubbles meet at a spine and form an open ledger |
| [C: the T-account](c-t-account/logo-horizontal.svg) | A circle with one square corner for the speech tail, holding one solid T: the rule of a T-account and the T of Tax Buddy. Reworked on 2026-11-18 at the owner's request, each version replacing the last: "more modern and eye catching" (a rounded bubble with a triangular tail became the circle, with the T cut in two); "make the T as one" with a taxi checker band; then the taxi design was removed ("not so good"), leaving the solid T |

## Files in each folder

| File | Use |
|---|---|
| `symbol.svg`, `logo-horizontal.svg`, `logo-stacked.svg` | Full colour on light surfaces: forest `#245C46` symbol, ink `#1E332C` wordmark |
| `*-reversed.svg` | Dark surfaces (`#111C18`): sage `#B7D8A4` symbol, `#EEF1E7` wordmark |
| `*-forest.svg`, `*-black.svg`, `*-white.svg` | One colour |
| `app-icon.svg`, `app-icon-512.png`, `app-icon-192.png` | Forest tile with a paper symbol |
| `favicon.svg`, `favicon-32.png`, `favicon-16.png` | Simplified glyph on a 16-unit grid, drawn for 16 px |

Direction A uses a second colour for the folded corner (sage on light, `#EEF1E7` on dark); the symbols of B and C are one colour. Direction C has a heavier wordmark in two tones ("Tax" in ink, "Buddy" in forest; `#EEF1E7` and sage when reversed), a sage symbol on the forest tile of its app icon, and a favicon where the symbol fills the square with no tile. All files have transparent backgrounds.

## Usage

- **Clear space:** one quarter of the symbol's width on every side of the symbol or lockup.
- **Minimum size:** horizontal lockup 120 px wide; stacked lockup 96 px wide; symbol 24 px. Below 24 px use the favicon.
- Pair light files with light surfaces and reversed files with dark surfaces. Keep proportions; no shadows, outlines, gradients or other colours.

## Rebuild

`generate.py` writes every SVG: `python3 generate.py . ../assets` from this folder. The second folder receives the adopted identity under the file names the prototype and the site load; copy those five files to `src/TaxBuddy.Web/wwwroot/images` afterwards. The PNGs were rendered from `app-icon.svg` and `favicon.svg` with Microsoft Edge headless on 2026-11-18.
