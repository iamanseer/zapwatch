# ZapWatch 🔔

A monitoring service that catches silent failures in Zapier/Make automations before your clients notice.

## The Problem

Zapier/Make automations handling revenue-critical work (orders, leads, invoices) can silently stop firing—no error, no visible failure. Your clients usually discover this days later when they notice missing data. There's no scalable way to catch this before it becomes a problem.

## The Solution

ZapWatch monitors your client automations with a simple heartbeat mechanism:

1. **Register** your client automations with expected frequency and grace period
2. **Add one webhook step** to each Zap (Webhooks by Zapier → POST to a unique ping URL)
3. **Get instant alerts** by email + SMS the moment an automation goes quiet past its expected window
4. **Auto-recover** — the next successful ping automatically clears the alert

## Target User

Independent Zapier/Make automation freelancers and 1–3 person automation agencies who manage 5+ client Zaps and need scalable silent-failure detection.

## v1 Features

- ✅ Signup/login, plus sign-in with Google, Microsoft, or GitHub
- ✅ Email verification, forgot/change password (with "set a password" for OAuth-only accounts)
- ✅ Automation CRUD (name, expected frequency, grace period, unique ping URL)
- ✅ Ping ingestion endpoint
- ✅ Hangfire-based watchdog scheduler
- ✅ Email alerts via Resend
- ✅ SMS alerts via Twilio
- ✅ Razorpay subscription billing (tiered by automation count), with an active-subscription view
- ✅ Live-updating dashboard via SignalR
- ✅ Zapier onboarding documentation
- ✅ Per-automation grace periods & quiet hours
- ✅ Marketing site: landing page, pricing, how-it-works, About Us, Privacy Policy
- ✅ SEO baseline: per-page meta tags, OG/Twitter, JSON-LD, robots.txt, sitemap.xml

See `MODULES.md`'s Module 3 for the full pre-launch-polish build-out. Both follow-up slices are
code-complete: the automations-list readability pass shipped, and Microsoft/GitHub sign-in only
needs Anseer to register real OAuth app credentials before those two providers' buttons appear
(their absence is inert, not a bug — see the incident writeup in `MODULES.md`).

## Explicitly Out of Scope for v1

- Polling-based monitoring (Shopify/QuickBooks direct API checks)
- WhatsApp alerts
- Make/n8n-specific onboarding (Zapier only)
- Team/multi-user accounts
- Public status page
- Volume-anomaly detection
- AI-generated summaries
- Admin tooling

## Tech Stack

- **Backend:** ASP.NET Core MVC (single app, no separate API)
- **Frontend:** Razor Views + per-module vanilla JavaScript
- **Real-time updates:** SignalR
- **Database:** SQL Server
- **Background jobs:** Hangfire
- **Hosting:** MonsterASP.NET Premium Single
- **Payments:** Razorpay
- **SMS:** Twilio (switch to MSG91 post-launch)
- **Email:** Resend

## Project Structure

- **PRD.md** — Product requirements, user stories, feature list, monetization, timeline
- **TRD.md** — Technical decisions, stack rationale, data model, key integrations
- **DESIGN.md** — Frontend design system: theming, tokens, reusable components
- **MODULES.md** — Module breakdown, dependencies, effort estimates, build order
- **CLAUDE.md** — Builder constraints, working agreement, definition of done
- **AGENTS.md** — Build team roles (Planner, Engineer, QA, Reviewer) and handoff protocol

## Development Timeline

| Milestone | Effort | Target |
|-----------|--------|--------|
| Core monitoring engine working end-to-end | 7–9 evening-blocks | Week 1–2 |
| Billing + onboarding doc + deploy | 2–3 evening-blocks + 2 Saturday-blocks | Week 2–3 |
| Pre-launch polish (restyle, social sign-in, auth completeness, SEO) | ~15–19 evening-blocks | Post-launch-readiness |
| First paying customer | — | Week 3–4 |

**Build constraints:** Solo builder, weekday 8:00–10:30 PM + up to 3 hrs Saturday. No multi-day continuous work.

## Success Metrics

- **v1 goal:** 3 paying accounts within 30 days of launch

## Core User Flow

1. Sign up with email
2. Add an automation (name, expected frequency, grace period)
3. Copy the unique ping URL
4. Add a "Webhooks by Zapier" POST step to your Zap pointing at that URL
5. Dashboard confirms first ping arrived
6. Repeat for each client automation (up to plan limit)
7. Get instant email + SMS if automation goes quiet
8. Fix it — next ping auto-clears the alert

## Future Considerations (Post-v1)

- Auto-suggested expected-frequency defaults learned from ping history
- Volume-anomaly detection (catches filter-gone-wrong scenarios)
- Auto-written client health digests from run data
- Shopify/QuickBooks polling connectors
- WhatsApp alerts
- Make/n8n onboarding

## Getting Started

See `MODULES.md` for the build order and dependencies. Start with Module 1 (Core Monitoring Engine) — it's the entire product risk surface.

**Day-0 smoke test required:** Deploy bare-bones app to MonsterASP.NET with Hangfire + SignalR to confirm both work on the hosting platform before sinking evening-blocks into feature development.

## Risk Surface

- **Watchdog overdue-detection logic** — Wrong grace period/timezone math either cries wolf constantly or misses real outages. **Needs real tests.**
- **Razorpay webhook handler** — A bug here either bills someone wrongly or fails to activate paying accounts. **Needs real tests.**
- **SignalR scoping** — Unscoped broadcasts leak one customer's automation status to another's browser. Scoping must be per authenticated user.
- **Differentiation** — Generic ping tools (Healthchecks.io, Cronitor) exist and are free/cheap. Differentiation comes from Zapier-native onboarding and client-facing angle, not the ping detection itself.

## Testing Approach

- **Full coverage:** Watchdog overdue-detection logic, Razorpay webhook handler
- **Light smoke-testing:** Everything else (CRUD screens, ping ingestion, alert formatting)
- **No over-testing v1** — ship rough slices except on the two core paths above

## Definition of Done

A real freelancer can:
1. Sign up
2. Add their client automations
3. Wire up one Webhooks-by-Zapier step per the onboarding doc
4. Pay via Razorpay
5. Receive SMS + email within the expected window the first time a monitored automation goes quiet
6. **Zero manual intervention from the builder required**

---

Built for Zapier/Make automation freelancers who need reliable, simple, scalable monitoring.
