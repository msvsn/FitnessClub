using System;
using FitnessClub.Entities;
using FitnessClub.BLL.Interfaces;

namespace FitnessClub.BLL.Services
{
    public class MembershipBookingStrategy : IBookingStrategy
    {

        public virtual bool CanBook(int? userId, string? guestName, int clubId, DateTime classDate) =>
            userId.HasValue;
        public virtual Booking CreateBooking(int? userId, string? guestName, int classScheduleId, DateTime classDate)
        {
            if (!userId.HasValue)
            {
                throw new ArgumentException("ID користувача не може бути порожнім", nameof(userId));
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