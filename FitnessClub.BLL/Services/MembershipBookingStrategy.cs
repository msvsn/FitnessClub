using System;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using FitnessClub.BLL.Interfaces;

namespace FitnessClub.BLL.Services
{
    public class MembershipBookingStrategy : IBookingStrategy
    {

        public bool CanBook(int? userId, string? guestName, int clubId, DateTime classDate, IUnitOfWork unitOfWork) =>
            userId.HasValue;
        public Booking CreateBooking(int? userId, string? guestName, int classScheduleId, DateTime classDate)
        {
             if (!userId.HasValue)
             {
                 throw new ArgumentException("User ID must be provided for membership bookings.", nameof(userId));
             }

            return new Booking
            {
                UserId = userId.Value,
                GuestName = null,
                ClassScheduleId = classScheduleId,
                ClassDate = classDate,
                BookingDate = DateTime.UtcNow,
                IsMembershipBooking = true
            };
        }
    }
}