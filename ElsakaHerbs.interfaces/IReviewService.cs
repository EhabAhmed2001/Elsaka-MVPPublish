using ElsakaHerbs.ViewModels.Reviews;

namespace ElsakaHerbs.interfaces;

public interface IReviewService
{
    Task<List<CustomerReviewImageViewModel>> GetAllReviewsAsync(bool? visibleOnly = null);
    Task<List<CustomerReviewImageViewModel>> GetActiveReviewsAsync();
    Task<bool> AddReviewImagesAsync(List<(Stream Stream, string FileName)> images);
    Task<bool> ToggleVisibilityAsync(int id);
    Task<bool> DeleteReviewAsync(int id);
    Task<int> GetTotalReviewsCountAsync();
}
