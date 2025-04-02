using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.DAL
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync<TKey>(TKey id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        IQueryable<T> Query();
    }
}