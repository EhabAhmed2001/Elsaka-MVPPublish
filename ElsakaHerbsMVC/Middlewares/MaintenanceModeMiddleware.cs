using ElsakaHerbs.interfaces;

namespace ElsakaHerbsMVC.Middlewares;

public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceModeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISiteContentService siteContentService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // 1. Bypass static resources and assets
        if (path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/assets") ||
            path.StartsWith("/uploads") ||
            path.StartsWith("/lib") ||
            path.EndsWith(".ico") ||
            path.EndsWith(".png") ||
            path.EndsWith(".jpg") ||
            path.EndsWith(".jpeg") ||
            path.EndsWith(".svg") ||
            path.EndsWith(".css") ||
            path.EndsWith(".js") ||
            path.EndsWith(".woff") ||
            path.EndsWith(".woff2") ||
            path.EndsWith(".ttf"))
        {
            await _next(context);
            return;
        }

        // 2. Bypass Dashboard routes so Admin can always manage and restore the site
        if (path.StartsWith("/dashboard"))
        {
            await _next(context);
            return;
        }

        // 3. Bypass Maintenance page itself to prevent redirect loops
        if (path == "/maintenance")
        {
            await _next(context);
            return;
        }

        // 4. Check if website is enabled
        var siteContent = await siteContentService.GetSiteContentAsync();
        if (!siteContent.IsWebsiteEnabled)
        {
            // For AJAX / API requests (e.g. /Checkout/ProcessOrder, /api/catalog, etc.)
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers.Accept.ToString().Contains("application/json") ||
                path.StartsWith("/api/") ||
                path.StartsWith("/checkout/processorder"))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json; charset=utf-8";
                var msg = string.IsNullOrWhiteSpace(siteContent.MaintenanceMessage)
                    ? "الموقع قيد الصيانة حالياً. يرجى المحاولة لاحقاً."
                    : siteContent.MaintenanceMessage;
                await context.Response.WriteAsJsonAsync(new { success = false, message = msg, isMaintenance = true });
                return;
            }

            // Normal browser request: redirect to Maintenance page
            context.Response.Redirect("/Maintenance");
            return;
        }

        await _next(context);
    }
}
