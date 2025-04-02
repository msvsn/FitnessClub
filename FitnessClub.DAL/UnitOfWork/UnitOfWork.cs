using FitnessClub.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitnessClubContext _context;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed = false;

        public UnitOfWork(FitnessClubContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new Repository<TEntity>(_context);
                _repositories.Add(type, repositoryInstance);
            }
            return (IRepository<TEntity>)_repositories[type];
        }

        public async Task<int> SaveAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}