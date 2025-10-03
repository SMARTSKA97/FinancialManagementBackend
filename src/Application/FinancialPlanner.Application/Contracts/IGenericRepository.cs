using FinancialPlanner.Domain.Entities;
using System.Linq.Expressions;

namespace FinancialPlanner.Application.Contracts;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, bool includeRelated = false);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> UpsertAsync(T entity);
    Task DeleteAsync(T entity);
}
