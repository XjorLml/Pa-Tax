# Tax Buddy UI and UX standards

**Authority:** visual and interaction standards. [PRD](PRD.md) owns feature scope. [SDD](SDD.md) owns technical implementation. Status: visual direction applied to the prototype under the user's delegated design brief, 2026-11-11 (then named Kuwenta); renamed Tax Buddy by the user's instruction, 2026-11-12. Palette chosen by the user, 2026-11-13: Paper & Forest. Applied to the application's directory and profile pages in F01, 2026-11-13. Product and release acceptance remain pending.

## Scope notice

Applies to the release 1 pages in the [SDD navigation table](SDD.md#navigation-and-screen-state):
- directory
- profile
- sign-in
- conversation list
- chat thread
- my profile editor
- account settings
- private client/added-accountant lists, service requests, checklists, progress and service documents ([F03](../specs/client-services/spec.md))

It is one responsive website, used in phone and desktop browsers.

The clickable [design prototype](../prototypes/README.md) uses Tax Buddy as its default direction. The user authorized executing a brief that delegates choosing the name, creating the logo and improving the prototype, and on 2026-11-12 instructed renaming the result from Kuwenta to Tax Buddy; [F02](../specs/brand-design-refresh/spec.md) records the scope and observable outcomes. The four earlier directions remain available for comparison and are preserved in [DESIGN-DIRECTIONS](../prototypes/DESIGN-DIRECTIONS.md). Behavior depending on D03–D06 and D18 remains a proposal. A prototype is not evidence of production accessibility or authorization to expand the feature scope.

## Visual system

| Role | Value / semantic resource | Rationale and verification |
|---|---|---|
| Brand identity | Tax Buddy: a round speech bubble with one square corner, holding a solid T, beside a drawn "Tax Buddy" wordmark in two tones ("Tax" in the text colour, "Buddy" in the primary colour). Shown at 161 × 30 CSS px in the site bar and footer. [Assets and usage](../prototypes/assets/README.md). Domain remains PRD D01 | Chosen by the user on 2026-11-18 from three proposals and applied to the prototype and the site the same day ([F02 BD12](../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)). It replaces the ledger with a conversation corner and the lowercase wordmark of 2026-11-11 and 2026-11-12. Original vector paths, no downloaded font or artwork. Since 2026-11-19 the wordmark is filled outlines, not strokes, and the directory's decorative ledger illustration carries the same bubble in place of its old rectangular one. A preliminary web survey of similar marks and of domains was made on 2026-11-19 ([record](delivery/evidence/2026-11-19-f02-logo-outlines-hero.md)); it is not clearance. No name/domain clearance or release approval claimed |
| Color | Warm paper and forest green; exact semantic values below, implemented in [taxbuddy.css](../prototypes/taxbuddy.css) | Error and unread states retain text/icons; observed contrast belongs in the dated evidence |
| Typography | One typeface: Quicksand, a rounded sans chosen as the closest match to the logo's drawn letters, for headings, body and controls. Fallbacks: Segoe UI, then the system sans. Weights: body 500, labels and buttons 600, headings 700. Body 15 px; supporting text 12–14 px; section headings 24–30 px; hero 31–52 px | Set by the user on 2026-11-19 ("stick to one font style used in the logo"; [F02 BD13](../specs/brand-design-refresh/spec.md#one-typeface-2026-11-19-bd13)). It replaces Georgia serif headings with a Segoe UI body, and ends the earlier rule of system fonts with no downloads: the site and the prototype each serve two local font files (56 KB together), with no request to another site. Quicksand is under the SIL Open Font License 1.1; the licence file sits beside the fonts. The logo is still drawn paths and does not use the font. Browser zoom and screen-reader acceptance remain TP06 |
| Layout | Spacing 4/8/12/16/24/32/48 px; 1040 px content including 32 px side padding; two desktop card columns. Container breakpoints 760/480 px, compact phone chrome at 420 px. Buttons normally 44 px; phone controls at least 44 px | Narrow widths are observations, not closure of D15. Fixed device frames are prototype controls, not a production layout requirement |
| Motion | Pressed, loading and sending feedback only | Respect the browser's reduced-motion setting |
| Theme | Follows the device (System) until a person chooses. Since 2026-11-23 ([F02 BD14](../specs/brand-design-refresh/spec.md#theme-toggle-in-the-header-2026-11-23-bd14)) there is one icon button, always in the header: after Sign in or the account link in the site bar, and at the end of the phone app bar. It shows a moon ("Switch to dark theme") in a light theme and a sun in a dark one, and pressing it stores the choice | The user's instruction of 2026-11-23, in the prototype and the site; it replaces the footer switch of BD09/BD10 and its recorded deviation. Before that, from 2026-11-16, Light, Dark and System were toggle buttons in the footer ([F02 BD09](../specs/brand-design-refresh/spec.md#theme-switch-in-the-application-2026-11-16)) |

Centralize the variables in one stylesheet. Decided on 2026-11-11: Tailwind CSS for styling, with these variables as its theme and no component library ([SDD](SDD.md#technology-stack)). Document exceptions instead of allowing each page to invent colors, spacing, and status symbols.

### Tax Buddy palette

| Semantic token | Light | Dark |
|---|---|---|
| `app-bg` | `#F7F7F2` | `#111C18` |
| `surface` | `#FFFFFF` | `#192720` |
| `surface-2` | `#ECEFE8` | `#24352B` |
| `text` | `#1E332C` | `#EEF1E7` |
| `muted` | `#556259` | `#B3C0B3` |
| `primary`, `link`, `focus` | `#245C46` | `#B7D8A4` |
| `on-primary` | `#FFFFFF` | `#162D20` |
| `primary-soft` / `on-soft` | `#E5EEE4` / `#254C36` | `#2B402C` / `#D8EACD` |
| `border` (decorative) / `ctl` (input edge) | `#D7DED4` / `#7C8B80` | `#3A4D3F` / `#7E9383` |
| `danger` / `danger-soft` | `#A32925` / `#FCEBE9` | `#FFB4A9` / `#40251F` |
| `warn` / `warn-soft` | `#745010` / `#FAEED4` | `#ECCC8D` / `#3D321C` |
| `hero` / `hero-art` | `#E9EDE1` / `#D9E1CD` | `#223328` / `#304631` |

Palette proposals, 2026-11-12: the prototype's Palette control also offers Ink & Ledger Blue, Lagoon Teal, Plum & Blush and Marigold & Charcoal for comparison ([F02 BD08](../specs/brand-design-refresh/spec.md#palette-options-2026-11-12), [DESIGN-DIRECTIONS](../prototypes/DESIGN-DIRECTIONS.md#palette-options)). On 2026-11-13 the user chose Paper & Forest ("for pallete go for paper and forest"), so the table above is the palette. The four proposals stay in the prototype stylesheet as history and are not used by the application.

### Stack and component mapping

The prototype uses plain CSS as a buildless design artifact. At scaffold, map the semantic roles above to Tailwind theme values and reusable Razor components: SiteHeader, BrandMark, DirectorySearch, AccountantCard, ProfileSummary, ContactPanel, MessageComposer, StatusBanner and SiteFooter. Component names are planning labels, not existing source. Keep public directory/profile HTML suitable for static rendering and the chat/editor boundary as defined in SDD; adding JavaScript interactivity here does not change those application contracts. Reuse SVG assets from `wwwroot` without an image service or component library.

As built in F01, 2026-11-13 ([evidence](delivery/evidence/2026-11-13-f01-browse-accountants.md)):
- **Tokens:** the palette table above is the only colour source, in `src/TaxBuddy.Web/Styles/app.css`. Tailwind 4's own colour palette is switched off, so a class such as `bg-surface` or `text-muted` must name a semantic role. Dark mode follows the browser's `prefers-color-scheme` setting unless the visitor chose a theme. Since 2026-11-16 (F02 BD09) the server writes `data-theme="light"` or `"dark"` on `<html>` for a chosen theme. The stylesheet holds the dark values twice, once for the browser setting and once for a chosen dark, and a test keeps the two identical. Tailwind's `dark:` variant follows the same rule.
- **Breakpoints:** 480 px (`sm`) and 760 px (`md`), as in the layout row above, plus 1040 px for the content width.
- **Razor components:** `BrandLogo` and `BrandIcon` (both files written; the stylesheet shows the one for the current theme, without script), `ThemeToggle` (the header's theme icon in `SiteUser` and `PhoneBar`, F02 BD14; it replaced the footer's `ThemeSwitch` on 2026-11-23), `SiteUser`, `PhoneBar`, `PhoneTabs` and `DeskTitle` (the prototype's shell pieces, placed by pages through Blazor sections, BD10), `Icon` (the prototype's line icons, hidden from screen readers), `SeoHead` (title, description, canonical address), `StateMessage` (empty, no match, unavailable and error states), `AccountantCardItem` (one link per card, covering the whole card) and `Pager`. The layout supplies the header, skip link and footer.
- **The directory form's source order matters:** text box, service menu, Search, then service buttons. Comments in `Styles/app.css` and the SDD explain why.
- **Grid columns on the hero use `minmax(0, 1fr)`,** so the search form fits at 320 px.

**Aligned to the prototype in F02 BD10 (2026-11-16):**
- **Directory:** the `heroHTML` hero (heading, arched illustration with "People behind the numbers"), 6 px service buttons, and the trust line "You only sign in when you send a message".
- **Cards:** `cardHTML` cards with tone-coloured avatars, 4 px tags and "Read their story".
- **Profile:** the `profileBodyHTML` layout. A breadcrumb (desktop), summary chips (years, services), sections, a services list and timeline. The contact card has an accent border and Message first. A credentials note card. On phones, a fixed Message bar.
- **Deviations:** none left for the profile's parts: "Report this profile" came with R15 (F10, 2026-11-18), and the viewer-specific Message action with BD11. Highlights came with R12 (F08, 2026-11-17); the rating line, reviews and the "Highest rated" sort with R13 and R14 (F09, 2026-11-17), when the credentials note got "read the reviews" back. Email, phone and website stay links.

**Checked against the prototype again in BD11 (2026-11-18, the user's instruction; [evidence](delivery/evidence/2026-11-18-prototype-alignment.md)).** Three read-only audits compared every built page with its render function. These unrecorded differences were then changed to match the prototype:
- **Shell:**
  - The site bar runs the full width with 32 px sides.
  - "Find an accountant" is current on profiles too.
  - The footer logo is full size.
  - The phone directory app bar shows the symbol with "Tax Buddy" in the typeface at weight 700.
- **States:** state messages are the prototype's `.state` block (a circled icon and an 18 px heading, no card).
  - Page not found: "We couldn't find that page" in a "Page not found" panel.
  - Error: "Something went wrong on our side" with the support code in bold and "Back to the directory".
  - Missing profile: "The accountant may have removed their profile, or the link is wrong." with "Back to directory".
- **Phones:**
  - Info, error, not-found and review pages lose their panel, and the title is only in the app bar.
  - On the profile there is no card, and Contact is a plain section.
  - The Message bar carries its hint.
  - The editor's Save sits in a bar at the bottom.
- **Directory:**
  - Signed-in users get the plain "Find an accountant" hero.
  - Visitors get "How it works": a toggle on phones and steps after the results on wide screens.
  - The eyebrow, heading and meta sizes follow `taxbuddy.css`.
  - On phones the search fields sit on the hero.
  - Card summaries are muted and cards have the soft shadow.
- **Profile:**
  - The owner sees "This is your public profile. Edit profile", and other accountants "You're signed in as an accountant. Only clients can start chats."
  - A client with a chat gets "Open chat"; other clients get "It's free to send a message. Ask about pricing in the chat."
  - The name is 34 px on phones too.
- **Other pages:**
  - The review page's states sit in the "Write a review" panel.
  - The report page says "Sign in to send a report" and its "Reason" legend is for screen readers.
  - The legal pages' section headings are 24 px, and the draft note has no icon.
  - Account says "My profile and portfolio".
  - Sign-in names the accountant ("Sign in to send your message to Nina.") and says "Go to directory" when already signed in.
  - The register agreement box has the prototype's spacing.
- **Chat:**
  - Visitors to `/chats` on wide screens see the two panes.
  - The block dialog is a centred box at every width.
  - "Blocked. They can't message you." and "Unblocked." show as a line.
  - The thread title is 18 px, and message ideas use the 12 px chip style.

**Kept and recorded as deviations in BD11:**
- **The footer** shows on every page, including the desktop single pages (R16: legal links reachable from every page).
- **Sign-in:**
  - There is no "Redirecting to Google…" step: the form posts straight to Google.
  - The draft isn't quoted, because it lives in the tab, not on the server.
- **Legal pages:** Privacy has "Cookies" and Terms "Your account" sections, which describe what F04 does. Superseded on 2026-11-21 by F11: both pages now carry the full approved text in the long-document layout (row "Information pages" below).
- **The profile editor's failure line** reads "we couldn't reach the server", because it is a static form.
- **Chat:**
  - A visitor's `/chats/{id}` shows a single sign-in state.
  - Blocking has failure lines that the prototype lacks.
- **Reports:**
  - The report page names the item ("Nina Ocampo's profile") and has "You can't report your own …" and "You've already reported this" states.
  - "Choose a reason." sits under the chips.
- **A blocked client's profile** says "You can't review {first}." (BR19).
- **The search box's × clear button** needs script; the "Clear" link in the meta row replaces it.
- **"How it works"** says "Sign in with Google first." (it names the providers the site offers; Microsoft is archived, so it stays "Google" where the prototype says "Google or Microsoft"). It ends with "Are you an accountant? See how listing works", in the phone toggle and the wide-screen section, as in the prototype; that line was left out until the For accountants page (R17) existed, and joined on 2026-11-24.
- **State headings** are the page's h1 where the state is the whole page; they look the same as the prototype's h2.

Directory pages lead with a short introduction and labelled search, followed by comparable cards. The desktop introduction uses an original decorative ledger illustration rather than fabricated statistics or portraits. Cards show initials, career title, rating/count, location/experience, services, summary and profile action in a consistent order. Profiles keep a clearly separated contact panel; mobile keeps its contact action below the scrolling content. Serif display type is limited to brand/headings; controls, forms, messages and dense facts stay in the body face.

## Screen and component states

This table says which states each screen has. How a loading, busy or failed state looks and when it shows is in [Loading and busy feedback](#loading-and-busy-feedback) (Proposed, 2026-11-25).

| Component / workflow | States to design | Observable behavior |
|---|---|---|
| Directory list and search (R01, R02) | Ready, empty directory, no match, load error | A static page with a plain search form ([SDD](SDD.md#navigation-and-screen-state), C03 resolved 2026-11-12): search and the service menu apply when the form is submitted, and on wide screens each service button submits it at once. Sort and page are links. Search, service, sort and page live in the address, so Back and shared links keep them. No in-page loading state, and the prototype's brief placeholder stands in for the page load. A full page load (first visit, reload, Search) shows the browser's own indicator. Links inside the site use Blazor's enhanced navigation and currently show no indicator; see [F01 polish P1](../specs/browse-accountants/tasks.md#polish-follow-ups). Cards look the same to every viewer. Empty and "no match" use different messages. A failed load is an error with a retry, never an empty list |
| Profile (R03) | Ready, not available (removed or malformed link), error | Contact details and services are readable without sign-in. A "Message" action is shown to everyone; visitors are taken to sign-in first and returned afterwards |
| Sign-in (R04) | Choose provider, redirecting, cancelled, failed, choose role | Cancelling at the provider returns to the previous page. Built in F04 and aligned to the prototype's `signinHTML` in BD10 (2026-11-16): a centred card with the symbol, "Welcome to Tax Buddy", a lead that depends on where the visitor is going, "Continue with Google", a red banner for a failure and a notice when Google isn't set up. The role step has the agreement box, then two large choice buttons. Microsoft was built in F15 (2026-11-23) and archived the same day by the owner ("Google sign-in is enough"): the page shows only "Continue with Google", and the failure line, the footnote, the page description and the help name only Google. **Deviations:** the prototype's "Continue with Microsoft" button and "Google or Microsoft" wording are left out, by the owner's decision. Since 2026-11-24, at the owner's request, "Continue with Google" starts with Google's standard four-colour "G" (`GoogleMark`, 18 px, 10 px from the text, `.btn-provider`); the prototype's button has no mark. Google's sign-in branding rules ask for the mark unchanged, so it keeps Google's own colours in both themes instead of the site's tokens, and it is hidden from screen readers because the text names Google. The choices aren't disabled until the box is ticked (that needs script); the server asks for the tick instead, and the page says "Tick the box, then choose" |
| Profile editor (R05) | Pristine, draft, validation error, saving, failed, saved, changed elsewhere | Edits are never cleared by a failure. Leaving with unsaved changes asks for confirmation. Built in F05 as a static form and aligned to the prototype's `editorParts` in BD10 (2026-11-16): titled "Create your profile" until listed; a status pill with "View public profile"; a listing checklist with a meter and "Add: …" links; "Fix these before saving" with links to fields; fieldsets About you, Experience (rows), Services (rows with a dropdown), Contact details; markers "(needed to be listed)"; a save status line; Save. **Deviations:** the portfolio fieldset came with R12 (F08); no leave confirmation, "Saving…" spinner or live disabling of the end month (they need script); the stale-save warning has no prototype counterpart and uses the error-summary style; on phones Save sits in a bar at the bottom of the form (BD11); the failure line says "we couldn't reach the server" |
| Account (R04, R05) | Visitor, client, accountant | Built in BD10 from the prototype's `meHTML`: an avatar card with the name and "Accountant · Signed in with Google" (or "Client · …") and the picture pencil (R24, below), then one menu, in the prototype's order: the workspace row, "Clients and service work" for an accountant or "My accountants and service progress" for a client, with "Private checklists, shared updates and service documents", linking to `/workspace` (F03, added 2026-11-24); for an accountant, My profile and portfolio (with Listed or Not listed yet); Email notifications (F14); Sign out. A separate menu holds the red Delete account (F12). For visitors, "You're browsing as a visitor" with Sign in. **Deviations:** the accountant's "My reviews" row is left out by the user's choice in F09 (2026-11-17), confirmed on 2026-11-24. When the listing can't be read, the profile row says "Status unavailable right now", which the prototype has no state for. Until 2026-11-24 the rows for unbuilt features were left out: service work until then, email settings until F14 and delete account until F12. **Client details (F17, R25; built 2026-11-25 from the prototype's design B, [F02 BD15](../specs/brand-design-refresh/spec.md#client-details-on-account-2026-11-25-bd15)):** a client's "Your details" card under the avatar card shows phone, city and business ("Not added" when empty) with Edit details. `/account?edit=details#details` turns the card into the form: About you (name, phone, city, and the email not editable), Your business (name, type, industry), a sharing line naming the accountants with a service, a note not to add a TIN, and Save details and Cancel. A refused field gets its message below it and a red border, with everything typed kept. After a save the card says "Saved. The accountants you contact see these details." **Deviation:** the prototype moves focus to the card's heading after Edit, Save and Cancel, which needs script; the static site uses the `#details` fragment instead |
| Avatars and profile pictures (R24) | Picture, initials (default); picture panel closed, open, result, upload not set up | Added 2026-11-16 ([F06](../specs/profile-pictures/spec.md)): one `Avatar` component everywhere. It shows the picture, cropped to fill the prototype's avatar shape and size, or by default the initials on one of the prototype's three tones (`t1` to `t3`, chosen from the name). Always decorative (`alt=""`), since the name is shown beside it. **Changing it, since 2026-11-19 ([PP10](../specs/profile-pictures/spec.md#pick-preview-save-2026-11-19-pp10), prototype `meCardHTML`, design E):** the Account avatar and the editor's About you avatar carry a round pencil badge, 28 px on a 72 px avatar, labelled "Change profile picture". On Account the pencil opens the device's file chooser. The chosen photo shows in the avatar as a preview, with "Preview. Not saved yet." and "Save" and "Cancel" beside it under the name; nothing is stored until Save. When there is a picture, a "Remove picture" link sits under the name. Results show as a banner under the name card: green for done, red for a problem. A wrong file type or a file over 5 MB is refused before the preview, with the same words the server uses. The editor's pencil goes to Account. **Without script ([PP09](../specs/profile-pictures/spec.md#pencil-on-the-picture-2026-11-19-pp09), design A):** the pencil is a plain link to `#picture`, which shows a "Profile picture" panel under the name card: a file picker with a 44 px button, "Upload picture", a hint (types, 5 MB, cropping, location removed, who sees it), "Remove picture" when there is one, and "Close". **Deviations:** the prototype keeps the picture only in the page. The site's script is a small addition on top of the static page ([SDD](SDD.md#render-modes)); when upload isn't set up the site shows no pencil and an open panel saying so. **Not chosen:** one-tap upload, a small menu and a separate page stay in the prototype's "Picture design" control for comparison ([options](../specs/profile-pictures/spec.md#picture-design-options-2026-11-19)). Earlier designs: 2026-11-16 to 2026-11-18, an always-open "Profile picture" card on Account and a "Change picture" text link in the editor, when the prototype had no picture upload; 2026-11-19, the panel alone (PP09) |
| Site shell | Desktop, phone | Aligned to the prototype's `desktopShell` and `phoneShell` in BD10, with links added as their features were built. **Site bar:** the logo, "Find an accountant", "Chats" with the unread count (F07), and for a signed-in user "Clients" or "My accountants" (F03, 2026-11-23). On the right: the avatar and name linking to Account, or for visitors a ghost "For accountants" (F13, hidden below 900 px) and a primary "Sign in"; then the theme switch as an icon (BD14, 2026-11-23). **Current item** (since 2026-11-24, the prototype's map, `NavSection`): "Find an accountant" on the directory and profiles; Chats on every chat page; Clients or My accountants on the workspace, client, service and Request a service pages; the account link on Account, the profile editor and Delete account. Nothing is current on the review, agree, report, sign-in and information pages. Until then Request a service and the review page marked "Find an accountant", the client and service pages marked nothing, and the agree page marked the account link. **Phones:** an app bar (back arrow and title, or on the directory the symbol and "Tax Buddy") and, on top-level screens, a bottom tab bar: Accountants, Chats, then Clients or My accountants and Account when signed in, or Sign in. Desktop single pages have a heading band. **Deviation:** the footer shows on every page, phones and desktop single pages included (R16); its theme switch moved to the header on 2026-11-23 (BD14). Until 2026-11-24 this row said Clients and "For accountants" would join when built; both had joined by 2026-11-23 |
| Conversation list (R07) | Loading, ready, empty, error, signed out | Unread conversations have a badge with text, not color only. **Built in F07** from `convListHTML` in the prototype's panes: "1 new" badges, "You:" previews, Manila times; "Chats" in the navigation shows a count read as "unread" by screen readers. Since 2026-11-24 an accountant's list starts with the prototype's note, "New client messages are also emailed to you." or "Email notifications are off.", with "Change" linking to Account (R08, built in F14; left out until then). The note is left out when the setting can't be read |
| Chat thread (R06, R07) | Loading, ready, sending, sent, failed (retry), outcome unknown (checking), connection lost | Unsent text stays in the input field. Each message shows its send state until confirmed. A failed message offers retry. **Built in F07** from `messagesHTML`, `composerHTML` and `threadParts`: day separators, bubbles, message ideas, the count, Enter to send where there is a mouse. **Deviations:** a failed message's text also stays saved in the tab, so a reload puts it back in the field (BR18). Wide-screen panes fill the window, with the site footer below them (R16). A listed profile without an account says "This accountant can't receive messages yet" (no prototype state). Report and Block came with R15 (F10). The deleted-account notes came with F12 (D07). **States in place of a conversation** ("This conversation isn't available", "Only clients can start chats", "This accountant is no longer available", "can't receive messages yet"): since 2026-11-24 they sit in the detail pane under its title, with the conversation list beside them on wide screens, as in the prototype's panes. Until then they were a single centred page. They are drawn without a live connection. A visitor at a malformed conversation address gets "Sign in to see this chat", as at any conversation address |
| Email notifications (R08) | On, off | **Built in F14 (2026-11-23)** from the prototype's `emailRow` and the email-off page: "Email notifications: On/Off" on Account with the address, a line about what is sent, and Turn off/on; `/email/off` shows the state and turns emails back on. Recorded deviation: the off page shows the address only to the signed-in owner. The account page shows which address gets emails and lets the accountant turn them off. Every email has a link to turn them off |
| Connection (interactive pages) | Connected, reconnecting, could not reconnect | A visible notice while reconnecting; typed text kept (R10). If reconnecting fails, a reload button. **Built in F07** as the prototype's offline banner ("Connection lost. … Your typed text is kept."), fixed to the top of the window and not modal, so typed text stays in reach; "Try again" when reconnecting fails |
| Account deletion (R09) | Confirm, deleting, failed, deleted | Two-step confirmation that names what will be removed. Failure keeps the user signed in. **Built in F12 (2026-11-23)** from the prototype's `deleteHTML`: a red "Delete account" item in a separate Account menu; "Delete your account?" with the list for the role and the chats note; then "Are you sure?" with "Delete my account". The list names "Your profile picture and email notifications for this account." as in the prototype; until 2026-11-24 it left out the email part, while R08 wasn't built. Recorded deviations: static steps (`?step=confirm`); and a "Your account was deleted" state on the page instead of the prototype's toast on the directory, because static pages have no toasts. In chats the other person sees "Deleted user" and "This client/accountant deleted their account. You can still read this conversation, but you can't reply." |
| Ratings and reviews (R13, R14) | No reviews, some reviews, can review, cannot review (with the reason), writing, saved, replying | Stars always come with a number and a count, never stars alone. The review form explains what will be public. An accountant's own profile shows Reply on each review. **Built in F09 (2026-11-17)** from `reviewsHTML`, `reviewPageHTML`, `ratingLine` and the sort links. **Deviations:** the star picker is real radio inputs drawn as the prototype's star buttons (stars up to the checked one light up through CSS), so it works without script. Toasts become a status line in the Reviews section after the redirect. Reply and Edit reply are links that open the box (`?reply=`), and Cancel is a link. "Report this review" came with R15 (F10). No sample reviews in the development seed |
| Portfolio highlights (R12) | None, some, editing | Shown as cards on the profile and as a count on directory cards. The editor warns against naming clients. **Built in F08** (2026-11-17) from the prototype's `portfolioFieldset`, `.pf` section and card fact: the section sits between Services and Experience and is left out when there are none; "What you did" is a one-line field, as in the prototype |
| Report and block (R15) | Choose a reason, sent, cancelled; blocked by me, blocked by them | Report sits on profiles, reviews and chats. Block asks for confirmation and can be undone. **Built in F10 (2026-11-18)** from the prototype's report dialog, `REPORT_REASONS`, `thread-tools`, block dialog and notes. **Deviations:** reporting is a page (`/report/{kind}/{id}`) rather than a dialog, so it works without script. The reasons are real radio inputs drawn as the prototype's chips. The toast becomes a "Thanks. Your report was sent to the Tax Buddy team." line where the reporter returns. The blocked client's review page ("You can't review {first}", BR19), the profile line "You can't review {first}.", the report page's item name, "own content" and "already reported" states, and the block failure lines have no prototype counterpart. "Blocked…" and "Unblocked." show as a line (BD11). New tokens from the prototype: `--on-danger`, `--overlay` |
| Information pages (R16–R18) | Privacy, terms, about, help, for accountants, emails off, not found, error | Linked from the footer on wide screens and from the home and account pages on phones. **About, Help and contact, For accountants: built in F13 (2026-11-23)** from the prototype's `infoPage`. They're linked from the footer and Account, and visitors see "For accountants" in the site bar from 900 px. Recorded deviations: Help keeps only questions about built features (until 2026-11-23 no service work or email questions; since then the prototype's service, documents and email questions, the documents answer with the decided policy; the providers the site offers, so Google only while Microsoft is archived), and About names the real operator where the prototype keeps example contact details. For accountants tells a signed-in client that an account has one role. The "emails off" page waits for R08. **Legal pages, prototype 2026-11-21 (D10):** a long-document layout made up of the draft note, a version line, a "The short version" box, a "Contents" list (two columns on wide screens, one on phones) and numbered `h2` sections in the 24 px section style. Tables (the cookie table) stack into labelled rows on phones. The contents links jump to the section heading; on the site they become plain `#` anchors. The prototype shows a summary of each section. **Site, F11 (2026-11-21):** the same layout with the full owner-approved text from [docs/legal](legal/drafting-notes.md), without the blocks for unbuilt features. It has three tables (purposes, providers and cookies), all stacking on phones, and contents links of the form `/terms#terms-3`, because a bare `#` link would resolve against the base address |
| Legal and privacy prompts (D10, 2026-11-21) | Shown always; agree page: ready, not ticked (error), failed save | Decided by the user in a question session. The prototype has them first, and the site has them since F11 (2026-11-21). **Agreeing again (R16):** prototype `agreeHTML`, site `/account/agree`. It shows "We've updated our terms", the date, the same box as registration, Continue and Sign out. Recorded deviation: on the site, Sign out is a second form under Continue, not beside it, because forms can't nest. **Registration:** "I agree to the Terms of use and have read the Privacy policy." **Chat:** a one-line hint with a lock icon above the message box, "Don't share TINs, tax returns or ID numbers in chat.", tied to the box with `aria-describedby`. **Profile editor:** the professional title hint says to use only titles you hold and not to add licence or ID numbers. **Profile:** the "doesn't check licences" note adds "check a CPA's licence with the PRC". **Report page:** "review them by hand within 10 working days" (D22). **Deleted accounts (D07):** the other person sees "Deleted user" and "You can still read this conversation, but you can't reply"; the deletion page names what is removed and that chats stay |

Link business rules to the PRD instead of redefining them. Keep keyboard focus where the user was when a list updates. On phones, the on-screen keyboard must not hide the chat send button.

## Loading and busy feedback

**Status: Proposed, 2026-11-25; its open choices (D29–D34) were decided the same day. Nothing in this section is built or checked yet.** Written at the user's request by adapting the loading rules of another web app, which found and fixed about 60 loading defects in one audit. That app's checks and approvals are not evidence for Tax Buddy; its failures are kept below as reasons ("Why"). The [states table](#screen-and-component-states) says which states each screen has; this section says how each state looks and when it shows.
- How pages are built to it: [DEVELOPMENT](DEVELOPMENT.md#loading-and-busy-feedback).
- The procedure: TP28 in [TEST-PLAN](delivery/TEST-PLAN.md#scenario-mapping).
- Its choices, decided on 2026-11-25 by the user's instruction: [PRD D29–D34](PRD.md#open-decisions).

It applies to new pages and to changes to built ones. The pages built before 2026-11-25 haven't been compared with it.

**Where Tax Buddy waits.** Almost every page is static ([SDD render modes](SDD.md#render-modes)): the server sends a page only when it is complete, and no page streams (D32). So these pages have nothing to draw while their data loads:
- a first visit, a reload, a search or a form post shows the browser's own indicator;
- a link inside the site (enhanced navigation) keeps the old page until the new one is patched in. There is no indicator yet: D33 chose a thin bar along the top of the window, not built yet ([F01 P1](../specs/browse-accountants/tasks.md#polish-follow-ups));
- the prototype's skeletons and loading lines stand for that wait (the directory row above).

The in-page waits are in the chat panel, the one interactive component, and in page scripts: the chat composer and the picture preview. The rules below cover those, and any page the owner later lets stream or makes interactive.

**The prototype comes first** ([CLAUDE.md](../CLAUDE.md)). Where it draws a loading or busy state, follow it:
- the directory skeleton (`skeletonHTML`) and the workspace's "Loading your workspace…" line;
- the editor's "Saving…" and account deletion's "Deleting…" buttons, and the chat's "· Sending…";
- the spinner and skeleton styles (`.spin`, `.skel`, `.skeleton`, `.sk`).

Where the request behind this section differed from the prototype, the prototype stays (D34), and the rules below follow it. The page-move bar, the dimmed refresh, the delayed reveal and the blocking dialog have no prototype screen; draw one in the prototype before a page uses them.

### States

- **Every view that loads data has explicit states:** loading, ready, empty and error; a filtered list adds no match. A write has idle, busy, done and failed. A write whose answer was lost adds outcome unknown (checking), as the chat thread already does ("Checking whether it was sent…").
- **A view starts in loading,** never in idle or empty. Empty is something the server answered, never the starting value. *Why:* lists that started as an empty array said "No records found" before the first answer came back.
- **An error is never shown as empty.** A failed load says what failed and offers Try again in place of the empty message, with the built `StateMessage`, as the directory's "We couldn't load the accountants" does.
- **Nothing that looks like data stands in for it.** No "0 unread", "0 of 0 tasks", "No reviews yet", default name or zero count while the real value is on its way: show a skeleton for it, or nothing. *Why:* a wallet showed "0 credits" until its request answered.
- **A menu whose options are still loading** says so in the open list, for example "Loading tasks…", not "No options".
- **A check shows neither branch while it runs** (sign-in, access, a signed link). It never draws the allowed branch by default. Static pages finish their checks before anything is sent, so this binds interactive components and scripts. *Why:* a reset-password form was drawn before its link was checked.

### Which indicator, and when

| Situation | Indicator | Timing |
|---|---|---|
| First paint of a page | A static page: the finished page, with the browser's indicator until then. An interactive component with prerendering off, which loads after it is drawn: the page frame with a skeleton in the page's shape. No page streams (D32) | At once |
| Move to another page | The chrome stays, and a thin bar runs along the top of the window (D33). When the move stays on the directory, its results are dimmed too | After 300 ms, for at least 400 ms |
| A list or section loading, nothing shown yet | Skeleton rows in the list's own shape, as the prototype's `skeletonHTML`. The workspace lists show the prototype's "Loading your workspace…" line instead (D34) | After 300 ms, for at least 400 ms |
| A refresh with content on screen | The content stays, dimmed (opacity about 0.3, not clickable), with a thin bar along its top | After 300 ms, for at least 400 ms |
| A button's action | A busy button ([Writes](#writes)) | At once |
| Menu options loading | "Loading …" in the open list | At once |
| A wait the page can't continue without | One dialog with a named spinner and one line, for example "Uploading 2 files…". Rare: no page has one, and static uploads show the browser's own progress | At once |
| Connection status | The built connection notice (Connection row above): the wifi-off icon and "Reconnecting…", with no spinner (D34) | At once |

- **The delay and the minimum are working values (D29).** The 300 ms delay keeps a quick answer from flashing an indicator; the 400 ms minimum keeps one that has appeared from flickering. The prototype shows its skeletons at once.
- **Without script (server-sent HTML), the delay is CSS:** an animation keeps the element hidden for 300 ms and then shows it, so an element removed sooner is never seen.
- **Content hidden behind a skeleton shows anyway** if the script never runs or a file never loads: a CSS timeout (for example 12 s) and a `<noscript>` rule.

### Shape

- **A skeleton has the same boxes as what it stands for:** the same heights, columns, gaps and number of rows on screen. Its sizes come from the same values as the real component (for a card: its padding, avatar size and line heights), so the two can't drift apart.
- **The chrome stays:** the site bar, the phone app bar and tabs, the footer and the title band stay in place while the next page loads. A skeleton never removes them or draws them somewhere else. Enhanced navigation patches the new page into the old one, so they shouldn't flash; the frame test confirms it.
- **The frame test:** load each page on a throttled connection and look at each frame. Every step from one frame to the next must be "no change" or "content arrives". Anything that moves, resizes, disappears and comes back, or a general skeleton swapped for a specific one, is a defect (TP28). *Why:* in the other app, Home → Log in showed a grey grid, then the form's skeleton, then the form, and each swap moved the page.
- **A skeleton takes no data and no services,** so it draws the same on the server and in the browser.

### Content already on screen

- **A refresh keeps the content,** dimmed with the thin bar as in the table. It never collapses to a spinner or a skeleton, which would make the layout jump on every refresh. In the chat panel, a reload after a live update is a refresh.
- **A change only to the address** (search, service, sort, page) updates the list in place in the same way, never with a whole-page skeleton, and focus stays where it was (the rule under the states table). On the static directory, D33's script dims the results while a sort or page link loads. Until it is built, the old list stays until the new one arrives. A search is a full page load, with the browser's indicator.

### Writes

- **Check first, then go busy:** a refused field never shows a spinner. On static forms the server checks after the post, so this binds the checks made in the page: the picture preview's type and size, and the chat composer's.
- **A second press while busy does nothing.** How: [DEVELOPMENT](DEVELOPMENT.md#loading-and-busy-feedback). On a static form the browser sends the form again, and the server makes the repeat change nothing (D31).
- **A busy button** keeps its size, is disabled, sets `aria-busy="true"` and shows the 16 px spinner before its words. The words change, as the prototype's "Saving…" (editor) and "Deleting…" (account deletion) do (D34). The button is as wide as its longer label, so the change doesn't resize it. Static forms have no busy button: they run no script (D31), so the browser's indicator shows while they post.
- **A write the page knows will fail isn't started.** While the connection notice shows, writes are off, the notice says why, and the form stays as it is (R10).
- **A failure keeps everything typed and says what failed** ([BR18](PRD.md#business-rules)).
- **A wait that blocks the page** is the dialog in the table, used rarely.

### One spinner

The prototype's styles define it:
- `.spin`: 16 px, a 2 px ring in the current text colour with one side clear, turning every 0.8 s. Inside a button it takes the button's own text colour.
- `.spin.lg`: 32 px with a 3 px ring in `--link`, which is `--primary` in the site (the palette's shared `primary`/`link` value).
- Skeleton bars (`.sk`): 12 px lines on `surface-2` that pulse, their opacity falling to 0.45 and back every 1.2 s. They pulse rather than stay still (D30).

The request's 16, 24 and 40 px spinners were not taken (D34). Port these styles into `app.css`. Tailwind's `animate-pulse` is a different pulse (2 s, another curve), so don't use it in their place.

Contrast of each tone against what it sits on, where WCAG 1.4.11 asks for 3:1. Computed on 2026-11-25 from the palette values with the WCAG formula; not measured in a browser:

| Spinner or bar (colour) | Sits on | Light | Dark |
|---|---|---|---|
| `.spin.lg`, a ghost button's spinner, a refresh bar if drawn in `primary` | `surface` | 7.79:1 | 9.89:1 |
| The same | `app-bg` | 7.25:1 | 11.11:1 |
| The same | `surface-2` | 6.71:1 | 8.26:1 |
| In a default button (`text`) | `surface`, or `surface-2` on hover | 13.42:1, 11.55:1 | 13.58:1, 11.35:1 |
| In a primary button (`on-primary`) | `primary`, or `primary-strong` on hover | 7.79:1, 10.53:1 | 9.35:1, 11.04:1 |
| In a danger button (`on-danger`) | `danger` | 7.24:1 | 9.95:1 |
| In the connection notice (`warn`) | `warn-soft` | 6.30:1 | 8.13:1 |
| A skeleton bar (`surface-2`) | `surface` | 1.16:1 | 1.20:1 |

The skeleton bars are far below 3:1, as the prototype draws them. That is acceptable only because a skeleton carries no information: it is hidden from screen readers, and the wait has a name.

- **Every wait has a name.** A spinner with words beside it ("Saving…") is hidden from screen readers, as in the prototype. A spinner, skeleton or bar without words carries a name: `role="status"` with hidden text such as "Loading conversations…", as the prototype's directory skeleton does ("Loading accountants…"). A container that is loading sets `aria-busy="true"`.
- **The end of a wait the user started is announced** through the existing live regions, for example "Message sent." or "Couldn't save".
- **Reduced motion:** the site already stops every animation when the browser asks for reduced motion (`app.css`, as the prototype does; Motion row above), so skeletons are still and the spinner is a still ring. Its words or hidden name still say that it's busy.

### Failures

- **A page that fails** keeps the chrome and shows the built error page: a fixed message, Try again, a way back and the support code (R18; the BD11 "States" list above). Never the exception text: it can hold client names, messages or document names.
- **A list that fails** says what failed and offers Try again ([States](#states)).
- **A session that can't be confirmed,** for example when the database doesn't answer the sign-in check, shows the error page: never an empty page, and never the visitor's view. It has no screen of its own (D34).

### Live connections

- **A move must not close a live connection that the next view needs** before that view has its own. Going between the conversation list and a thread must not drop messages or the panel's live updates. The panel reads from the database whenever it is drawn ([SDD missed updates](SDD.md#live-chat-and-email-notifications)). *Why:* in the other app, a loading screen removed a call lobby as soon as the move to the call began; that closed the lobby's connection, and the other person saw "Call ended".

### Components

Planning labels, not existing source, as in [Stack and component mapping](#stack-and-component-mapping):

| Label | What it is | From |
|---|---|---|
| Spinner | The one spinner, 16 or 32 px | Prototype `.spin`, `.spin.lg` |
| BusyButton | A button with the busy state under [Writes](#writes) | The prototype editor's Save and "Delete my account" |
| Skeleton | Placeholder boxes in one list's or page's shape; one per list, sized from the real component's values | Prototype `skeletonHTML`, `.skel`, `.skeleton`, `.sk` |
| DelayedReveal | Shows its content after 300 ms and keeps it for at least 400 ms: CSS on server-sent HTML, one shared C# helper in interactive components | No prototype counterpart |
| RefreshBar | The thin bar: along the top of the window during a move between pages (D33), or along the top of content on screen during a refresh, which it dims | No prototype counterpart |
| ListStatus | The built `StateMessage`: empty, no match, unavailable, and error with Try again. Not a second component | Prototype `.state` |
| StatusBanner | The built connection notice, `ReconnectModal` | Prototype `offlineBanner` |

## Service workspace presentation

[PRD](PRD.md) R19–R23 / [F03](../specs/client-services/spec.md) add private work pages in the existing Tax Buddy visual system. Keep the public directory separate from private progress; no public client names, service files, completion statistics or verification badges.

**Built in the site, F03 stage 1 (2026-11-23):** the prototype's `service-workspace.js` screens, ported to static pages with the `.work-*` styles. Recorded deviations:
- A task's status is a menu of the four statuses, without the prototype's extra checkbox, so it works without script as one form field.
- A stale save shows "Saved now: …" beside the kept choice.
- Service cards said "Waiting for the accountant", "Not taken on" or "Shared service work" instead of a document count until stage 2; since stage 2 they show the count, as in the prototype.
- "Remove from my list" is a button on each saved accountant in My accountants.
- Unavailable pages answer 404 with the same "This workspace is not available" message. Since 2026-11-24 that includes Request a service opened by an accountant or for an unknown or unlisted profile; until then it said "This isn't available / Only clients can request services…" with HTTP 200. A visitor at a malformed workspace address is asked to sign in first.

**Aligned with the prototype on 2026-11-24** (after a [comparison](delivery/evidence/2026-11-24-prototype-site-comparison.md)):
- **Frame:** every workspace page (My accountants or Clients, Client services, Request a service, Service progress) now uses the prototype's desktop frame: the full 1040 px page width, a breadcrumb starting with "Clients" or "My accountants", and the page's h1 (`WorkFrame`, `.workspace`, `.work-page-title` at 22 px). This includes their not-available states. Until then they used the 680 px single column under a heading band, which left the service detail's main column about 356 px wide. On phones the app bar shows the title, so the breadcrumb is hidden and the h1 is kept for screen readers.
- **Failed saves:** "We couldn't change your list. Please try again." is the prototype's red `.work-error` alert, on the profile and in My accountants. It used to be styled as a success line. On My accountants it stays above the list, where it is seen after the page reloads; the prototype puts it under the list.
- **Times:** history entries show "3:05 PM" today, and "Yesterday · 3:05 PM" or "Nov 16 · 3:05 PM" before that. Until then today's entries showed the time twice.

**Built in the site, F03 stage 2, documents (2026-11-23):** the prototype's `filesHTML` panel under the checklist, with the `.work-files`, `.work-upload` and `.work-field` styles ported and the prototype's file icon. A real file input replaces the sample selector, with the hint "PDF, JPEG or PNG, up to 10 MB. Both of you can download it." and the "Attach to" menu. Each document shows its name, "Available · uploader · time · task or Whole service · size", Download, and Remove for its uploader. Recorded deviations:
- No upload progress bar or "Uploading…" state: the page has no script, so the browser shows its own progress while the form posts.
- After a failed or refused upload the message shows above the button, which reads "Retry upload", and the chosen task is kept; the file must be chosen again, because browsers don't keep a file choice across pages.
- The prototype's remove dialog is a confirmation step in place (`?remove=`), "Remove name?" with Cancel and a red "Remove document" button, styled `.work-confirm` with the danger edge the request panel uses in primary.
- Each list item shows the file size, which the prototype leaves out, so people know what a download costs on a phone.

| Component | States / presentation |
|---|---|
| Workspace navigation | Signed-in accountant: Clients; signed-in client: My accountants. Available in desktop navigation, phone tabs and Account. Visitors get sign-in; unrelated deep links get generic unavailable |
| Private lists | Accountant groups services by client with request counts. Client groups own services under added accountants, with profile/chat/request links. Distinct loading, empty, load-error/retry and unavailable states. Since 2026-11-25 (F17) the accountant's page for a client starts with an "About Maria" card (phone, city, business), and a request awaiting acceptance shows a compact one above Accept |
| Request/acceptance | Explicit pending, active and declined states; acceptance is not payment, booking or a verification claim. Client-name sharing is explained before request submission. Confirmed private-save/request/accept model and remaining D24 proposals are distinguished in notes/context |
| Checklist | Named tasks with visible statuses; accountant can stage checkbox/status edits and explicitly save. Client sees read-only saved values. Progress shows completed/total plus percentage, never color alone; no tasks means checklist not set |
| Shared updates | Notes/activity include author and Asia/Manila display time; newest first; no private/internal accountant notes are implied. Failed saves keep draft and say they failed |
| Service documents | Original display name, uploader, time, whole-service/task association, download and uploader-only proposed removal. Service documents do not appear in public profiles or chat. Uploading/available/failed/retry and invalid type/size are visually distinct |
| Upload prototype | Explicit generated-sample selector; no native picker or real file intake. Demo PDF/CSV/TXT and 5 MiB limits are marked proposed. A busy upload cannot be submitted again; account changes interrupt it. The site uses the decided policy instead (D26: PDF, JPEG and PNG up to 10 MB); the prototype's demo values are not updated |

Reuse existing semantic tokens, paper surfaces, the one typeface for headings and controls (serif headings until 2026-11-19), 44 px phone targets and visible focus. Desktop service detail uses a main checklist/documents column and a narrower update-history column; phone/narrow layouts stack them. Labels remain visible and no task/filename forces horizontal scrolling. Announce saves and upload failure/completion with existing live regions. Service status/progress derives from the PRD's proposed rules; do not create a second business-rule source here.

## Accessibility, responsive layout, and localization

- Use semantic HTML first: real links, buttons, headings, lists and form labels. Add ARIA only where HTML has no element for the job.
- Provide a visible focus indicator, a logical focus order, and announcements for status changes (new message, send failed, reconnecting).
- Test with a screen reader on desktop and on a phone, keyboard only, and at the zoom target in D15. Include light, dark and system themes, portrait and landscape, and the smallest supported width.
- Desktop keyboard: Enter sends a message, and Shift+Enter adds a new line (Proposed).
- English only in release 1. Keep UI text out of logic; use typed values, not display labels, in code.
- Times are displayed in Asia/Manila time (PRD BR07). RTL is out of scope.

## Verification and exceptions

Procedures: [TEST-PLAN](delivery/TEST-PLAN.md) TP06. Observed results: [STATUS](delivery/STATUS.md). The user delegated the current prototype's visual choice; owner walkthrough, unresolved behavior decisions and production accessibility remain STATUS gate G9 and TP06. Valid HTML and a passing build alone do not establish compliance.
