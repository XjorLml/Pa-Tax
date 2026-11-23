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
3. Links to features that aren't built yet are left out: Chats, Clients or My accountants, For accountants, About, Help, email settings, My reviews, Delete account, and the portfolio fieldset (R12). *Superseded in part:* Chats came with F07, the portfolio fieldset with F08; My reviews was later left out by the user's choice (F09).
4. Microsoft sign-in is not offered until it is built.
5. The editor's stale-save warning has no prototype counterpart; it uses the error-summary style.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD10 / DESIGN, prototype | Open the directory, a profile, sign-in, registration, Account, the profile editor and the legal pages, signed out, as a client and as an accountant, on phone and desktop, light and dark | Each page follows its prototype screen's layout, wording and states, apart from the recorded deviations. The behaviour of F01, F04 and F05 is unchanged, and their tests still pass | Web tests, Edge screenshots compared with the prototype |

## Prototype alignment check, 2026-11-18 (BD11)

The user said "check current implementation against the prototype. follow what's on the prototype then update the documentation".
- **Audit:** three read-only audits compared every built page (F01, F02, F04–F10) with its prototype render function and styles.
- **Changes:** the unrecorded differences were changed to match. The list of changes and of the deviations kept is in [DESIGN](../../docs/DESIGN.md#visual-system) ("Checked against the prototype again in BD11").
- **Kept:** the user's own decisions stay. These are the static editor, review and report pages, the report page instead of a dialog, radio inputs drawn as stars and chips, and status lines instead of toasts.
- **Evidence:** [2026-11-18 record](../../docs/delivery/evidence/2026-11-18-prototype-alignment.md).

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD11 / DESIGN, prototype | Every built page, signed out, as a client and as an accountant, at 390 and 1440 px, light and dark | Layout, wording and states follow the prototype, apart from the deviations recorded in DESIGN; all tests still pass | Web tests, Edge checks and screenshots |

## Logo replaced, 2026-11-18 (BD12)

The user gave a logo brief, reviewed three proposals ([logo directions](../../prototypes/logo-directions/README.md)) and reworked the third: "improve option c to make it more modern and eye catching", "make the T as one, not two separated lines", and, after a taxi motif was tried, "just remove the taxi related design". The user then said "update the docs then implement this to the entire app".

- **Identity:** a round speech bubble with one square corner, holding a solid T, and a drawn "Tax Buddy" wordmark in title case and two tones. It replaces the ledger with a conversation corner and the lowercase wordmark.
- **Files:** the five files in [prototypes/assets](../../prototypes/assets/README.md) keep their names and are copied to `src/TaxBuddy.Web/wwwroot/images`. The logo's box changes from 280 × 48 to 258 × 48 units, so it is shown at 161 × 30 CSS px instead of 175 × 30.
- **Prototype:** the inline copy used by the palette options is redrawn; its second word takes the palette's primary colour. The phone app bar's symbol now follows the theme, as the sign-in symbol already did.
- **Site:** `BrandLogo` takes the new size. `BrandIcon`, the favicon link and the pages need no change, because the file names are the same.
- **Not changed:** the palette, the type and every page layout.
- **Deviation from the brief:** the wordmark is a drawn rounded sans, not a serif.
- **Follow-up, 2026-11-19** (the user said "do this all" of three items the agent had left open; [evidence](../../docs/delivery/evidence/2026-11-19-f02-logo-outlines-hero.md)):
  - **Outlines:** the wordmark's letters are now filled outlines. Each stroke of a letter is its own closed shape, so the shapes overlap and are not merged into one contour.
  - **Directory illustration:** the decorative ledger keeps its open book; its rectangular speech bubble became the logo's bubble with the T, in the prototype and the site.
  - **Build:** the Tailwind step now also runs in WSL when only the Windows Node is installed ([SETUP](../../docs/SETUP.md#troubleshooting-and-maintenance)).
  - **Survey:** a preliminary web survey of similar marks and of domain records. It is not clearance.
- **Not done:** name and domain clearance (D01), a search of IPOPHL or any trademark register for the name or the figurative mark, and a comparison with the two existing TaxBuddy logos.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD12 / D01, DESIGN | Open the prototype and the site: site bar, footer, phone app bar, sign-in, browser tab, light and dark, and a palette option in the prototype | The new symbol and wordmark show everywhere the old logo did; the dark files show on dark surfaces; palette options recolour the inline copy; no old logo remains | Prototype checker, build, tests, and screenshots of the prototype and the running site ([evidence](../../docs/delivery/evidence/2026-11-18-f02-logo.md)) |

## One typeface, 2026-11-19 (BD13)

The user said "i can see that there are different font styles stick to one font style used in the logo it it stays consistent? update the docs then implement for prototype and the site". The agent read this as: one typeface across the prototype and the site, matching the logo.

- **Choice:** the logo's letters are drawn, not set in a font, so no font is identical. Ten open-licence families were rendered beside the logo; Quicksand was the closest (rounded strokes, single-storey "a", round bowls).
- **Applied:** headings, body and controls all use Quicksand. Weights: body 500, labels and buttons 600, headings 700. Sizes, spacing and layouts are unchanged.
- **Files:** two font files and the licence, in `prototypes/assets/fonts` and `src/TaxBuddy.Web/wwwroot/fonts`, loaded by `@font-face` with `font-display: swap`. Nothing is requested from another site.
- **Rule changed:** DESIGN's "system fonts, no downloads" no longer holds. The earlier comparison looks in the prototype keep their own fonts.
- **Not changed:** the logo, which stays drawn paths.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD13 / DESIGN typography | Open any page of the prototype's Tax Buddy look or of the site, light and dark | Headings, body text, buttons and the phone app bar's name are in one typeface that matches the logo; no serif remains; the font files load from the same site | Prototype checker, build, tests, and screenshots of both ([evidence](../../docs/delivery/evidence/2026-11-19-f02-typeface.md)) |

## Theme toggle in the header, 2026-11-23 (BD14)

The user sent screenshots of the footer's Light/Dark/System switch and of "Sign in" with a moon icon beside it, and said: "in the prototype and the app move these beside the sign in the theme change icon should always be in the header". This replaces BD09's footer switch and the "no theme control in the prototype" deviation.

- **Where:** at the end of the site bar, after Sign in or the account link, and at the end of the phone app bar, after a page's own actions. Every page has it, signed in or not.
- **What:** one icon button. A moon ("Switch to dark theme") shows in a light theme and a sun ("Switch to light theme") in a dark one, the same choice the logo makes, so it is right for System too. Pressing it stores light or dark (the same cookie and address as BD09) and returns to the page.
- **System:** there's no System button any more. A visitor follows the device until they press the icon; after that the choice stays for a year. The address still accepts `system`, in case a "use the device setting" link is wanted later.
- **Site:** `ThemeToggle.razor` replaces `ThemeSwitch.razor`, in `SiteUser` and `PhoneBar`. Both buttons are in the page, and the stylesheet hides one, so it works without script. The footer no longer has a theme control.
- **Prototype:** `themeToggleHTML` in `desktopShell` and `phoneShell`. It changes the prototype's theme, and the "Theme" simulation control follows.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD14 / DESIGN theme | Open any page at phone and desktop widths, with the device light and dark; press the icon, then press it again | One icon in the header: a moon on light, a sun on dark. Pressing switches to dark, then back to light, on the same page; nothing in the footer; no horizontal scroll at 320 px | Web tests, prototype checker, and Edge on the running site and the prototype ([evidence](../../docs/delivery/evidence/2026-11-23-f02-theme-toggle.md)) |

## Name availability, 2026-11-16

This was a preliminary web survey for D01 and the open rename task ([record](../../docs/delivery/evidence/2026-11-16-name-availability.md)), not clearance. It found active "TaxBuddy" tax products in the Philippines (taxbuddyph.com) and India (taxbuddy.com), and a live US TAXBUDDIES mark for tax services. IPOPHL, WIPO and domain registrars were not searched directly. Nothing was decided; D01 stays with the owner.

## Evidence limits

Prototype verification cannot establish server authorization, real provider sign-in, delivery, persistence, production accessibility or release acceptance. D15's product accessibility targets remain open; tested widths and contrast are implementation observations. Results belong in [STATUS](../../docs/delivery/STATUS.md).
