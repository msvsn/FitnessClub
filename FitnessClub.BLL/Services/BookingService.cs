using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.BLL.Services
{
    public interface IBookingStrategy
    {
        bool CanBook(int? userId, string guestName, int clubId, DateTime classDate, IUnitOfWork unitOfWork);
        Booking CreateBooking(int? userId, string guestName, int classScheduleId, DateTime classDate);
    }

    public class MembershipBookingStrategy : IBookingStrategy
    {
        private readonly UserService _userService;

        public MembershipBookingStrategy(UserService userService)
        {
            _userService = userService;
        }

        public bool CanBook(int? userId, string guestName, int clubId, DateTime classDate, IUnitOfWork unitOfWork) =>
            userId.HasValue && _userService.HasValidMembershipForClub(userId.Value, clubId, classDate);

        public Booking CreateBooking(int? userId, string guestName, int classScheduleId, DateTime classDate) =>
            new Booking
            {
                UserId = userId,
                ClassScheduleId = classScheduleId,
                ClassDate = classDate,
                BookingDate = DateTime.Now,
                IsMembershipBooking = true
            };
    }

    public class GuestBookingStrategy : IBookingStrategy
    {
        public bool CanBook(int? userId, string guestName, int clubId, DateTime classDate, IUnitOfWork unitOfWork) =>
            !userId.HasValue && !string.IsNullOrEmpty(guestName);

        public Booking CreateBooking(int? userId, string guestName, int classScheduleId, DateTime classDate) =>
            new Booking
            {
                GuestName = guestName,
                ClassScheduleId = classScheduleId,
                ClassDate = classDate,
                BookingDate = DateTime.Now,
                IsMembershipBooking = false
            };
    }

    public enum BookingResult
    {
        Success,
        InvalidScheduleOrDate,
        NoAvailablePlaces,
        UserOrGuestRequired,
        BookingLimitExceeded
    }

    public class BookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly IEnumerable<IBookingStrategy> _strategies;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, UserService userService, IEnumerable<IBookingStrategy> strategies)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _strategies = strategies;
        }

        public async Task<(BookingResult Result, string BookingId)> BookClassAsync(int? userId, string guestName, int classScheduleId, DateTime classDate)
        {
            var classSchedule = await _unitOfWork.ClassSchedules.Query()
                .Include(cs => cs.Club)
                .FirstOrDefaultAsync(cs => cs.ClassScheduleId == classScheduleId);

            if (classSchedule == null || classDate.DayOfWeek != classSchedule.DayOfWeek || classDate <= DateTime.Now)
                return (BookingResult.InvalidScheduleOrDate, null);

            if (classSchedule.BookedPlaces >= classSchedule.Capacity)
                return (BookingResult.NoAvailablePlaces, null);

            var strategy = _strategies.FirstOrDefault(s => s.CanBook(userId, guestName, classSchedule.ClubId, classDate, _unitOfWork));
            if (strategy == null)
                return (BookingResult.UserOrGuestRequired, null);

            if (userId.HasValue)
            {
                var userBookings = await _unitOfWork.Bookings.Query()
                    .CountAsync(b => b.UserId == userId && b.ClassDate >= DateTime.Now);
                if (userBookings >= AppSettings.BookingSettings.MaxBookingsPerUser)
                    return (BookingResult.BookingLimitExceeded, null);
            }

            var booking = strategy.CreateBooking(userId, guestName, classScheduleId, classDate);
            classSchedule.BookedPlaces++;
            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveAsync();
            return (BookingResult.Success, booking.BookingId.ToString());
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _unitOfWork.Bookings.Query()
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Club)
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Trainer)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.ClassDate)
                .ToListAsync();

            return _mapper.Map<List<BookingDto>>(bookings);
        }
    }
}