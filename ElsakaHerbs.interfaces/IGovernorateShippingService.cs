using ElsakaHerbs.ViewModels.Shipping;

namespace ElsakaHerbs.interfaces;

public interface IGovernorateShippingService
{
    Task<List<GovernorateShippingViewModel>> GetAllAsync();
    Task<List<GovernorateShippingViewModel>> GetActiveAsync();
    Task<GovernorateShippingViewModel?> GetByIdAsync(int id);
    Task<bool> UpdateShippingAsync(GovernorateShippingEditModel model);
    Task<bool> BulkUpdateShippingAsync(List<GovernorateShippingEditModel> models);
}
