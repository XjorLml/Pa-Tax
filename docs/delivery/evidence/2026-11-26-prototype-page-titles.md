# Prototype: page titles on the workspace pages: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-26, last checks about 15:10 Asia/Manila (UTC+8); the start time wasn't recorded.
- Observer: agent-observed (Claude Code). The user sent two screenshots, of Clients and of Maria Santos's client page in the dark theme, and said: "create recommended designs for these, it shows redundant titles".
- Requirement and scenario: no requirement changes; this is a layout proposal for [DESIGN](../../DESIGN.md). The pages are F03's workspace pages (R19–R23). The site builds the same header in `Components/Work/WorkFrame.razor` and `WorkIntro.razor`, following the prototype. The [prototype README](../../../prototypes/README.md#page-titles-options) describes the options.
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`. Another session was working in the same tree; only the files below and this record were changed here.
- Artifact (SHA-256, first 12 characters), as checked:

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `34aa0c4d6390` |
  | `prototypes/options.html` | `18ddc7f744ec` |
  | `prototypes/service-workspace.js` | `4cafb199bcd0` |
  | `prototypes/taxbuddy.css` | `3329f44f7571` |
  | `tools/check_prototype.mjs` | `c6da3fec1ece` |
  | Options-page click script, in the session's scratch folder, outside the repository | `814a27d48fdf` |

- Environment: WSL2 with Node 24.16.0 for syntax checks; Windows Edge 158.0.4370.33, headless; Windows Node 22.12.0.

## What was built

- **Options (`opt=heads-a|b|c`):** only with the option set, the workspace pages get one title each:
  - A: a breadcrumb of the pages above, then the name as the h1, then the page's sentence;
  - B: Back, the name and a privacy pill in one bar;
  - C: the pages above and the name as one title line.

  The eyebrow and the slogans go in all three. On phones, the app bar shows the name, and Back goes to the page above. B adds the pill and C the pages-above links at the top of the page.
- **The service page's sentence:** under the options it says who can see the work ("Only Maria and you can see this work."), because the breadcrumb already names the client.
- **Code:**
  - `service-workspace.js`: `heading()` follows the option; `api.head(r)` gives the name, the pages above and who can see the page; `api.privatePill`.
  - `index.html`: `headsHTML(r)` draws the desktop header. The Side list proposal keeps its own header.
  - `taxbuddy.css`: a `heads-` section using existing tokens.
- **`options.html`:**
  - step 5 of the accountant's flow, "Page titles";
  - a **View** switch (the State mechanism, relabelled, with a page address per view);
  - notes for the pages covered, the recommendation (A), the heading levels and the scope.

## Execution

| Procedure | Expected result | Actual observation | Outcome |
|---|---|---|---|
| `node --check` on the inline scripts, `service-workspace.js` and the checker | No syntax errors | None | Pass |
| `node tools/check_prototype.mjs` on a Windows temp copy, after the last change | All pass | `PASS: 279 prototype checks` (259 before, 20 new); exit 0 | Pass |
| The new checks | As in the option notes | All pass:<br>• the default client page keeps "Client services" and the eyebrow;<br>• A: Clients has one title and no breadcrumb, eyebrow or slogan; the client page is titled "Maria Santos" under "Clients"; the service "Bookkeeping" sits under "Clients / Maria Santos" with "Only Maria and you can see this work."; Request a service says its name once;<br>• A on a 320 px phone: the app bar reads "Bookkeeping" and Back goes to the client;<br>• B, desktop dark: Back to Maria Santos, the h1 and "Private to you and Maria"; on a 320 px phone as the client, "Private to you and Andrea";<br>• C: the trail and the name on one line; on a 320 px phone the trail links are 44 px tall.<br>No horizontal overflow on the four pages checked for it | Pass |
| Options-page click checks (scratch script, updated) | All pass | 18 of 18:<br>• the dashboard's 14, adjusted for the new step;<br>• five steps ending with Page titles;<br>• the View label and six views;<br>• the Recommended note;<br>• A's service view framing `opt=heads-a` and `#/work/e1`;<br>• the Request view signing in the client | Pass |
| Screenshots in headless Edge, viewed:<br>• A on Clients, the client and the service (desktop dark);<br>• B and C on the service (desktop light);<br>• A on the client, B on the client's service and C on the service (phone) | One title per page; existing look | As designed. The service page's sentence repeated the breadcrumb, so under the options it now says who can see the work. Rechecked by the checker | Pass after the fix |

## Restoration and limits

- **Cleanup:**
  - the Windows temp copy and Edge's profile were removed;
  - the screenshots and the click script stay in the session's scratch folder, outside the repository.
- **Not run:**
  - a DOM comparison of the default pages with the previous prototype (no committed baseline). The new code runs only with `opt=heads-…`, and the earlier 259 checks still pass;
  - real phones, screen readers or contrast calculations.
- **Not changed:**
  - the heading levels of the sections: they stay h3 in the prototype, so under the options the outline goes from h1 to h3. The notes say the site would move them up a level;
  - the site, DESIGN and the PRD. None of the options is chosen.
