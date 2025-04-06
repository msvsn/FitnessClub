using System.Linq.Expressions;

namespace FitnessClub.Core.Abstractions
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        Task DeleteByIdAsync(int id);
        IQueryable<TEntity> Query();
    }
} 