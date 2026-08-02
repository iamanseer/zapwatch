# POST-V1-ROADMAP.md — ZapWatch

> **This file is not part of the build's source of truth.** `CLAUDE.md` §"Source of truth" lists
> five files (`PRD.md`, `TRD.md`, `DESIGN.md`, `AGENTS.md`, `MODULES.md`) — this isn't a sixth
> one. Nothing here is scheduled, nothing here is a v1 slice, and no agent should treat anything
> below as buildable without Anseer first promoting it into `PRD.md`'s feature list. This is a
> parking lot for known weaknesses and how to think about them later — written now, while the
> reasoning is fresh, for a moment (real paying customers, stable v1) that hasn't happened yet.
> **Do not open this file for work until the trigger conditions in §0 are met.**

## 0. When to actually open this file for work

Not before: **3 paying accounts, 30 days of stable v1 in production, Razorpay live-mode active.**
That's PRD.md §7's own success metric — reuse it as the gate here too, rather than inventing a
second bar. Below that line, every evening-block belongs to closing TRD.md §8's three remaining
blockers (Razorpay live-mode KYC, Twilio out of trial, real onboarding screenshots) — not to
anything in this file.

## 1. The five cons, and why each is real

1. **Zapier-only.** Anyone also running Make or n8n — increasingly common as agencies scale —
   needs a second tool.
2. **Only catches missing runs.** A Zap that fires on schedule but errors out silently mid-run
   still reads as "on time." Not caught.
3. **Per-automation pricing punishes growth.** The agencies most likely to convert are exactly
   the ones who'll feel squeezed first as they cross 5→15→40 client Zaps.
4. **No free persistent tier, no team/workspace concept, no Slack integration.** Agencies
   coordinate in Slack, not email/SMS alone; a solo-operator-shaped product doesn't fit a growing
   team.
5. **`.runasp.net` domain and generic template feel undermine trust** for a tool whose entire
   pitch is reliability.

Worth naming directly: these aren't five unrelated nitpicks. PRD.md §9 already carries the risk
that the core ping mechanic is available free via Healthchecks.io/Cronitor/PulseMon, and that
differentiation has to come from the Zapier-native, client-facing angle — not the ping detection
itself. Cons #1, #3, and #4 are literally the shape of that differentiation not being finished
yet. A generic dev tool like Healthchecks.io has no reason to build agency workspaces or a Slack
integration for client-monitoring workflows; ZapWatch does. That's the actual moat, once built.

## 2. Fix, step by step, per con

### Con 1 — Zapier-only
The good news first: this is a docs gap, not an engineering gap. `/ping/{token}` doesn't know or
care whether the POST came from Zapier's Webhooks action, Make's HTTP module, or n8n's HTTP
Request node — they're identical from ZapWatch's side. No backend change required.
1. Validate first, free: once there are real customers, ask directly which platforms they run.
   Don't build a Make doc on a guess.
2. Write a Make.com onboarding doc (same ping URL, their HTTP module) — docs only.
3. Write an n8n onboarding doc — docs only.
4. Widen landing-page/README copy from "Zapier automations" to "Zapier, Make, and n8n" once the
   docs exist, not before (don't advertise support you haven't documented).
- *Est. effort:* ~1–2 evening-blocks per platform doc. *Gate:* real demand signal from step 1.

### Con 2 — Only catches missing runs, not failed-or-slow ones
Two separable capabilities, ship the cheaper one first.
1. **Failure-ping** (mechanical, additive): add an optional second endpoint variant
   (`/ping/{token}/fail`) a Zap's error path can hit. Extend `AlertEvent` with a `reason` field
   (`silent` vs `reported-failure`) — additive column, not a breaking schema change.
2. **Slow-run / anomaly detection** (statistical, needs data): `PingEvent.received_at` is already
   being recorded for every hit — the raw data this needs already exists. Compute the normal gap
   between pings per automation and flag a gap that's abnormally large but still inside the
   manual grace period. This is the same underlying capability as the "volume-anomaly detection"
   idea already sitting in `PRD.md` §5 — building one gets you most of the other.
- *Est. effort:* failure-ping ~2–3 evening-blocks; anomaly detection ~3–4 evening-blocks.
  *Gate for #2:* needs real per-automation ping history to be meaningful — do this after usage
  exists, not before.

### Con 3 — Pricing punishes exactly the agencies it wants
Don't touch v1 pricing now — simple per-automation tiers are easier to explain at zero customers
than a "correct" model is. `TRD.md` §2's `Subscription.plan` / `automation_limit` fields already
support swapping in new tiers later without a schema rewrite; this is config, not architecture.
1. Wait for real distribution data: how many automations does a typical paying agency actually
   run.
2. Evaluate, don't pre-decide: stepped volume discounts, a per-client/per-workspace price instead
   of per-automation, or an unlimited "agency" tier above a price point.
3. Concrete trigger, not a vague "someday": a paying customer crossing ~60–70% of their tier's
   ceiling more than once is real evidence of the squeeze — revisit then.
- *Est. effort:* ~2–3 evening-blocks once a direction is picked. *Gate:* trigger in step 3.

### Con 4 — No free tier, no team/workspace, no Slack
Three different problems, three different gates — don't bundle them.
1. **Slack alert channel** — cheapest, ships first. The alert-dispatch point already has an
   email→SMS fallback pattern (`TRD.md` §3); Slack is a third channel slotted into the same
   pattern, not a redesign. ~2–3 evening-blocks.
2. **Team/workspace concept** — the biggest lift in this entire doc. Needs a `Workspace` entity,
   membership/roles, re-scoping `Automation` off `User` onto `Workspace`, and re-testing the
   SignalR per-user scoping (`Context.UserIdentifier`) as per-workspace instead. Don't build this
   speculatively — gate it on multiple customers explicitly asking for teammate access, not on
   assumption. ~5–7 evening-blocks.
3. **Free persistent tier** — cheap to build (~1 evening-block, one more plan row), but sequence
   it *after* the first real paid conversion, not before. `PRD.md` §9's still-open risk is "no
   willingness-to-pay signal yet" — giving product away free before that signal exists muddies
   the one thing this whole project has been trying to validate since Stage 1/2.
- *Priority order when the time comes:* Slack → free tier (after first payment) → workspace
  (only once teammate-access requests are real, not assumed).

### Con 5 — Domain and template feel undermine trust
The cheapest fixes in this whole document — do these essentially as soon as there's a live
customer pointed at the product, not gated behind "v1 works great for months."
1. Buy a real domain (`zapwatch.com` or similar, ~$10–15/year). `TRD.md` already flags this as
   needed before a real launch — Premium Single's custom-domain support was the reason that plan
   was chosen over Free in the first place. This is a purchase, not a build task.
2. Point MonsterASP.NET's custom-domain feature at it.
3. Fill `DESIGN.md` §13's five `.zw-screenshot-slot` placeholders with real screenshots — already
   flagged as open there, not a new idea. A visible dashed placeholder box is a bigger trust
   problem than any font choice.
4. Once there are 1–3 paying customers, add a short testimonial/logo strip to the landing page —
   free, just needs the ask, and it's the highest-leverage trust signal for a niche B2B tool.
5. Only after 1–4: consider a genuine visual refresh (real wordmark/logo) beyond the existing
   `DESIGN.md` token system. Lowest priority here — the mechanical fixes above almost certainly
   move trust more per hour spent.
- *Est. effort:* steps 1–3 combined, well under 1 evening-block (mostly non-coding). Step 4:
  no build effort. Step 5: not estimated — revisit only if 1–4 turn out insufficient.

## 3. Suggested phase order, once §0's gate is met

- **Phase 0 (essentially immediate, barely "post-v1"):** Con 5, steps 1–3 — domain + real
  screenshots. Already flagged elsewhere as pre-launch work, not new scope.
- **Phase 1 (once PRD §7's 3-paying-accounts metric is hit):** Slack channel (Con 4.1),
  failure-ping (Con 2.1), testimonials (Con 5.4).
- **Phase 2 (once there's real usage data — platform mix, automation-count distribution):**
  Make/n8n docs (Con 1, if demand confirms), anomaly detection (Con 2.2), pricing revisit (Con 3).
- **Phase 3 (only once agency-segment demand is unambiguous):** Team/workspace (Con 4.2), free
  tier (Con 4.3, sequenced after first paid conversion specifically).
