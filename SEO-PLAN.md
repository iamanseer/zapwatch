# SEO-PLAN.md — ZapWatch

> **Trimmed 2026-08-03.** §0–4 of the original plan (meta tags/OG/Twitter, JSON-LD, robots.txt,
> sitemap.xml, deferring the SignalR script) all shipped in `MODULES.md` Module 3 — that content
> is now redundant with the live code (`Views/Shared/_Layout.cshtml`, `HomeController.cs`'s
> `sitemap.xml` action, `wwwroot/robots.txt`), so it's been removed here rather than kept as a
> second copy that can drift from what's actually deployed. What's left below is the two items
> that are still genuinely open — both need Anseer's direct action, not more code.

## Still open

### §5. Google Search Console + Bing Webmaster Tools

1. Go to Google Search Console, add a property — use "URL Prefix" for faster setup rather than the
   Domain property option, which needs DNS-level verification.
2. Verify via the HTML meta tag method — the one verification `<meta>` tag Google provides slots
   into `_Layout.cshtml`'s existing `<head>` alongside the tags already there.
3. Once verified, open the Sitemaps report, paste the sitemap URL
   (`https://zap-watch.runasp.net/sitemap.xml` — or the real domain once purchased, see
   `LAUNCH-PLAN.md`), and submit — test the URL loads and returns real XML before submitting it, a
   broken sitemap URL is a common, easy-to-miss mistake.
4. Repeat verification + sitemap submission on Bing Webmaster Tools — smaller volume than Google,
   but a 10-minute add-on once the Google steps are done, and it's what feeds some AI answer
   engines' indexes too.
5. Don't expect movement for days-to-weeks; that's normal for a brand-new domain, not a sign
   something's broken.

### §6. First long-tail content piece

Don't compete on "monitoring tool" or "uptime monitor" — broad, saturated, and not what the actual
buyer searches for. The real target keywords are long-tail and match the exact language already
found in research for this project: *"zapier zap monitoring," "catch silent zap failures," "zapier
automation not running," "zapier client monitoring for agencies."*

The single best-grounded content idea available: a genuinely useful post titled something close to
*"How to catch a Zapier automation that silently stopped running"* — because that's not a guess,
it's the literal, repeated question that's been asked on Zapier's own community forum since 2021
(`PRD.md` §1's own cited evidence). Write the real answer (the DIY heartbeat-table pattern,
honestly, before pointing at ZapWatch as the easier version) rather than a thin product pitch, and
it's the kind of page that could plausibly rank for the exact query the target buyer types.

**Sequence this after `LAUNCH-PLAN.md`'s go-live checklist, not before** — SEO is a compounding
channel that takes months to show up for a brand-new domain with zero backlink history; it won't
produce the first paying customer. Both items above are worth doing because they're cheap
(especially #1, mostly Anseer's own time waiting on Google/Bing, not build time), not because
they're urgent.
