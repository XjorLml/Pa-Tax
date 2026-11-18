# Tax Buddy identity assets

Original code-native vector artwork created by the agent for this repository. No third-party artwork, icons, downloaded fonts or traced logos were used. The wordmark consists of original drawn paths; the SVGs contain no font dependency. Source-code licensing remains PRD D16. The logo files carry no third-party licence; the one third-party asset here is the Quicksand font in [fonts](fonts/OFL.txt), under the SIL Open Font License 1.1. Name/domain clearance has not been performed. No trademark register was searched for the mark; a web survey on 2026-11-19 is the only originality check.

## History

- 2026-11-11: the first identity was created under the user's delegated branding brief: a ledger with a speech corner on a forest tile, and a lowercase geometric wordmark, as Kuwenta.
- 2026-11-12: the user renamed the app to Tax Buddy. The agent redrew the wordmark as "tax buddy" in the same strokes, widened the logo canvas from 254 to 280 units, and renamed the files from `kuwenta-*` to `taxbuddy-*`. The symbol and favicon were unchanged.
- 2026-11-18: the user chose a new logo from three proposals and asked for it across the app ([F02 BD12](../../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)). The five files below were replaced under the same names; the logo canvas is now 258 × 48 units. The proposals and the generator are in [logo-directions](../logo-directions/README.md). Evidence dated before this change shows the earlier logo.
- 2026-11-19: the user asked for one font style matching the logo. Quicksand was added in `fonts/` for the prototype's and the site's text ([F02 BD13](../../specs/brand-design-refresh/spec.md#one-typeface-2026-11-19-bd13)). The logo itself stays drawn paths and does not use the font.
- 2026-11-19: the wordmark's strokes became filled outlines, and the directory's decorative illustration took the same bubble. A preliminary web survey of similar marks was made ([record](../../docs/delivery/evidence/2026-11-19-f02-logo-outlines-hero.md)); it is not clearance.

## Files

| Asset | Use |
|---|---|
| [Light logo](taxbuddy-logo-light.svg) | Full identity on white or warm paper |
| [Dark logo](taxbuddy-logo-dark.svg) | Full identity on dark forest surfaces |
| [Icon](taxbuddy-icon.svg), [dark icon](taxbuddy-icon-dark.svg) | Phone app bar, sign-in, compact branding |
| [Favicon](favicon.svg) | Simplified 16-unit browser icon |

The symbol is a round speech bubble with one square corner, the speech tail, holding a solid T: a conversation with the T of Tax Buddy, which also recalls the rule of a bookkeeper's T-account. It is not a verification badge. The T is cut out of the bubble, so the surface behind shows through it. The wordmark is "Tax Buddy" in title case, drawn rather than typeset: "Tax" in ink `#1E332C` and "Buddy" in forest `#245C46`, or `#EEF1E7` and sage `#B7D8A4` on dark surfaces. Since 2026-11-19 its letters are filled outlines with no strokes. Each stroke of a letter is its own closed shape, so the shapes overlap; have them merged into single contours before print or a trademark filing.

## Usage

- Show the full logo at 161 × 30 CSS px in the site bar and footer, and no narrower than 120 px. Use the icon at 24 CSS px or larger; below that use the favicon, which is drawn for 16 px.
- Keep clear space of one quarter of the icon's width on every side.
- Pair light assets with light surfaces, dark assets with dark surfaces. Preserve proportions and avoid shadows, outlines or unapproved colour changes.
- Standalone logos have accessible titles. In an HTML header with the adjacent name, use `alt=""` on the icon to avoid announcing the name twice.

[DESIGN](../../docs/DESIGN.md#visual-system) owns the palette and UI identity. The site loads copies of these files from `src/TaxBuddy.Web/wwwroot/images`; keep the two folders the same. One-colour, stacked and PNG versions are in [logo-directions/c-t-account](../logo-directions/README.md).
