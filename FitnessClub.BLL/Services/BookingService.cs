using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessClub.BLL.Services
{
    public enum BookingResult
    {
        Success,
        InvalidScheduleOrDate,
        NoAvailablePlaces,
        UserOrGuestRequired,
        BookingLimitExceeded,
        MembershipRequired,
        UnknownError
    }

    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEnumerable<IBookingStrategy> _strategies;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IEnumerable<IBookingStrategy> strategies, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _strategies = strategies;
            _logger = logger;
        }

        public async Task<(BookingResult Result, string? BookingId)> BookClassAsync(int? userId, string? guestName, int classScheduleId, DateTime classDate)
        {
            _logger.LogInformation("Attempting to book class schedule {ClassScheduleId} for date {ClassDate}, User: {UserId}, Guest: {GuestName}",
                classScheduleId, classDate.ToString("yyyy-MM-dd"), userId?.ToString() ?? "N/A", guestName ?? "N/A");

            if (!userId.HasValue && string.IsNullOrWhiteSpace(guestName))
            {
                _logger.LogWarning("Booking failed: Neither User ID nor Guest Name provided.");
                return (BookingResult.UserOrGuestRequired, null);
            }
            if (userId.HasValue && !string.IsNullOrWhiteSpace(guestName))
            {
                _logger.LogWarning("Booking failed: Both User ID and Guest Name provided.");
                guestName = null;
            }

            var classSchedule = await _unitOfWork.ClassSchedules.Query()
                .Include(cs => cs.Club)
                .FirstOrDefaultAsync(cs => cs.ClassScheduleId == classScheduleId);

            if (classSchedule == null)
            {
                _logger.LogWarning("Booking failed: Class schedule {ClassScheduleId} not found.", classScheduleId);
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            if (classDate.DayOfWeek != classSchedule.DayOfWeek || classDate.Date < DateTime.Today)
            {
                _logger.LogWarning("Booking failed: Invalid date {ClassDate} for schedule {ClassScheduleId} (DayOfWeek: {ScheduleDay}, Today: {Today}).",
                    classDate.ToString("yyyy-MM-dd"), classScheduleId, classSchedule.DayOfWeek, DateTime.Today.ToString("yyyy-MM-dd"));
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            int bookingWindowDays = AppSettings.BookingSettings.BookingWindowDays;
            if (classDate.Date > DateTime.Today.AddDays(bookingWindowDays))
            {
                _logger.LogWarning("Booking failed: Class date {ClassDate} is outside the booking window ({BookingWindowDays} days).",
                    classDate.ToString("yyyy-MM-dd"), bookingWindowDays);
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            if (classSchedule.BookedPlaces >= classSchedule.Capacity)
            {
                _logger.LogWarning("Booking failed: No available places for class schedule {ClassScheduleId} on {ClassDate}.", classScheduleId, classDate.ToString("yyyy-MM-dd"));
                return (BookingResult.NoAvailablePlaces, null);
            }

            IBookingStrategy? strategy = null;
            bool requiresMembershipCheck = false;

            if (userId.HasValue)
            {
                var futureBookingsCount = await _unitOfWork.Bookings.Query()
                    .CountAsync(b => b.UserId == userId.Value && b.ClassDate.Date >= DateTime.Today);

                int maxBookings = AppSettings.BookingSettings.MaxBookingsPerUser;
                if (futureBookingsCount >= maxBookings)
                {
                    _logger.LogWarning("Booking failed: User {UserId} has reached the booking limit ({MaxBookings}).", userId.Value, maxBookings);
                    return (BookingResult.BookingLimitExceeded, null);
                }

                strategy = _strategies.OfType<MembershipBookingStrategy>().FirstOrDefault();
                requiresMembershipCheck = true;
            }
            else
            {
                strategy = _strategies.OfType<GuestBookingStrategy>().FirstOrDefault();
            }

            if (strategy == null)
            {
                _logger.LogError("Booking failed: No suitable booking strategy found for User: {UserId}, Guest: {GuestName}.", userId?.ToString() ?? "N/A", guestName ?? "N/A");
                return (BookingResult.UnknownError, null);
            }

            if (requiresMembershipCheck && userId.HasValue)
            {
                bool hasValidMembership = await _userService.HasValidMembershipForClubAsync(userId.Value, classSchedule.ClubId, classDate);
                if (!hasValidMembership)
                {
                    _logger.LogWarning("Booking failed: User {UserId} does not have a valid membership for club {ClubId} on date {ClassDate}.",
                        userId.Value, classSchedule.ClubId, classDate.ToString("yyyy-MM-dd"));
                    return (BookingResult.MembershipRequired, null);
                }
            }

            try
            {
                var booking = strategy.CreateBooking(userId, guestName, classScheduleId, classDate);

                classSchedule.BookedPlaces++;
                _unitOfWork.ClassSchedules.Update(classSchedule);
                await _unitOfWork.Bookings.AddAsync(booking);

                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Booking successful for class schedule {ClassScheduleId} on {ClassDate}. Booking ID: {BookingId}",
                    classScheduleId, classDate.ToString("yyyy-MM-dd"), booking.BookingId);

                return (BookingResult.Success, booking.BookingId.ToString());
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Booking failed due to concurrency conflict for class schedule {ClassScheduleId} on {ClassDate}.", classScheduleId, classDate.ToString("yyyy-MM-dd"));
                return (BookingResult.NoAvailablePlaces, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during booking for class schedule {ClassScheduleId} on {ClassDate}.", classScheduleId, classDate.ToString("yyyy-MM-dd"));
                return (BookingResult.UnknownError, null);
            }
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(int userId)
        {
            _logger.LogInformation("Fetching bookings for user {UserId}.", userId);
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