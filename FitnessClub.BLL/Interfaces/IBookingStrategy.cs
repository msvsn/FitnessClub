using System;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingStrategy
    {
        bool CanBook(int? userId, string? guestName, int clubId, DateTime classDate, IUnitOfWork unitOfWork);
        Booking CreateBooking(int? userId, string? guestName, int classScheduleId, DateTime classDate);
    }
}