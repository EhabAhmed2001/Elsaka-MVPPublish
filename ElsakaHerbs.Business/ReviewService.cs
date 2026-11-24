using ElsakaHerbs.DataAccess.Entities;
using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElsakaHerbs.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, ILogger<ReviewService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<CustomerReviewImageViewModel>> GetAllReviewsAsync(bool? visibleOnly = null)
    {
        var query = _unitOfWork.CustomerReviewImages.Query(asNoTracking: true);

        if (visibleOnly.HasValue)
        {
            query = query.Where(r => r.IsVisible == visibleOnly.Value);
        }

        return await query
            .OrderByDescending(r => r.Id)
            .Select(r => new CustomerReviewImageViewModel
            {
                Id = r.Id,
                ImageUrl = r.ImageUrl,
                IsVisible = r.IsVisible,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<CustomerReviewImageViewModel>> GetActiveReviewsAsync()
    {
        return await GetAllReviewsAsync(visibleOnly: true);
    }

    public async Task<bool> AddReviewImagesAsync(List<(Stream Stream, string FileName)> images)
    {
        if (images == null || images.Count == 0) return false;

        var reviewEntities = new List<CustomerReviewImage>();

        foreach (var (stream, fileName) in images)
        {
            var imageUrl = await _fileStorage.SaveFileAsync(stream, fileName, "reviews");

            reviewEntities.Add(new CustomerReviewImage
            {
                ImageUrl = imageUrl,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.CustomerReviewImages.AddRangeAsync(reviewEntities);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Added {Count} new customer review image(s).", reviewEntities.Count);
        return true;
    }

    public async Task<bool> ToggleVisibilityAsync(int id)
    {
        var review = await _unitOfWork.CustomerReviewImages.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        review.IsVisible = !review.IsVisible;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var review = await _unitOfWork.CustomerReviewImages.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        var imageUrl = review.ImageUrl;

        // 1. Remove from database first
        _unitOfWork.CustomerReviewImages.Remove(review);
        await _unitOfWork.SaveChangesAsync();

        // 2. Delete physical disk file second (Rule 6.4)
        await _fileStorage.DeleteFileAsync(imageUrl);

        _logger.LogInformation("Hard deleted review image {Id}: {Url}", id, imageUrl);
        return true;
    }

    public async Task<int> GetTotalReviewsCountAsync()
    {
        return await _unitOfWork.CustomerReviewImages.CountAsync();
    }
}
