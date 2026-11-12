# F02: Brand and prototype refresh (Kuwenta, renamed Tax Buddy)

Amended 2026-11-12: the user instructed renaming the app to Tax Buddy; see [the rename section](#rename-to-tax-buddy-2026-11-12). The text before that section records the original 2026-11-11 scope under the name Kuwenta.

Status: authorized prototype work, 2026-11-11. The user asked to execute the preceding design brief: review documentation and prototype, improve the design within the existing stack, choose a replacement name, create reusable logo assets, and verify the result. This delegates the design choice; it does not approve product release or unresolved behavior.

Governing requirements: [PRD](../../docs/PRD.md) R01–R18, especially R01–R03, R06, R10–R14, R16–R17; D01 (name, domain remains open). Contracts: [DESIGN](../../docs/DESIGN.md), [SDD stack](../../docs/SDD.md#technology-stack). Implementation: [plan](plan.md); tracking: [tasks](tasks.md).

## Intent and scope

Baseline: commit `668a9a9e122787f2c86f6f6b4a63c398cc78d282`; clean working tree. The existing HTML/JavaScript prototype contains four visual proposals, directory, profiles, messaging, account, and information flows. No Blazor projects exist.

Apply Kuwenta as the current identity, with an original ledger/conversation logo, a coherent light/dark visual system, clearer directory hierarchy and responsive layouts. Keep the existing simulation controls and alternative looks. Work in this checkout; no publication, runtime scaffold, dependency change, migration, new API, or new product behavior. Historical evidence and ADRs retain the old name and their provenance. Other PRD decisions remain open.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD01 / D01 | Open the prototype and current docs | Kuwenta replaces the working identity; header, sign-in, help and simulated emails agree; domain remains an example | Source review and browser |
| BD02 / R01–R03, R12–R13 | Browse and open a profile | Search leads the directory; comparable cards show services, experience and ratings; profile contact action remains clear | Desktop/phone browser |
| BD03 / DESIGN | Switch theme and use a narrow viewport | Readable light/dark themes, visible focus, no horizontal clipping; reduced motion is respected | Browser layout and token contrast checks |
| BD04 / R04–R10, R14–R18 | Exercise existing sign-in, chat failure/retry, editor and information flows | Existing prototype interactions still work; typed text survives the simulated failures | Browser simulation, not server acceptance |
| BD05 / D01 | Open logo variants and favicon | Original SVG assets render locally without fonts or network downloads; usage and provenance documented | XML/source and browser |
| BD06 / documentation | Run documentation checks and inspect diff | Links resolve, scope remains traceable, historical observations are preserved | Documentation checker and review |

## Rename to Tax Buddy, 2026-11-12

The user's instruction was "rename the app name to tax buddy". Baseline: commit `1fccef362fe04050977c3f550997ac6637091b4b`, clean working tree.

Scope: the display name becomes Tax Buddy. Identifiers become `taxbuddy` (stylesheet, logo files, look id, example domain) and `TaxBuddy` (planned project and configuration names). The wordmark is redrawn; the logo symbol, favicon, palette, layout and behavior stay as they are. BD01 and BD05 now read with Tax Buddy as the name. ADRs and evidence dated before the rename keep their names and paths. No name/domain clearance, publication or new product behavior.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD07 / D01 | Open the prototype, logo files and current docs | Tax Buddy appears in the title, header, footer, sign-in, information pages, simulated email and generated samples; no visible "Kuwenta" or "AccApp"; both logo variants read "tax buddy" and load locally; current docs and planned project names use the new name; earlier records are preserved | Browser checker, screenshot review, documentation checker and diff |

## Palette options, 2026-11-12

The user asked for other colour palettes to be suggested, then instructed "add these choices in the index.html". Baseline: commit `1fccef362fe04050977c3f550997ac6637091b4b` plus the uncommitted rename above.

Scope: four colour proposals for the Tax Buddy look (Ink & Ledger Blue, Lagoon Teal, Plum & Blush, Marigold & Charcoal), selectable in the prototype beside the current Paper & Forest palette. They change colour tokens only. This is a comparison aid: no palette is chosen, [DESIGN](../../docs/DESIGN.md#visual-system) keeps Paper & Forest as the current palette, and the logo files and favicon are not regenerated. No new product behavior.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD08 / DESIGN | Open the prototype and use the Palette control or the `palette` URL setting | Paper & Forest is the default; each option recolours the site and its logo in light and dark, keeps layout and type, and meets the measured text and control-edge contrast targets; the control is disabled for earlier looks; the notes panel marks an option as a proposal | Browser checker with per-option contrast, screenshot review |

## Evidence limits

Prototype verification cannot establish server authorization, real provider sign-in, delivery, persistence, production accessibility or release acceptance. D15's product accessibility targets remain open; tested widths and contrast are implementation observations. Results belong in [STATUS](../../docs/delivery/STATUS.md).
