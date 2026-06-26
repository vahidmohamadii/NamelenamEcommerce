using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Common.Interfaces;
using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Infrastructure.Persistence;

public sealed class EfRepository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query()
    {
        return dbContext.Set<TEntity>().AsQueryable();
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<TEntity>().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }
}
