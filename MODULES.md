# MODULES.md — ZapWatch

v1 splits into three modules — see effort estimates below.

## Module 1: Core Monitoring Engine — ✅ Done
Built, tested, and deployed to production (`zap-watch.runasp.net`). All 6 slices shipped; see
git history on `main` for the slice-by-slice commits.
- **Purpose** — Lets an operator register automations and get reliably alerted the moment one
  goes silent. This is the product; everything else is packaging around it.
- **Core details**
  - Day-0 smoke test (do this before anything else below): deploy a bare-bones app to
    MonsterASP.NET Premium Single with a Hangfire recurring job and a SignalR hub, leave it
    untouched for 30–60 minutes, confirm the job still fires and a test SignalR connection still
    works with no page requests keeping it awake. **Resolved:** both work cleanly on MonsterASP.NET
    Premium Single — confirmed by the smoke test itself and re-confirmed since via the live
    cross-user SignalR scoping test in Module 1 Slice 6.
  - Signup/login
  - Automation CRUD: name, expected frequency, grace period, generated ping URL
  - Ping ingestion endpoint (`/ping/{token}`), updates `last_seen_at`
  - Hangfire recurring watchdog job: scans for overdue automations
  - Email alert + SMS alert (Twilio)
  - AlertEvent dedup / auto-resolve logic (one alert per quiet episode, not one per tick)
  - SignalR hub (`AutomationStatusHub`) + JS client so the dashboard updates live on status
    change, instead of requiring a manual refresh (see `TRD.md`)
  - Any view or `wwwroot` work in this or later modules follows `DESIGN.md` — reuse existing
    `zw-*` tokens/components rather than introducing new ones per screen
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
  Third risk (did not materialize): the day-0 smoke test passed cleanly, so no replan was needed.
- **Effort estimate** — 7–9 evening-blocks (was 6–7; +1.5–2 for the SignalR hub, its two emit
  points, and a reconnect-aware JS client with per-user scoping).
- **Done condition** — A test automation that stops pinging past its window triggers exactly one
  email + SMS alert, returns to "ok" on the next successful ping with no duplicate alerts firing
  while still overdue, and the dashboard reflects each status change live without a refresh —
  visible only to that automation's own owner. **Met, with one caveat** — verified live in
  production with two real accounts and real WebSocket connections (Slice 6), and a live
  overdue→alert→resolve cycle (Slice 4). Email delivery confirmed working end-to-end; SMS is
  code-complete but blocked by the Twilio account still being in trial mode (only predefined
  templates allowed, not our alert text) — an account/billing decision, not a code gap.
- **Build order priority** — Done.

## Module 2: Monetization & Launch — ✅ Done
Built, tested, and deployed to production. All 6 slices shipped (subscription entity/plan
selection, Razorpay checkout, webhook handler, automation-limit gating, landing page, onboarding
doc).
- **Purpose** — Turns the working engine into something a stranger can pay for and set up without
  Anseer's help.
- **Core details**
  - Razorpay subscription integration + webhook to gate account status
  - Landing page — new visual treatment per `DESIGN.md` §12 (spacious/hero-led, not a copy of the
    dashboard's dense table-first layout), built on the same token system
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
  up their first ping. **Met, with two caveats** — 25 tests plus a live test: a real Razorpay
  test-mode subscription was created via the app, a signed webhook call correctly activated it,
  and a forged webhook was correctly rejected (401). Caveats: (1) the actual Checkout.js payment
  widget itself hasn't been clicked through in a real browser yet (only the webhook side was
  exercised directly) — worth doing once with a Razorpay test card; (2) the onboarding doc's 5
  screenshot slots are still placeholders (`.zw-screenshot-slot` in `DESIGN.md` §13) — no browser
  automation was available to capture real ones.
- **Build order priority** — Done. Razorpay KYC/business verification for a *live* (non-test-mode)
  account is still outstanding and remains the real blocker before charging real customers — the
  above is all built and verified against Razorpay test mode.

## Module 3: Pre-Launch Polish — ✅ Done (+ 1 open follow-up slice)
Built and merged via PRs #1–14 on `worktree-module3-prelaunch-polish` (plus a same-day sitemap.xml
regression fix). This file wasn't updated at the time despite several of those commit messages
referencing it — written up here after the fact, from the actual shipped commits, not a plan.
- **Purpose** — Fixes a live trademark-color risk, rebuilds the landing page against real 2026
  conversion research, adds social sign-in + auth completeness, and lays down baseline SEO —
  the "fix before real outreach starts" batch identified in `RESTYLING-PLAN.md`, `LAUNCH-PLAN.md`,
  and `SEO-PLAN.md`.
- **Core details (all shipped)**
  - **Restyle**: `--zw-lamp` moved off Zapier's exact trademarked orange to a signal-violet family
    (`RESTYLING-PLAN.md`); `DESIGN.md` §1/§4 updated to match.
  - **Landing page rebuild**: ported from `landing-page-mockup.html` into real routed marketing
    views (`/pricing`, `/how-it-works`, `/onboarding`, etc.) with clean URLs.
  - **Google sign-in + profile update**: `Microsoft.AspNetCore.Authentication.Google`, external
    login matched/created against `User` by email, profile edit page.
  - **Auth completeness**: email verification on signup, forgot-password flow (Resend-delivered
    reset token), change-password from account settings, with Google-only accounts correctly
    shown a "set a password" option instead of a broken "change password" form.
  - **Billing view design pass + active-subscription state**: billing page now conditionally
    renders an active subscriber's real plan/status. **Caveat carried into the follow-up slice
    below**: `PRD.md` §10's line item was "Automation **& billing** view design pass" — only the
    billing half actually shipped at the time.
  - **Privacy Policy page**: real content — what's collected, which third parties process it
    (Razorpay, Twilio/MSG91, Resend, Google, MonsterASP.NET), contact/deletion process. Starter
    draft, not legal advice.
  - **About Us page**: extends the founder-note voice from the landing page.
  - **SEO baseline**: per-page `ViewData`-driven meta tags, OG/Twitter tags, one JSON-LD block on
    the landing page, `robots.txt`, a `sitemap.xml` controller action, SignalR CDN script deferred
    off every non-dashboard page. Full detail in `SEO-PLAN.md`; §5 (Search Console/Bing
    verification) and §6 (first long-tail content piece) are the two parts of that plan not yet
    done — both need Anseer to create external properties/write content, not more code.
- **Depends on** — Module 1 and Module 2 (touched existing auth, views, `site.css` directly).
- **Blocks** — Nothing scheduled downstream; `LAUNCH-PLAN.md`'s go-live checklist no longer waits
  on this module (brand-color fix row updated there too).
- **Risky factors (as they played out)** — The auth change touched the existing login flow
  directly, as flagged; no lockout regression reported. Email verification/forgot-password send
  real emails — exercised against real inboxes per the module's own risk note.
- **Effort estimate** — ~15–19 evening-blocks, matching the pre-build estimate.
- **Done condition** — Met for everything except `SEO-PLAN.md` §5/§6 (external, not code). The
  automations-list half of the view-design-pass line item is now also closed — see follow-up
  slice 2 below.
- **Build order priority** — Done.

### Follow-up slice 1 — Microsoft + GitHub sign-in — Done
Added 2026-08-02, Anseer request, extending the already-shipped Google sign-in slice above.
Google's `ExternalLogin`/`ExternalLoginCallback` flow in `AccountController.cs` was already
provider-agnostic; this slice added `Microsoft.AspNetCore.Authentication.MicrosoftAccount` and the
aspnet-contrib `AspNet.Security.OAuth.GitHub` package alongside it (including a GitHub-specific
`/user/emails` fallback for accounts with no public email), generalized the two Google-hardcoded
error messages and the `ViewBag.IsGoogleLinked` flag (now `ViewBag.LinkedProviders`), and replaced
the single-button `_GoogleSignInButton` partial with `_ExternalSignInButtons` (one divider, three
buttons). No migration needed.

**Incident, 2026-08-03**: the first two attempts to ship this (deploys for what became PR #15 and
a re-ship as #18) took production down completely — every route returned a bare 500, including
static files. Root cause, confirmed from a captured stdout log (`ZapWatch.Web/web.config`'s
temporary `stdoutLogEnabled=true`, added specifically to catch this): ASP.NET Core's
`AuthenticationMiddleware` validates *every registered scheme's* options on *every single
request*, not just sign-in attempts. Registering `AddMicrosoftAccount`/`AddGitHub` unconditionally
with an empty `ClientId` (no real secrets existed yet) meant `OAuthOptions.Validate()` threw
`ArgumentException: The value cannot be an empty string. (Parameter 'ClientId')` on every request.
Google never surfaced this because its secret was real from day one — the "validates lazily on
first sign-in" assumption in the original code comment was simply wrong, just never tested against
an actually-empty credential until now. Fixed by only calling `AddGoogle`/`AddMicrosoftAccount`/
`AddGitHub` when their `ClientId`/`ClientSecret` are both non-empty (`Program.cs`), hiding the
corresponding sign-in button when a scheme isn't registered
(`_ExternalSignInButtons.cshtml`, via `IAuthenticationSchemeProvider`), and guarding
`AccountController.ExternalLogin` against a direct hit for an unregistered provider. Covered by
`ZapWatch.Tests/ExternalAuthenticationConfigurationTests.cs`, which reproduces the exact crash
mechanism (`IOptionsMonitor<T>.Get()` throwing for an empty-ClientId scheme) and confirms the
conditional-registration fix avoids it — this is the one piece of this incident that *is* now
under automated test, specifically because `dotnet build`/`dotnet test` gave zero signal of the
bug both times it shipped broken.

**Bottleneck (unchanged)**: Anseer needs to register an Azure AD app and a GitHub OAuth App and
add four new GitHub Actions secrets (`MICROSOFT_CLIENT_ID`/`MICROSOFT_CLIENT_SECRET`,
`OAUTH_GITHUB_CLIENT_ID`/`OAUTH_GITHUB_CLIENT_SECRET` — note the `OAUTH_` prefix, GitHub Actions
rejects a `GITHUB_`-prefixed secret name) before either provider is actually usable — but with
this fix, their absence is now inert (button hidden, no crash) rather than fatal.
- **Effort estimate** — ~1 evening-block (was ~3, including incident response).
- **Build order priority** — Done. Credential setup outstanding, external to this repo, no longer
  blocking or risky.

### Follow-up slice 2 — Automations-list readability pass — ✅ Done
Closes out the automations half of the "Automation & billing view design pass" line item that
Follow-up slice 1's context found was never actually delivered. Shipped: capped the ping-URL
column to a fixed max-width with a `title` tooltip carrying the full URL (and fixed a real bug
along the way — `.zw-ping-url code`'s `text-overflow:ellipsis` never actually triggered, because a
flex child's default `min-width:auto` blocks shrinking below content size; added `min-width:0` to
both the flex container and the child); merged "Expected every"/"Grace" into one human-formatted
"Check window" column (`FormatMinutes` helper - "1 hr 30 min" instead of "90 min"); replaced the
inline `style="color: var(--zw-ok)"` with a new `.zw-text-ok` token class next to the existing
`.zw-alert-fail`; collapsed "Last alert" to one line per row (partial-failure detail — a channel
succeeded while another failed — moved into a `title` tooltip instead of a second stacked line,
preserving that information rather than hiding it); and, instead of a full parallel card-based
template (more engineering than the rest of this slice combined for a "consider" item), used
Bootstrap's `d-none d-md-table-cell` to hide the two more secondary columns (Check window, Last
seen) below the `md` breakpoint so narrow screens don't force horizontal scroll just to read
status/name/alert/ping URL. Verified visually in both themes and at a sub-`md` viewport width
using a static harness against the real `site.css` (no local SQL Server available to click through
the live authenticated page). Shared `.zw-table`/`.zw-table-wrap` shell untouched — Billing/
Subscription reuses it too.
- **Effort estimate** — ~1 evening-block.
- **Build order priority** — Done.

## Module dependency map
`M1 (Core Monitoring Engine) → M2 (Monetization & Launch) → M3 (Pre-Launch Polish)`

## Suggested build order (historical — M1–M3 all done; both M3 follow-up slices done except Microsoft/GitHub credential setup)
The day-0 smoke test comes before anything else, full stop — it's an hour of work that either
confirms the hosting decision or forces a replan while the cost of being wrong is still small.
After that, M1 — it's the entire risk surface of v1 (the watchdog logic is the one piece of "this
needs to actually be correct" business logic in the whole product) and the thing worth failing
fast on if the evening-block pace can't sustain it. Start Razorpay's KYC/business verification in
parallel with M1 so it isn't sitting on the critical path when M2 starts.

Actual order followed this exactly. What's left before real customers: Razorpay live-mode KYC, a
Twilio account upgrade (SMS is in trial-mode restriction), a real-browser Checkout.js click-through,
real screenshots for the onboarding doc, Microsoft/GitHub OAuth app credentials, and Search
Console/Bing verification + a first long-tail content piece (`SEO-PLAN.md` §5–6).
