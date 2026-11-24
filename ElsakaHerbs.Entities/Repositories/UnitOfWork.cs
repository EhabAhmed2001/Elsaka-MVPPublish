using System.Collections.Concurrent;
using ElsakaHerbs.DataAccess.Entities;
using ElsakaHerbs.interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ElsakaHerbs.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IGenericRepository<SiteContent> SiteContents => Repository<SiteContent>();
    public IGenericRepository<GovernorateShipping> GovernorateShippings => Repository<GovernorateShipping>();
    public IGenericRepository<Package> Packages => Repository<Package>();
    public IGenericRepository<PackageImage> PackageImages => Repository<PackageImage>();
    public IGenericRepository<CustomerReviewImage> CustomerReviewImages => Repository<CustomerReviewImage>();
    public IGenericRepository<Order> Orders => Repository<Order>();
    public IGenericRepository<OrderItem> OrderItems => Repository<OrderItem>();
    public IGenericRepository<OrderStatus> OrderStatuses => Repository<OrderStatus>();

    public IGenericRepository<T> Repository<T>() where T : class
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return _currentTransaction;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
            await _context.DisposeAsync();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
