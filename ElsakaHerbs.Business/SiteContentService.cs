using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.SiteContent;
using Microsoft.EntityFrameworkCore;

namespace ElsakaHerbs.Services;

public class SiteContentService : ISiteContentService
{
    private readonly IUnitOfWork _unitOfWork;

    public SiteContentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
}
