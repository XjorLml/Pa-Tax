# F01 browse accountants: implementation evidence

## Identity and attribution

- Date: 2026-11-13. The final test run and browser check ended at about 07:35 UTC (15:35 Asia/Manila).
- Observer: agent-executed builds, tests and browser checks, plus agent review of screenshots. No owner acceptance is claimed.
- Instruction: the user wrote "start f01", approved the two ADR 010 proposals ("1. is approved") and chose the Paper & Forest palette ("for pallete go for paper and forest"), and asked for the changes to be recorded and left uncommitted ("i will do the commit after f01").
- Scope: [F01](../../../specs/browse-accountants/spec.md) FT01–FT07; PRD R01–R03, R10 (database failures), R11, BR01, BR02, D04 and D05 (provisional); [ADR 010](../../adr/010-single-project-per-page-render-modes.md); TEST-PLAN TP01 and TP05, part of TP06 and TP08.
- Source: `5ceee374da2f2622930fa4a1e5b69601852d22f3` plus uncommitted changes. The 62 files under `src/` and `tests/`, excluding `bin`, `obj`, `node_modules` and the generated `wwwroot/app.css`, have a combined fingerprint of `eae0d6ae96dfc344fcf50e3c628f206666217534dcddbf89bc15231a7090b9f4` (SHA-256 of the sorted `sha256sum` list). Selected files:
  - `Program.cs`: `5d17aeef600280145271597e7627bbd6c986f888a92dfcb96434ec039254d148`
  - `DirectoryPage.razor`: `84e7ac1f164b84bc95ba8caba565239d0a1335ed615e9c151ea57d66d89c46e0`
  - `AccountantProfilePage.razor`: `b07ab7f1a97a26a85b0e96972e7a30aff1f1e752a5e1a04a104dec945fb075f4`
  - `Styles/app.css`: `9a2bbd9d166d9de7960a2d02bf6a00b211db450a2d07802b0780c77f7a954794`
  - `package-lock.json`: `ff7413328aaf014c7d3fa0f6e95faea57f8519f3be008421c495b6a7da7f20a5`
  - Migration: `20261113063008_InitialDirectory`.
- Toolchain:
  - .NET SDK 11.0.100, pinned in `global.json` with roll-forward disabled.
  - EF Core 10.0.15, Npgsql EF Core provider 10.0.3, `dotnet-ef` 10.0.15 as a local tool.
  - xUnit 2.9.3, Microsoft.AspNetCore.Mvc.Testing 11.0.0.
  - Tailwind CSS 4.3.3 through npm 11.13.0 and Node 24.16.0 in WSL.
- Environment:
  - Builds, tests and the app ran in WSL2 Ubuntu 24.04.1 on Windows 10.0.26300.9974.
  - Browser checks used headless Edge `Edg/157.0.4341.46` on Windows, driven by Windows Node 22.12.0 against `http://localhost:5234`.
  - Database: the Supabase development project (`ap-southeast-1`, PostgreSQL 17.11) through the session pooler.
- Fixtures: synthetic profiles only, all addresses `@example.com`. The development database holds the seed: 8 listed accountants and 1 without consent. Tests use throwaway `test_…` schemas that are dropped afterwards.

## Execution

| Procedure / command | Expected | Actual observation | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | Clean build with warnings treated as errors | 0 warnings, 0 errors | Pass |
| `dotnet test TaxBuddy.slnx -c Release`, final code | All tests pass and none are skipped, so the data tests really ran against PostgreSQL | `TaxBuddy.Tests` 44 passed; `TaxBuddy.Web.Tests` 18 passed; 0 failed; 0 skipped | Pass, automated boundary |
| Earlier integration runs | All pass | First run: 10 of 18 failed. The tests compared encoded HTML (fixed by decoding). Two real defects were found: a page's own 503 page was replaced by the generic not-found page, and Blazor sent an empty body for a page-set 404. Fixes: switch off status-code pages for that response, and render "This profile is no longer available" from the not-found page for `/accountants/…` addresses, using `NavigationManager.NotFound()` | Fail, then fixed; kept as history |
| `dotnet ef database update`; `dotnet run … -- seed-development` (Development) | Schema created in `public`; seed applied | Migration `InitialDirectory` applied. "Seeded 8 listed and 1 unlisted synthetic profiles" | Pass |
| Database state after tests | No leftover test schemas; seed intact | 0 `test_` schemas; 9 rows in `public.accountant_profiles`. Supabase's automatic RLS had switched on row-level security for all 4 tables. The app connects as their owner `postgres`, and the directory reads normally | Pass |
| Browser check script ([script](2026-11-13-f01-browse-accountants/app-check.mjs.txt), [report](2026-11-13-f01-browse-accountants/app-check.json)): the directory, page 2, a profile and an unavailable profile, at 320, 390, 760 and 1440 px, in light and dark (32 views) | No sideways scrolling; no box sticking out of its container; buttons and links at least 44 px; theme backgrounds match DESIGN | First extended run, on the previous build: 4 views at 320 px had the hero text, search form and note sticking out of the hero panel. Fixed with `minmax(0, 1fr)` grid columns. Final run: 0 problems in 32 views. Backgrounds `rgb(247,247,242)` light and `rgb(17,28,24)` dark, matching `#F7F7F2` and `#111C18` | Pass after fix, Edge emulation only |
| Raw HTTP checks with `curl` against the running app | Profile 404 shows its own message; unknown address shows "Page not found" | `/accountants/not-a-guid` returns 404 with "This profile is no longer available"; `/no-such-page` returns 404 | Pass |
| `python3 tools/check_docs.py --ready`; `git diff --check` | Links and anchors resolve; no whitespace errors | Recorded in [STATUS](../STATUS.md) with this change | See STATUS |

What the tests cover:
- FT01: listed accountants appear in the first HTML response, in A–Z order, with title, description and canonical address, and without a sign-in prompt.
- FT02: the empty directory and "no match" messages differ.
- FT03: 16 profiles, 14 with the same name, appear once each across pages, in name then ID order. The same holds through the pages of the seed.
- FT04: the profile shows every published field, including both experience date ranges.
- FT05: unknown, unlisted and malformed IDs return 404 with "This profile is no longer available" and a way back.
- FT06: with the database unreachable, the directory and profile return 503 with "Try again", never an empty list.
- FT07, structure only: the form is a plain GET form; menu, Search and service buttons come in the right source order; two service values redirect to the one-value address; unknown or malformed values are ignored. Search text is literal (`%` and `_`), and BR01 holds for missing consent or missing services.
- Architecture (ADR 010): Domain and Application use no web, EF Core or Npgsql types, and no component injects the data layer.

Agent-reviewed screenshots:
- [Desktop, light](2026-11-13-f01-browse-accountants/directory-1440-light.png) and [dark](2026-11-13-f01-browse-accountants/directory-1440-dark.png): the hero, search with the Search button, service buttons, and two columns of cards.
- [320 px phone, light](2026-11-13-f01-browse-accountants/directory-320-light.png): the service menu, a full-width Search button, single-column cards and "Page 1 of 2" with Next.
- [Profile, desktop light](2026-11-13-f01-browse-accountants/profile-1440-light.png) and [390 px dark](2026-11-13-f01-browse-accountants/profile-390-dark.png): About, Services, Experience and the contact panel with "Message Andrea".

## Decisions made while implementing

Working values, recorded in the documents named:
- **Page size 6**, matching the reviewed prototype; D05 still leaves it open ([PRD](../../PRD.md#open-decisions)).
- **Field length limits** in `ProfileLimits`; D04 still leaves them open.
- **EF Core 10 and Npgsql 10**, because Npgsql's EF Core 11 provider was only at `11.0.0-rc.3` ([SETUP](../../SETUP.md#reproducibility-record)).
- **Tailwind 4.3.3** rather than 4.3.5, which was a week old.
- **Two service values in the address** (from a service button on a wide screen) redirect to the one-value address.

## Limits and restoration

Not exercised:
- **Browser matrix:** Chrome, Firefox, Brave and Safari, real phones, screen readers, true browser zoom and keyboard-only use (TP06), and the browser offline case (TP08). Only Edge was used, in emulated viewports.
- **Without JavaScript:** content was proven by the integration tests' script-less HTTP client, but a real browser submitting the form with scripts off was not tried.
- **Not needed in F01:** antiforgery (F01 has no forms that post data; the .NET 11 template no longer calls `UseAntiforgery()` explicitly, so check that at the first such form) and sign-in.
- **Deferred to deployment:** rate limiting on directory search and forwarded headers, because limiting by IP needs the real client address behind a proxy.

Acceptance decision: pending. The owner walkthrough and the browser matrix remain.

The app was stopped after the checks. The temporary browser profiles were deleted by the script. The Windows temporary folder `C:\Users\Dell\AppData\Local\Temp\taxbuddy-f01` holds the full set of screenshots and can be deleted. The development database keeps the synthetic seed, which `seed-development` resets.
