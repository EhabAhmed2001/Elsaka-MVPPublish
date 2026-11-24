using ElsakaHerbs.ViewModels.SiteContent;

namespace ElsakaHerbs.interfaces;

public interface ISiteContentService
{
    Task<SiteContentViewModel> GetSiteContentAsync();
    Task<bool> UpdateSiteContentAsync(SiteContentEditViewModel model);
}
