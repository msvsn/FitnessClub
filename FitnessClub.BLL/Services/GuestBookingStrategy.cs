using System;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using FitnessClub.BLL.Interfaces;

namespace FitnessClub.BLL.Services
{
    public class GuestBookingStrategy : IBookingStrategy
    {
        public bool CanBook(int? userId, string? guestName, int clubId, DateTime classDate, IUnitOfWork unitOfWork) =>
            !userId.HasValue && !string.IsNullOrWhiteSpace(guestName);

        public Booking CreateBooking(int? userId, string? guestName, int classScheduleId, DateTime classDate)
        {
             if (string.IsNullOrWhiteSpace(guestName))
             {
                 throw new ArgumentException("Guest name cannot be empty for guest bookings.", nameof(guestName));
             }

             return new Booking
             {
                 UserId = null,
                 GuestName = guestName,
                 ClassScheduleId = classScheduleId,
                 ClassDate = classDate,
                 BookingDate = DateTime.UtcNow,
                 IsMembershipBooking = false
             };
        }
    }
}