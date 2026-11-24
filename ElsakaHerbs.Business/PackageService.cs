using ElsakaHerbs.DataAccess.Entities;
using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Packages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElsakaHerbs.Services;

public class PackageService : IPackageService
{
    private const string DefaultPlaceholderImage = "/assets/prod1.png";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<PackageService> _logger;

    public PackageService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, ILogger<PackageService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<PackageViewModel>> GetAllActiveAsync()
    {
        return await _unitOfWork.Packages.Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .Select(p => new PackageViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DurationDays = p.DurationDays,
                PacketsCount = p.PacketsCount,
                OriginalPrice = p.OriginalPrice,
                Price = p.Price,
                PurchaseCount = p.PurchaseCount,
                WeightGrams = p.WeightGrams,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                DefaultImageUrl = p.Images.Where(i => i.IsDefault).Select(i => i.ImageUrl).FirstOrDefault()
                                  ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault()
                                  ?? DefaultPlaceholderImage,
                Images = p.Images.OrderBy(i => i.Id).Select(i => new PackageImageViewModel
                {
                    Id = i.Id,
                    PackageId = i.PackageId,
                    ImageUrl = i.ImageUrl,
                    IsDefault = i.IsDefault
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<PackageViewModel>> GetAllForAdminAsync(string? search = null, bool? activeOnly = null)
    {
        var query = _unitOfWork.Packages.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.Description.Contains(s));
        }

        if (activeOnly.HasValue)
        {
            query = query.Where(p => p.IsActive == activeOnly.Value);
        }

        return await query
            .OrderByDescending(p => p.Id)
            .Select(p => new PackageViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DurationDays = p.DurationDays,
                PacketsCount = p.PacketsCount,
                OriginalPrice = p.OriginalPrice,
                Price = p.Price,
                PurchaseCount = p.PurchaseCount,
                WeightGrams = p.WeightGrams,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                DefaultImageUrl = p.Images.Where(i => i.IsDefault).Select(i => i.ImageUrl).FirstOrDefault()
                                  ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault()
                                  ?? DefaultPlaceholderImage,
                Images = p.Images.OrderBy(i => i.Id).Select(i => new PackageImageViewModel
                {
                    Id = i.Id,
                    PackageId = i.PackageId,
                    ImageUrl = i.ImageUrl,
                    IsDefault = i.IsDefault
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<PackageViewModel?> GetByIdAsync(int id)
    {
        var p = await _unitOfWork.Packages.Query()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new PackageViewModel
        {
            Id = p.Id,
            Name = p.Name,
            DurationDays = p.DurationDays,
            PacketsCount = p.PacketsCount,
            OriginalPrice = p.OriginalPrice,
            Price = p.Price,
            PurchaseCount = p.PurchaseCount,
            WeightGrams = p.WeightGrams,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            DefaultImageUrl = p.Images.FirstOrDefault(i => i.IsDefault)?.ImageUrl
                              ?? p.Images.FirstOrDefault()?.ImageUrl
                              ?? DefaultPlaceholderImage,
            Images = p.Images.OrderBy(i => i.Id).Select(i => new PackageImageViewModel
            {
                Id = i.Id,
                PackageId = i.PackageId,
                ImageUrl = i.ImageUrl,
                IsDefault = i.IsDefault
            }).ToList()
        };
    }

    public async Task<int> CreatePackageAsync(
        PackageFormModel model,
        List<(Stream Stream, string FileName)>? newImages = null,
        int? defaultImageIndex = null)
    {
        // Validation: At least one image required (Requirement 5.D)
        if (newImages == null || newImages.Count == 0)
        {
            throw new InvalidOperationException("يجب أن تحتوي العبوة على صورة واحدة على الأقل.");
        }

        var package = new Package
        {
            Name = model.Name.Trim(),
            DurationDays = model.DurationDays,
            PacketsCount = model.PacketsCount,
            OriginalPrice = model.OriginalPrice,
            Price = model.Price,
            PurchaseCount = model.PurchaseCount,
            WeightGrams = model.WeightGrams,
            Description = model.Description.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Packages.AddAsync(package);

        if (newImages != null && newImages.Count > 0)
        {
            // If no default image explicitly selected, first image is default (Rule 6.3)
            var chosenDefaultIndex = defaultImageIndex.HasValue && defaultImageIndex.Value >= 0 && defaultImageIndex.Value < newImages.Count
                ? defaultImageIndex.Value
                : 0;

            for (var i = 0; i < newImages.Count; i++)
            {
                var (stream, fileName) = newImages[i];
                var imageUrl = await _fileStorage.SaveFileAsync(stream, fileName, "packages");

                package.Images.Add(new PackageImage
                {
                    ImageUrl = imageUrl,
                    IsDefault = (i == chosenDefaultIndex)
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        return package.Id;
    }

    public async Task<bool> UpdatePackageAsync(
        PackageFormModel model,
        List<(Stream Stream, string FileName)>? newImages = null,
        int? defaultExistingImageId = null,
        int? defaultNewImageIndex = null,
        List<int>? deletedImageIds = null)
    {
        if (!model.Id.HasValue) return false;

        var package = await _unitOfWork.Packages.Query(asNoTracking: false)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == model.Id.Value);

        if (package == null) return false;

        // 1. Calculate remaining total images to enforce at-least-one-image rule
        var toDeleteIds = deletedImageIds?.Distinct().ToList() ?? new List<int>();
        var existingCount = package.Images.Count;
        var willBeDeletedCount = package.Images.Count(i => toDeleteIds.Contains(i.Id));
        var newImagesCount = newImages?.Count ?? 0;
        var remainingCount = (existingCount - willBeDeletedCount) + newImagesCount;

        if (remainingCount < 1)
        {
            throw new InvalidOperationException("يجب أن تحتوي العبوة على صورة واحدة على الأقل.");
        }

        package.Name = model.Name.Trim();
        package.DurationDays = model.DurationDays;
        package.PacketsCount = model.PacketsCount;
        package.OriginalPrice = model.OriginalPrice;
        package.Price = model.Price;
        // PurchaseCount is added only once upon package creation and incremented automatically by orders; cannot be edited.
        package.WeightGrams = model.WeightGrams;
        package.Description = model.Description.Trim();
        package.IsActive = model.IsActive;

        // 2. Process deletions of existing images (DB-first, disk-second)
        var deletedFileUrls = new List<string>();
        if (toDeleteIds.Count > 0)
        {
            var imagesToRemove = package.Images.Where(i => toDeleteIds.Contains(i.Id)).ToList();
            foreach (var img in imagesToRemove)
            {
                deletedFileUrls.Add(img.ImageUrl);
                _unitOfWork.PackageImages.Remove(img);
                package.Images.Remove(img);
            }
        }

        // 3. Process new uploaded images (appended to existing images, never replacing)
        var addedImages = new List<PackageImage>();
        if (newImages != null && newImages.Count > 0)
        {
            for (var i = 0; i < newImages.Count; i++)
            {
                var (stream, fileName) = newImages[i];
                var imageUrl = await _fileStorage.SaveFileAsync(stream, fileName, "packages");

                var newImg = new PackageImage
                {
                    ImageUrl = imageUrl,
                    IsDefault = false
                };
                package.Images.Add(newImg);
                addedImages.Add(newImg);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // 4. Safely delete physical files from disk after DB save succeeded
        foreach (var url in deletedFileUrls)
        {
            await _fileStorage.DeleteFileAsync(url);
        }

        // 5. Handle default image selection (Requirement 5.C)
        var allCurrentImages = await _unitOfWork.PackageImages.Query(asNoTracking: false)
            .Where(pi => pi.PackageId == package.Id)
            .OrderBy(pi => pi.Id)
            .ToListAsync();

        if (allCurrentImages.Count > 0)
        {
            int? targetDefaultId = null;

            if (defaultExistingImageId.HasValue && allCurrentImages.Any(i => i.Id == defaultExistingImageId.Value))
            {
                targetDefaultId = defaultExistingImageId.Value;
            }
            else if (defaultNewImageIndex.HasValue && newImages != null &&
                     defaultNewImageIndex.Value >= 0 && defaultNewImageIndex.Value < addedImages.Count)
            {
                targetDefaultId = addedImages[defaultNewImageIndex.Value].Id;
            }
            else
            {
                // If current default still exists in remaining images, keep it; otherwise assign first remaining image
                var currentDefault = allCurrentImages.FirstOrDefault(i => i.IsDefault);
                if (currentDefault != null)
                {
                    targetDefaultId = currentDefault.Id;
                }
                else
                {
                    targetDefaultId = allCurrentImages.First().Id;
                }
            }

            if (targetDefaultId.HasValue)
            {
                await SetDefaultImageAsync(package.Id, targetDefaultId.Value);
            }
        }

        return true;
    }

    public async Task<bool> DeletePackageAsync(int id)
    {
        var package = await _unitOfWork.Packages.Query(asNoTracking: false)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null) return false;

        // 1. Capture file paths in memory before deleting records
        var filePaths = package.Images.Select(i => i.ImageUrl).ToList();

        // 2. Remove package from DB (Cascades PackageImages; SetNull on OrderItem.PackageId)
        _unitOfWork.Packages.Remove(package);
        await _unitOfWork.SaveChangesAsync();

        // 3. Database-first, disk-second: Delete physical files after DB commit succeeds
        foreach (var path in filePaths)
        {
            await _fileStorage.DeleteFileAsync(path);
        }

        return true;
    }

    public async Task<bool> SetDefaultImageAsync(int packageId, int imageId)
    {
        var targetImage = await _unitOfWork.PackageImages.Query(asNoTracking: false)
            .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.PackageId == packageId);

        if (targetImage == null) return false;
        if (targetImage.IsDefault) return true; // Already default

        // Two-step transaction to avoid violating filtered unique index [IsDefault] = 1 (Rule 6.3)
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Step 1: Clear current default and save
            var currentDefault = await _unitOfWork.PackageImages.Query(asNoTracking: false)
                .FirstOrDefaultAsync(pi => pi.PackageId == packageId && pi.IsDefault);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                await _unitOfWork.SaveChangesAsync();
            }

            // Step 2: Set new default and save
            targetImage.IsDefault = true;
            await _unitOfWork.SaveChangesAsync();

            // Step 3: Commit transaction
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to switch default image for package {PackageId} to image {ImageId}", packageId, imageId);
            return false;
        }
    }

    public async Task<bool> DeletePackageImageAsync(int packageId, int imageId)
    {
        var image = await _unitOfWork.PackageImages.Query(asNoTracking: false)
            .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.PackageId == packageId);

        if (image == null) return false;

        var wasDefault = image.IsDefault;
        var imageUrl = image.ImageUrl;

        _unitOfWork.PackageImages.Remove(image);
        await _unitOfWork.SaveChangesAsync();

        // If the default image was deleted and other images remain, reassign default to first remaining image (Rule 6.3)
        if (wasDefault)
        {
            var firstRemaining = await _unitOfWork.PackageImages.Query(asNoTracking: false)
                .Where(pi => pi.PackageId == packageId)
                .OrderBy(pi => pi.Id)
                .FirstOrDefaultAsync();

            if (firstRemaining != null)
            {
                firstRemaining.IsDefault = true;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // Delete physical file after DB commit
        await _fileStorage.DeleteFileAsync(imageUrl);

        return true;
    }

    public async Task<bool> TogglePackageStatusAsync(int id)
    {
        var package = await _unitOfWork.Packages.Query(asNoTracking: false).FirstOrDefaultAsync(p => p.Id == id);
        if (package == null) return false;

        package.IsActive = !package.IsActive;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalPackagesCountAsync()
    {
        return await _unitOfWork.Packages.CountAsync();
    }
}
