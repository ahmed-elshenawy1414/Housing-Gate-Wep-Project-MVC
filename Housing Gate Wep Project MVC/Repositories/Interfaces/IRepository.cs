using System.Linq.Expressions;

namespace StudentHousing.Repositories.Interfaces
{
    /// <summary>
    /// Generic data-access interface. Specific repositories add domain queries on top of this.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);

        Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "");

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, string includeProperties = "");

        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        Task AddAsync(T entity);

        void Update(T entity);

        void Remove(T entity);
    }
}
