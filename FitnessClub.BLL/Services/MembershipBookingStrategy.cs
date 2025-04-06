using System;
using FitnessClub.DAL.Entities;
using FitnessClub.BLL.Interfaces;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Services
{
    public class MembershipBookingStrategy : IBookingStrategy
    {
        private readonly IUserService _userService;

        public MembershipBookingStrategy(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<bool> CanBookAsync(int? userId, int clubId, DateTime classDate)
        {
            if (!userId.HasValue)
            {
                return false;
            }
            return await _userService.HasValidMembershipForClubAsync(userId.Value, clubId, classDate.Date);
        }

        public Booking CreateBooking(int? userId, int classScheduleId, DateTime classDate)
        {
            if (!userId.HasValue)
            {
                throw new InvalidOperationException("Cannot create booking for null user ID. Should be checked by CanBookAsync.");
            }

            return new Booking
            {
                UserId = userId.Value,
                ClassScheduleId = classScheduleId,
                ClassDate = classDate.Date,
                BookingDate = DateTime.UtcNow,
                IsMembershipBooking = true
            };
        }
    }
}