# AccApp UI and UX standards

**Authority:** visual and interaction standards. [PRD](PRD.md) owns feature scope. [SDD](SDD.md) owns technical implementation. Status: Draft — no visual system chosen yet. Review date: 2026-11-11.

## Scope notice

Applies to the release 1 pages in the [SDD navigation table](SDD.md#navigation-and-screen-state):
- directory
- profile
- sign-in
- conversation list
- chat thread
- my profile editor
- account settings

It is one responsive website, used in phone and desktop browsers.

Proposed concepts: the clickable [design prototype](../prototypes/README.md), updated on 2026-11-11 for the web. It shows every page and state in the table below, and one option for each of decisions D03–D06, D08 and D18. For the visual system it offers four looks to choose from (Modern, Ledger, Neighbourly and Clear), described with the reference websites behind them and their contrast figures in [DESIGN-DIRECTIONS](../prototypes/DESIGN-DIRECTIONS.md). All of it is a proposal for review, and the table below stays "not chosen" until the owner picks. A mock, screenshot, or design proposal is not authorization to build a feature or evidence of accessibility compliance.

## Visual system

| Role | Value / semantic resource | Rationale and verification |
|---|---|---|
| Brand identity | Not chosen; waits for the final name (PRD D01). Interim: a plain letter mark, replaced before any public launch | Assets must be original or licensed; licenses recorded beside the assets |
| Color | Not chosen. Must define surface, text, primary, error and unread-indicator roles for light and dark themes as CSS variables | Contrast to be measured once chosen; unread and error states also need a non-color cue (badge text, icon with label) |
| Typography | Not chosen. Proposed: system fonts until branding is decided; heading, body and caption roles | Must respond to browser zoom and the user's text size (D15) |
| Layout | Not chosen. Must define a spacing scale, a maximum content width for wide windows, breakpoints, and a minimum touch target size | Smallest supported width comes from D15 |
| Motion | Pressed, loading and sending feedback only | Respect the browser's reduced-motion setting |

Centralize the variables in one stylesheet. Decided on 2026-11-11: Tailwind CSS for styling, with these variables as its theme and no component library ([SDD](SDD.md#technology-stack)). Document exceptions instead of allowing each page to invent colors, spacing, and status symbols.

## Screen and component states

| Component / workflow | States to design | Observable behavior |
|---|---|---|
| Directory list and search (R01, R02) | Loading, ready, empty directory, no match, load error | Empty and "no match" use different messages. Search text and filter live in the address, so Back and shared links keep them. A failed load is an error with a retry, never an empty list |
| Profile (R03) | Ready, not available (removed or malformed link), error | Contact details and services are readable without sign-in. A "Message" action is shown to everyone; visitors are taken to sign-in first and returned afterwards |
| Sign-in (R04) | Choose provider, redirecting, cancelled, failed, choose role | Cancelling at the provider returns to the previous page |
| Profile editor (R05) | Pristine, draft, validation error, saving, failed, saved, changed elsewhere | Edits are never cleared by a failure. Leaving with unsaved changes asks for confirmation |
| Conversation list (R07) | Loading, ready, empty, error, signed out | Unread conversations have a badge with text, not color only |
| Chat thread (R06, R07) | Loading, ready, sending, sent, failed (retry), outcome unknown (checking), connection lost | Unsent text stays in the input field. Each message shows its send state until confirmed. A failed message offers retry |
| Email notifications (R08) | On, off | The account page shows which address gets emails and lets the accountant turn them off. Every email has a link to turn them off |
| Connection (interactive pages) | Connected, reconnecting, could not reconnect | A visible notice while reconnecting; typed text kept (R10). If reconnecting fails, a reload button |
| Account deletion (R09) | Confirm, deleting, failed, deleted | Two-step confirmation that names what will be removed. Failure keeps the user signed in |
| Ratings and reviews (R13, R14) | No reviews, some reviews, can review, cannot review (with the reason), writing, saved, replying | Stars always come with a number and a count, never stars alone. The review form explains what will be public. An accountant's own profile shows Reply on each review |
| Portfolio highlights (R12) | None, some, editing | Shown as cards on the profile and as a count on directory cards. The editor warns against naming clients |
| Report and block (R15) | Choose a reason, sent, cancelled; blocked by me, blocked by them | Report sits on profiles, reviews and chats. Block asks for confirmation and can be undone |
| Information pages (R16–R18) | Privacy, terms, about, help, for accountants, emails off, not found, error | Linked from the footer on wide screens and from the home and account pages on phones |

Link business rules to the PRD instead of redefining them. Keep keyboard focus where the user was when a list updates. On phones, the on-screen keyboard must not hide the chat send button.

## Accessibility, responsive layout, and localization

- Use semantic HTML first: real links, buttons, headings, lists and form labels. Add ARIA only where HTML has no element for the job.
- Provide a visible focus indicator, a logical focus order, and announcements for status changes (new message, send failed, reconnecting).
- Test with a screen reader on desktop and on a phone, keyboard only, and at the zoom target in D15. Include light, dark and system themes, portrait and landscape, and the smallest supported width.
- Desktop keyboard: Enter sends a message, and Shift+Enter adds a new line (Proposed).
- English only in release 1. Keep UI text out of logic; use typed values, not display labels, in code.
- Times are displayed in Asia/Manila time (PRD BR07). RTL is out of scope.

## Verification and exceptions

Procedures: [TEST-PLAN](delivery/TEST-PLAN.md) TP06. Observed results: [STATUS](delivery/STATUS.md). Known gap: the visual system is not chosen; owner lumabanls; the next check is the prototype review (STATUS gate G9). Valid HTML and a passing build alone do not establish compliance.
