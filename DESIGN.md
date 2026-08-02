# DESIGN.md — ZapWatch frontend design system

Binding for any new screen, not just a visual reference. Implemented in full across every
existing view. Referenced from `CLAUDE.md`, `TRD.md`, `AGENTS.md`, `PRD.md`, and `MODULES.md` —
this file is the single source of truth for UI/UX; the others point here rather than duplicating
it.

## 1. Philosophy

ZapWatch's job is sitting quietly and watching for a signal that should arrive on schedule. The
product should read as calm and trustworthy at rest, and unambiguous the moment something needs
attention — not as a moody dark-mode dev tool by default. The default theme, **"Zapier Bright,"**
is a deliberate match to the actual product our exact audience (Zapier freelancers/agencies)
already uses daily — familiarity there builds trust faster than an original visual identity
would. **"Night Watch"** (ink-dark, violet-accent) is kept as the alternate theme via a nav
toggle, not discarded — some operators will prefer it for a dashboard they leave open all day.

The brand accent itself (`--zw-lamp`) deliberately does **not** borrow Zapier's own orange, even
though the rest of the visual language nods to Zapier's product conventions. `--zw-lamp` originally
was `#ff4a00` — Zapier's exact registered brand orange, not just a similar hue — which put ZapWatch
at odds with Zapier's own Solution Partner Brand Guidelines (partners must use their own color
palette, not Zapier's) right as `LAUNCH-PLAN.md`'s outreach plan for that exact directory. `--zw-lamp`
moved to a signal-violet family (`#4f3ff0` light / `#a196ff` dark) instead — same job (energetic,
unambiguous against the reserved status colors, never confusable with a status color), same
per-theme tinting pattern as the rest of the palette, just not Zapier's own trademarked hue. Full
reasoning in `RESTYLING-PLAN.md`.

Research behind this (Zapier's actual product design language; light-mode conventions from
Healthchecks.io, Better Stack, Cronitor, UptimeRobot) plus the full 9-stage user-flow mapping that
drove these decisions live in the design research memo — ask Anseer for the artifact link if you
need the reasoning, not just the rules below.

## 2. Ownership

- All theming lives in `wwwroot/css/site.css` — one file, CSS custom properties (`--zw-*`
  tokens) plus component classes prefixed `zw-`. `Views/Shared/_Layout.cshtml.css` (Razor CSS
  isolation) is deliberately left empty; don't put theme rules there, they'll fight load order
  with `site.css`.
- Theme-switching logic: a pre-paint inline script in `_Layout.cshtml` (sets `data-theme` from
  `localStorage` before first paint, avoiding a flash of the wrong theme) and a click handler in
  `wwwroot/js/site.js` (flips the attribute, persists the choice, updates the toggle's
  `aria-label` and the `theme-color` meta tag).

## 3. Bootstrap stays

This reskins Bootstrap 5 via `--bs-*` variable overrides in `:root`, it does not replace it. New
views should keep using Bootstrap's grid/utilities/JS per `TRD.md`'s stack decision; only reach
for a new `zw-*` class when Bootstrap has no equivalent (status pills, the ping-URL/copy
affordance, the table shell).

## 4. Two themes, one token set

Every component reads a `--zw-*` token — never a hardcoded color — so the two themes are a single
variable swap, not two parallel stylesheets. Values live in `site.css`: the bare `:root` holds
Zapier Bright's (light) values as the default; `:root[data-theme="dark"]` overrides them with
Night Watch's values. Adding a color to one theme without adding the matching override to the
other is the most common way this drifts — check both when touching a token.

| Token | Zapier Bright (light, default) | Night Watch (dark, alternate) |
|---|---|---|
| `--zw-ink-950` (page bg) | `#ffffff` | `#0b0d12` |
| `--zw-ink-800` (card surface) | `#ffffff` | `#171b24` |
| `--zw-ink-700` (raised/hover) | `#f3f4f6` | `#1e232e` |
| `--zw-text-1` (primary text) | `#1b1b1d` | `#edeff4` |
| `--zw-text-2` (muted text) | `#5b6270` | `#9aa3b2` |
| `--zw-lamp` (brand accent) | `#4f3ff0` (signal violet) | `#a196ff` (lighter violet tint) |
| `--zw-on-lamp` (text on accent) | `#ffffff` | `#14102e` |
| `--zw-ok` | `#1c9a5b` | `#34c78a` |
| `--zw-overdue` | `#e0392b` | `#f1555d` |
| `--zw-waiting` | `#2f6fed` | `#6e93d6` |
| `--zw-paused` | `#8a8f98` | `#7a8494` |

Full token list (line/shadow/glow variants, wash colors) is in `site.css` `:root` and
`:root[data-theme="dark"]` — this table is the ones worth knowing by heart, not the complete set.

## 5. Color rule — status color is reserved

`--zw-ok` / `--zw-overdue` / `--zw-waiting` / `--zw-paused` exist *only* to signal an automation's
state, in both themes. The brand/action accent (`--zw-lamp` — links, primary buttons, focus rings,
the brand mark) is a deliberately different hue so it's never ambiguous whether a color on screen
means "this is a warning" or "this is a button." Don't introduce a fifth saturated color without
updating this rule.

## 6. Four status states, not three

`AutomationStatus` only has `Ok / Overdue / Paused`, but the dashboard shows a fourth,
`zw-status--waiting` ("Awaiting first ping"), derived purely in the view from
`Status == Ok && LastSeenAt == null` — an automation that's never received a ping yet shouldn't
read as "Live." Follow this pattern for future status-adjacent UI: derive presentation states from
real data in the view rather than adding enum values just to drive a badge.

## 7. Typography

Manrope (self-hosted variable woff2, `wwwroot/fonts/`) for UI text and headings; JetBrains Mono
for anything literal — ping URLs, tokens, timestamps, durations — the monospace treatment signals
the value is meant to be copied, not decoration. Use `<code>` or `.zw-mono` for any new technical
value shown to the user. No Google Fonts CDN dependency at runtime.

## 8. Theme switching mechanism

- Pre-paint inline script in `_Layout.cshtml` reads `localStorage["zw-theme"]`, defaults to
  `"light"` for first-time visitors, sets `data-theme` and `data-bs-theme` on `<html>` before any
  CSS paints.
- Nav toggle button (`#zwThemeToggle`, sun/moon SVG pair shown/hidden by CSS off `data-theme`)
  flips the attribute, saves the choice, and stays in sync via `site.js`.
- New pages don't need to do anything extra — the attribute is set at the document root and every
  `zw-*` component already reads through the token layer.

## 9. Reusable components

Check these before inventing a new pattern: `.zw-card` (surface), `.zw-table-wrap` / `.zw-table`
(data tables), `.zw-status` + `.zw-status--{ok|overdue|waiting|paused}` + `.zw-status-dot` (status
pill), `.zw-empty` (empty state), `.zw-eyebrow` (small monospace section label), `.zw-ping-url` +
`.zw-copy-btn` (copyable value with clipboard button), `.zw-form-card` (auth/CRUD form shell),
`.zw-flow-step` (numbered process step, only for content that's a genuine sequence),
`.zw-theme-toggle` (the nav sun/moon button), `.zw-screenshot-slot` (dashed placeholder box for
doc content awaiting a real screenshot).

## 10. Motion budget: one signature, spent already

The pulsing status dot (`zw-pulse-calm` / `zw-pulse-alert` keyframes, faster for overdue than
live) is the one animated flourish, and it respects `prefers-reduced-motion`. Don't add further
decorative animation without a specific reason tied to the product, not just "make it feel alive."

## 11. Accessibility

Status colors need more saturation/depth on white than on dark ink to hold contrast — the two
themes' status hex values are deliberately not the same numbers, see the table in §4. Status is
always dot + colored pill + text label, never color alone.

## 12. Layout registers

The dashboard (dense, table-first, utilitarian) and the landing page (spacious, hero-led,
marketing-first) look different — that's normal for this product category (monitoring tool vs.
its own marketing page), not an inconsistency to fix. Both built; landing page reuses `.zw-card`
for its positioning/pricing sections rather than inventing marketing-specific components.

## 13. Open / deferred

- The onboarding doc (`Views/Home/Onboarding.cshtml`) ships with `.zw-screenshot-slot` placeholders
  instead of real screenshots — no browser automation was available when it was written. Drop real
  screenshots into those 5 slots before pointing real customers at it; the text alone is accurate
  and usable in the meantime.
- Billing/account-settings screens reuse `.zw-form-card` / `.zw-card` as planned, no new surface
  style was introduced.
