# PRD — ZapWatch

## 1. Problem & user
- **Problem (1 sentence):** Zapier/Make automations that handle revenue-critical work (new orders,
  leads, invoices) can silently stop firing — no error, no visible failure — and the person
  responsible usually finds out only when a client or the business itself notices something
  missing, often days later.
- **Target user (specific):** Independent Zapier/Make automation freelancers and 1–3 person
  automation agencies (active on Upwork, Fiverr, Zapier's Expert directory, and Zapier's own
  community forum) who build and maintain 5+ client Zaps and have no scalable way to catch a
  client automation going silent before the client does.
- **Why now / why this user has this pain today:** Zapier's own community has been asking for a
  fix since at least 2021 with nothing shipped natively. The only existing answer is a manual
  "heartbeat table + watchdog Zap" pattern each freelancer hand-builds and maintains separately
  per client — it doesn't scale past a handful of clients, and it's the kind of thing that gets
  skipped under deadline pressure, which is exactly when it's needed most.

## 2. Goal & non-goals
- **v1 goal:** A freelancer can register their client Zaps, add one ping step to each, and get an
  immediate email + SMS alert the moment one goes quiet past its expected window — no dashboards,
  no per-client spreadsheet, no hand-built watchdog Zap.
- **Explicit non-goals for v1:** Polling-based monitoring (Shopify/QuickBooks direct API checks),
  WhatsApp alerts, Make/n8n-specific onboarding (Zapier only for v1), team/multi-user accounts, a
  public status page, volume-anomaly detection, AI-generated summaries, admin tooling.

## 3. Core user flow (v1 only)
1. Sign up.
2. Add an automation: name it, set an expected frequency + grace period, get a unique ping URL.
3. Add a "Webhooks by Zapier" → POST step to the end of the relevant Zap, pointed at that URL
   (per onboarding doc).
4. Dashboard confirms the first ping arrived.
5. Repeat per client automation, up to plan limit.
6. If an automation goes quiet past its expected window, get an immediate email + SMS.
7. Fix it — the next successful ping auto-clears the alert.

UI/UX for this flow (and every screen beyond it) follows `DESIGN.md` — not a separate concern from
the product spec above.

## 4. Feature list — v1 only
| Feature | Why it's in v1 | Cut if time runs out? (Y/N) |
|---|---|---|
| Signup/login | No product without it | N |
| Automation CRUD (name, expected frequency, grace period, ping URL) | Core loop | N |
| Ping ingestion endpoint | Core loop | N |
| Watchdog scheduler | Core loop | N |
| Email alert | Core loop | N |
| SMS alert | The "immediate" part of the pitch — email alone gets missed | Y — ship email-first if the week runs out, add SMS days later |
| Razorpay subscription billing | No product to sell without it | N |
| Zapier onboarding doc w/ screenshots | Only self-serve path for a non-Anseer user | N |
| Per-automation grace period / quiet-hours | Prevents false-positive fatigue that gets the tool uninstalled week one | Y, but risky to cut |
| Live-updating dashboard (SignalR) | Avoids a manual refresh to see current status | Y — dashboard is fully functional via refresh if this slips; alerting itself doesn't depend on it |

## 5. Where AI could fit (optional)
None in v1 — revisit post-launch. Concrete candidates for later:
1. Auto-suggested expected-frequency default, learned from an automation's first 1–2 weeks of
   ping history, instead of asking the operator to guess a number.
2. Volume-anomaly detection — flags a zap that's still pinging but running at a fraction of its
   normal volume (e.g. a filter silently blocking most runs), which pure heartbeat timing can't
   catch and generic dead-man's-switch tools don't do either.
3. Auto-written plain-English monthly client health digests from the same run data, for the
   freelancer to forward to clients.

## 6. Monetization
- **Pricing model:** Flat monthly subscription, tiered by number of monitored automations (e.g.
  up to 5 / up to 15 / up to 40).
- **Payment mechanism for v1:** Razorpay — faster to stand up for an India-based solo operator
  than Stripe.
- **First-dollar target:** One Zapier freelancer or micro-agency converted from direct outreach
  (Upwork/Fiverr/community forum), ideally lined up before or during the build rather than after
  — target first payment within ~4 weeks of build start.

## 7. Success metric for v1
3 paying accounts within 30 days of launch.

## 8. Timeline (evening-blocks / Saturday-blocks only)
| Milestone | Block estimate | Target |
|---|---|---|
| Core monitoring engine working end-to-end | ~7–9 evening-blocks | Week 1–2 of build |
| Billing + onboarding doc + deploy | ~2–3 evening-blocks + 2 Saturday-blocks | Week 2–3 of build |
| First paying customer | — | ~Week 3–4 of build |

## 9. Risks carried over from the feasibility study
- No willingness-to-pay signal yet as of this PRD — the Stage 2 study called this idea "Not-yet"
  pending a monetization test; run that outreach in parallel with the build, not after it.
- The core ping mechanic is already available free/cheap via existing generic tools
  (Healthchecks.io, Cronitor, PulseMon) to the exact freelancer audience being targeted first —
  differentiation has to come from Zapier-native onboarding and the client-facing angle, not the
  ping detection itself.
- False positives (alerting on an automation that's actually fine) will burn trust fast — the
  grace-period/quiet-hours feature is more load-bearing than its size suggests.
- Shopify/QuickBooks polling and WhatsApp are deferred specifically because each carries an
  external approval-process timeline outside build-time control.
