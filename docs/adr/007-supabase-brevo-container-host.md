# ADR 007: Supabase database, Brevo email, and a host other than Vercel

Status: Accepted. The owner approved the proposed parts on 2026-11-11 with the stack table in the [SDD](../SDD.md#technology-stack) ("stack approved for choices column"): Supabase is used as a PostgreSQL database only, and the host is Render. Render's paid price had not been confirmed at that time. Decision date: 2026-11-11. Decision owner/evidence: lumabanls, message of 2026-11-11 ("database is supabase, hosting is vercel, email service: recommend between brevo and SendGrid") and answers the same day ("Keep Blazor, change host", "Brevo").

## Context

[ADR 005](005-web-platform-blazor.md) made AccApp one Blazor Web App whose signed-in pages hold a long-lived server connection, with an in-process chat notifier and a background email worker ([SDD](../SDD.md#live-chat-and-email-notifications)). That design needs a host that keeps the application running.

The owner then chose Supabase for the database and Vercel for hosting, and asked for a recommendation between Brevo and SendGrid. Facts read on 2026-11-11 ([REFERENCES](../REFERENCES.md#hosting-database-and-email-services)):
- Vercel's official runtimes do not include .NET. Its Container Runtime is in beta, stops an app after five minutes without traffic, and closes WebSocket connections when a function reaches its time limit (300 seconds on the Hobby plan).
- Supabase offers managed PostgreSQL with a Singapore region. Its Free plan pauses a project after a week without activity.
- SendGrid's and Brevo's own pricing pages could not be read. Third-party pages agree that Brevo has an ongoing free tier and SendGrid has only a trial.

This was recorded as conflict C02 in the [docs index](../README.md#history-and-unresolved-conflicts).

## Decision

1. The database is Supabase's managed PostgreSQL.
2. The email service is Brevo.
3. The Blazor design of ADR 005 is kept, so Vercel is not used. The application runs on a host that keeps it running and allows long-lived WebSockets.
4. Proposed: Supabase is used as a database only, through EF Core and Npgsql. Sign-in stays in the application ([ADR 006](006-web-sign-in.md)), and Supabase's own sign-in, live-update and web API features are not used.
5. Proposed: the host is chosen from the shortlist below, with Render in Singapore as the first choice.

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Keep Blazor, change host (chosen) | The design, the prototype and the documents stay valid | The host costs money once testers rely on it |
| Keep Vercel, rebuild as a Blazor WebAssembly static site with Supabase as the whole back end | No server to run; Vercel hosts static sites well | A large redesign: access rules move into database policies, public pages load slower, and email sending needs non-C# code. Rejected by the owner |
| Keep Vercel, run the app in its beta container | Keeps both choices on paper | Chat pages would disconnect at the time limit, idle apps stop, and Vercel publishes no .NET guidance. Rejected by the owner |
| Brevo (chosen) | An ongoing free tier that includes sending through its API, enough for internal testing | Daily cap on the free tier; one review rated its delivery below other services |
| SendGrid | Widely used | No permanent free plan; a paid plan is needed after the trial |
| Use Supabase for sign-in and live updates too | Less application code | Ties the access rules to Supabase, and replaces ADR 006's proposal. Not chosen for now |

Host shortlist. Every row has a Singapore region, which is where the Supabase project would be.

| Host | What was confirmed | Not confirmed |
|---|---|---|
| Render (proposed first choice) | Runs Docker images. "Render does not enforce a maximum duration for WebSocket connections." A free web service exists, but it stops after 15 minutes without traffic, takes about a minute to wake, and is not meant for production | The price and memory of its paid always-on service |
| Fly.io | A 512 MB machine running all month costs $3.69, and monthly bills under $5 are waived. No free tier | Its behavior for long-lived connections and automatic stopping |
| Railway | $5 a month plus usage, billed per second | Docker and WebSocket support, on the pages read |
| Azure App Service | Runs .NET; its free tier allows 60 CPU minutes a day and is not for production | The price of its Basic tier, and when it supports .NET 11 |

Render is proposed first because it is the only one whose own documentation confirms both Docker deployment and unlimited WebSocket duration, and its free service allows a first deployment at no cost. Fly.io is the cheapest confirmed always-on option.

## Consequences and verification

- A development Supabase project is needed before the first feature, and a separate one for the test site ([SETUP](../SETUP.md#first-checkout)).
- The application connects with the direct connection or the session pooler, never the transaction pooler ([SDD](../SDD.md#data-and-persistence-contracts)).
- Supabase's web API must be switched off or limited, and row-level security enabled on every table, because the application is the only intended way in. Confirm this when the project is created.
- A paused Supabase project or a sleeping free host takes the test site down until it wakes. Move to paid plans before testers depend on the site.
- Brevo needs an account, a sender address and a verified sender domain before R08 work (PRD D18). Confirm its current free limits on its own pricing page first.
- The host decision stays open as PRD D11. Before choosing, confirm the unconfirmed items in the shortlist and that the host can run a .NET 11 container.
- Accept the proposed parts when the first deployment passes TP07 and TP15.

## Provenance and supersession

Original decision, recorded the day it was made. Supersedes nothing. It resolves conflict C02.
