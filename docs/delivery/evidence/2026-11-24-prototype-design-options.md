# Prototype: design options for the client's flow and a workspace side list: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-24, from about 17:10 Asia/Manila (UTC+8), for the build and the checks in a browser. Committed and pushed on 2026-11-25 at 08:31. The prototype checker was run, and this record written, on 2026-11-25 at about 08:50.
- Observer: agent-observed (Claude Code). The user asked for navigation recommendations, such as a sidebar for the private pages, then said "show in the prototype". After that they asked for a page of design options, at least three per page, for the client's flow after sign-in. They then said "proceed with that prompt, profile as step 2" and kept every proposed option except profile C.
- Requirement and scenario: proposals only; nothing is adopted. Pages: the directory (R01, R02), the accountant profile (R03, R06), chats (R06, R07, R15) and My accountants (R19–R22). Also the workspace navigation in [DESIGN](../../DESIGN.md#service-workspace-presentation). The [prototype README](../../../prototypes/README.md#design-options-for-the-clients-flow) describes the options page and its address settings.
- Source revision: `06e9be6` on branch `feature/f02-f04-theme-switch-google-sign-in`. It was built on `5237dab`, which is the baseline for the unchanged-default check below.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `66de93417f23` |
  | `prototypes/options.html` | `199f894851df` |
  | `prototypes/service-workspace.js` | `8715c18f6b4d` |
  | `prototypes/taxbuddy.css` | `74747c32548e` |
  | `prototypes/README.md` | `efc3a4d88539` |

- Environment: WSL2 with Node 24.16.0 for the syntax checks; Windows Edge 158.0.4370.33, headless, opening the files from WSL; Windows Node 22.12.0 for the prototype checker.

## What was built

The prototype's current design stays the default. The proposals appear only when chosen.

- **Workspace navigation switch** ("Workspace navigation" in the prototype bar, or `wsnav=side`). Its proposal:
  - from 960 px, a list of the viewer's clients (or accountants) and their services beside the client and service pages;
  - a breadcrumb that names the client and the service;
  - on phones, Back from a service goes to its client.
- **Design options page,** `prototypes/options.html`:
  - Steps: 1 Directory, 2 Accountant profile, 3 Chats, 4 My accountants.
  - Options, picked through `opt=page-x`:
    - Directory: A, B, C;
    - Profile: A, B;
    - Chats: A, B, C;
    - My accountants: A, B, C.
  - Each option is the real prototype in a frame (`embed=1`), signed in as the client Maria Santos.
  - Notes per option; Current, option and Compare views; Phone/Desktop and theme switches; and an address that keeps the page, option, frame and theme.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `node --check` on `service-workspace.js`, and on the inline scripts of `index.html` and `options.html` (extracted) | No syntax errors | No errors, after the last change to each file | Pass |
| Side list screenshots, Edge headless. Accountant at 1200, 1010 and 900 px windows (light); client at 1200 (dark) | From 960 px: the list beside the service page with the open service marked, and the full breadcrumb. Narrower: no list, the breadcrumb kept. The service page's two columns drop to one when the content gets narrow | As expected at each width. At about 1000 px the progress panel moved under the checklist. At 900 px there was no list | Pass |
| Phone Back on `#/work/e1` as the accountant, read from the rendered page, with the side list then the current design | `#/clients/u-maria` with the side list; `#/workspace` without it | As expected | Pass |
| 10 rendered states of the workspace pages (both designs; accountant, client, visitor, outsider; unknown service) | The current design is unchanged. With the side list: no list for a visitor, and only the viewer's own work | Current: "Clients / Service progress", no list. Side list: "Clients / Maria Santos / Bookkeeping". A visitor had no list. An unrelated accountant saw only their own empty list, never another person's work | Pass |
| Screenshots of the 11 options in a 1120 × 860 desktop frame (`embed=1`, light) | Each option draws as described in its notes | All drew. One fault, fixed and seen again: Chats A's star rating broke across three lines | Pass after fix |
| Screenshots of the 11 options in a 430 × 820 phone frame, grouped into contact sheets | Each option draws as described for phones | All drew. Two findings, the first fixed and seen again. **Chats A:** the "About" disclosure was scrolled out of view, because the conversation opens at its newest message. It is now pinned under the app bar. **Off-centre frames:** headless Edge won't make a window narrower than 500 px, so the frames looked shifted in these screenshots. Inside `options.html` the frame is exactly 430 px wide and centred | Pass after fix |
| `options.html` screenshots: Chats A in the desktop and phone frames; My accountants in Compare with phones; Directory in Compare with the desktop frame, dark | The step bar, tabs, notes and frames as designed | As expected. Fixed: the Current frame's caption read "Current Current". Directory B's search column was widened to 260 px so its placeholder fits | Pass after fix |
| The default design unchanged: the app's rendered DOM (`#app`) for 14 pages, with no option chosen, compared with `5237dab`. Times were masked. Pages: directory, profile, thread, chats, workspace, service page and Account, for client, accountant and visitor, on desktop and phone | Identical | 13 identical. The 14th (the accountant's service page) differed only in the seconds of one `<time datetime>` written at page load | Pass |
| 26 checks on the rendered DOM in 9 states: the tabs in the address, the chosen accountant, the section-link targets, the More menu, and the fallbacks | Each check holds | 26 of 26 held. B's Completed tab showed "No services here." with `?status=completed` in the address. C with Elaine chosen showed her service only. Profile A had all five targets. Chats B had Report and Block inside More and no tools row. An accountant fell back to the current design for Chats A, Directory C and My accountants A. A visitor kept the visitor band with option A's rows | Pass |
| `python3 tools/check_docs.py prototypes/README.md` | 0 errors | `PASS: 1 Markdown files; 38 local links; 0 errors.` | Pass |
| `node tools/check_prototype.mjs "<Edge>"` with Windows Node 22.12.0, on a copy of `prototypes/` and the checker in the Windows temp folder, at `06e9be6` | All pass | `PASS: 202 prototype checks; Edg/158.0.4370.33`, exit 0 | Pass |

## Restoration and limits

- **Cleanup:**
  - Edge's temporary profile was removed after each session of checks.
  - The checker's Windows copy and its artifacts were deleted.
  - The screenshots were viewed in the session's scratch folder, outside the repository. That folder was cleared when WSL restarted, before this record was written, so they aren't kept.
- **What the checker covers:** its 202 checks cover the default design, which is unchanged. It has no checks for the options or the side list. Those were checked only by the screenshots and DOM reads above.
- **Not run:**
  - real phones, screen readers, or a keyboard-only walk-through of the options;
  - true browser zoom;
  - contrast calculations for the new styles, which reuse the existing token pairs (muted on surface, text on primary-soft, on-soft on primary-soft);
  - clicks were not simulated: the section links, the More menu and the tabs were checked by their markup and addresses.
- **Nothing adopted:** these are proposals. DESIGN, the feature specs and the site are unchanged until the owner chooses an option for each page.
