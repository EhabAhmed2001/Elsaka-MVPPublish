using ElsakaHerbs.ViewModels.SiteContent;

namespace ElsakaHerbs.interfaces;

public interface ISiteContentService
{
    Task<SiteContentViewModel> GetSiteContentAsync();
    Task<bool> UpdateSiteContentAsync(SiteContentEditViewModel model);
    Task<(bool Success, string? ErrorMessage)> UpdateIngredientsImageAsync(Stream imageStream, string fileName);
}
