using ElsakaHerbs.DataAccess.Entities;
using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Landing;
using Microsoft.AspNetCore.Mvc;

namespace ElsakaHerbsMVC.Controllers;

public class HomeController : Controller
{
    private readonly ISiteContentService _siteContentService;
    private readonly IPackageService _packageService;
    private readonly IReviewService _reviewService;
    private readonly IGovernorateShippingService _shippingService;

    public HomeController(
        ISiteContentService siteContentService,
        IPackageService packageService,
        IReviewService reviewService,
        IGovernorateShippingService shippingService)
    {
        _siteContentService = siteContentService;
        _packageService = packageService;
        _reviewService = reviewService;
        _shippingService = shippingService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new LandingPageViewModel
        {
            SiteContent = await _siteContentService.GetSiteContentAsync(),
            Packages = await _packageService.GetAllActiveAsync(),
            Reviews = await _reviewService.GetActiveReviewsAsync()
        };

        return View(model);
    }

    [Route("product/{id:int}")]
    public async Task<IActionResult> ProductDetails(int id)
    {
        var package = await _packageService.GetByIdAsync(id);
        if (package == null || !package.IsActive)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(package);
    }

    [HttpGet("Maintenance")]
    public async Task<IActionResult> Maintenance()
    {
        var siteContent = await _siteContentService.GetSiteContentAsync();
        if (siteContent.IsWebsiteEnabled)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(siteContent);
    }

    [HttpGet("api/governorates")]
    public async Task<IActionResult> GetGovernorates()
    {
        var governorates = await _shippingService.GetActiveAsync();
        var result = governorates.Select(g => new
        {
            id = g.GovernorateKey,
            dbId = g.Id,
            name = g.GovernorateName,
            shippingCost = g.ShippingCost
        });

        return Json(result);
    }

    [HttpGet("api/catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var packages = await _packageService.GetAllActiveAsync();
        var result = packages.ToDictionary(
            p => p.Id.ToString(),
            p => new
            {
                id = p.Id,
                name = p.Name,
                duration = $"{p.DurationDays} يوم",
                price = p.Price,
                image = p.DefaultImageUrl,
                description = p.Description,
                packets = p.PacketsCount,
                weight = p.WeightGrams > 0 ? $"{p.WeightGrams} جم" : null
            });

        return Json(result);
    }
}
