using ElsakaHerbs.ViewModels.Packages;
using ElsakaHerbs.ViewModels.Reviews;
using ElsakaHerbs.ViewModels.SiteContent;

namespace ElsakaHerbs.ViewModels.Landing;

public class LandingPageViewModel
{
    public SiteContentViewModel SiteContent { get; set; } = new();
    public List<PackageViewModel> Packages { get; set; } = new();
    public List<CustomerReviewImageViewModel> Reviews { get; set; } = new();
}

public class ProductDetailsPageViewModel
{
    public PackageViewModel CurrentPackage { get; set; } = new();
    public List<PackageViewModel> AllPackages { get; set; } = new();
    public SiteContentViewModel SiteContent { get; set; } = new();
}
