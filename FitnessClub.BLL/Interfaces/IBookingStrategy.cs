using System;
using FitnessClub.DAL.Entities;
using FitnessClub.Core.Abstractions;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingStrategy
    {
        bool CanBook(int? userId, string? guestName, int clubId, DateTime classDate);
        Booking CreateBooking(int? userId, string? guestName, int classScheduleId, DateTime classDate);
    }
}