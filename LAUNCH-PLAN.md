# LAUNCH-PLAN.md — ZapWatch

Ties together `PRD.md` §6's first-dollar target with actual research on where that person is and
what moves them — plus the concrete checklist between here and being able to point them at the
product at all.

## 1. Who, more specifically than the PRD says

`PRD.md` targets "one Zapier freelancer or micro-agency" — real research sharpens that further.

- **Rates**: <cite index="24-1">most Zapier consultants charge $40–150/hour, with fixed-scope
  projects at $300–3,000 and ongoing retainers from roughly $500 to $5,000/month</cite> — and
  <cite index="24-2">agency retainers specifically often run $2,500/month and up, covering
  "ongoing monitoring, fixes, and new builds."</cite> That last point matters more than the
  numbers: **monitoring is already a line item they bill for by hand.** ZapWatch isn't asking
  them to value something new — it's asking them to stop doing manually, at scale, something
  they already charge clients for.
- **They're findable, by name, right now.** <cite index="31-1">Zapier's Solution Partner Program
  (rebranded from "Zapier Experts" in 2025) is a public, verified directory</cite> at
  `zapier.com/partnerdirectory/` — real agencies with real names, sites, and contact info, not a
  cold-outreach guess. <cite index="30-1">Listed partners describe themselves in exactly the
  target language — "Zapier Certified Expert," managing client automation at scale.</cite>
- **They're active under their own names on Zapier's own forum** — the same forum threads
  underpinning `PRD.md` §1's problem statement surface identifiable, named professionals (e.g. a
  repeat "Top Zapier Solution Partner" contributor answering these exact monitoring questions
  across multiple years) — not anonymous accounts. Worth reading their actual posts before
  reaching out; they've already described the pain in their own words.
- **Upwork/Fiverr are real but noisier** — thousands of "Zapier automation" gig listings exist,
  skill level and English proficiency vary widely, and there's no way to pre-filter for "manages
  5+ client Zaps" the way the Partner directory's tier system does. Use as a secondary channel,
  not the first one.

## 2. Outreach sequence

1. **Pull 15–20 names from the Solution Partner directory** — filter for agencies (not solo
   Certified Experts only), since agencies are the segment `PRD.md` §9 already flags as feeling
   the current per-automation pricing squeeze first, and the ones most likely to have 5+ client
   Zaps worth monitoring today.
2. **Read before writing** — check each one's public forum activity or case studies for a
   specific detail to reference. A message that clearly wasn't sent to 20 people converts
   differently than one that was.
3. **Lead with the billing angle, not the feature list**: something close to *"You're already
   billing clients for keeping their Zaps healthy — this is the tool that lets you actually do
   that without checking Zap History by hand."* Ties directly to finding #1 above.
4. **Offer the manual concierge version first, not the app** — this was the Stage 2 feasibility
   study's original monetization test, and it's still the fastest way to a "yes": watch one
   prospect's Zap History for a week, text them if anything looks stale, charge a flat fee. Costs
   nothing to build, proves willingness to pay before the pitch depends on the product being
   fully launch-ready.
5. **Convert the concierge "yes" into an app signup** once Razorpay live-mode is active (§3
   below) — same person, same conversation, no cold restart.

## 3. Go-live checklist before outreach starts in earnest

Outreach can start now (steps 1–4 above don't require a live payment flow), but don't point
anyone at a real signup until:

| Item | Status | Source |
|---|---|---|
| Razorpay live-mode KYC/business verification | Not started | `TRD.md` §8 |
| Twilio account out of trial mode (real SMS text, not template-only) | Blocked | `TRD.md` §8 |
| Onboarding doc's 5 screenshot slots filled with real screenshots | Placeholder | `DESIGN.md` §13 |
| Custom domain purchased and pointed at MonsterASP.NET Premium Single | Not done | `TRD.md` §1 |
| Brand-color fix applied | **Done** — shipped in `MODULES.md` Module 3 | `RESTYLING-PLAN.md` |

None of these are large — the point of listing them together is that they're all "can't put a
real Zapier professional in front of this yet" items, and batching them avoids discovering them
one at a time mid-outreach.

## 4. Timeline, mapped to the 4-week target

`PRD.md` §6 targets first payment within ~4 weeks of build start; build is done, so this is now a
4-week outreach clock, not a build one.

- **Week 1**: Go-live checklist (table above) + directory research (§2 step 1–2) run in parallel.
- **Week 2**: First outreach batch (10 names), concierge offer, not app signup yet.
- **Week 3**: Follow up, convert any "yes" to concierge, second outreach batch if week 2 was
  quiet.
- **Week 4**: First real payment via Razorpay live mode, if a concierge "yes" converts — or a
  clear "no" pattern worth naming honestly (see `PRODUCT-VALIDATION.md`).

## 5. What "working" looks like

`PRD.md` §7's own bar — 3 paying accounts within 30 days of launch — is the metric here too.
Reuse it rather than inventing a separate launch-specific number.
