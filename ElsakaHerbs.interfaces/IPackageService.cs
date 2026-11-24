using ElsakaHerbs.ViewModels.Packages;

namespace ElsakaHerbs.interfaces;

public interface IPackageService
{
    Task<List<PackageViewModel>> GetAllActiveAsync();
    Task<List<PackageViewModel>> GetAllForAdminAsync(string? search = null, bool? activeOnly = null);
    Task<PackageViewModel?> GetByIdAsync(int id);
    Task<int> CreatePackageAsync(PackageFormModel model, List<(Stream Stream, string FileName)>? newImages = null, int? defaultImageIndex = null);
    Task<bool> UpdatePackageAsync(PackageFormModel model, List<(Stream Stream, string FileName)>? newImages = null, int? defaultExistingImageId = null, int? defaultNewImageIndex = null, List<int>? deletedImageIds = null);
    Task<bool> DeletePackageAsync(int id);
    Task<bool> SetDefaultImageAsync(int packageId, int imageId);
    Task<bool> DeletePackageImageAsync(int packageId, int imageId);
    Task<bool> TogglePackageStatusAsync(int id);
    Task<int> GetTotalPackagesCountAsync();
}
