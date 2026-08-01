# CLAUDE.md — ZapWatch

## What this project is
ZapWatch is a monitoring service for Zapier/Make automations that catches silent failures — the
case where a Zap looks "on" and healthy but has quietly stopped producing results, so orders,
leads, or invoices stop flowing with no visible error. It confirms real execution via a ping
added to the end of the automation, and alerts the operator by email + SMS the moment one goes
quiet past its expected window. v1 targets Zapier freelancers and micro-agencies monitoring
client automations, not end SMB owners directly.

## Source of truth
- Product scope: `PRD.md`
- Technical decisions: `TRD.md`
- Agent roles and handoff order: `AGENTS.md`
- Module breakdown, dependencies, and build order: `MODULES.md`
Do not build anything not traceable to these four files without flagging it as a scope change.
Work happens module by module — only the module currently kicked off (via a
`07-MODULE_WISE-PROMPT_GENERATOR.md` prompt) is in scope for a given session.

## Builder constraints (real, not aspirational)
- Built solo, in weekday 8:00–10:30 PM blocks and up to 3 hrs on Saturday. No task breakdown, PR
  size, or "next steps" list may assume multi-day continuous work.
- Prefer shipping a rough v1 slice over a polished unfinished one — EXCEPT on the watchdog
  overdue-detection logic and the Razorpay webhook handler. Those two get real tests per
  `TRD.md`, not just a rough pass, because they're the two ways this specific product can
  quietly betray a paying customer's trust.
- Defer anything in the TRD's "deferred to post-v1" list without being asked twice.
- Stack: ASP.NET Core MVC (single app — Controllers + Razor Views + per-module JS, backend and
  frontend together, no separate API project or SPA framework) + SignalR (live dashboard status
  updates) + SQL Server + Hangfire, hosted on MonsterASP.NET (Premium Single plan — bundles
  hosting and the SQL Server database in one provider). Razorpay for billing, Twilio for SMS
  during build/testing (switch to an India-focused provider like MSG91 once live — cheaper
  ongoing), Resend for email.

## Working agreement
- Follow the Planner → Engineer → QA → Reviewer sequence from `AGENTS.md` for any nontrivial
  feature. Skip the ceremony for a trivial one-line fix — use judgment.
- Every slice must map to a PRD line item. If it doesn't, stop and ask before building.
- Tests: full coverage on the watchdog logic and the Razorpay payment path; light
  smoke-testing everywhere else. Don't over-test v1.
- Commit messages reference the PRD feature or milestone they implement.

## Definition of done for v1
A real freelancer can sign up, add their client automations, wire up one Webhooks-by-Zapier step
per the onboarding doc, pay via Razorpay, and receive an SMS + email within the expected window
the first time a monitored automation genuinely goes quiet — with zero manual intervention from
Anseer.

## What NOT to do
- Don't build Shopify/QuickBooks polling, WhatsApp alerts, or Make/n8n onboarding in v1 — all
  explicitly deferred in `TRD.md`.
- Don't add team/multi-user accounts, an admin dashboard, or a public status page beyond what v1
  needs.
- Don't refactor working code mid-slice unless it blocks the current task.
- Don't silently swap Razorpay for Stripe, the MonsterASP.NET hosting choice, or MVC for
  Blazor/a SPA framework, without flagging it — each was picked for a specific, stated reason in
  `TRD.md`.
- Don't broadcast SignalR status updates to all connected clients — must be scoped per
  authenticated user (see `TRD.md`). Getting this wrong leaks one customer's automation status to
  another customer's browser.
