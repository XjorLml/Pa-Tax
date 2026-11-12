# Prototype scope and stack-fit review

## Identity and attribution

- Date: 2026-11-12. Recorded at 04:54 UTC.
- Observer: agent read-through of documents and prototype source. No browser run, build or test was executed for this review. The findings are the agent's observations and recommendations. No owner decision, approval or scope change is claimed.
- Scope: the user asked the agent to "review the protype, check on extra or overengineered features as per docs. consider the stack limitations etc", then to record the result. Compared: prototype behaviour against [PRD](../../PRD.md) R01–R23, BR01–BR18 and D01–D28, the [SDD](../../SDD.md), [DESIGN](../../DESIGN.md) screen states and service workspace presentation, the [F01 plan](../../../specs/browse-accountants/plan.md) and the [F03 spec](../../../specs/client-services/spec.md).
- Baseline: `1fccef362fe04050977c3f550997ac6637091b4b` on `develop` plus the uncommitted [rename](2026-11-12-tax-buddy-rename.md) and [palette options](2026-11-12-palette-options.md) of the same day.
- Reviewed source, SHA-256: `prototypes/index.html` `934e39bbfa55cdc521c3707b32f47f717271fdff8b5f9d7ba168a6f9304e3ddd`; `prototypes/taxbuddy.css` `2dddc60aa13e09c30eebaf81042a65445604bfe83e654fff0ba1233cc4624ec8`; `prototypes/service-workspace.js` `cc83306f3db10889a7c26b3b5f0a8e1a9ebc4abba66e3d314883efe8eb85ab8b`.
- Environment: not applicable to a read-through. Hashes were computed on the development PC with Git Bash `sha256sum`.
- Fixtures: none. Nothing in the prototype, the PRD, the SDD or DESIGN was changed by this review.

## Method and limits

- Read in full: SDD, `service-workspace.js`, PRD scope, requirements, rules, workflows and open decisions, [ADR 009](../../adr/009-private-service-workspaces.md).
- Read in part: `index.html` (data fixtures, routes, directory, profile, chat, sign-in, account, editor and deletion code), DESIGN (stack mapping, screen states, service workspace, accessibility), [REFERENCES](../../REFERENCES.md) Blazor and hosting rows.
- Not read: TEST-PLAN, the F03 plan and tasks in full, and the review and report rendering code line by line.
- Stack statements marked "agent's knowledge" below are not in REFERENCES and were not checked against .NET 11 documentation. Confirm them before relying on them.

## Findings recorded as open conflicts

These are contradictions between current documents, or between the prototype and a documented contract. They are listed as C03 and C04 in the [conflict table](../../README.md#history-and-unresolved-conflicts).

| ID | Prototype behaviour | Documented contract | Effect |
|---|---|---|---|
| C03 | The directory filters while the user types, applies the service filter and the Sort control without a submit, and appends cards with "Show more" (`index.html` lines 925–927 and the `input` handler at 2033). Cards show a per-user "Chatting" tag (line 896) | The directory is static server-rendered. Its state is "Query: search text, service filter, page", all in the address ([SDD](../../SDD.md#navigation-and-screen-state)). "Search and filter are a plain GET form, so they work without JavaScript" ([F01 plan](../../../specs/browse-accountants/plan.md)). DESIGN keeps the public pages "suitable for static rendering". The address contract has no sort value. The "Chatting" tag is in no PRD requirement or DESIGN state | Built as drawn, the directory needs either an interactive page with a server connection for every anonymous visitor, or JavaScript that the SDD's stack excludes. Built to the contract, search is a submitted form, paging is links and sort needs an address value. "Highest rated" (a D05 proposal) also needs an order over a computed average with a unique tie-breaker (BR02). The tag adds a per-user query to a public page |
| C04 | In-page failure messages appear while offline: "Not saved: no connection. Your changes are still here" in the editor (line 1211), kept review text (1868), kept report text, kept checklist, note and upload selections in the workspace. The directory and profile show their own offline states with a retry button (lines 908 and 945) | PRD BR18 and the R05, R13, R15 and R21–R23 workflow rows keep drafts and selections through failed or offline changes. The SDD restores only the chat draft from the tab's session storage and treats "state kept only in server memory" as disposable ([SDD](../../SDD.md#di-startup-async-work-and-lifecycle)). REFERENCES records that a user who cannot rejoin the circuit "receives a new circuit with newly initialized state" | On Interactive Server pages the form state lives on the server. While the connection is down the page cannot draw a per-form message, and Blazor shows its reconnecting notice instead (DESIGN's "Connection" row already says this). If the circuit is lost, every draft except chat text is gone. The PRD promise and the SDD mechanism disagree for the editor, review, report, checklist, note and upload selection. On the static directory and profile pages a failed load while offline is the browser's own error page, so the prototype's offline states with a retry button cannot be served |

## Findings that are not contradictions

Documented as proposals or open decisions already. They are recorded here because they add build cost or depend on something the stack does not yet have.

| Item | Where it is documented | Observation |
|---|---|---|
| Private service documents (R23) | PRD R23, BR17, D26–D28; SDD real upload contract; STATUS G14 | The largest dependency in release 1. No byte store is chosen (D28) and Supabase is database-only with 500 MB on the Free plan. The SDD already plans application-owned form and download endpoints and leaves the reselect/retry strategy to implementation, so this is an open dependency, not a contradiction. Agent's knowledge, unverified: Blazor's file input streams over the circuit with a default limit of about 500 KB per file, and Render's container disk is not persistent. The prototype's upload percentage needs browser script in a real upload. Recommendation: decide whether real file intake stays in release 1 before F03 implementation |
| Checklist: a checkbox and a four-value status per task, staged and saved together | DESIGN service workspace ("stage checkbox/status edits and explicitly save"); PRD BR16 (Proposed, D25); SDD optimistic concurrency | Two controls express one state, and the staged save is what requires draft retention, conflict handling and retry. Saving each change immediately with a single status control would remove most of that. "Waiting for client" has no client action and no notification (SDD: no service-progress emails) |
| Automatic activity entries in the update history | DESIGN "Notes/activity" | R22 asks for saved progress, shared notes, author and time. Activity entries written for every action are a presentation proposal beyond that |
| Three summary tiles on the workspace page | Not found in PRD R19–R23, DESIGN's service workspace table or the F03 spec scenarios | Prototype-only addition |
| Accountant-added services, decline flow | PRD D24, BR14 (prototype proposals) | Still undecided |
| "Highest rated" sort | prototypes README (D05 proposal) | See C03. Also ranks one five-star review above many reviews at a slightly lower average unless a rule is added |
| Years of experience and highlight count on cards | DESIGN card order and R12 state; D04 proposal | R01 requires name, summary and services. The rest is DESIGN presentation |
| Conversation starter suggestions | Not found in PRD or DESIGN | Small; prototype-only |
| "Checking whether it was sent" message state | PRD R06 workflow; DESIGN chat thread | Required by the PRD. On Interactive Server an unknown outcome usually ends in a reload from the database (SDD missed updates), so the state will rarely be visible |
| "Redirecting to Google…" screen with Cancel | DESIGN sign-in states | Real sign-in leaves the site at once. Cancel happens at the provider (SDD `/signin`) |
| Experience start and end month fields | PRD BR07 and D04 (if chosen); SDD `YYYY-MM` | Agent's knowledge, unverified: desktop Firefox and Safari do not provide a month picker for `type="month"`, so a fallback is needed for two of the five supported browsers |
| Dark theme | DESIGN accessibility testing (light, dark, system) | Not a PRD requirement. It doubles colour verification |
| Unread marks that update on every page | PRD R07 | The SDD notifier informs "open chat pages" only. A badge that updates live elsewhere needs wider subscriptions; a badge refreshed on page load does not |
| Hosting limits | SDD and REFERENCES | Each interactive page holds a circuit of about 250 KB at minimum, a restart drops them, Render's free service sleeps after 15 minutes and Supabase's Free plan pauses after a week. Brevo's free tier of about 300 emails a day is unconfirmed and is shared by message and report emails |
| Prototype-only variants | prototypes README, DESIGN-DIRECTIONS | Four earlier looks and four palette proposals remain beside the current look, and the checker runs 178 checks. Choosing a palette and retiring the unchosen variants would reduce upkeep |

Matches the documents with no finding: profile, portfolio highlights, reviews and replies, report and block, sign-in with role choice and agreement, account deletion, the information pages and email notifications (R03–R18).

## Corrections to the chat summary

The agent's first summary of this review, given in the session before this record was written, described the checklist's two controls with a staged save, the automatic activity entries, the highlight count and experience on cards, the redirecting screen and the dark theme as not asked for by the documents. On re-reading DESIGN for this record, each of those is in DESIGN as a presentation proposal. They are not PRD requirements. The table above states their documented source.

## Decisions this review asks for

Owner: lumabanls. None is decided.

1. C03: build the directory to the static contract (submitted search, page links), or change the SDD and F01 plan to an interactive directory.
2. C04: store more drafts in the browser, or narrow the PRD promise to chat text plus drafts that survive a reconnect.
3. Whether real file intake (R23) stays in release 1, given D28.
4. Whether the checklist becomes one status control with immediate save (D25).

## Restoration and limits

No files outside the documentation were changed, no browser or device settings were touched, and no dependencies were installed.

Documentation check after this record, the conflict rows and the STATUS entries were added: `uv run --no-project python tools/check_docs.py --ready` exited 0 with 47 Markdown files, 503 local links and 0 errors; 53 external URLs were not executed. `git diff --cached --check` exited 0 with the record staged.

Not exercised: any application behaviour, any browser, Blazor, upload, hosting or email behaviour. Historical evidence reused: source hashes for `index.html` and `taxbuddy.css` match the [palette options record](2026-11-12-palette-options.md). Acceptance decision: pending.
