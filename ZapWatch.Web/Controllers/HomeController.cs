using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using ZapWatch.Web.Models;
using ZapWatch.Web.Services;

namespace ZapWatch.Web.Controllers;

public class HomeController(IConfiguration config) : Controller
{
    public IActionResult Index()
    {
        return View(PlanCatalog.All(config));
    }

    [Route("pricing")]
    public IActionResult Pricing()
    {
        return View(PlanCatalog.All(config));
    }

    [Route("how-it-works")]
    public IActionResult HowItWorks()
    {
        return View();
    }

    [Route("onboarding")]
    public IActionResult Onboarding()
    {
        return View();
    }

    [Route("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [Route("about")]
    public IActionResult About()
    {
        return View();
    }

    public IActionResult SmokeTest()
    {
        return View();
    }

    // Thin controller action rather than a static file, so it stays accurate as public
    // pages get added - only lists actual content, not functional endpoints like
    // /Automations, /Subscription, or /Account/* (those are excluded via robots.txt instead).
    [Route("sitemap.xml")]
    public IActionResult Sitemap()
    {
        // Must build elements via `ns + "name"`, not a plain `new XAttribute("xmlns", ...)` -
        // the latter throws XmlException ("prefix '' cannot be redefined") the moment
        // XDocument.ToString() tries to serialize it. Confirmed by running this exact
        // pattern in isolation before shipping it.
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        // "/" not "" for the homepage, so it matches the canonical tag's Context.Request.Path
        // for the root ("/") exactly instead of a bare domain with no trailing slash.
        var urls = new[] { "/", "/pricing", "/how-it-works", "/onboarding", "/privacy", "/about" };
        var xml = new XElement(ns + "urlset",
            urls.Select(u => new XElement(ns + "url",
                new XElement(ns + "loc", $"https://zap-watch.runasp.net{u}"),
                new XElement(ns + "changefreq", "monthly"))));
        return Content(new XDocument(xml).ToString(), "application/xml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
