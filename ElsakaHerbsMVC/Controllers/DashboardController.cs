using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Dashboard;
using ElsakaHerbs.ViewModels.Orders;
using ElsakaHerbs.ViewModels.Packages;
using ElsakaHerbs.ViewModels.Shipping;
using ElsakaHerbs.ViewModels.SiteContent;
using Microsoft.AspNetCore.Mvc;

namespace ElsakaHerbsMVC.Controllers;

[Route("dashboard")]
public class DashboardController : Controller
{
    private readonly ISiteContentService _siteContentService;
    private readonly IPackageService _packageService;
    private readonly IOrderService _orderService;
    private readonly IReviewService _reviewService;
    private readonly IGovernorateShippingService _shippingService;

    public DashboardController(
        ISiteContentService siteContentService,
        IPackageService packageService,
        IOrderService orderService,
        IReviewService reviewService,
        IGovernorateShippingService shippingService)
    {
        _siteContentService = siteContentService;
        _packageService = packageService;
        _orderService = orderService;
        _reviewService = reviewService;
        _shippingService = shippingService;
    }

    private async Task LoadSummaryAsync()
    {
        var summary = new DashboardSummaryViewModel
        {
            PackagesCount = await _packageService.GetTotalPackagesCountAsync(),
            NewOrdersCount = await _orderService.GetNewOrdersCountAsync(),
            TotalOrdersCount = await _orderService.GetTotalOrdersCountAsync(),
            ReviewsCount = await _reviewService.GetTotalReviewsCountAsync()
        };

        ViewBag.Summary = summary;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Content));
    }

    [HttpGet("content")]
    public async Task<IActionResult> Content()
    {
        await LoadSummaryAsync();
        ViewBag.SiteContent = await _siteContentService.GetSiteContentAsync();
        ViewBag.Governorates = await _shippingService.GetAllAsync();
        return View();
    }

    [HttpGet("packages")]
    public async Task<IActionResult> Packages(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        await LoadSummaryAsync();

        bool? activeOnly = null;
        if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            activeOnly = true;
        }
        else if (string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            activeOnly = false;
        }

        ViewBag.Packages = await _packageService.GetAllForAdminAsync(search, activeOnly);
        ViewBag.Search = search;
        ViewBag.Status = status ?? "all";
        return View();
    }

    [HttpGet("orders")]
    public async Task<IActionResult> Orders(
        [FromQuery] string? phone,
        [FromQuery] string? orderNumber,
        [FromQuery] int? statusId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        await LoadSummaryAsync();

        var validPageSizes = new[] { 5, 10, 20, 50, 100 };
        if (!validPageSizes.Contains(pageSize)) pageSize = 10;
        if (page < 1) page = 1;

        var pagedResult = await _orderService.GetOrdersPagedAsync(phone, orderNumber, statusId, page, pageSize);
        var stats = await _orderService.GetOrderStatisticsAsync();
        var statuses = await _orderService.GetAllStatusesAsync();

        ViewBag.OrdersResult = pagedResult;
        ViewBag.Orders = pagedResult.Orders;
        ViewBag.Stats = stats;
        ViewBag.Statuses = statuses;
        ViewBag.Phone = phone;
        ViewBag.OrderNumber = orderNumber;
        ViewBag.StatusId = statusId;
        ViewBag.PageSize = pageSize;

        return View();
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews()
    {
        await LoadSummaryAsync();
        ViewBag.Reviews = await _reviewService.GetAllReviewsAsync();
        return View();
    }

    [HttpPost("content/update")]
    public async Task<IActionResult> UpdateContent([FromForm] SiteContentEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "يرجى التحقق من صحة البيانات المدخلة لمحتوى الموقع";
            return RedirectToAction(nameof(Content));
        }

        var result = await _siteContentService.UpdateSiteContentAsync(model);
        if (result)
        {
            TempData["SuccessMessage"] = "تم حفظ محتوى الصفحة الرئيسية بنجاح";
        }
        else
        {
            TempData["ErrorMessage"] = "تعذر تحديث محتوى الصفحة الرئيسية";
        }

        return RedirectToAction(nameof(Content));
    }

    [HttpPost("shipping/update")]
    public async Task<IActionResult> UpdateShipping([FromBody] List<GovernorateShippingEditModel> models)
    {
        if (models == null || models.Count == 0)
        {
            return BadRequest(new { success = false, message = "لا توجد بيانات شحن للحفظ" });
        }

        var result = await _shippingService.BulkUpdateShippingAsync(models);
        return Ok(new { success = result, message = result ? "تم تحديث مصاريف الشحن بنجاح" : "تعذر تحديث مصاريف الشحن" });
    }

    [HttpGet("packages/get/{id:int}")]
    public async Task<IActionResult> GetPackage(int id)
    {
        var package = await _packageService.GetByIdAsync(id);
        if (package == null)
            return NotFound(new { success = false, message = "العبوة غير موجودة" });

        return Json(new { success = true, package });
    }

    [HttpPost("packages/save")]
    public async Task<IActionResult> SavePackage(
        [FromForm] PackageFormModel model,
        [FromForm] List<IFormFile>? newImages,
        [FromForm] int? defaultExistingImageId,
        [FromForm] int? defaultNewImageIndex,
        [FromForm] List<int>? deletedImageIds)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "بيانات العبوة غير صحيحة";

            return BadRequest(new { success = false, message = firstError });
        }

        var imageStreams = new List<(Stream Stream, string FileName)>();
        if (newImages != null && newImages.Count > 0)
        {
            foreach (var file in newImages)
            {
                if (file.Length > 0)
                {
                    imageStreams.Add((file.OpenReadStream(), file.FileName));
                }
            }
        }

        if (model.Id.HasValue && model.Id.Value > 0)
        {
            // Edit existing package
            try
            {
                var updated = await _packageService.UpdatePackageAsync(
                    model,
                    imageStreams,
                    defaultExistingImageId,
                    defaultNewImageIndex,
                    deletedImageIds);

                return Ok(new { success = updated, message = updated ? "تم تعديل العبوة بنجاح" : "تعذر تعديل العبوة" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        else
        {
            // Create new package
            if (imageStreams.Count == 0)
            {
                return BadRequest(new { success = false, message = "يجب اختيار صورة واحدة على الأقل للعبوة." });
            }

            try
            {
                var newId = await _packageService.CreatePackageAsync(
                    model,
                    imageStreams,
                    defaultNewImageIndex);

                return Ok(new { success = true, message = "تمت إضافة العبوة بنجاح", packageId = newId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    [HttpPost("packages/delete/{id:int}")]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var result = await _packageService.DeletePackageAsync(id);
        return Ok(new { success = result, message = result ? "تم حذف العبوة بنجاح" : "تعذر حذف العبوة" });
    }

    [HttpPost("packages/toggle/{id:int}")]
    public async Task<IActionResult> TogglePackage(int id)
    {
        var result = await _packageService.TogglePackageStatusAsync(id);
        return Ok(new { success = result });
    }

    [HttpGet("orders/detail/{id:int}")]
    public async Task<IActionResult> GetOrderDetail(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new { success = false, message = "الطلب غير موجود" });

        var statuses = await _orderService.GetAllStatusesAsync();
        return Json(new { success = true, order, statuses });
    }

    [HttpPost("orders/status")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusUpdateViewModel model)
    {
        var result = await _orderService.UpdateOrderStatusAsync(model.OrderId, model.NewStatusId);
        return Ok(new { success = result, message = result ? "تم تحديث حالة الطلب بنجاح" : "تعذر تحديث حالة الطلب" });
    }

    [HttpPost("reviews/upload")]
    public async Task<IActionResult> UploadReviews([FromForm] List<IFormFile> images)
    {
        if (images == null || images.Count == 0)
        {
            return BadRequest(new { success = false, message = "يرجى اختيار صورة واحدة على الأقل" });
        }

        var imageStreams = new List<(Stream Stream, string FileName)>();
        foreach (var img in images)
        {
            if (img.Length > 0)
            {
                imageStreams.Add((img.OpenReadStream(), img.FileName));
            }
        }

        var result = await _reviewService.AddReviewImagesAsync(imageStreams);
        return Ok(new { success = result, message = result ? "تم رفع الصور بنجاح" : "تعذر رفع الصور" });
    }

    [HttpPost("reviews/toggle/{id:int}")]
    public async Task<IActionResult> ToggleReview(int id)
    {
        var result = await _reviewService.ToggleVisibilityAsync(id);
        return Ok(new { success = result });
    }

    [HttpPost("reviews/delete/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var result = await _reviewService.DeleteReviewAsync(id);
        return Ok(new { success = result, message = result ? "تم حذف الصورة بنجاح" : "تعذر حذف الصورة" });
    }
}
