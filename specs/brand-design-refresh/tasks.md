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
- [ ] Check name, trademark and domain availability for Tax Buddy (PRD D01). 2026-11-16: preliminary web survey done ([record](../../docs/delivery/evidence/2026-11-16-name-availability.md)). It found active TaxBuddy tax products in the Philippines and India and a live US TAXBUDDIES mark. Remaining: the owner's own search of IPOPHL and WIPO, a registrar check, then a decision to keep or change the name.

Palette options, 2026-11-12 (BD08):

- [x] Draft four palettes and check their values against the checker's contrast pairs before adding them.
- [x] Add the option tokens to `prototypes/taxbuddy.css` and the Palette control, `palette` URL setting and token-coloured logo to `prototypes/index.html`.
- [x] Extend the browser checker with default, per-option, phone and control checks; rerun all checks; review screenshots; [dated evidence](../../docs/delivery/evidence/2026-11-12-palette-options.md).
- [x] Record the options as proposals in DESIGN, DESIGN-DIRECTIONS and the prototype README.
- [x] Owner chooses a palette or keeps Paper & Forest; then update DESIGN, the logo files and favicon, and remove the unchosen options. On 2026-11-13 the user chose Paper & Forest ("for pallete go for paper and forest"). DESIGN records it. The logo files and favicon were already Paper & Forest, so they needed no change. By DESIGN's decision of the same day, the four proposals stay in the prototype as history and the application doesn't use them. Closed 2026-11-16.

The tasks above were prototype work; no application existed when they were done.

Theme switch in the application, 2026-11-16 (BD09):

- [x] Check the baseline and identify what F01 had already applied; record the user's chosen scope and the BD09 scenario.
- [x] Add the cookie, the endpoint with antiforgery and local-only return, `data-theme` on `<html>`, the header switch, the theme-aware logo and the stylesheet rules.
- [x] Add integration tests; build in Release; run all tests; run the browser check at four widths in both browser settings; review screenshots ([evidence](../../docs/delivery/evidence/2026-11-16-f02-theme-switch.md)).
- [x] Update DESIGN, SDD, PRD D10, SETUP, STATUS and the documentation index.
- [x] Run the database tests in an environment with the development database: 82 passed, 0 skipped, 2026-11-16 ([evidence](../../docs/delivery/evidence/2026-11-16-f02-theme-switch.md)).
- [ ] Owner walkthrough of the switch, including whether its own row on phones is acceptable (gate G9). Since BD10 the switch is in the footer.

Application aligned to the prototype, 2026-11-16 (BD10):

- [x] Record the owner's instruction in CLAUDE.md, AGENTS.md, the prototype README and DESIGN; save it to the agent's memory.
- [x] Read the prototype's render functions and styles for every built page.
- [x] Rebuild the shell (site bar, phone app bar and tab bar, footer), sign-in and role, the new Account page, the profile editor and the legal pages; port the styles and tokens.
- [x] Update the tests to the prototype's wording and fields: 149 passed, 0 skipped. Compare screenshots with the prototype at 390 and 1440 px; fix the gaps found ([evidence](../../docs/delivery/evidence/2026-11-16-f02-prototype-alignment.md)).
- [x] Record the deviations in DESIGN, the SDD, and the F04 and F05 specs.
- [x] Side-by-side check of F01's directory and profile pages against the prototype, and alignment, on 2026-11-16 ([evidence](../../docs/delivery/evidence/2026-11-16-f02-prototype-alignment.md#f01-pages-compared-and-aligned-later-the-same-day)).
- [ ] Owner walkthrough of the aligned pages (G9).
