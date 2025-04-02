namespace FitnessClub.Core.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        Task<int> SaveAsync();
    }
} 