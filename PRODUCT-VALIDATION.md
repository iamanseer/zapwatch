# PRODUCT-VALIDATION.md — ZapWatch

> **Since this was written**: items 3 (brand-color fix) and 4 (landing page rebuild) in §3's list
> have both shipped — see `MODULES.md` Module 3. Item 2 (the three `TRD.md` §8 blockers) and item
> 1 (run the outreach) have **not** moved — this doc's core point, that the market-risk question
> is still completely open regardless of how much engineering has shipped since, is still exactly
> as true today. Kept as-is below rather than edited, since re-litigating a dated honest-assessment
> doc after the fact would undercut the point of writing one.

Honest state-of-things, not a status report dressed up as one. Split into what's actually
verified vs. what's still assumed, because those two lists have gotten blurred together across
`MODULES.md`'s "Done" checkmarks.

## 1. What's actually verified

- The core loop works end-to-end in production: ping → watchdog → alert → auto-resolve, tested
  live with real WebSocket connections (`MODULES.md` Module 1, Slice 6).
- Razorpay's webhook path is tested against test mode: a real test-mode subscription activates
  correctly, a forged webhook is correctly rejected. Real engineering rigor, not a smoke test.
- Email delivery works end-to-end (Resend, confirmed live).
- The hosting bet paid off — MonsterASP.NET's Free plan runs Hangfire and SignalR cleanly, the
  single biggest technical unknown from `TRD.md`'s original stack decision, resolved cleanly.

That's a real, working product. Say that plainly before the rest of this doc, which is mostly
about what it hasn't proven yet.

## 2. What's still assumed, not verified

This is the important list, and it's the same one flagged at Stage 1/2 of this project, before a
single evening-block was spent building:

- **Willingness to pay is still zero-evidence.** `PRD.md` §9 has carried "no willingness-to-pay
  signal yet" as an open risk since before the PRD existed. It's still open. Test-mode
  subscriptions and forged-webhook rejection are real engineering wins — they are not a customer
  saying yes. The Stage 2 feasibility study's original call on this idea was "Not-yet," pending
  exactly this signal, and it was overridden to proceed with building. That was a legitimate call
  to make — but it means the thing that was supposed to happen *in parallel* with the build
  (`PRD.md` §6: "ideally lined up before or during the build rather than after") hasn't happened
  yet, and the build is now finished. Outreach in `LAUNCH-PLAN.md` is that test, run late rather
  than skipped — worth doing now, at full effort, not as an afterthought.
- **The differentiation argument is still untested against a real prospect.** `PRD.md` §9 also
  flags that the core ping mechanic is available free via Healthchecks.io/Cronitor/PulseMon, and
  that differentiation has to come from the Zapier-native framing and client-facing angle. That's
  a reasonable thesis. Nobody has actually heard the pitch and chosen ZapWatch over "I'll just use
  Healthchecks.io" yet. The landing page copy leans directly into this objection rather than
  hoping nobody asks — but copy isn't the same as a prospect actually agreeing.
- **The brand-color/trademark issue was live in production, unnoticed, until this review.** Not a
  disaster, but worth naming: a real, checkable problem (see `RESTYLING-PLAN.md`) sat in a shipped
  product through two "Done" modules without anyone catching it. A useful data point on how much
  scrutiny "Done" has actually been getting versus how much it looked like it was getting.

## 3. Areas to improve, ranked by what's actually blocking a first dollar

1. **Run the outreach.** Everything else on this list is secondary to this one. No amount of
   further polish substitutes for finding out if anyone will pay.
2. **Close the three `TRD.md` §8 blockers** (Razorpay live KYC, Twilio out of trial, real
   onboarding screenshots) — mechanical, not risky, but genuinely blocking.
3. **Fix the brand color** before the Solution Partner directory gets used for outreach — small
   fix, real reason, see `RESTYLING-PLAN.md`.
4. **Ship the new landing page** — the current one wasn't evaluated against 2026 B2B SaaS
   conversion conventions until now (single clear CTA, social proof or an honest equivalent above
   the fold, problem-agitate-solve copy, a real product visual instead of description alone). The
   mockup in this review applies those; whether it's actually better is itself something only real
   traffic will confirm — ship it, don't deliberate it further.
5. **Everything in `POST-V1-ROADMAP.md`** stays exactly where it was gated — behind 3 paying
   accounts and 30 stable days, not before. Nothing in this review changes that gate.

## 4. The honest read

The technical risk this project was most worried about (can a solo builder actually ship this on
evening-blocks, does the hosting bet hold, does the payment path work) is resolved, and resolved
well. The market risk this project was *also* worried about from the very first scoring pass —
will a real Zapier freelancer actually pay for this over doing it themselves — has not moved since
before the first line of code was written. That's not a reason to have not built it; the call to
override "Not-yet" was made with that risk named clearly at the time. It is a reason to treat
`LAUNCH-PLAN.md`'s outreach as the actual next milestone, not a followup to it.
