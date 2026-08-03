# RESTYLING-PLAN.md — ZapWatch

> **Status: executed.** Shipped as `MODULES.md` Module 3's restyle slice — `--zw-lamp` moved to
> the signal-violet values proposed below, and `DESIGN.md` §1/§4 were updated to match. Kept here
> as the reasoning record; the steps below are no longer an open proposal.

> **Proposal, not an edit** *(original framing, left as written)*. This does not modify
> `DESIGN.md` — that stayed the live source of truth until Anseer approved the change above. What
> follows is a specific, small fix with the reasoning behind it, sized honestly so it doesn't get
> mistaken for a bigger redesign than it is.

## The finding

`DESIGN.md` §4's `--zw-lamp` (brand accent, light theme) is `#ff4a00`. That is not "close to"
Zapier's brand orange — it's the exact hex. <cite index="61-1">Zapier's own brand orange
("aerospace international orange") is documented at #FF4A00 across brand-color references.</cite>

That alone might be defensible as homage. What makes it worth fixing now: <cite index="56-1">Zapier's own Solution Partner Brand Guidelines (July 2025) explicitly tell partners to build
their own look — "your own brand's look and feel—logos, imagery, color palette, typography"—and
list an explicit restriction: partners "may not use Zapier illustrations, color palettes, or
proprietary design elements" without permission.</cite> <cite index="54-1">Zapier's trademark
notice separately reserves the right to request changes from any third party whose use creates
"a likelihood of confusion" or "dilutes Zapier's trademarks."</cite>

The Solution Partner directory is the exact channel `PRD.md` and `LAUNCH-PLAN.md` point outreach
at. That's the audience most likely to notice, and the one whose governing document says not to
do this. Worth fixing before that channel gets used, not filed under "someday."

## Why this isn't a redesign

`DESIGN.md` §2 and §4 already centralized every color into CSS custom properties in one file,
specifically so a color change is "a single variable swap, not two parallel stylesheets." That
architecture pays off here: this is a two-value edit, not a pass over every view.

## Proposed tokens

| Token | Current (light) | Proposed (light) | Current (dark) | Proposed (dark) |
|---|---|---|---|---|
| `--zw-lamp` | `#ff4a00` (= Zapier's exact orange) | `#4f3ff0` (signal violet) | `#e3a542` (brass — close to Zapier's `#FFC43E` sunglow, worth moving too) | `#a196ff` (lighter tint, same family) |
| `--zw-on-lamp` | `#ffffff` | `#ffffff` (unchanged — still passes contrast) | `#1a1204` | `#14102e` |

Same reasoning as the original pick, minus the trademark overlap: unambiguous against the
reserved status colors (ok/overdue/waiting/paused untouched — still green/red/blue/gray), reads
as alert-but-not-alarming in both themes, and — unlike the old two-hue-family split
(orange/brass) — one consistent hue tinted per theme is one less thing to maintain going forward.
Nothing in Zapier's named palette (`#FF4A00`, `#FD7622`, `#FFC43E`, `#5F6C72`, `#499DF3`,
`#13D0AB`) sits anywhere near violet, so this is a clean break, not a lighter shade of the same
problem.

The landing-page mockup (`landing-page-mockup.html`) is built with these proposed values so you
can see it in practice rather than judge it from hex codes.

## Step by step

1. Update the two `--zw-lamp` values (and `--zw-on-lamp` if contrast requires it) in
   `wwwroot/css/site.css`'s `:root` and `:root[data-theme="dark"]` blocks. That's the entire code
   change if the token discipline in `DESIGN.md` §4 has actually held.
2. Grep the codebase for `#ff4a00` and `#e3a542` literals outside `site.css` — a stray hardcoded
   hex in a view instead of `var(--zw-lamp)` is the one way this doesn't Just Work. `DESIGN.md`
   flags this exact drift risk in §4's own text.
3. Visual smoke-test every existing view in both themes — dashboard, auth/CRUD forms, onboarding
   doc. Token-driven components should pick up the new color with zero per-view changes; confirm
   that's actually true rather than assuming it.
4. Update `DESIGN.md` §1 and §4 to match — the philosophy paragraph currently justifies the *old*
   color ("a deliberate match to the actual product our audience already uses daily"); it needs a
   sentence swapped for the new reasoning, not a rewrite of the section.
5. Re-check the token table in `DESIGN.md` §4 against the new values so the doc and the CSS don't
   drift apart on day one.

## Effort and timing

~0.5–1 evening-block, most of it the visual smoke-test in step 3, not the CSS edit itself.
Sequence this alongside the custom-domain purchase (already flagged as needed pre-launch in
`TRD.md`) and before the Zapier Solution Partner directory gets used for outreach per
`LAUNCH-PLAN.md` — batch the "things to fix before a real Zapier professional looks at this"
work together rather than doing it in two passes.
