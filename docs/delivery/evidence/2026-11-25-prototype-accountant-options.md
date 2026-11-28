# Prototype: design options for the accountant's flow: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-25, about 09:00–09:45 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user said "now update prototype design recommendations for accountant pages". From a one-line list of three options for each of four pages, they kept all twelve.
- Requirement and scenario: proposals only; nothing is adopted. Pages: My profile (R05, R11, R12), Chats (R07, R08, R15), Clients (R19) and Service progress (R21–R23). The [prototype README](../../../prototypes/README.md#design-options-for-the-accountants-flow) describes the options. They extend the client's options of 2026-11-24 ([evidence](2026-11-24-prototype-design-options.md)).
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`. `06e9be6` is also the baseline for the unchanged-default check.
- Artifact (SHA-256, first 12 characters), as checked:

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `0e759992df5d` |
  | `prototypes/options.html` | `eae92b3af14c` |
  | `prototypes/service-workspace.js` | `847d6c0f520f` |
  | `prototypes/taxbuddy.css` | `65a532c57ef5` |

- Environment: WSL2 with Node 24.16.0 for the syntax checks; Windows Edge 158.0.4370.33, headless; Windows Node 22.12.0 for the click checks and the prototype checker.

## What was built

- **Options page:** `options.html` gained a **Flow** switch, Client or Accountant (`?flow=accountant`). The accountant's steps, signed in as Andrea Villanueva:
  1. My profile, `opt=editor-…`: A section menu, B preview beside the form, C compact top with Save in view.
  2. Chats, `opt=achats-…`: A client panel, B waiting for your reply, C All / Unread tabs.
  3. Clients, `opt=clients-…`: A requests first, B services by status, C client table.
  4. Service progress, `opt=work-…`: A section links, B documents per task, C updates first.
- **Chats setting split:** the accountant's Chats options have their own setting. The client's Chats options of 2026-11-24 now apply only to clients. Before, client option B also changed an accountant's list.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Current accountant pages viewed before proposing: editor, Chats, Clients, a client page, a service as a request and as active work. Desktop 1120 × 860 and a 430 px phone frame | A baseline for the options | Seen. Finding, not fixed: on a service still awaiting acceptance, the accountant's own checklist says "Updated by your accountant" and "Progress starts after the accountant adds a checklist", which is the client's wording | Recorded |
| `node --check` on `service-workspace.js`, and on the inline scripts of `index.html` and `options.html` (extracted) | No syntax errors | No errors after the last change | Pass |
| Screenshots of the 12 options in the desktop frame, signed in as the accountant (light) | Each draws as its notes say | All drew. Fixed and seen again: Editor C's sticky Save left a 24 px strip where the form showed through below it. Small fix, not re-screenshotted: in Service C's narrow checklist card, the status menus now fill the card | Pass after fixes |
| Screenshots of the 12 options in the phone frame, grouped into contact sheets | Each draws as described for phones | All drew. Fixed: Clients C's cards had the browser's default list indent. The frames sit off-centre in these screenshots, because headless Edge won't size a window under 500 px; this is not a page fault | Pass after fix |
| 21 click checks with Windows Node and Edge over the debugging protocol, on a copy of `prototypes/` in the Windows temp folder (script kept outside the repository) | Each action has its result | 21 of 21 passed, 0 script errors. Checked: Clients A, Accept then Decline from the list; Service A, the Updates link moves focus; Service B, a task lists its document, and Attach picks that task in the upload form and focuses it; Chats A, Review request links to the request; Chats C, Unread leaves the unread and open conversations, with `?filter=unread` in the address; Editor A, a section link moves focus; Editor B, the card shows; Editor C, the checklist is folded and opens. Client options: profile A, My accountants B and C, Chats B's More menu, Directory B's Payroll filter. The options page: four accountant steps, Compare frames signed in as the accountant, and back to the client's flow | Pass |
| The default design unchanged: the app's rendered DOM (`#app`) for 16 pages with no option chosen, compared with `06e9be6`, with times masked. Accountant: editor (also a new accountant), Chats, Clients, a client page, service pages. Client: service page, chat, My accountants, directory and profile. Desktop and phone | Identical | 16 of 16 identical | Pass |
| `node tools/check_prototype.mjs "<Edge>"`, Windows Node 22.12.0, on the same copy | All pass | `PASS: 202 prototype checks; Edg/158.0.4370.33`, exit 0 | Pass |
| `options.html?flow=accountant&page=work&option=compare` viewed | The Flow switch, four steps and four frames | As expected | Pass |

## Restoration and limits

- **Cleanup:** the Windows temp copy, the click script's browser profile and Edge's screenshot profile were removed. The screenshots stay in the session's scratch folder, outside the repository.
- **Not run:** real phones, screen readers, a keyboard-only walk-through, true zoom, and contrast calculations for the new styles, which reuse the existing token pairs. Not every action was clicked: the click checks covered one or two per option, and saving the profile with an option on wasn't tried.
- **Nothing adopted:** DESIGN, the feature specs and the site are unchanged until the owner chooses.
