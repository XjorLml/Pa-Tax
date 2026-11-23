# F15: Microsoft sign-in (archived)

Governing requirement: [PRD](../../docs/PRD.md#requirements) R04, second half ("signs in with a Google or Microsoft account"), with BR01–BR03 and [ADR 006](../../docs/adr/006-web-sign-in.md). Technical contract: [SDD sign-in](../../docs/SDD.md#sign-in-sessions-and-authorization). Screen: the prototype's `signinHTML` and [DESIGN](../../docs/DESIGN.md). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

Status: **archived 2026-11-23.** Built that day, uncommitted, at the user's request "implement Service documents and Microsoft sign-in". Later the same day the user said: "archive the microsoft signin feature. google signin is enough but since it already develop just hide it". The code and its tests stay; the feature is off unless `Authentication:Microsoft:Enabled` is `true`, so stored keys alone show nothing. R04 is now Google only ([PRD](../../docs/PRD.md#requirements)), and the legal text keeps its Microsoft blocks off.

## Bringing it back

Only with an owner decision that changes R04 again. Then: a Microsoft Entra app registration for personal and work or school accounts, with the Web redirect URI `http://localhost:5234/signin-microsoft` (and the deployed site's); its client ID and secret value in user-secrets or the host's secret store; `Authentication:Microsoft:Enabled` set to `true`; the legal text's `[OPTIONAL — … Microsoft …]` blocks switched on, in docs/legal and the two pages, with a new legal version; and the SETUP, RELEASE, DESIGN and TEST-PLAN rows that this archive changed. Note the client secret's expiry date.

## Intent and scope

People with a Microsoft account (personal, or work or school) can sign in and register as they can with Google.

Existing behavior: Google only ([F04](../google-sign-in/spec.md)). New behavior: "Continue with Microsoft" under Google when Microsoft's client ID and secret are configured, through the same registration, session, return address, cancel and failure handling. The site names the providers it offers: "Google", "Microsoft" or "Google or Microsoft" on the sign-in page's failure line and footnote, the help page, "How it works", the profile's message hint and the delete page ("Your Microsoft account itself is not affected").

Exclusions: linking a Google and a Microsoft login to one account (each provider identity is its own account, as accounts are matched by provider and identifier, not email); a Microsoft-only tenant restriction; the legal text (its Microsoft blocks stay off while archived).

## Acceptance scenarios

MS01–MS06 run with the switch on in tests, so the archived code keeps working; MS00 checks that it stays hidden.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| MS00 / archive | Microsoft's keys are stored but the switch is off | Only "Continue with Google"; no "Microsoft" on the sign-in or help pages; a post for Microsoft is refused (400) | Web test |
| MS01 / R04 | Both providers configured; a visitor opens `/signin` | "Continue with Google", then "Continue with Microsoft", each a form with an antiforgery token and the return address | Web test |
| MS02 / R04 | The visitor chooses Microsoft | Redirect to `login.microsoftonline.com/common/oauth2/v2.0/authorize` with the client ID and `…/signin-microsoft` as the callback | Web test |
| MS03 / R04, BR02 | First Microsoft sign-in | The role step says "Signed in with Microsoft"; after registering, Account says "Client · Signed in with Microsoft"; the login is stored with provider `Microsoft` | Web test with the test sign-in |
| MS04 / R04 | A returning Microsoft sign-in | Straight back to the starting page, signed in | Web test |
| MS05 / R04 | Cancel at Microsoft | Back to the starting page, not signed in | Web test |
| MS06 / R04 | Only Microsoft configured; or neither | Only the Microsoft button and no "isn't set up" notice; with neither, the notice as before | Web test |
| MS07 / R04 | A real Microsoft account on the development site | The full round trip with Microsoft's pages | Dropped with the archive: **not run** |

## State, dependencies, and unresolved decisions

- Package `Microsoft.AspNetCore.Authentication.MicrosoftAccount` 11.0.0; settings `Authentication:Microsoft:ClientId` and `ClientSecret`.
- Microsoft client secrets expire; the owner notes the date and renews it ([RELEASE](../../docs/RELEASE.md)).
- The legal text doesn't describe Microsoft: version `2026-11-23.2` keeps its Microsoft blocks off.

## Evidence limits

Web tests stand in for Microsoft's pages, as they do for Google's. No real Microsoft sign-in has been made. [Evidence](../../docs/delivery/evidence/2026-11-23-f03-stage2-f15.md).
