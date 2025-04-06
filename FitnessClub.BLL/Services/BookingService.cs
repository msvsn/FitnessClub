using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Helpers;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
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
        UserRequired,
        BookingLimitExceeded,
        MembershipRequired,
        MembershipClubMismatch,
        StrategyNotFound,
        AlreadyBooked,
        UnknownError
    }

    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEnumerable<IBookingStrategy> _strategies;
        private readonly ILogger<BookingService> _logger;
        private readonly IRepository<ClassSchedule> _scheduleRepository;
        private readonly IRepository<Booking> _bookingRepository;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IEnumerable<IBookingStrategy> strategies, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scheduleRepository = _unitOfWork.GetRepository<ClassSchedule>();
            _bookingRepository = _unitOfWork.GetRepository<Booking>();
        }

        public async Task<(BookingResult Result, string? BookingId)> BookClassAsync(int userId, int classScheduleId, DateTime classDate)
        {
            _logger.LogInformation("Attempting to book class schedule {ClassScheduleId} for date {ClassDate}, User: {UserId}",
                classScheduleId, classDate.ToString("yyyy-MM-dd"), userId);

            var schedules = await _scheduleRepository.FindAsync(
                cs => cs.ClassScheduleId == classScheduleId,
                cs => cs.Club
            );
            var classSchedule = schedules.FirstOrDefault();

            if (classSchedule == null)
            {
                _logger.LogWarning("Booking failed: Class schedule {ClassScheduleId} not found.", classScheduleId);
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            var classDateOnly = classDate.Date;
            if (classDateOnly.DayOfWeek != classSchedule.DayOfWeek || classDateOnly < DateTime.Today)
            {
                _logger.LogWarning("Booking failed: Invalid date {ClassDate} for schedule {ClassScheduleId} (DayOfWeek: {ScheduleDay}, Today: {Today}).",
                    classDateOnly.ToString("yyyy-MM-dd"), classScheduleId, classSchedule.DayOfWeek, DateTime.Today.ToString("yyyy-MM-dd"));
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            if (classSchedule.BookedPlaces >= classSchedule.Capacity)
            {
                _logger.LogWarning("Booking failed: No available places for class schedule {ClassScheduleId} on {ClassDate}. Capacity: {Capacity}, Booked: {Booked}",
                    classScheduleId, classDateOnly.ToString("yyyy-MM-dd"), classSchedule.Capacity, classSchedule.BookedPlaces);
                return (BookingResult.NoAvailablePlaces, null);
            }

            var existingBooking = await _bookingRepository.FindAsync(b =>
                b.UserId == userId &&
                b.ClassScheduleId == classScheduleId &&
                b.ClassDate.Date == classDateOnly);

            if (existingBooking.Any())
            {
                _logger.LogWarning("Booking failed: User {UserId} is already booked for class schedule {ClassScheduleId} on {ClassDate}.",
                    userId, classScheduleId, classDateOnly.ToString("yyyy-MM-dd"));
                return (BookingResult.AlreadyBooked, null);
            }

            var userBookings = await _bookingRepository.FindAsync(b => b.UserId == userId && b.ClassDate.Date >= DateTime.Today);
            int futureBookingsCount = userBookings.Count();
            int maxBookings = AppSettings.BookingSettings.MaxBookingsPerUser;
            if (futureBookingsCount >= maxBookings)
            {
                _logger.LogWarning("Booking failed: User {UserId} has reached the booking limit ({MaxBookings}).", userId, maxBookings);
                return (BookingResult.BookingLimitExceeded, null);
            }

            IBookingStrategy? strategy = _strategies.OfType<MembershipBookingStrategy>().FirstOrDefault();

            if (strategy == null)
            {
                _logger.LogError("Booking failed: Membership booking strategy not found for User: {UserId}. Check DI configuration.", userId);
                return (BookingResult.StrategyNotFound, null);
            }

            bool canBook = await strategy.CanBookAsync(userId, classSchedule.ClubId, classDateOnly);

            if (!canBook)
            {
                _logger.LogWarning("Booking failed via strategy: User {UserId} cannot book class {ClassScheduleId} for club {ClubId} on date {ClassDate}. Check membership.",
                    userId, classScheduleId, classSchedule.ClubId, classDateOnly.ToString("yyyy-MM-dd"));
                return (BookingResult.MembershipRequired, null);
            }

            try
            {
                var booking = strategy.CreateBooking(userId, classScheduleId, classDateOnly);

                classSchedule.BookedPlaces++;
                _scheduleRepository.Update(classSchedule);
                await _bookingRepository.AddAsync(booking);

                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Booking successful via strategy for class schedule {ClassScheduleId} on {ClassDate}. Booking ID: {BookingId}",
                    classScheduleId, classDateOnly.ToString("yyyy-MM-dd"), booking.BookingId);

                return (BookingResult.Success, booking.BookingId.ToString());
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Booking failed due to concurrency conflict for class schedule {ClassScheduleId} on {ClassDate}. Places might be full.",
                    classScheduleId, classDateOnly.ToString("yyyy-MM-dd"));
                return (BookingResult.NoAvailablePlaces, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during booking for class schedule {ClassScheduleId} on {ClassDate} for User {UserId}.",
                    classScheduleId, classDateOnly.ToString("yyyy-MM-dd"), userId);
                return (BookingResult.UnknownError, null);
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            _logger.LogInformation("Attempting to cancel booking {BookingId} for user {UserId}.", bookingId, userId);

            var bookings = await _bookingRepository.FindAsync(
                b => b.BookingId == bookingId,
                b => b.ClassSchedule
            );
            var booking = bookings.FirstOrDefault();

            if (booking == null)
            {
                _logger.LogWarning("Cancel booking failed: Booking {BookingId} not found.", bookingId);
                return false;
            }

            if (booking.UserId != userId)
            {
                _logger.LogWarning("Cancel booking failed: User {UserId} attempted to cancel booking {BookingId} belonging to user {BookingUserId}.", userId, bookingId, booking.UserId);
                return false;
            }

            if (booking.ClassSchedule == null)
            {
                 _logger.LogError("Cancel booking failed: ClassSchedule associated with Booking {BookingId} not found (likely data inconsistency). Cannot verify cancellation window.", bookingId);
                 return false;
            }

            var classDateTime = booking.ClassDate.Date + booking.ClassSchedule.StartTime;
            if (classDateTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Cancel booking failed: Cannot cancel booking {BookingId} for user {UserId} because the class has already started or passed ({ClassDateTime}).", bookingId, userId, classDateTime);
                return false;
            }

            try
            {
                if (booking.ClassSchedule.BookedPlaces > 0)
                {
                    booking.ClassSchedule.BookedPlaces--;
                    _scheduleRepository.Update(booking.ClassSchedule);
                }
                else
                {
                     _logger.LogWarning("Cancel booking {BookingId}: ClassSchedule {ScheduleId} booked places was already 0. Proceeding with booking deletion.", bookingId, booking.ClassScheduleId);
                }

                await _bookingRepository.DeleteByIdAsync(booking.BookingId);

                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Booking {BookingId} cancelled successfully by user {UserId}.", bookingId, userId);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict during cancellation of booking {BookingId} for user {UserId}.", bookingId, userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while cancelling booking {BookingId} for user {UserId}.", bookingId, userId);
                return false;
            }
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(int userId)
        {
            _logger.LogInformation("Fetching bookings for user {UserId}.", userId);
            var bookings = await _bookingRepository.FindAsync(
                b => b.UserId == userId,
                b => b.ClassSchedule.Club,
                b => b.ClassSchedule.Trainer,
                b => b.ClassSchedule
            );

            var sortedBookings = bookings.OrderByDescending(b => b.ClassDate);

            foreach (var booking in sortedBookings)
            {
                 if (booking.ClassSchedule != null)
                 {
                      _logger.LogDebug("Booking ID {BookingId}: Schedule ID {ScheduleId}, StartTime={StartTime}, EndTime={EndTime}", 
                                        booking.BookingId, booking.ClassScheduleId, booking.ClassSchedule.StartTime, booking.ClassSchedule.EndTime);
                 }
                 else
                 {
                     _logger.LogWarning("Booking ID {BookingId}: ClassSchedule is null.", booking.BookingId);
                 }
            }

            return _mapper.Map<List<BookingDto>>(sortedBookings);
        }
    }
}