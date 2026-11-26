using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.SiteContent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElsakaHerbs.Services;

public class SiteContentService : ISiteContentService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const string IngredientsFolder = "content";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<SiteContentService> _logger;

    public SiteContentService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, ILogger<SiteContentService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SiteContentViewModel> GetSiteContentAsync()
    {
        var content = await _unitOfWork.SiteContents.Query().FirstOrDefaultAsync();

        if (content == null)
        {
            return new SiteContentViewModel
            {
                HeroTitle = "",
                HeroHighlight = "",
                CtaEyebrow = "",
                CtaTitle = "",
                CtaDescription = "",
                WhatsAppNumber = "",
                IsWebsiteEnabled = true,
                MaintenanceMessage = null,
                IngredientsImageUrl = null,
                UpdatedAt = DateTime.UtcNow
            };
        }

        return new SiteContentViewModel
        {
            Id = content.Id,
            HeroTitle = content.HeroTitle,
            HeroHighlight = content.HeroHighlight,
            CtaEyebrow = content.CtaEyebrow,
            CtaTitle = content.CtaTitle,
            CtaDescription = content.CtaDescription,
            WhatsAppNumber = content.WhatsAppNumber,
            IsWebsiteEnabled = content.IsWebsiteEnabled,
            MaintenanceMessage = content.MaintenanceMessage,
            IngredientsImageUrl = content.IngredientsImageUrl,
            UpdatedAt = content.UpdatedAt
        };
    }

    public async Task<bool> UpdateSiteContentAsync(SiteContentEditViewModel model)
    {
        var content = await _unitOfWork.SiteContents.Query(asNoTracking: false).FirstOrDefaultAsync();

        if (content == null)
            return false;

        content.HeroTitle = model.HeroTitle.Trim();
        content.HeroHighlight = model.HeroHighlight.Trim();
        content.CtaEyebrow = model.CtaEyebrow.Trim();
        content.CtaTitle = model.CtaTitle.Trim();
        content.CtaDescription = model.CtaDescription.Trim();
        content.WhatsAppNumber = model.WhatsAppNumber.Trim();
        content.IsWebsiteEnabled = model.IsWebsiteEnabled;
        content.MaintenanceMessage = string.IsNullOrWhiteSpace(model.MaintenanceMessage) ? null : model.MaintenanceMessage.Trim();
        content.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateIngredientsImageAsync(Stream imageStream, string fileName)
    {
        // ── 1. Validate file type ───────────────────────────────────────────────
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, $"نوع الملف غير مدعوم. الأنواع المقبولة: {string.Join(", ", AllowedExtensions)}");
        }

        // ── 2. Validate file size ───────────────────────────────────────────────
        if (imageStream.Length > MaxFileSizeBytes)
        {
            return (false, "حجم الصورة كبير جداً. الحد الأقصى المسموح به هو 5 ميجابايت.");
        }

        // ── 3. Load the singleton SiteContent record (tracked) ─────────────────
        var content = await _unitOfWork.SiteContents.Query(asNoTracking: false).FirstOrDefaultAsync();
        if (content == null)
        {
            return (false, "لا يوجد سجل محتوى للموقع.");
        }

        // ── 4. Save new file to disk ────────────────────────────────────────────
        string newUrl;
        try
        {
            newUrl = await _fileStorage.SaveFileAsync(imageStream, fileName, IngredientsFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save ingredients image file: {FileName}", fileName);
            return (false, "تعذر رفع الصورة. يرجى المحاولة مرة أخرى.");
        }

        // ── 5. Database-first: update the record, then delete old physical file ─
        var oldUrl = content.IngredientsImageUrl;
        content.IngredientsImageUrl = newUrl;
        content.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // ── 6. Disk-second: delete old file (errors logged, never thrown) ───────
        if (!string.IsNullOrWhiteSpace(oldUrl))
        {
            await _fileStorage.DeleteFileAsync(oldUrl);
        }

        return (true, null);
    }
}
