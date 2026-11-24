using ElsakaHerbs.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace ElsakaHerbs.interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IGenericRepository<SiteContent> SiteContents { get; }
    IGenericRepository<GovernorateShipping> GovernorateShippings { get; }
    IGenericRepository<Package> Packages { get; }
    IGenericRepository<PackageImage> PackageImages { get; }
    IGenericRepository<CustomerReviewImage> CustomerReviewImages { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<OrderStatus> OrderStatuses { get; }

    IGenericRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
