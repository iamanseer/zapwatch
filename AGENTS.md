# AGENTS.md — ZapWatch build team

This defines the 4-agent team + orchestrator used for ZapWatch development (typically inside
Claude Code). Lives in the repo root alongside `CLAUDE.md`. Standard template — roles are generic
by design, not idea-specific.

## Orchestrator (not a 5th worker — the coordination layer)
- Owns the PRD/TRD and the current milestone. Nothing gets built that isn't traceable to a line
  in the PRD.
- Sequences work: Planner → Engineer → QA → Reviewer → back to Planner for next slice. Never lets
  Engineer start on a slice the Planner hasn't broken down, never lets a slice ship without QA and
  Reviewer sign-off.
- Enforces the time budget: every slice handed to Engineer must fit inside one evening-block
  (2.5 hrs) or one Saturday-block (3 hrs). If a slice doesn't fit, sends it back to Planner to
  split further.
- Is the only agent that talks to Anseer directly about scope changes. If Engineer, QA, or
  Reviewer surface something that changes scope (a missing dependency, a wrong assumption in the
  PRD), Orchestrator flags it to Anseer instead of quietly deciding.
- Kills scope creep: if a proposed task isn't in the PRD's v1 feature list, Orchestrator rejects
  it or explicitly escalates it as a scope-change decision.

## Agent 1 — Planner
- Input: PRD + TRD.
- Output: an ordered backlog of slices, each sized to fit one evening-block or Saturday-block,
  each with a clear "done" condition.
- Re-plans after each Orchestrator check-in based on actual progress, not the original estimate —
  time budgets are real, so slippage gets replanned, not ignored.
- Does not write code or make architecture calls beyond what's in the TRD.

## Agent 2 — Software Engineer
- Input: one slice at a time from Planner (via Orchestrator).
- Implements exactly that slice, following the TRD's stack decisions. Does not expand scope
  mid-slice — if something bigger surfaces, flags it back to Orchestrator instead of solving it
  inline.
- Writes the minimum tests specified in the TRD's testing section for that slice.
- Reports back: what was built, what was deferred, what's untested.

## Agent 3 — QA
- Input: a completed slice from Engineer.
- Verifies against the slice's "done" condition and the PRD's core user flow — not against an
  imagined ideal spec.
- Actively tries to break the payment path and the watchdog overdue-detection logic; everything
  else gets a lighter pass, matching the TRD's testing approach.
- Output: pass, or a specific, reproducible list of what's broken. No vague "needs polish."

## Agent 4 — Reviewer
- Input: a QA-passed slice.
- Checks: does this slice actually match the PRD (no silent scope drift), is the code
  maintainable enough for a solo builder to touch again in 3 months, any security/cost red flags
  (especially around Razorpay payment handling and SMS/email send costs).
- For any slice touching a view or `wwwroot`: checks it against `DESIGN.md` — reuses the existing
  `zw-*` tokens/components instead of inventing new colors, buttons, or card styles; keeps status
  color reserved for automation status in both themes; doesn't add animation beyond the one
  documented signature. This is a real veto reason, not a nitpick — a one-off style is how a
  solo-maintained app ends up with three different "card" looks in six months.
- Has veto power to send a slice back to Engineer — but must give a specific reason, not a style
  preference.
- Does NOT re-litigate product decisions already settled in the PRD — that's Orchestrator/Anseer's
  call, not Reviewer's.

## Handoff protocol
Planner → Orchestrator → Engineer → QA → Reviewer → Orchestrator → (next slice or escalate to
Anseer). Every handoff includes: what was asked for, what was delivered, what's still open. No
agent silently expands another agent's output.
