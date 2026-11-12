# Brand and prototype refresh plan (Kuwenta, renamed Tax Buddy)

Status: authorized, local prototype work. Scenarios: [spec](spec.md); execution: [tasks](tasks.md).

Rename, 2026-11-12 (BD07): `prototypes/kuwenta.css` is now `prototypes/taxbuddy.css`, the logo files are `prototypes/assets/taxbuddy-*.svg`, and the look id is `taxbuddy`. Affected sources: the prototype page, stylesheet, service module and checker; the two logo wordmarks; current authorities and planned project names in SDD, SETUP, RELEASE and the F01/F03 plans. The plan below is the original 2026-11-11 record and keeps the names of that day.

Palette options, 2026-11-12 (BD08): `prototypes/taxbuddy.css` gains four `[data-palette]` token blocks, light and dark, that override colours only. `prototypes/index.html` gains a Palette control, a `palette` URL setting, and an inline token-coloured copy of the logo used while an option is selected. `tools/check_prototype.mjs` gains the matching checks. The default path, logo files and DESIGN's current palette are unchanged.

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

No schema migration or application build applies. Domain selection, external name clearance, real phones, screen readers and application acceptance remain separate work. Authority for visual tokens stays in [DESIGN](../../docs/DESIGN.md); prototype code is a disposable implementation of that contract.
