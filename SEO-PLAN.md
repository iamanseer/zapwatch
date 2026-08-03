# SEO-PLAN.md — ZapWatch

> **Status: mostly executed.** §2 (meta tags/OG/Twitter), §3 (JSON-LD), and §4 (robots.txt,
> sitemap.xml, deferred SignalR script) all shipped in `MODULES.md` Module 3. **Still open**: §5
> (Google Search Console + Bing Webmaster verification — needs Anseer to create those external
> properties) and §6 (the first long-tail content piece — needs writing, not code). Kept here as
> the implementation reference for what shipped and the to-do list for what didn't.

Detailed, step-by-step, researched against current (2026) practice — not generic advice copied
from a 2022 checklist. Read §0 first; it changes how much of this to prioritize right now.

## 0. Reality check before any of this

`LAUNCH-PLAN.md`'s 4-week first-dollar clock runs on direct outreach, not search traffic. SEO is
a compounding channel that takes months to show up for a brand-new domain with zero backlink
history — it will not produce the first paying customer. Do the technical parts below because
they're nearly free and stop being free later (metadata is cheapest to get right before a page
has traffic and history), but don't let this compete with outreach for evening-blocks. This is
parallel-track hygiene, not the launch plan.

## 1. What actually matters in 2026, briefly

<cite index="69-1">The dominant ranking factors for 2026 are E-E-A-T (demonstrated expertise),
topical authority, Core Web Vitals, content comprehensiveness, and AI-optimized content
structure</cite> — <cite index="76-1">Core Web Vitals alone accounts for roughly 28% of ranking
weight, making page performance a direct signal rather than a tiebreaker.</cite>
<cite index="70-1">Technical SEO is described as "the silent ranking factor — when it's right,
nobody notices; when it's wrong, everything suffers,"</cite> which is the right way to think about
§3–4 below: this is floor-setting, not a growth lever by itself. <cite index="75-1">Content still
needs to be genuinely useful, not padded — Google's helpfulness systems are explicitly built to
filter out surface-level optimization.</cite> One structural shift worth knowing: <cite index="83-1">clean structured metadata (title, description, Open Graph, schema) increasingly functions as a
machine-readable signal for AI answer engines too, not just classic search</cite> — another reason
to get the mechanical parts right once rather than skip them.

## 2. Meta tags — exact implementation for this stack

`_Layout.cshtml` is already the single shared `<head>` per `DESIGN.md` §2. Make title/description
per-page via `ViewData`, keep OG/Twitter/canonical there too since they rarely change per-page for
a small site.

```html
<!-- _Layout.cshtml <head> -->
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>@(ViewData["Title"] != null ? $"{ViewData["Title"]} — ZapWatch" : "ZapWatch — Know before your client does")</title>
<meta name="description" content="@(ViewData["Description"] ?? "ZapWatch pings your client Zapier automations and alerts you the moment one goes silently quiet — before your client notices.")" />
<link rel="canonical" href="@($"https://zapwatch.com{Context.Request.Path}")" />
<meta name="robots" content="@(ViewData["Robots"] ?? "index, follow")" />

<!-- Open Graph -->
<meta property="og:type" content="website" />
<meta property="og:site_name" content="ZapWatch" />
<meta property="og:title" content="@(ViewData["Title"] ?? "ZapWatch — Know before your client does")" />
<meta property="og:description" content="@(ViewData["Description"] ?? "Catch a silently failing Zapier automation before your client does.")" />
<meta property="og:url" content="@($"https://zapwatch.com{Context.Request.Path}")" />
<meta property="og:image" content="https://zapwatch.com/img/og-cover.png" />
<meta property="og:image:width" content="1200" />
<meta property="og:image:height" content="630" />
<meta property="og:locale" content="en_US" />

<!-- Twitter Card -->
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:title" content="@(ViewData["Title"] ?? "ZapWatch")" />
<meta name="twitter:description" content="@(ViewData["Description"] ?? "Catch a silently failing Zapier automation before your client does.")" />
<meta name="twitter:image" content="https://zapwatch.com/img/og-cover.png" />
```

Per-page override, e.g. in a pricing view:
```csharp
@{ ViewData["Title"] = "Pricing"; ViewData["Description"] = "Flat monthly plans by automation count, from $9/mo. No setup fee."; }
```

Length rules that actually hold up: <cite index="83-1">og:title under 60 characters, og:description
between 55–200 characters, og:image at minimum 1200×630px</cite> — same practical range applies to
the `<title>`/meta description pair for classic search snippets. Don't reuse the H1 verbatim as
the title tag; they serve different jobs (title = search/tab label, H1 = on-page).

Set `ViewData["Robots"] = "noindex, nofollow"` on the dashboard, account settings, and any
authenticated view — those shouldn't be indexed at all, and `robots.txt` alone doesn't reliably
keep already-linked authenticated pages out of the index.

## 3. Structured data (JSON-LD)

Add once to the landing page, not every view:

```html
<script type="application/ld+json">
{
  "@@context": "https://schema.org",
  "@@type": "SoftwareApplication",
  "name": "ZapWatch",
  "applicationCategory": "BusinessApplication",
  "operatingSystem": "Web",
  "description": "Monitoring for Zapier automations that catches silent failures before clients notice.",
  "offers": {
    "@@type": "Offer",
    "price": "9.00",
    "priceCurrency": "USD"
  }
}
</script>
```

Validate with Google's Rich Results Test before shipping — a malformed JSON-LD block is worse
than none, since it can trigger manual-review flags rather than just being ignored.

## 4. Technical SEO checklist

- **robots.txt** at the site root:
  ```
  User-agent: *
  Allow: /
  Disallow: /dashboard
  Disallow: /account
  Disallow: /ping/
  Sitemap: https://zapwatch.com/sitemap.xml
  ```
- **sitemap.xml** — a thin controller action, not a static file, so it stays accurate as pages
  get added:
  ```csharp
  [Route("sitemap.xml")]
  public IActionResult Sitemap()
  {
      var urls = new[] { "", "/pricing", "/how-it-works", "/onboarding" };
      var xml = new XElement("urlset",
          new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
          urls.Select(u => new XElement("url",
              new XElement("loc", $"https://zapwatch.com{u}"),
              new XElement("changefreq", "monthly"))));
      return Content(new XDocument(xml).ToString(), "application/xml");
  }
  ```
  Only list public marketing pages — `/dashboard`, `/account`, `/ping/*` don't belong in a
  sitemap; they're functional endpoints, not content for search.
- **Page speed, specific to what's actually on this site**: fonts are already self-hosted
  (`DESIGN.md` §7 — good, that's a common Core Web Vitals hit avoided already). One real,
  specific win: the `@microsoft/signalr` CDN script (`TRD.md`'s real-time updates section) is only
  needed on the dashboard — don't load it on the landing page, pricing page, or onboarding doc.
  It's dead weight on every page that isn't the dashboard.
- **Mobile-friendliness** — the landing page mockup in this review is responsive down to a single
  column; confirm the same holds for the dashboard and onboarding doc if they weren't built
  mobile-first.
- **HTTPS** — already satisfied via MonsterASP.NET's subdomain cert (`TRD.md` §1's correction).

## 5. Google Search Console + Bing Webmaster Tools

1. <cite index="102-1">Go to Google Search Console, add a property — use "URL Prefix" for faster
   setup rather than the Domain property option, which needs DNS-level verification.</cite>
2. Verify via the HTML meta tag method — since the `<head>` is already being edited for §2, add
   the one verification `<meta>` tag Google provides at the same time rather than a separate pass.
3. <cite index="103-1">Once verified, open the Sitemaps report, paste the sitemap URL
   (`https://zapwatch.com/sitemap.xml`), and submit — test the URL loads and returns real XML
   before submitting it, a broken sitemap URL is a common, easy-to-miss mistake.</cite>
4. Repeat verification + sitemap submission on Bing Webmaster Tools — smaller volume than Google,
   but it's a 10-minute add-on once the Google steps are done, and it's what feeds some AI
   answer engines' indexes too.
5. Don't expect movement for days-to-weeks; that's normal for a brand-new domain, not a sign
   something's broken.

## 6. Keyword and content angle specific to this product

Don't compete on "monitoring tool" or "uptime monitor" — broad, saturated, and not what the
actual buyer searches for. The real target keywords are long-tail and match the exact language
already found in research for this project: *"zapier zap monitoring," "catch silent zap
failures," "zapier automation not running," "zapier client monitoring for agencies."*

The single best-grounded content idea available: a genuinely useful post titled something close
to *"How to catch a Zapier automation that silently stopped running"* — because that's not a
guess, it's the literal, repeated question that's been asked on Zapier's own community forum
since 2021 (`PRD.md` §1's own cited evidence). Write the real answer (the DIY heartbeat-table
pattern, honestly, before pointing at ZapWatch as the easier version) rather than a thin
product pitch — matches the "helpful content over surface optimization" point in §1, and it's
the kind of page that could plausibly rank for the exact query the target buyer types.

## 7. Rollout sequence

| Step | Effort | Notes |
|---|---|---|
| Meta tags + OG/Twitter + robots meta (§2) | ~1 evening-block | One `_Layout.cshtml` pass |
| JSON-LD on landing page (§3) | ~0.5 evening-block | Validate before shipping |
| robots.txt + sitemap.xml controller (§4) | ~1 evening-block | |
| Defer SignalR script off non-dashboard pages (§4) | ~0.5 evening-block | Real, free speed win |
| Search Console + Bing setup (§5) | ~0.5 evening-block | Mostly waiting on Google, not work |
| First long-tail content piece (§6) | ~2–3 evening-blocks | Write it well or skip it — a thin post does nothing |

Sequence this after the `LAUNCH-PLAN.md` go-live checklist, not before — a well-optimized page
nobody's found the customers for yet is lower priority than closing the three `TRD.md` §8
blockers standing between here and a real payment.
