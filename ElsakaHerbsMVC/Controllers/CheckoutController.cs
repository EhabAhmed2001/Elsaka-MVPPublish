using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace ElsakaHerbsMVC.Controllers;

public class CheckoutController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IGovernorateShippingService _shippingService;

    public CheckoutController(
        IOrderService orderService,
        IGovernorateShippingService shippingService)
    {
        _orderService = orderService;
        _shippingService = shippingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Governorates = await _shippingService.GetActiveAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ProcessOrder([FromBody] OrderCheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "البيانات المدخلة غير صحيحة";

            return BadRequest(new { success = false, message = firstError });
        }

        var (success, errorMessage, order) = await _orderService.PlaceOrderAsync(model);

        if (!success || order == null)
        {
            return BadRequest(new { success = false, message = errorMessage ?? "حدث خطأ أثناء حفظ الطلب" });
        }

        TempData["AllowOrderSuccess"] = true;

        return Ok(new
        {
            success = true,
            orderNumber = order.OrderNumber,
            redirectUrl = Url.Action("Confirmation", "Checkout", new { orderNumber = order.OrderNumber })
        });
    }

    [HttpGet("checkout/confirmation/{orderNumber}")]
    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        if (TempData["AllowOrderSuccess"] == null)
            return RedirectToAction("Index", "Home");

        if (string.IsNullOrWhiteSpace(orderNumber))
            return RedirectToAction("Index", "Home");

        var order = await _orderService.GetOrderByNumberAsync(orderNumber);
        if (order == null)
            return RedirectToAction("Index", "Home");

        return View(order);
    }
}
