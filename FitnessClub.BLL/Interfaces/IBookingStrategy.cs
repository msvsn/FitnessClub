using System;
using FitnessClub.DAL.Entities;
using FitnessClub.Core.Abstractions;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingStrategy
    {
        Task<bool> CanBookAsync(int? userId, int clubId, DateTime classDate);
        Booking CreateBooking(int? userId, int classScheduleId, DateTime classDate);
    }
}