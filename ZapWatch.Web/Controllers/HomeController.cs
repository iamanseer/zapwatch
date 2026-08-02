using System.Diagnostics;
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

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Onboarding()
    {
        return View();
    }

    public IActionResult SmokeTest()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
