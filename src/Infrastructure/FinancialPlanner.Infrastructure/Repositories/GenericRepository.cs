using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinancialPlanner.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, bool includeRelated = false)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T> UpsertAsync(T entity)
    {
        if (entity.Id > 0)
        {
            _dbSet.Update(entity);
        }
        else
        {
            await _dbSet.AddAsync(entity);
        }
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}