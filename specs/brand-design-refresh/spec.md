# F02: Brand and prototype refresh (Kuwenta, renamed Tax Buddy)

Amended 2026-11-12: the user instructed renaming the app to Tax Buddy; see [the rename section](#rename-to-tax-buddy-2026-11-12). Amended 2026-11-16: an application theme switch ([BD09](#theme-switch-in-the-application-2026-11-16)) and a name survey ([record](#name-availability-2026-11-16)), at the user's instruction. The text before that section records the original 2026-11-11 scope under the name Kuwenta.

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

## Theme switch in the application, 2026-11-16

The user's instruction was "implement f02". Asked which remaining F02 work to do, the user chose: add a theme switch, check name availability, and close the F02 records. Baseline: commit `af9d3ea60af1cda9ad022322904b76a74e3900cb`, clean apart from an untracked `.vscode/`.

Before this, F01 had already brought F02's brand into the application: Paper & Forest tokens, light and dark following the browser, the logo, favicon, type, focus outline and reduced motion ([DESIGN as built](../../docs/DESIGN.md#stack-and-component-mapping)). The application had no way to choose a theme. The prototype's Theme menu is a review control, so this is new application behaviour.

Scope:
- The header gets a Light / Dark / System switch, and the choice applies on every page.
- System is the default and follows the browser setting.
- The switch is a plain form. It works on static pages without script, and the server writes the chosen theme into the first response, so the page never flashes the wrong colours.
- The choice is stored in a cookie with no personal data ([SDD](../../docs/SDD.md#data-and-persistence-contracts)).

Exclusions: no account-level setting, no change to the palette or the logo files, no other new behaviour.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD09 / DESIGN; SDD forms and no-script rule | A visitor on any page presses Light, Dark or System in the header | The page reloads at the same address with that theme; the logo, colours and the pressed option agree; the choice is kept across pages and visits until changed; System removes it and follows the browser again; without script it still works; a forged post (no antiforgery token, unknown value or an off-site return address) changes nothing or returns home; the switch is keyboard reachable with visible focus, its buttons are at least 44 px, and phones show icons with the names kept for screen readers | Web integration tests; Edge at 320–1440 px in both browser settings; screenshot review |

## Application aligned to the prototype, 2026-11-16 (BD10)

The user instructed: "update the current implementation based on the prototype, that should be the origin of the design". The prototype is now the design origin for every page ([CLAUDE.md](../../CLAUDE.md)). Baseline: branch `feature/f02-f04-theme-switch-google-sign-in` with F02 and F04 staged, plus F05 unstaged.

In scope:
- The site shell from `desktopShell` and `phoneShell`: the top bar with logo, navigation and avatar, or a "Sign in" button; the phone app bar with back and title; the phone tab bar; the footer.
- The Account page from `meHTML`.
- Sign-in and role choice from `signinHTML`.
- The profile editor from `editorParts`: status pill, listing checklist and meter, error summary, service rows with a dropdown, field labels, hints and messages, save status.
- The legal pages from `infoPage`.
- The tokens these use (`ok`, `ok-soft`, avatar tones, `primary-strong`, radii, elevation).

Kept, and recorded as deviations from the prototype:
1. The theme switch (BD09, asked for by the user) stays, moved from the header to the footer. The prototype has no theme control on the site.
2. Pages stay static and work without script (SDD; C04 for the editor). Choice buttons can't be disabled until the box is ticked, the end month can't be disabled live, and there is no leave confirmation or "Saving…" spinner; the server checks and messages replace them.
3. Links to features that aren't built yet are left out: Chats, Clients or My accountants, For accountants, About, Help, email settings, My reviews, Delete account, and the portfolio fieldset (R12).
4. Microsoft sign-in is not offered until it is built.
5. The editor's stale-save warning has no prototype counterpart; it uses the error-summary style.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD10 / DESIGN, prototype | Open the directory, a profile, sign-in, registration, Account, the profile editor and the legal pages, signed out, as a client and as an accountant, on phone and desktop, light and dark | Each page follows its prototype screen's layout, wording and states, apart from the recorded deviations. The behaviour of F01, F04 and F05 is unchanged, and their tests still pass | Web tests, Edge screenshots compared with the prototype |

## Name availability, 2026-11-16

This was a preliminary web survey for D01 and the open rename task ([record](../../docs/delivery/evidence/2026-11-16-name-availability.md)), not clearance. It found active "TaxBuddy" tax products in the Philippines (taxbuddyph.com) and India (taxbuddy.com), and a live US TAXBUDDIES mark for tax services. IPOPHL, WIPO and domain registrars were not searched directly. Nothing was decided; D01 stays with the owner.

## Evidence limits

Prototype verification cannot establish server authorization, real provider sign-in, delivery, persistence, production accessibility or release acceptance. D15's product accessibility targets remain open; tested widths and contrast are implementation observations. Results belong in [STATUS](../../docs/delivery/STATUS.md).
