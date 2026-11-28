# Development standards

**Adopted baseline (2026-11-11) for a Blazor Web App.** It replaces the MAUI baseline inherited from the starter kit. No source exists yet, so none of these practices has been exercised in this repository. [SDD](SDD.md) owns actual project contracts. Each exception should have a reason and an owner.

## Essential practices

| Area | Standard and purpose | Review / verification |
|---|---|---|
| Scope and architecture | Link work to stable requirement/scenario IDs. Keep rules in the Domain and Application folders, away from pages; those folders use no Razor, HTTP or EF Core types ([SDD](SDD.md#architecture-and-dependencies)) | Review changed source against PRD/SDD; record contradictions. `LayerRulesTests` checks folder dependencies on every test run; review covers method bodies |
| Components | Pages render state and forward user actions to Application services. No database access or business rules in `.razor` files | Tests cover invalid input, state changes and saved effects through the services |
| Render modes | Public pages stay static. Add interactivity per page, only where the [SDD render-mode table](SDD.md#render-modes) lists it; never app-wide. Signed-in actions on public pages are named forms, not interactive components. Interactive pages prerender by default, so start-up code must be safe to run twice ([prerendered start-up](#loading-and-busy-feedback)) | Check each new page's mode in review; test that loading a page does not repeat a write |
| Authorization | Enforce every rule in Application services on the server. Hiding a link or button is not a check | Integration tests call each protected operation as the wrong user and as a visitor |
| Forms | Antiforgery on every form. Validate on the server; client checks are for convenience only | Integration tests post invalid and forged input |
| DI and lifetimes | Constructor dependencies, registered in `Program.cs`. A scoped service on an interactive page lives as long as the connection, so it holds no per-request state | Review singleton and scoped dependencies; document lifetimes in SDD |
| Database access | One short-lived context per operation, from the context factory. Parameterized queries, explicit ordering | Data tests against the real provider, including equal-sort-key cases |
| Async and events | Return and await tasks; no blocking waits. Disable a button while its operation runs so repeated clicks can't run it twice ([how, and static forms](#loading-and-busy-feedback)) | Controlled failure, repeated click and late-result tests |
| Lifecycle | Components cancel work and unsubscribe when disposed. State kept only in server memory can vanish on reconnect; durable state goes to the database, the address, or browser storage | Lost-connection and reload checks (TP03, TP15) |
| Navigation | Validate route and query values. Return addresses after sign-in must be local | Tests with malformed IDs and external return addresses |
| Schema changes | EF Core migrations in source control; never edit an applied migration | Upgrade tests from the previous schema (TP04) |
| Error recovery | Separate loading, empty, error and "not available" states; keep the user's input. How each looks: [DESIGN](DESIGN.md#loading-and-busy-feedback) | Controlled read, write and start-up failures |
| Diagnostics | Log event IDs, categories, duration and correlation IDs. Never log message text, email addresses, contact details or tokens | Review each logged field; test failure paths |
| Security | HTTPS and HSTS, secure cookies, rate limits, capped input sizes, secrets outside the repository | Inspect production configuration (TP12) |
| UI and locale | Follow DESIGN tokens, semantic HTML, labels and focus order. Format times through the one Asia/Manila helper | Browser accessibility and layout checks (TP06) |
| Verification | Combine unit tests, data tests on the real provider, integration tests and browser checks | Evidence includes revision, environment, date, attribution, result |
| Documentation | Update the owning contract and link it from specs; preserve attributed history | Link/anchor checks and final diff review |

The official guidance behind the render-mode, context-factory, authorization and repeated-event rows is in [REFERENCES](REFERENCES.md#implementation-and-hosting).

## Loading and busy feedback

Proposed, 2026-11-25. [DESIGN](DESIGN.md#loading-and-busy-feedback) owns what people see; this table is how pages are built to it and what review checks. Its choices, [PRD D29–D34](PRD.md#open-decisions), were decided the same day. The API names were checked on 2026-11-25 against the .NET 11.0.0 reference assemblies, the `blazor.web.js` that SDK 11.0.100 ships, and the documentation's source ([REFERENCES](REFERENCES.md#implementation-and-hosting)).

| Area | Standard and purpose | Review / verification |
|---|---|---|
| Who owns a loading state | The component that sends a read owns its state, in its own field. Never one flag for several lists, and no app-wide "request in flight" flag. *Why:* in the other app, one flag covered three lists, so one list's answer ended another's loading | Review the state fields of each list |
| The newest request decides | Number each read, or give it a `CancellationTokenSource` and cancel the one before it. An older answer that arrives late is ignored | A test where the older of two reads answers last |
| Ending a wait | Busy and loading end when the call settles, success or failure, in a `finally` block; never on a timer. Add no delay before a request starts. The only timers are DESIGN's display delay and minimum (D29), in one shared helper. *Why:* 17 save buttons came back after a fixed second, and a slow save plus a second click wrote the record twice | Search for `Task.Delay` and timers near calls; after a controlled failure the control is usable again |
| Several writes | Report success only after every write has settled: `foreach` with `await`, or `Task.WhenAll`. Never `list.ForEach(async x => …)`: its lambda becomes `async void`, so nothing awaits it and its exceptions escape. *Why:* a multi-file upload said it was done before its writes had finished | Review; a test where one of several writes fails reports no success |
| Double submit, interactive components | A handler checks a `_busy` field and sets it before its first `await`, then starts the call; a second click finds it set and returns. Blazor renders when the handler first yields, so the busy button shows at once | A repeated-click test with the answer held back (TP28; TP02 owns the no-duplicate result) |
| Double submit, static forms | A plain form post has no component state, so a double click sends two posts. The server makes every repeat change nothing, with no script (D31): a key made when the form is drawn, as the upload's form key (F03) and the message ID (BR04); the record's version, as the profile (F05); or a unique index. "Add task" and the shared note on a service page had no such guard until 2026-11-25; since then the history line they write carries the form's key. A repeat must also show the form's usual result, not an error: on the service page, Accept, Decline, the checklist and uploads do since the same day ([SDD](SDD.md#private-service-workspaces)) | An integration test per static form that posts it twice and checks both the data and the answer: written for the service page's forms (F03 CS18–CS23), not yet for the other static forms |
| Prerendered start-up | An interactive component prerenders, so `OnInitializedAsync` runs twice: on the server, and again when the circuit connects. Unless the first result is carried over, the page is drawn again and its data is read twice. Carry it with `[PersistentState]` on a public property, as `ChatPanes` does (F07), or with `PersistentComponentState`. Otherwise turn prerendering off for that component (`new InteractiveServerRenderMode(prerender: false)`) and give it a skeleton, and record the choice in the [SDD table](SDD.md#render-modes). Prerendering waits until every component's start-up has finished, so a prerendered page already holds its data and needs no skeleton | The frame test and a count of reads in the development log (TP28) |
| Before the page is interactive, and while it reconnects | A prerendered button does nothing until the circuit connects, and a click in that gap is lost. Draw such controls disabled until `RendererInfo.IsInteractive` is true, or until the page's script has started, as the chat composer's Send is. While the connection notice shows, controls that write are disabled again; the notice's script hears Blazor's `components-reconnect-state-changed` event | A click before the page is interactive, on Slow 4G; TP03 while reconnecting (TP28) |
| Script on static pages | Runs from the existing JavaScript initializer (`TaxBuddy.Web.lib.module.js`, SDD Account row), with `blazor.addEventListener` for `enhancednavigationstart`, `enhancednavigationend` and `enhancedload`. D33's page-move bar lives here: `enhancednavigationstart` starts its 300 ms delay and `enhancednavigationend` ends it, keeping it at least 400 ms once shown. No page streams ([SDD](SDD.md#render-modes), D32) | Review; TP28 |
| Live connections | Moving between chat pages must not lose messages: the new panel subscribes and reads from the database (SDD missed updates). The .NET 11 script closes the circuit a short time after the last interactive component leaves the page. Its option is `ssr.circuitInactivityTimeoutMs`, 2 s in the shipped script; the documentation read doesn't describe it, so treat it as unconfirmed | TP24's live updates while moving between the list and a thread (TP28) |

## CI baseline

No CI exists yet, and there is no remote repository. When one is added:

1. On proposed merges and pushes to maintained branches, run both test projects and build the web project with SETUP's pinned SDK.
2. Check nullable and compiler warnings; document justified exceptions rather than hiding all warnings.
3. Run the documentation checker for documentation changes.
4. Keep deployment jobs separate from ordinary contributions, with minimal permissions and protected secrets.

Record an actual successful remote run before claiming CI works. Choose runner and action versions at setup time and maintain them.

## Optional additions and triggers

| Addition | Add when | Tradeoff |
|---|---|---|
| Browser automation tests | Repeated regressions in browser-only behavior | Test infrastructure and selectors need ownership |
| Component test library | Interactive components gain logic worth testing in isolation | Another dependency to keep current |
| Telemetry and error reporting | There is a defined support need and privacy policy | Useful diagnosis; data retention and operations costs |
| Separate API project | A native app or third-party client is approved | Versioning and compatibility contracts become mandatory |
| Caching and paging changes | Measured data scale or latency exceeds an approved target | Complexity and invalidation; measure first |

Introduce interfaces for meaningful substitution (email sending, the clock) or for tests. Avoid creating an interface for every class. Keep human-readable labels separate from enum and ID logic.

## Change completion

Review policy (Proposed, solo developer lumabanls). Before merging to `main`:
- review the full diff yourself;
- run the documentation checker for documentation changes;
- run the relevant test projects and builds for code changes, as defined in [TEST-PLAN](delivery/TEST-PLAN.md).

Branches and pull requests are optional while there is no remote. Revisit this policy when a second contributor or CI is added.

A completed change has its authorized behavior implemented, appropriate observed verification, updated contracts/evidence, and explicit remaining acceptance. Documentation-only changes require documentation checks, not application test runs.
