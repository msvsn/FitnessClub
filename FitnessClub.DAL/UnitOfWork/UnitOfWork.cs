using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.DAL.Entities;

namespace FitnessClub.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitnessClubContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed = false;

        public UnitOfWork(FitnessClubContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private IRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)_repositories[typeof(T)];
            }

            var repository = new Repository<T>(_context);
            _repositories[typeof(T)] = repository;
            return repository;
        }

        public IRepository<User> Users => GetRepository<User>();
        public IRepository<Club> Clubs => GetRepository<Club>();
        public IRepository<Trainer> Trainers => GetRepository<Trainer>();
        public IRepository<ClassSchedule> ClassSchedules => GetRepository<ClassSchedule>();
        public IRepository<MembershipType> MembershipTypes => GetRepository<MembershipType>();
        public IRepository<Membership> Memberships => GetRepository<Membership>();
        public IRepository<Booking> Bookings => GetRepository<Booking>();

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
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