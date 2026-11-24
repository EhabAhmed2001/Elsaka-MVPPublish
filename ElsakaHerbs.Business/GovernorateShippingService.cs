using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Shipping;
using Microsoft.EntityFrameworkCore;

namespace ElsakaHerbs.Services;

public class GovernorateShippingService : IGovernorateShippingService
{
    private readonly IUnitOfWork _unitOfWork;

    public GovernorateShippingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<List<GovernorateShippingViewModel>> GetAllAsync()
    {
        return await _unitOfWork.GovernorateShippings.Query(asNoTracking: true)
            .OrderBy(g => g.Id)
            .Select(g => new GovernorateShippingViewModel
            {
                Id = g.Id,
                GovernorateKey = g.GovernorateKey,
                GovernorateName = g.GovernorateName,
                ShippingCost = g.ShippingCost,
                IsActive = g.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<GovernorateShippingViewModel>> GetActiveAsync()
    {
        return await _unitOfWork.GovernorateShippings.Query(asNoTracking: true)
            .Where(g => g.IsActive)
            .OrderBy(g => g.Id)
            .Select(g => new GovernorateShippingViewModel
            {
                Id = g.Id,
                GovernorateKey = g.GovernorateKey,
                GovernorateName = g.GovernorateName,
                ShippingCost = g.ShippingCost,
                IsActive = g.IsActive
            })
            .ToListAsync();
    }

    public async Task<GovernorateShippingViewModel?> GetByIdAsync(int id)
    {
        var g = await _unitOfWork.GovernorateShippings.Query(asNoTracking: true).FirstOrDefaultAsync(x => x.Id == id);
        if (g == null) return null;

        return new GovernorateShippingViewModel
        {
            Id = g.Id,
            GovernorateKey = g.GovernorateKey,
            GovernorateName = g.GovernorateName,
            ShippingCost = g.ShippingCost,
            IsActive = g.IsActive
        };
    }

    public async Task<bool> UpdateShippingAsync(GovernorateShippingEditModel model)
    {
        var entity = await _unitOfWork.GovernorateShippings.Query(asNoTracking: false).FirstOrDefaultAsync(g => g.Id == model.Id);
        if (entity == null) return false;

        entity.ShippingCost = model.ShippingCost;
        entity.IsActive = model.IsActive;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BulkUpdateShippingAsync(List<GovernorateShippingEditModel> models)
    {
        if (models == null || models.Count == 0) return false;

        var ids = models.Select(m => m.Id).ToList();
        var entities = await _unitOfWork.GovernorateShippings.Query(asNoTracking: false).Where(g => ids.Contains(g.Id)).ToListAsync();

        foreach (var entity in entities)
        {
            var model = models.FirstOrDefault(m => m.Id == entity.Id);
            if (model != null)
            {
                entity.ShippingCost = model.ShippingCost;
                entity.IsActive = model.IsActive;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
