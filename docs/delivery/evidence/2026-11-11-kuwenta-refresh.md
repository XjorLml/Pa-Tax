# Kuwenta brand and prototype refresh evidence

## Identity and attribution

- Date: 2026-11-11. Final browser report timestamp: 21:05:57 Asia/Taipei (13:05:57 UTC).
- Observer: agent-executed automated checks and agent visual inspection. No owner acceptance is claimed.
- Scope: [F02 BD01–BD06](../../../specs/brand-design-refresh/spec.md#acceptance-scenarios); PRD D01 and R01–R18 presentation. [DESIGN](../../DESIGN.md) owns current visual choices.
- Baseline: `668a9a9e122787f2c86f6f6b4a63c398cc78d282`, initially clean; current uncommitted F02 changes. Earlier records remain historical and their passes were not reused.
- Exact exercised source: SHA-256 values for the prototype, stylesheet, all five SVGs and checker are in the retained [browser report](2026-11-11-kuwenta/results.json). Prototype `index.html`: `2a26c527ba12abd775cd4b46d48f5ffe4b4a5c39fbd8624a9cf33d3c1ff815dc`; `kuwenta.css`: `5ccd78e75871958d5a9ecff5b8846332b950f0220982aaefd8f3ca858b0998f7`.
- Environment: Windows `10.0.26300`, x64 as returned by Node; Node 24.16.0, Python 3.14.6; headless Edge reports `Edg/157.0.4341.42` through CDP. The older inventory lists a different Edge file version; this record uses the actual running browser response. OS edition was not established; the historical host edition is not transferred to this run.
- Fixtures: local `file:` prototype, fresh isolated Chromium profile, synthetic directory and message/review data, example contact addresses. No real sign-in, account, database, email or deployment.

## Execution

| Procedure / command | Expected | Actual observation | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"` | Existing simulated flows and revised layouts remain usable | Exit 0; 78 checks passed; exact results and source hashes retained | Pass, prototype boundary |
| Semantic contrast computed from actual browser tokens | Text at least 4.5:1; input edges/focus at least 3:1 | 34 pairs across light/dark themes passed; [light values](2026-11-11-kuwenta/contrast-light.json), [dark values](2026-11-11-kuwenta/contrast-dark.json). Lowest light text/control 4.76/3.01; dark 5.43/4.06 | Pass for measured pairs |
| Inline script extraction, `node --check`; `node --check tools/check_prototype.mjs` | No JavaScript syntax errors | Both commands exit 0 | Pass |
| Python standard-library XML parsing and SVG source inspection | All five SVG assets parse without font/text dependencies | Five parsed; every logo/wordmark uses vector paths; local images loaded in browser | Pass |
| Browser source-hash comparison and profile directory inspection | Retained report matches current source; isolated profile removed | Every source hash agrees; no `browser-profile-*` directory remains | Pass |
| `python tools/check_docs.py --ready`; `git diff --check` | Local documentation paths/anchors resolve, no unresolved customized templates or whitespace errors | Exit 0 for both; external URLs and application routes not executed | Pass |

The 78 checks cover directory paging, service filter, no-match/empty/error/retry, profile/removed-profile content, visitor draft through simulated sign-in and required agreement, send failure/retry without duplication, offline draft retention, email preference, review validation/posting and reply, block/unblock, unsaved profile guard, information/error pages, all five comparison directions, reduced motion, keyboard skip access and 44 px phone controls. Contrast pair groups are two of those 78 checks, not 34 additional browser assertions.

Layout checks used desktop mode at 1440 and 700 CSS px, phone mode at 390 and 320 CSS px, and light/dark themes. These are desktop browser viewport sizes, not phone devices. Horizontal overflow was checked at the document, frame and principal scrolling containers.

Agent-reviewed screenshots include desktop directory and cards, narrow desktop directory, phone directory at 320/390 px, phone profiles and chat, dark directory and desktop chat. Retained samples: [desktop introduction](2026-11-11-kuwenta/directory-desktop-light.png), [desktop cards](2026-11-11-kuwenta/directory-cards-desktop-light.png), [phone dark theme](2026-11-11-kuwenta/directory-phone-390-dark.png). Other captures and the extracted syntax-check script remain in ignored `artifacts/brand-refresh`.

## Failures and corrections

Initial restricted-process Edge launches reached CDP but timed out enabling the page. The same isolated local check succeeded with approved executor process permissions. This was a verification-environment limitation; no browser pass was claimed for the failed launches.

Two early checker assertions used unsuitable selectors/text (transformed heading case and the confirmation-dialog selector); the assertions were corrected to the existing DOM. A first contrast calculation found secondary text on the light illustration panel at 4.42:1; the muted color was darkened and the final pair is above 4.5:1. Screenshot review prompted a shorter hero, a taller desktop review frame and collapsed simulation controls on small screens. Final observations above apply to the corrected fingerprinted sources.

Current docs now use Kuwenta and matching planned project/configuration names. Historical ADRs, earlier evidence and prior name/design studies remain preserved. The root README's malformed UTF-16 trailing old-name heading was removed during the identity update. An old navigation-summary statement that the replacement host was open was reconciled with the already-decided Render stack; this does not choose a new host.

## Restoration and limits

The helper closed only its own Edge instance and removed its fresh profile; no real browser profile or device settings were changed. All review/message/profile mutations lived in the isolated in-memory prototype and disappeared on navigation/reset or browser shutdown. No dependencies were installed and no server was started.

Not exercised: other browsers; real phones; screen readers; a full keyboard walkthrough; true browser zoom; system-theme switching; real network loss or reload retention; production Tailwind/Blazor output; server authorization, persistence, OAuth, email, hosting or deployment. Name/domain availability and PRD D15 remain open. The application does not exist, so no .NET build or application tests were run. This evidence does not complete TP01–TP18 or release acceptance. Owner walkthrough and acceptance remain pending.
