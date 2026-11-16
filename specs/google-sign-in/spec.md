# F04: Google sign-in and registration

Amended 2026-11-16 by [F02 BD10](../brand-design-refresh/spec.md#application-aligned-to-the-prototype-2026-11-16-bd10): the pages now follow the prototype. The header shows the avatar and name, linking to a new Account page, and "Sign out" lives there. The role is chosen with two large choice buttons, and the footer links read Privacy and Terms. Behaviour is unchanged.

Governing requirements: [PRD](../../docs/PRD.md) R04 (Google part) and R16 (agreement at first sign-in), with BR07 for stored times. Contracts: [SDD sign-in](../../docs/SDD.md#sign-in-sessions-and-authorization), [ADR 006](../../docs/adr/006-web-sign-in.md), [DESIGN states](../../docs/DESIGN.md#screen-and-component-states). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A visitor signs in with their Google account. On the first sign-in they choose to be a client or an accountant and agree to the terms and privacy policy, which creates their Tax Buddy account. After that they stay signed in in that browser until they sign out or the session ends. Every later feature (chat, reviews, profile editing, F03 workspaces) depends on this.

**Existing behaviour** (`af9d3ea` plus uncommitted F02 BD09): no accounts, no sign-in, and no terms or privacy pages. The F01 profile's "Message" link leads to a static "coming soon" page.

**Authorization and decisions** (all on 2026-11-16):
- The user asked to "focus on google signin first implementation and requirements" after the F03 readiness review.
- They decided PRD D03: one role per account. The prototype proposal "an account is either a client or an accountant" is adopted.
- They chose clearly marked draft terms and privacy pages until the legal text exists (D10, gate G13).

**In scope:**
- a Google sign-in page;
- first-sign-in registration (role and agreement);
- returning sign-in, sign-out, and the signed-in state in the header;
- draft `/terms` and `/privacy` pages, linked from every page;
- cancel and failure handling;
- running without Google credentials configured.

**Exclusions:**
- Microsoft sign-in. It is the second half of R04, and the plan keeps the structure provider-neutral so it can be added later.
- Profile editing (R05). Registering as an accountant creates no profile, so nothing is listed (BR01).
- Account deletion (R09), chat (R06), and restoring a chat draft after sign-in.
- Persisting data-protection keys outside the app, which the SDD requires before deployment. Sessions survive restarts only on the same machine until then.
- Final legal text (D10).

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| GS01 / R04, R16 | A visitor on any page uses "Sign in", continues with Google for the first time, chooses a role and agrees | One account is created holding the one chosen role, with the Google sign-in linked, the agreement time (UTC) and the agreed text version. The visitor is signed in and returned to the page they started from. The header shows their name and "Sign out" | Web.Tests with a test sign-in; real Google by hand (GS09) |
| GS02 / R16, D03 | On the registration page, the visitor submits without agreeing, or with no or an unknown role | "Tick the box…" or "Choose…" error; the choice already made is kept; no account is created; still not signed in | Web.Tests |
| GS03 / R04 | A registered person signs in again with the same Google account | No registration step; signed in and returned to the starting page | Web.Tests |
| GS04 / R04, DESIGN | The person cancels on Google's page, or Google reports an error | Cancel: back on the starting page, not signed in. Error: the sign-in page with "We couldn't sign you in" and a way to try again | Web.Tests (callback simulated with a real state value); by hand with Google |
| GS05 / SDD | A forged request: an off-site return address, no antiforgery token, an unknown provider, or the registration page opened without a Google sign-in in progress | Off-site addresses go to `/`; a missing token or unknown provider gets 400 with nothing changed; registration without a pending sign-in goes to `/signin` | Web.Tests |
| GS06 / R04 | A signed-in person presses "Sign out" | The session ends in that browser; the header shows "Sign in" again | Web.Tests, browser |
| GS07 / R16 | Any page | Footer links to Terms of use and Privacy. Both pages load without sign-in and say plainly that they are a draft, not reviewed legal text | Web.Tests |
| GS08 / SETUP | The site runs with no Google client ID or secret configured | The sign-in page says Google sign-in isn't set up and offers no broken button; the rest of the site works | Web.Tests |
| GS09 / R04, TP07 | The owner runs the site locally with real Google credentials | Real round trip: Google's page, back to the site, registration, then a returning sign-in | Manual, owner or agent with the owner's credentials |
| GS10 / R11, SDD diagnostics | Any of the above | No email address or name is written to the logs. Client accounts appear on no public page. Accountant registration lists nothing | Code review, Web.Tests |

## State, dependencies, and unresolved decisions

Sign-in states:

| State | Holds | Ends |
|---|---|---|
| Signed out | — | "Continue with Google" |
| At Google | Google's state and correlation values in the browser | Return to the callback, cancel or error |
| Pending registration (first time only) | Identity's short-lived external cookie: the Google key, email and name | Registration completes, the cookie expires, or the person signs in again |
| Signed in | The session cookie (HTTPS-only outside development, unreadable by scripts) | Sign out or expiry |

Session lifetime: persistent, 14 days, renewed on use (Identity's default). The SDD left the lifetime to be set at implementation; the 14-day value is **Proposed** and the owner can change it.

Data: new tables for accounts and their sign-in links (plan). There is no link to `accountant_profiles` yet; R05 adds it.

**Prerequisite the owner has to set up:** a Google Cloud OAuth client with redirect addresses for development and each deployed site ([plan](plan.md#google-cloud-setup)).

**Recorded clarification:** F01's FT01 says the directory loads "without a sign-in prompt". A "Sign in" link in the header, as drawn in the prototype, is navigation, not a prompt. The FT01 test now checks that browsing needs no sign-in, rather than that the words never appear.

Still open, and not blocking this feature:
- D01 (name), D10 (legal text), D17 browser matrix.
- Whether one person may link both Google and Microsoft to one account. That is decided when Microsoft is added.

## Evidence limits

Integration tests use a test sign-in in place of Google. They establish the site's own handling, not Google's. A real Google round trip is GS09 locally, then TP07 on a deployed site. Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
