using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FitnessClub.Core.Abstractions;

namespace FitnessClub.DAL
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly FitnessClubContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(FitnessClubContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;
            query = query.Where(predicate);

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
             await _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(TEntity entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                 _dbSet.Attach(entity);
                 entry.State = EntityState.Modified;
            }
        }

        public virtual void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public virtual async Task DeleteByIdAsync(int id)
        {
            var entityToDelete = await _dbSet.FindAsync(id);
            if (entityToDelete != null)
            {
                Delete(entityToDelete);
            }
        }

        public virtual IQueryable<TEntity> Query()
        {
            return _dbSet.AsNoTracking();
        }
    }
}