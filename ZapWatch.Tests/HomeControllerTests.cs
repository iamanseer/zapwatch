using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ZapWatch.Web.Controllers;

namespace ZapWatch.Tests;

public class HomeControllerTests
{
    // Regression test: an earlier version built this XML via `new XAttribute("xmlns", ...)`,
    // which compiles fine but throws XmlException the moment XDocument.ToString() serializes
    // it - a runtime-only failure that shipped a 500 on /sitemap.xml in production despite a
    // clean build and passing test suite. This exercises the actual action end to end.
    [Fact]
    public void Sitemap_ProducesWellFormedXmlWithExpectedUrls()
    {
        var config = new ConfigurationBuilder().Build();
        var controller = new HomeController(config);

        var result = controller.Sitemap();

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/xml", content.ContentType);

        var doc = XDocument.Parse(content.Content!);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var locs = doc.Root!.Elements(ns + "url")
            .Select(u => u.Element(ns + "loc")!.Value)
            .ToList();

        Assert.Contains("https://zap-watch.runasp.net/", locs);
        Assert.Contains("https://zap-watch.runasp.net/pricing", locs);
        Assert.Contains("https://zap-watch.runasp.net/how-it-works", locs);
        Assert.Contains("https://zap-watch.runasp.net/onboarding", locs);
        Assert.Contains("https://zap-watch.runasp.net/privacy", locs);
        Assert.Contains("https://zap-watch.runasp.net/about", locs);
        Assert.Equal(6, locs.Count);
    }
}
