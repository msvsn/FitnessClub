using System;
using FitnessClub.Entities;
using FitnessClub.BLL.Interfaces;

namespace FitnessClub.BLL.Services
{
    public class GuestBookingStrategy : IBookingStrategy
    {
        public virtual bool CanBook(int? userId, string? guestName, int clubId, DateTime classDate) =>
            !userId.HasValue && !string.IsNullOrWhiteSpace(guestName);

        public virtual Booking CreateBooking(int? userId, string? guestName, int classScheduleId, DateTime classDate)
        {
            if (string.IsNullOrWhiteSpace(guestName))
            {
                throw new ArgumentException("Ім'я гостя не може бути порожнім", nameof(guestName));
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