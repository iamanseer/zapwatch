# MODULES.md — ZapWatch

v1 is small enough that it splits cleanly into two modules rather than an artificially longer
list — see effort estimates below; both land inside the 3–10 evening-block sizing rule.

## Module 1: Core Monitoring Engine
- **Purpose** — Lets an operator register automations and get reliably alerted the moment one
  goes silent. This is the product; everything else is packaging around it.
- **Core details**
  - Day-0 smoke test (do this before anything else below): deploy a bare-bones app to
    MonsterASP.NET Premium Single with a Hangfire recurring job and a SignalR hub, leave it
    untouched for 30–60 minutes, confirm the job still fires and a test SignalR connection still
    works with no page requests keeping it awake. Hangfire is confirmed working by MonsterASP.NET's
    own support team; SignalR/WebSocket support on their platform is not yet confirmed either way.
    This is the single biggest unknown in the whole hosting decision — resolve it in an hour before
    sinking evening-blocks into the rest.
  - Signup/login
  - Automation CRUD: name, expected frequency, grace period, generated ping URL
  - Ping ingestion endpoint (`/ping/{token}`), updates `last_seen_at`
  - Hangfire recurring watchdog job: scans for overdue automations
  - Email alert + SMS alert (Twilio)
  - AlertEvent dedup / auto-resolve logic (one alert per quiet episode, not one per tick)
  - SignalR hub (`AutomationStatusHub`) + JS client so the dashboard updates live on status
    change, instead of requiring a manual refresh (see `TRD.md`)
- **Depends on** — None. Starts first.
- **Blocks** — Module 2 (billing needs real automations/accounts to gate; the onboarding doc
  needs a working ping URL to document).
- **Bottlenecks** — None external. Twilio account setup is same-day, no approval wait for basic
  SMS sending (unlike WhatsApp, which is out of scope).
- **Risky factors** — The watchdog's overdue-detection logic is the one piece of business logic
  that's easy to get subtly wrong: off-by-one on grace periods, timezone handling, or sloppy
  dedup causing duplicate-alert spam. This is the TRD's flagged core calculation logic — needs
  real tests, not just a manual pass. Second risk, new with SignalR: connections must be scoped
  per authenticated user — an ungrouped broadcast leaks one customer's automation status to
  another customer's browser. Worth a specific test, not just a visual check that updates arrive.
  Third risk: if the day-0 smoke test shows SignalR doesn't work cleanly on MonsterASP.NET,
  Module 1's effort estimate below is void and this needs replanning before continuing.
- **Effort estimate** — 7–9 evening-blocks (was 6–7; +1.5–2 for the SignalR hub, its two emit
  points, and a reconnect-aware JS client with per-user scoping).
- **Done condition** — A test automation that stops pinging past its window triggers exactly one
  email + SMS alert, returns to "ok" on the next successful ping with no duplicate alerts firing
  while still overdue, and the dashboard reflects each status change live without a refresh —
  visible only to that automation's own owner.
- **Build order priority** — Now.

## Module 2: Monetization & Launch
- **Purpose** — Turns the working engine into something a stranger can pay for and set up without
  Anseer's help.
- **Core details**
  - Razorpay subscription integration + webhook to gate account status
  - Landing page
  - Zapier "add a webhook step" onboarding doc with screenshots
  - End-to-end deploy to MonsterASP.NET
- **Depends on** — Module 1 (needs real automations to bill against and document).
- **Blocks** — Nothing downstream; last v1 module.
- **Bottlenecks** — Razorpay's KYC/business verification for a live (non-test-mode) account can
  take a few business days. Start that specific step early, in parallel with Module 1 — it's pure
  third-party waiting time, not build time, so no reason to let it sit on the critical path.
- **Risky factors** — The payment webhook handler is the other TRD-flagged path needing real
  tests, not just smoke-testing: a bug here either bills someone wrongly or fails to activate a
  paying account.
- **Effort estimate** — 2–3 evening-blocks + 2 Saturday-blocks.
- **Done condition** — A real payment via Razorpay activates monitoring on an account, and the
  onboarding doc alone (no direct help from Anseer) is enough for a test user to correctly wire
  up their first ping.
- **Build order priority** — Now (Razorpay KYC step, parallel to Module 1) / Next (everything
  else, once Module 1 lands).

## Module dependency map
`M1 (Core Monitoring Engine) → M2 (Monetization & Launch)`

## Suggested build order
The day-0 smoke test comes before anything else, full stop — it's an hour of work that either
confirms the hosting decision or forces a replan while the cost of being wrong is still small.
After that, M1 — it's the entire risk surface of v1 (the watchdog logic is the one piece of "this
needs to actually be correct" business logic in the whole product) and the thing worth failing
fast on if the evening-block pace can't sustain it. Start Razorpay's KYC/business verification in
parallel with M1 so it isn't sitting on the critical path when M2 starts.
