# Brand and prototype refresh tasks (Kuwenta, renamed Tax Buddy)

Scope: [spec](spec.md). Approach: [plan](plan.md).

- [x] Review relevant authorities, inspect source, identify requirements and clean baseline.
- [x] Record authorization, observable scenarios, exclusions and verification boundaries.
- [x] Apply Kuwenta identity and create original SVG assets with usage/provenance.
- [x] Implement cohesive default look and responsive directory/profile/shell refinements.
- [x] Update current authorities and preserve historical records.
- [x] Check script syntax, assets, contrast and existing interactions in the available browser.
- [x] Review desktop/mobile and light/dark screenshots; record concrete limits.
- [x] Validate documentation, traceability and final diff; [dated evidence](../../docs/delivery/evidence/2026-11-11-kuwenta-refresh.md).
- [ ] Record owner review and release acceptance (pending; outside this prototype task).

Rename to Tax Buddy, 2026-11-12 (BD07):

- [x] Check the clean baseline and list every current and historical use of the name.
- [x] Rename the stylesheet, logo files, look id, example domain and planned project names; redraw the wordmark.
- [x] Update current authorities with the name history; leave ADRs and earlier evidence unchanged.
- [x] Rerun the browser checker, syntax, SVG and documentation checks; review screenshots; [dated evidence](../../docs/delivery/evidence/2026-11-12-tax-buddy-rename.md).
- [ ] Check name, trademark and domain availability for Tax Buddy (PRD D01; not done).

Palette options, 2026-11-12 (BD08):

- [x] Draft four palettes and check their values against the checker's contrast pairs before adding them.
- [x] Add the option tokens to `prototypes/taxbuddy.css` and the Palette control, `palette` URL setting and token-coloured logo to `prototypes/index.html`.
- [x] Extend the browser checker with default, per-option, phone and control checks; rerun all checks; review screenshots; [dated evidence](../../docs/delivery/evidence/2026-11-12-palette-options.md).
- [x] Record the options as proposals in DESIGN, DESIGN-DIRECTIONS and the prototype README.
- [ ] Owner chooses a palette or keeps Paper & Forest; then update DESIGN, the logo files and favicon, and remove the unchosen options (pending).

No .NET application exists, so application builds and server/provider tests are not applicable. Prototype results do not satisfy those future gates.
