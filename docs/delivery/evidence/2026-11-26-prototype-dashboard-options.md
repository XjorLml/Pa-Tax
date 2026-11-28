# Prototype: accountant dashboard design options: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-26, last checks about 12:25 Asia/Manila (UTC+8); the start time wasn't recorded. The session was interrupted twice, and the interrupted checks were rerun.
- Observer: agent-observed (Claude Code). The user asked for an accountant dashboard as design options in the prototype, the same way as the earlier rounds. The request was explicit:
  - prototype only, keeping the current default;
  - three options with their idea, changes, trade-off, behaviour without script and phone layout;
  - for each, where it lives and what the site bar and phone tabs show;
  - four states: new and not listed, listed with no clients, busy, and a failed load;
  - desktop and phone, light and dark, 320 px with no overflow, and 44 px targets;
  - only data the site already has, with ideas that need new data listed separately.
- Requirement and scenario: **none.** The PRD has no dashboard. The options show existing data from R05 and BR01 (listing), R19–R23 and BR14–BR18 (requests and services), R07 (chats), and R13 and R14 (reviews). The [prototype README](../../../prototypes/README.md#accountant-dashboard-options) describes them.
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`. Another session was editing the documentation and the site in the same working tree; this work touched only the files below and this record.
- Artifact (SHA-256, first 12 characters), as checked:

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `94d685f52cfb` |
  | `prototypes/options.html` | `85fbfa0ad9d6` |
  | `prototypes/service-workspace.js` | `2d594dfcd268` |
  | `prototypes/taxbuddy.css` | `ce2b6feef915` |
  | `tools/check_prototype.mjs` | `fe8005b5238e` |
  | Options-page click script, in the session's scratch folder, outside the repository | `904a2c70504a` |

- Environment: WSL2 with Node 24.16.0 for syntax checks; Windows Edge 158.0.4370.33, headless; Windows Node 22.12.0.

## What was built

- **Options (`opt=dash-a|b|c`):**
  - A: a "Needs you" list at `/dashboard`;
  - B: tiles and sections at `/dashboard`;
  - C: an overview at the top of Clients.

  `/dashboard` exists only while A or B is shown. Without an option, the prototype is unchanged.
- **Navigation:**
  - A and B add Dashboard first in the site bar;
  - on phones it replaces the Accountants tab, with a new `home` icon in the existing line style;
  - C keeps Clients current.
- **Data for the states:**
  - two personas: Francis Lim (listed, nothing else) and Elaine Navarro (busy);
  - five services for Elaine (three requests, one waiting for the client, one completed) and three chats;
  - these were added after the existing records, so those keep their generated IDs. Nobody else sees them, and no review or profile changed.
  - The address takes `wsdata=` for the failed load, as the Workspace data control sets it.
- **Accept and Decline:** the service page's actions (`ws-accept`, `ws-decline`), with its error line.
- **Styles:** a `dash-` section in `taxbuddy.css`, built from the existing tokens, pills, meters, the `opt-next` list, `work-stat` tiles and `work-panel` sections.
- **`options.html`:**
  - a fifth accountant step, "Dashboard (new)";
  - a **State** switch (Andrea, Busy, New, not listed, Listed, no clients, Load fails), kept in the address as `state=`;
  - option rows for "Where it lives" and "Site bar and tabs";
  - page notes for Scope, Data, "Needs a PRD decision (left out)" and "Overlap with Clients".
- **Guide panel:** notes for `/dashboard`.

## Execution

| Procedure | Expected result | Actual observation | Outcome |
|---|---|---|---|
| `node --check` on the inline scripts of `index.html` and `options.html`, `service-workspace.js` and the checker | No syntax errors | None | Pass |
| `node tools/check_prototype.mjs` on a copy of `prototypes/` in the Windows temp folder, after the last change | All pass | `PASS: 259 prototype checks` (224 before, 35 new); exit 0. The same count also passed before the three fixes below | Pass |
| The new checks: the default, A, B, C and the failed load, across the states | As in the option notes | All pass:<br>• the default has no Dashboard item, and `/dashboard` is "We couldn't find that page";<br>• A as Andrea: Dashboard first and current, four items. Accept removes the request, and the work line becomes "1 service in progress";<br>• A listed with no clients: "You're all caught up", with no items;<br>• A new, on a 320 px phone in dark: four tabs, Dashboard first and current, no Accountants tab, "Create your profile" first, "1 of 6 details";<br>• B busy at 320 px: tiles read 3, 3, 2, 4.0; five sections; three Accept buttons; both unreplied reviews; a tile moves focus to its section;<br>• B desktop dark: the four tiles in one row;<br>• C busy desktop: Clients current, eight items, three cards, then "Your clients";<br>• C at 320 px: "Needs you" first and the tabs unchanged;<br>• the failed load shows "We couldn't load your dashboard" and Try again, which loads it; C keeps Clients' message.<br>Every phone page has 44 px targets, and every checked page has no horizontal overflow | Pass |
| Options-page click checks (scratch script) | All pass | 11 of 11:<br>• five accountant steps, the fifth "Dashboard (new)";<br>• the State switch appears only on that step, and leaving the step clears it;<br>• Current frames Clients with no `opt`;<br>• A and C frame `/dashboard` and `/workspace` with their `opt`;<br>• Busy and Load fails change the frame's `role` and add `wsdata=error`;<br>• the notes rows are present;<br>• Compare shows four frames | Pass |
| Screenshots in headless Edge, viewed:<br>• A as Andrea and as a new accountant (desktop light);<br>• A busy (phone dark);<br>• B busy (desktop light and dark, phone light);<br>• C busy (desktop dark, phone light);<br>• the failed load;<br>• B listed with no clients (desktop dark);<br>• the options page | Matches the notes; existing look | Three faults found and fixed, then rechecked by screenshot:<br>• B's group headings took the panels' 24 px size;<br>• A showed "Create your profile" twice for a new accountant;<br>• the dashboard's heading sat against the site bar.<br>One design change: C on phones first put the cards above "Needs you", which pushed the actions below the fold, so "Needs you" now comes first | Pass after fixes |

## Restoration and limits

- **Cleanup:**
  - the Windows temp copy, the click script's copy and Edge's profiles were removed;
  - the screenshots and the click script stay in the session's scratch folder, outside the repository.
- **Not run:**
  - a page-by-page DOM comparison of the default pages with the prototype before this change. The previous version was uncommitted, so there was no baseline to compare. The new code paths depend on `opt=dash-…`, the two new personas or `wsdata=`, and all 224 earlier checks still pass;
  - real phones, screen readers, keyboard-only use, or contrast calculations (existing tokens only).
- **Scope:** none of the options is chosen. Choosing one needs an owner decision recorded in the PRD or DESIGN and in the SDD's navigation table.

## Follow-up the same day: archived

- **Request:** the user asked "is this the best homepage you can recommend for the accountant users?", then said: "archive dashboard design in the prototype for now".
- **Recommended in the conversation, not built:** no separate dashboard. Clients would become the accountant's home:
  - signed-in accountants land there;
  - a short "Needs you" strip shows requests and chats waiting;
  - a listing banner shows until they're listed.

  This was a judgement from the product's purpose and data, not user research.
- **Changed:**
  - `options.html` leaves archived pages out of the steps and out of Previous and Next, and lists them under **Archived** in the header. The accountant's steps are back to four.
  - The Dashboard page says "archived page, not in the flow". Its notes start with an Archived row, and its options read "archived, not chosen".
  - The code comments and the guide panel say archived.
  - The designs, the two personas and the 35 checks stay, so the archive keeps working.
- **Artifact (SHA-256, first 12 characters):**

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `7705688d9337` |
  | `prototypes/options.html` | `ff75c75a942b` |
  | `prototypes/taxbuddy.css` | `71da7172a838` |
  | `tools/check_prototype.mjs` | `a4cc2e2579bb` |
  | Click script (scratch) | `7f30bce667bf` |

| Procedure | Expected result | Actual observation | Outcome |
|---|---|---|---|
| `node --check` on the inline scripts and the checker | No syntax errors | None | Pass |
| `node tools/check_prototype.mjs` on a Windows temp copy, about 14:20 | All pass | `PASS: 259 prototype checks`; exit 0 | Pass |
| Options-page click checks, updated | All pass | 14 of 14:<br>• four accountant steps, and Dashboard only under Archived;<br>• no Archived group in the client flow;<br>• the archived page labelled, with no step current and Previous and Next off;<br>• option A reads "archived, not chosen";<br>• the states, frames and Compare as before | Pass |
| Screenshot of the options page header | Archived link beside the steps | "Archived · Dashboard" after Service progress, marked current on that page | Pass |

Cleanup: the Windows temp copy and Edge's profile were removed.
