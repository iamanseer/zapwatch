# TRD — ZapWatch

## 1. Stack decision
- **Backend:** ASP.NET Core MVC — the same app as Frontend, not a separate API project.
  Controllers handle the ping ingestion endpoint and the Razorpay webhook receiver alongside the
  dashboard pages. Matches Anseer's day-to-day .NET Core MVC/REST API work directly. No ramp-up
  cost.
- **Scheduler / background jobs:** Hangfire, backed by the same SQL Server database. Needs a
  reliable recurring "scan for overdue automations" job; Hangfire is the standard low-setup
  choice in the .NET ecosystem, avoids standing up a separate queue/infra service, and its
  dashboard doubles as a rough ops view for "is the watchdog itself actually running."
- **Frontend:** ASP.NET Core MVC + Razor Views, one dedicated JS file per frontend module (e.g.
  `automations.js`, `dashboard.js`) for anything that needs client-side interactivity — no SPA
  framework, no Blazor. Changed from the original Blazor Server pick at Anseer's request:
  familiarity is the explicit deciding factor — his day job already uses MVC, and plain
  request/response has no persistent SignalR connection to manage for ordinary page loads, which
  removes a category of "why did this reconnect" debugging surface for a solo builder shipping in
  short evening-blocks.
- **Real-time updates:** ASP.NET Core SignalR — added directly, independent of the MVC choice
  above. SignalR isn't a Blazor-only technology; it's the same transport Blazor Server uses
  internally, wired up here for a narrow purpose instead of full-page server rendering. One hub,
  `AutomationStatusHub`. Two emit points: the Hangfire watchdog job and the ping-ingestion
  controller action both call `IHubContext<AutomationStatusHub>` to broadcast a small
  "automation {id} status changed to {status}, last_seen: {timestamp}" event whenever an
  Automation flips ok↔overdue or receives a new ping — never a full page re-render. Frontend
  side: the official `@microsoft/signalr` JS client, loaded via CDN (no npm build step, matching
  the no-SPA-framework choice above), connects on dashboard load with `.withAutomaticReconnect()`,
  and on receiving an event just swaps that one automation's status badge and last-seen text in
  the DOM. Connections are scoped per authenticated user via SignalR groups keyed to
  `Context.UserIdentifier`, so one customer's browser only ever receives their own automations'
  events — this scoping is the one part of this feature worth real care; get it wrong and it's a
  privacy bug (one customer seeing another's automation status), not just a UX bug. Adds real
  scope over a plain-refresh dashboard — see the updated effort estimate in `MODULES.md`.
- **Database:** SQL Server, bundled directly with the hosting plan below — no separate Azure SQL
  serverless quota to track, no vCore-second budget that could get eaten by 24/7 watchdog
  polling. Comfortably handles this scale (low write volume — pings from a handful of customers'
  Zaps, not a high-frequency event stream).
- **Hosting:** MonsterASP.NET, Premium Single plan (~$1.95/month first year, $2.50/month
  renewal) — swapped in for Azure at Anseer's request. Purpose-built for exactly this stack:
  Windows/IIS, MSSQL 2025 bundled into the same plan, native ASP.NET Core support, one-click
  Visual Studio deploy — a tighter fit than a generic cloud host translating a Windows/SQL Server
  app onto Linux, and it sidesteps the free-tier "Always On requires a paid tier" wall that ruled
  out Azure and Render. Their own support team confirmed directly on their forum that Hangfire
  runs cleanly on their platform. Premium Single, not the Free plan: the Free plan's actual
  feature-comparison table (not the general marketing copy, which oversimplifies) shows no HTTPS,
  256MB RAM, one 1GB database, EU-only. No HTTPS specifically is disqualifying once login and the
  Razorpay payment flow exist — so this isn't really a departure from "free," it's the cheapest
  way to remove a real blocker. Still open: whether SignalR/WebSocket connections work on their
  platform — unconfirmed either way. See Module 1's day-0 smoke test in `MODULES.md`, which
  exists specifically to resolve this before any real feature code is written.
- **AI/LLM component:** None in v1.

## 2. Data model
Core entities only:
- **User** — account owner.
- **Automation** — belongs to User: name, expected_frequency_minutes, grace_period_minutes,
  ping_token, last_seen_at, status (ok / overdue / paused).
- **PingEvent** — belongs to Automation: received_at. Retain a rolling window (e.g. 30–90 days)
  to bound storage growth.
- **AlertEvent** — belongs to Automation: triggered_at, resolved_at, channel (email/sms). Needed
  so the watchdog fires exactly one alert per quiet episode instead of spamming on every
  scheduler tick, and doubles as the raw data for a future client digest.
- **Subscription** — belongs to User: plan, automation_limit, razorpay_subscription_id, status.

## 3. Key integrations
- **Payment provider:** Razorpay (subscriptions) — chosen for faster India-based solo-operator
  setup than Stripe.
- **SMS:** Twilio (or comparable transactional SMS API). Failure mode: if the SMS send fails or
  hits a rate limit, the alert must still attempt email rather than silently dropping — a
  monitoring tool that fails silently on its own alert path is the one failure mode that can't be
  tolerated at all.
- **Email:** Resend — permanent free tier covers v1's volume many times over (SendGrid's free
  plan is gone as of May 2025; it's a 60-day trial now, then $19.95/month). Same fallback logic
  in reverse; log — visibly, somewhere Anseer actually checks — if both channels fail on a given
  alert, since there's no one else watching this layer in v1.

## 4. Architecture sketch
A single ASP.NET Core MVC app — Controllers, Razor Views, and per-module JS, backend and frontend
together with no separate API project — backed by SQL Server holds Users, Automations,
PingEvents, AlertEvents, and Subscriptions. Each Automation gets a unique token embedded in a
public ping URL (`/ping/{token}`) — the target of a "Webhooks by Zapier" step the customer adds
to the end of their own Zap; a thin controller action updates that Automation's `last_seen_at` on
each hit. A Hangfire recurring job runs every few minutes, finds Automations where
`now − last_seen_at > expected_frequency + grace_period` with no already-open AlertEvent, fires
email + SMS, and opens an AlertEvent; the next successful ping closes it automatically. Both the
watchdog job and the ping controller action also push a small status-changed event through a
SignalR hub to that automation owner's connected browser, so the dashboard updates in place
without a refresh. A Razorpay webhook — another thin controller action in the same app — flips a
Subscription's status, which gates whether an account's automations are actively watched.

## 5. What's explicitly deferred to post-v1
Shopify/QuickBooks polling connectors (each needs OAuth plus a production app-review process
outside build-time control), WhatsApp alerts (Business API template approval, same issue),
Make/n8n-specific onboarding, team/multi-user accounts, admin tooling, volume-anomaly detection,
AI-generated digests, public status pages, horizontal scaling (not a real concern at this
customer count).

## 6. Testing approach for v1
Automated tests on: the watchdog's overdue-detection logic (gets the grace-period/timezone math
wrong in either direction and the product either cries wolf constantly or misses a real outage —
this is the core calculation logic) and the Razorpay webhook handler (payment path — wrongly
bills someone or fails to activate a paying account). Everything else — CRUD screens, ping
ingestion, alert message formatting — manual smoke-testing only.

## 7. Definition of done for v1
A real freelancer can sign up, add their client automations, wire up one Webhooks-by-Zapier step
per Zapier's own onboarding doc, pay via Razorpay, and receive an SMS + email within the expected
window the first time a monitored automation genuinely goes quiet — with zero manual intervention
from Anseer.
