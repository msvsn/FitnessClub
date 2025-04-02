using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.DAL
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly FitnessClubContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(FitnessClubContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T> GetByIdAsync<TKey>(TKey id) => await _dbSet.FindAsync(id);
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }
        public async Task DeleteAsync(T entity) => _dbSet.Remove(entity);
        public IQueryable<T> Query() => _dbSet.AsQueryable();
    }
}