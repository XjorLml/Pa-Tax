# Tax Buddy design directions and earlier AccApp studies

**Current direction: Tax Buddy (visual direction 2026-11-11, name 2026-11-12).** The user asked to execute the branding/design brief and delegated the name, logo and visual choice. The agent selected the name Kuwenta and applied a paper/forest direction to the existing prototype. On 2026-11-12 the user instructed renaming the app to Tax Buddy; the visual direction and logo symbol are unchanged and the wordmark was redrawn. [DESIGN](../docs/DESIGN.md#visual-system) owns current tokens and implementation guidance; [F02](../specs/brand-design-refresh/spec.md) records scope and acceptance scenarios. Product scope stays in the [PRD](../docs/PRD.md); owner walkthrough and release acceptance remain pending.

## Current design review

The earlier prototype already covered the important flows, but retained a placeholder identity, four competing palettes, an oversized centered introduction, and controls whose hierarchy competed with profile comparison. The refresh adds:

- A name. The 2026-11-11 refresh chose Kuwenta as broad enough for bookkeeping, payroll, portfolio work and messaging, rather than a tax-only label. The user replaced it with Tax Buddy on 2026-11-12. Recorded tension: the new name is tax-specific while the product scope in the PRD is unchanged and still covers the other services. No external name clearance is claimed for either name.
- An original ledger/conversation mark and geometric wordmark, with light/dark SVG variants and favicon ([assets](assets/README.md)).
- Serif headlines, readable system sans controls, forest actions and warm paper surfaces. The chosen values are centralized in `taxbuddy.css` and map to the approved Tailwind/Blazor stack.
- An asymmetric desktop introduction with a small ledger illustration, labelled search and quiet service filters; results lead into comparable cards rather than promotional statistics.
- Clearer card structure, contact-panel emphasis, responsive container breakpoints, compact phone layouts and information links on additional mobile screens.

The four previous looks remain in the Look control as historical comparisons. Their historical contrast figures below describe those palettes, not the new Tax Buddy palette. Current verification is linked from [STATUS](../docs/delivery/STATUS.md).

The later client-service expansion uses the same direction, with private list summaries, grouped service cards, a two-column desktop checklist/history view, and stacked phone checklists/files/history ([DESIGN service presentation](../docs/DESIGN.md#service-workspace-presentation), [F03](../specs/client-services/spec.md)). Its generated sample uploads do not introduce public profile images or chat attachments.

## Palette options

Added 2026-11-12 at the user's request, as proposals. Each keeps the Tax Buddy type, spacing and layout and changes colour tokens only. Choose them with the **Palette** control or the `palette` URL setting; values are in `taxbuddy.css` and measured contrast is in the [dated evidence](../docs/delivery/evidence/2026-11-12-palette-options.md). Paper & Forest stays current until the owner decides.

| Palette | `palette` value | Character | Watch out for |
|---|---|---|---|
| Paper & Forest (current) | `forest` | Calm, grounded | Nothing to change |
| Ink & Ledger Blue | `ink` | Official, careful | The least distinctive; many banks and tax products are blue |
| Lagoon Teal | `lagoon` | Fresh, approachable | The smallest change from the current green; teal is also common in finance apps |
| Plum & Blush | `plum` | Warm, personal | Purple divides people; the error red sits close to plum |
| Marigold & Charcoal | `marigold` | Friendly, sunny | The brand colour sits in backgrounds, not buttons; the amber warning colour would need a new hue |

## Earlier studies preserved below

The remaining sections record earlier work from 2026-11-11 under the AccApp working name. Recommendations and claims that a choice remains open describe that earlier state; the current visual contract above supersedes them. Earlier reference-site observations are retained with their original attribution and have not been re-executed in this refresh.

To compare the directions, open [index.html](index.html) and use the **Look** control in the top bar.

## Review of the earlier mock

What worked: every page and failure state existed, the flows were complete, and the text was plain.

What hurt, most important first. Each was changed in the prototype.

| # | Problem | Change |
|---|---|---|
| 1 | No first impression. The site opened on a search box, so a new visitor never learned what it is, that browsing is free, or that messaging needs sign-in | A short introduction for visitors with "How it works" in three steps, and a way in for accountants ("List your practice") |
| 2 | Accountant cards were hard to compare. A paragraph of prose came first; city and experience were buried | Cards lead with name, title, city and years of experience, then services, then two lines of summary |
| 3 | On phones the service filter was a sideways-scrolling row that hid most options | A labelled menu on phones; buttons stay on wide screens. The result line says what is filtered and offers "Clear" |
| 4 | The profile gave no reassurance at the moment of contact, and nothing said who vouches for the content | Quick facts under the name; services before experience; a line under the Message button about cost and sign-in; a plain note that AccApp does not verify accountants yet |
| 5 | A visitor who typed a message and was sent to sign-in could not see that the message survived | The send button says "Sign in to send", and the sign-in page shows the saved message |
| 6 | A blank chat is hard to start when accounting makes you anxious | Three message ideas that fill the box with one tap |
| 7 | New accountants faced a long form with one sentence about what was required | A checklist with a progress bar that updates as they type, with links to each missing detail |
| 8 | The desktop layout looked like an installed app (a side rail) | A website header: name, navigation, "List your practice" and "Sign in"; centred single-column pages |

Also fixed across all looks:
- Input and button outlines now meet the 3:1 contrast needed to see a control's edge. The earlier outlines were about 1.4:1.
- Touch targets on the phone frame are at least 44 px.
- Motion respects the browser's reduced-motion setting.
- Sample data that contradicted itself was corrected.

## Reference websites

The owner asked on 2026-11-11 for a professional, modern design based on real websites. These pages were loaded and viewed that day as screenshots taken with a headless browser. Only patterns are borrowed. No images, wording, logos or brand colors are copied.

| Site | What it does well | What AccApp borrows | What AccApp leaves out |
|---|---|---|---|
| [Xero advisor directory](https://www.xero.com/us/advisors/find-advisors/) | An accountant directory. A tinted panel holds a bold headline, one search field and the filters | The hero with search and filter built in; deep navy text | Location and industry filters (PRD D05) |
| [Thumbtack](https://www.thumbtack.com/) | One search box in the hero, a reassurance line with a lock icon under it, and "Join as a pro" in the header | The reassurance line; the header with "List your practice" and a pill "Sign in" | The full-screen photo, and rating and volume numbers AccApp has no data for |
| [Psychology Today listings](https://www.psychologytoday.com/us/therapists/ny/new-york) | Profile cards that read in a fixed order: name, credential, location, a short summary, then clear actions. Filter pills and a breadcrumb | The card order, a "View profile" action on every card, filter pills, the breadcrumb on profiles | Photos (no uploads in release 1), verified ticks and endorsements |
| [Contra](https://contra.com/) | A centred headline, soft pill buttons and suggestion chips | Pill-shaped chips and buttons | Verified badges |
| [Wise](https://wise.com/) | Large confident type, generous corner radii, real product screens in place of stock photos | The type scale and rounded panels | The loud brand color and all-capitals headline |
| [Mercury](https://mercury.com/) | A calm finance site with one accent color and a single pill call to action | One accent color; restraint | The full-screen artwork |
| [Linear](https://linear.app/) | Precise spacing, tight headline letter-spacing, quiet borders in a dark theme | The dark theme treatment | Its density, which suits tools more than a public directory |
| [Taxumo](https://www.taxumo.com/) | A Philippine tax service that leads with accreditation logos | Nothing visual. It shows that local users expect proof of standing, which bears on PRD D09 | Accreditation marks, which AccApp does not have |

Four sites could not be loaded by the automated browser and are worth viewing by hand: QuickBooks' Find-a-ProAdvisor directory, Zocdoc, Malt and Clutch.

What the references have in common, and what "modern" means here:
- One clear headline and one search box above everything else.
- White space and a single accent color, not decoration.
- Cards with the same facts in the same order, so people can compare.
- A profile page with the contact action always in view.
- A plain website frame: header, centred content, footer.

## Four directions

Each look has a light and a dark palette. All use fonts already on the device, because the prototype must work offline. A font to consider for the real site is named for each.

### Modern

| | |
|---|---|
| Mood | Modern, crisp, confident |
| Suits | The brief of 2026-11-11: a professional, current-looking site for both business owners and accountants |
| Colour | Royal blue `#2F4FE0` on near-white `#F6F8FC`, with deep navy `#0F1B3D` text; in dark mode, periwinkle `#8EA2FF` on `#0B1020` |
| Type | Tight, heavy sans headlines and plain sans body. For the real site: Inter |
| Shape | Pill buttons and chips, 16 px panels, soft layered shadows |
| Icons and avatars | Thin line icons; round initials in three quiet tints |
| Motion | Brisk and small: cards lift slightly on hover |
| Voice | "Find an accountant who fits your business." / "Compare experience and services, then message them directly." |

### Ledger

| | |
|---|---|
| Mood | Calm, established, precise |
| Suits | Clients who want reassurance that this is a serious place to find a professional, and accountants who want their profile to read like a credential |
| Colour | Ink navy `#1F3A5F` on warm paper `#F6F2EA`; in dark mode, soft blue `#A9C3E6` on near-black `#11151C` |
| Type | Serif headings (Palatino or the device's serif), plain sans body. For the real site: Source Serif 4 with Source Sans 3 |
| Shape | Nearly square corners, hairline borders, no shadows |
| Icons and avatars | Thin line icons; square initials tiles |
| Motion | Almost none: quick colour changes only |
| Voice | "Find an accountant for your business." / "Read each accountant's experience and services, then message them directly." |

### Neighbourly

| | |
|---|---|
| Mood | Warm, friendly, encouraging |
| Suits | Freelancers, online sellers and first-time business owners who find accounting intimidating |
| Colour | Sunflower `#F4B63F` buttons with dark brown text on cream `#FFF7EE`; burnt sienna `#8A3F0C` links; in dark mode the same sunflower on deep brown `#1A1410` |
| Type | Rounded, heavier headings, plain sans body. For the real site: Nunito with Nunito Sans |
| Shape | Large rounded corners, pill buttons, soft shadows |
| Icons and avatars | Thin line icons; round initials in three soft tints (peach, mint, lavender) |
| Motion | Gentle: cards lift slightly on hover |
| Voice | "Accounting help, in plain words." / "Browse accountants, pick one you like, and say hello. You don't need an account to look around." |

### Clear

| | |
|---|---|
| Mood | Fresh, focused, efficient |
| Suits | People who want to get the task done; the most neutral of the three |
| Colour | Teal `#0B6E60` on cool white `#F3F7F6`; in dark mode, mint `#52C4B1` on `#0E1513` |
| Type | Clean sans throughout. For the real site: Inter |
| Shape | Medium rounded corners, light shadows |
| Icons and avatars | Thin line icons; round initials in one tint |
| Motion | Brisk, small |
| Voice | "Search. Compare. Message." / "Accountants by service, with their experience up front." |

The fonts named for the real site are published under the SIL Open Font License; confirm each licence when adopting it.

### Recommendation

**Modern**, because the owner asked for a professional, modern design, and it is the look built from the reference websites. It is now the prototype's default.

Before that request the recommendation was **Ledger**, and it is still the better choice if looking established matters more than looking current. Choose **Neighbourly** if the main audience is small sellers and freelancers. **Clear** is close to Modern in spirit, with teal in place of blue.

This is the owner's decision. Looks can also be mixed, for example Modern's layout and color with Ledger's serif headings.

## Contrast

Measured with the WCAG 2 formula from the exact colour values in the prototype. All 26 text and control pairs pass AA in all eight palettes. The lowest ratios are the control outlines, between 3.4 and 3.5 against a 3.0 minimum.

| Look, theme | Body text | Secondary text | Links | Primary button label | Selected filter label | Error text | Control outline | Focus ring |
|---|---|---|---|---|---|---|---|---|
| Modern, light | 16.9 | 6.8 | 8.2 | 6.3 | 9.8 | 6.6 | 3.7 | 6.7 |
| Modern, dark | 14.7 | 7.8 | 8.7 | 7.5 | 9.8 | 7.7 | 4.2 | 8.4 |
| Ledger, light | 15.7 | 6.2 | 11.3 | 11.5 | 10.9 | 7.2 | 4.0 | 6.6 |
| Ledger, dark | 13.5 | 7.4 | 9.1 | 9.4 | 9.0 | 7.6 | 4.4 | 7.9 |
| Neighbourly, light | 16.5 | 7.1 | 7.5 | 9.1 | 7.5 | 6.5 | 3.7 | 6.7 |
| Neighbourly, dark | 14.1 | 7.9 | 9.1 | 9.1 | 8.8 | 7.9 | 4.2 | 8.8 |
| Clear, light | 17.0 | 6.6 | 7.6 | 6.2 | 8.1 | 6.6 | 3.7 | 5.2 |
| Clear, dark | 14.0 | 7.4 | 9.2 | 8.0 | 9.4 | 7.5 | 4.1 | 7.1 |

Text columns are measured on card backgrounds and need 4.5. Outline and focus columns need 3.0. Neighbourly's sunflower buttons have a darker edge (`#9A6400`, 4.9:1) so the button shape is visible on white.

## Changes and the requirements they serve

| Change | Requirement |
|---|---|
| Introduction and "How it works" for visitors | R01, R04, R06 |
| Cards lead with city, years of experience and services | R01, R02 |
| Phone service menu; result line with "Clear" | R02 |
| "Chatting" tag on cards for accountants the client already messages | R06 |
| Profile: quick facts, services first, reassurance under the Message button | R03, R06 |
| Profile: note that AccApp does not verify accountants | R11, BR01 |
| "Sign in to send" and the saved message shown on the sign-in page | R04, R06 |
| Message ideas in an empty chat | R06 |
| Listing checklist with live progress; "View public profile" link | R05, BR01 |
| Website header on wide screens; "Sign in" in the phone's bottom bar for visitors | R04 |
| Search and the service filter built into the hero, with a line saying browsing is free | R01, R02 |
| Wide screens: a two-column grid of cards, and a profile page whose contact card stays in view | R01, R03, R06 |
| A "View profile" action on every card, and a breadcrumb on profiles | R01, R03 |
| A site footer that repeats that accountants are not verified | R11 |
| Portfolio highlights on profiles, with a count on directory cards | R12 |
| Star ratings with the average and count on cards and profiles; a "Highest rated" sort | R13 |
| A review form that says what becomes public, and one public reply from the accountant | R13, R14 |
| Report on profiles, reviews and chats; Block in chats | R15 |
| Agreement to the terms at first sign-in; privacy, terms, about, help and for-accountants pages | R16, R17 |
| Page-not-found and error pages | R18 |
| Notification emails for clients too, with a "turn off" link that leads to its own page | R08 |
| Friendlier error wording that says whose fault it is | R10 |
| Visible control outlines, 44 px targets, reduced-motion support | DESIGN accessibility; TP06 |

Open decisions stay marked as proposals in the notes panel: D03, D04, D05, D06, D08 and D18. Two new details lean on them:
- **Years of experience** on cards is counted from the earliest start month an accountant enters (D04).
- **The verification note's wording** depends on the trust-and-safety decision (D09).

## Ideas that need a decision or new scope

Not drawn in the prototype, because release 1 excludes them or a decision is missing.

| Idea | What it needs |
|---|---|
| A "verified" mark on profiles | A real verification process (PRD D09) |
| "Usually replies within a day" | Response-time data and a rule for showing it |
| Price ranges on services | Release 1 excludes prices |
| Profile photos | File upload, storage and moderation; initials are used for now |
| A city filter or "near me" | PRD D05 |
| "Taking new clients" status | A new profile field (D04) |
| Saved or favourite accountants | New scope |

## Checks run

On 2026-11-11, agent-observed:
- Contrast computed for 26 colour pairs in each of the six palettes; all pass.
- `node --check` on the prototype's script: no syntax errors.
- A scripted walk-through in headless Microsoft Edge 157 ran once per look. 83 checks passed in each, with no page errors, covering every flow including the new ones and switching looks.
- Screenshots reviewed: all three looks on phone and desktop frames, light and dark, plus a 500 px wide window for the phone frame and a 700 px wide window for the desktop frame.
- The file has no external addresses, so it works offline.

Second pass the same day, after the Modern look and the website layout were added:
- Contrast recomputed for all eight palettes; all pass.
- The walk-through, now 86 checks, passed for all four looks with no page errors.
- Seven screenshots of the Modern look were reviewed on phone and desktop frames, in light and dark themes. One alignment flaw on the desktop sign-in page was fixed.
- Reference websites were viewed as screenshots only. Their code was not inspected.

Third pass the same day, after the portfolio, ratings, report and information pages were added ([ADR 008](../docs/adr/008-portfolio-ratings-no-approval.md)):
- The walk-through, now 132 checks, passed for all four looks with no page errors.
- Nine screenshots of the new pages were reviewed. One punctuation flaw on the review form was fixed.

Not checked: other browsers, real phones, screen readers, keyboard-only use beyond focus placement, and true browser zoom. The narrow-window screenshots stand in for zoom and are a weaker check.
