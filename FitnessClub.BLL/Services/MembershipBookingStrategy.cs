using System;
using FitnessClub.DAL.Entities;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.BLL.Services
{
    public class MembershipBookingStrategy : IBookingStrategy
    {
        public Booking CreateBooking(int? userId, int classScheduleId, DateTime classDate, UserDto? user)
        {
            if (!userId.HasValue)
            {
                throw new InvalidOperationException("Cannot create booking for null user ID.");
            }

            return new Booking
            {
                UserId = userId.Value,
                ClassScheduleId = classScheduleId,
                ClassDate = classDate.Date,
                BookingDate = DateTime.UtcNow,
                IsMembershipBooking = true,
                GuestName = user != null ? $"{user.FirstName} {user.LastName}" : null
            };
        }
    }
}