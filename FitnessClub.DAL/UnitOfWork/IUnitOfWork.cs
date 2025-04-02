using System;
using System.Threading.Tasks;
using FitnessClub.DAL.Entities;

namespace FitnessClub.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Club> Clubs { get; }
        IRepository<Trainer> Trainers { get; }
        IRepository<ClassSchedule> ClassSchedules { get; }
        IRepository<MembershipType> MembershipTypes { get; }
        IRepository<Membership> Memberships { get; }
        IRepository<Booking> Bookings { get; }
        Task<int> SaveAsync();
        IRepository<T> GetRepository<T>() where T : class;
    }
}