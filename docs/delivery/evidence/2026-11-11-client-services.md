# Client services and progress prototype evidence

## Identity and attribution

- Date: 2026-11-11. Final browser report: 22:59:14 Asia/Taipei, 14:59:14 UTC.
- Observer: agent-executed browser/static checks and agent visual inspection. Owner review/release acceptance is not inferred.
- Scope: [F03 CS01–CS10](../../../specs/client-services/spec.md#acceptance-scenarios), PRD R19–R23, BR14–BR18; [prototype procedure](../TEST-PLAN.md#prototype-design-checks). Application procedures TP19–TP22 are defined, not executed.
- Authorization: user requested documentation/prototype execution, then selected private saving followed by service request in the clarification describing accountant acceptance before a shared relationship. Other D24–D28 policies remain open.
- Baseline: HEAD `668a9a9e122787f2c86f6f6b4a63c398cc78d282` plus pre-existing uncommitted F02 work. F02 files/assets and its historical execution reports were preserved. New evidence does not rewrite those passes.
- Exact exercised sources: retained [report with SHA-256 values](2026-11-11-client-services/results.json), covering HTML, CSS, external workspace module, checker and all SVG assets. `index.html`: `5349ac0a65b1855dcb8af5479deddbf3d4ac3fa113e12c23e7d7818b723d543c`; `service-workspace.js`: `8b8f0968db0b624c3327b2b8de79292f8eecf0ba18c9727652332a72f08e9b58`; CSS: `47be5f08c575b2d6fb9666dc554365ae43c85d8a30f01379795d11d35af94f84`.
- Environment: Windows `10.0.26300` x64 as reported by Node; Node 24.16.0, Python 3.14.6; headless Edge CDP reports `Edg/157.0.4341.42`. No other browser or .NET toolchain inventory is transferred to this run.
- Fixtures: local `file:` prototype and isolated browser profile; synthetic Maria/Andrea/other persona data, engagements `e1`–`e4` and generated PDF/CSV/TXT samples. No real user file, sign-in, database, email or storage service.

## Execution

| Procedure / command | Expected | Actual observation | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe" artifacts/client-services` | Existing flows preserved and new private-service workflows usable | Exit 0; all 142 checks passed, including the 78 existing checks rerun against this artifact | Pass at prototype boundary |
| Computed semantic color pairs in browser | 4.5:1 text and 3:1 control/focus minimums | All 34 pairs passed; [light](2026-11-11-client-services/contrast-light.json), [dark](2026-11-11-client-services/contrast-dark.json). Two grouped contrast assertions are included in the 142 total | Pass for measured pairs |
| Extract inline script, then `node --check`; check external module and browser checker similarly | No syntax errors | All three scripts exit 0 | Pass |
| PRD identifier/table validation with Python standard library | Stable identifiers unique, table columns consistent | R01–R23, BR01–BR18 and D01–D28 each appear once in their tables; affected authority tables structurally consistent | Pass |
| `python tools/check_docs.py --ready`; `git diff --check` | Local links/anchors, customization and whitespace valid | Exit 0 for both. External URLs and application routes are not executed | Pass |
| Compare retained source hashes and inspect isolated profile directory | Report matches current source; profile removed | All source hashes agree; no `browser-profile-*` directory remains | Pass |

New checks exercised:

- Accountant's scoped client list, client detail and accountant-assigned services; repeated assignment reuses the demo workspace.
- Private added-accountant list, adding/removing entries, no duplicate Add control, preservation of existing work, loading/empty/error/retry states.
- Client request validation, repeated-request deduplication, accountant acceptance with a checklist, declined versus active work, and client read-only views.
- Failed/offline progress saves retain staged values without changing saved totals; task addition changes the denominator; checkbox completion/reopening works; failed notes retain text; saved notes include author/time and appear to the client.
- Oversize/unsupported sample rejection, visible upload progress with repeated submit disabled, failure/retry retaining sample/task association, one file after retry, and explicit Available state.
- Counterpart may download but has no uploader-removal action. Generated PDF Blob header/type/content are verified before the anchor handoff; OS download completion is not tested.
- Connection loss during upload, actor change preventing a late insertion, interrupted uploader retry state, own-file confirmation/cancel/removal, visitor and unrelated-account guards without service/file metadata disclosure.
- New workspace/service layouts at 1440, 700, 390 and 320 CSS px in recorded light/dark modes; phone form/button/checkbox targets measured at least 44 px. These are desktop browser viewport checks, not real phone tests.

## Visual observations and corrections

Agent-reviewed captures include desktop accountant clients and service editing, the mobile client accountant list, the 320 px accountant checklist, the narrow dark client progress view, and service-upload failure/retry. Selected retained samples: [clients](2026-11-11-client-services/clients-desktop-light.png), [accountant progress](2026-11-11-client-services/service-accountant-desktop-light.png), [client phone list](2026-11-11-client-services/work-phone-390-dark-client-list.png), [320 px checklist](2026-11-11-client-services/work-phone-320-light-accountant-service.png), [upload failure](2026-11-11-client-services/service-upload-failure.png).

Earlier iterations: the original 78 checks passed after integration. An expanded checker run reached the generated-download assertion but its expression used top-level `await` without an async wrapper; that harness expression was corrected and the expanded runs passed. Visual inspection found duplicated same-day activity times; formatting was corrected. A temporary static identifier check initially expected unpadded IDs; it was corrected to the repository's R01/BR01/D01 format and passed. These were not passing results for the final artifact until rerun. After owner clarification, prototype labels were updated from proposed to confirmed for the save/request/accept model and the final fingerprinted 142-check run passed.

## Restoration, limits and remaining acceptance

The helper closed only its own Edge instance and removed its isolated profile. No real browser profile/device settings changed. Service requests, task/note mutations and sample attachments existed only in the loaded page and were discarded on reload/browser close. Download handoff was intercepted only inside the test page to inspect generated bytes; no actual personal document was selected, read, transmitted or retained. Other captures and extracted scripts remain in ignored `artifacts/client-services`.

Not exercised: production authentication/authorization, real uploads/download handoff, private byte storage/scanning, persistence/migrations, actual network/reconnect behavior, concurrent server writes, account/relationship/file retention, real phones, other browsers, full keyboard traversal, screen readers or browser zoom. Real file intake remains blocked by unresolved D26–D28 and implementation, not by this prototype's successful simulation. No app projects exist, so no application build, application tests, deployment or release acceptance is claimed. TP01–TP22 application gates remain pending; owner prototype review and remaining policy decisions are separate.

## Staging hygiene

After browser verification, staging exposed a single trailing space in `prototypes/service-workspace.js` at line 99. That space was removed without changing executable JavaScript. The retained browser report preserves the exact bytes exercised above; the script after this cosmetic edit has SHA-256 `9173998e31df386e0bc5c3288e9d4191ab8eea91a7c7522cb8d4781ac63734d8`. `node --check prototypes/service-workspace.js` passed after cleanup. Browser checks were not rerun for the whitespace-only edit.
