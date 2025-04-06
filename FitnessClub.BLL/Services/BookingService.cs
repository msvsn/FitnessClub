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
using FitnessClub.BLL.Enums;

namespace FitnessClub.BLL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEnumerable<IBookingStrategy> _strategies;
        private readonly IRepository<ClassSchedule> _scheduleRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IMembershipService _membershipService;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IEnumerable<IBookingStrategy> strategies, IMembershipService membershipService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _scheduleRepository = _unitOfWork.GetRepository<ClassSchedule>();
            _bookingRepository = _unitOfWork.GetRepository<Booking>();
            _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
        }

        public async Task<(BookingResult Result, string? BookingId)> BookClassAsync(int userId, int classScheduleId, DateTime classDate)
        {
            var schedules = await _scheduleRepository.FindAsync(
                cs => cs.ClassScheduleId == classScheduleId,
                cs => cs.Club
            );
            var classSchedule = schedules.FirstOrDefault();

            if (classSchedule == null)
            {
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            var classDateOnly = classDate.Date;
            if (classDateOnly.DayOfWeek != classSchedule.DayOfWeek || classDateOnly < DateTime.Today)
            {
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            if (classSchedule.BookedPlaces >= classSchedule.Capacity)
            {
                return (BookingResult.NoAvailablePlaces, null);
            }

            var existingBooking = await _bookingRepository.FindAsync(b =>
                b.UserId == userId &&
                b.ClassScheduleId == classScheduleId &&
                b.ClassDate.Date == classDateOnly &&
                !b.IsCancelled);

            if (existingBooking.Any())
            {
                return (BookingResult.AlreadyBooked, null);
            }

            var userBookings = await _bookingRepository.FindAsync(b => 
                b.UserId == userId && 
                b.ClassDate.Date >= DateTime.Today && 
                !b.IsCancelled
            );
            int futureBookingsCount = userBookings.Count();
            int maxBookings = AppSettings.BookingSettings.MaxBookingsPerUser;
            if (futureBookingsCount >= maxBookings)
            {
                return (BookingResult.BookingLimitExceeded, null);
            }

            IBookingStrategy? strategy = _strategies.OfType<MembershipBookingStrategy>().FirstOrDefault();

            if (strategy == null)
            {
                return (BookingResult.StrategyNotFound, null);
            }

            var activeMembership = await _membershipService.GetActiveMembershipAsync(userId);

            if (activeMembership == null)
            {
                return (BookingResult.MembershipRequired, null);
            }
            
            bool isNetwork = activeMembership.IsNetworkMembership;
            int? membershipClubId = activeMembership.ClubId;
            int targetClubId = classSchedule.ClubId;
            MembershipDto? oneTimePass = null;

            if (!isNetwork && membershipClubId.HasValue && membershipClubId.Value != targetClubId)
            {
                oneTimePass = await _membershipService.GetActiveOneTimePassAsync(userId);

                if (oneTimePass == null)
                {
                    return (BookingResult.MembershipClubMismatch, null);
                }
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                var booking = strategy.CreateBooking(userId, classScheduleId, classDateOnly, user);

                classSchedule.BookedPlaces++;
                _scheduleRepository.Update(classSchedule);
                await _bookingRepository.AddAsync(booking);

                if (oneTimePass != null)
                {
                    await _membershipService.ConsumeOneTimePassAsync(oneTimePass.MembershipId);
                }

                await _unitOfWork.SaveAsync();

                return (BookingResult.Success, booking.BookingId.ToString());
            }
            catch (DbUpdateConcurrencyException)
            {
                return (BookingResult.NoAvailablePlaces, null);
            }
            catch (Exception)
            {
                return (BookingResult.UnknownError, null);
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            var bookings = await _bookingRepository.FindAsync(
                b => b.BookingId == bookingId,
                b => b.ClassSchedule
            );
            var booking = bookings.FirstOrDefault();

            if (booking == null)
            {
                return false;
            }

            if (booking.UserId != userId)
            {
                return false;
            }

            if (booking.ClassSchedule == null)
            {
                 return false;
            }

            var classDateTime = booking.ClassDate.Date + booking.ClassSchedule.StartTime;
            if (classDateTime < DateTime.UtcNow)
            {
                return false;
            }

            if (booking.IsCancelled)
            {
                return false;
            }

            try
            {
                booking.IsCancelled = true;
                _bookingRepository.Update(booking);

                if (booking.ClassSchedule.BookedPlaces > 0)
                {
                    booking.ClassSchedule.BookedPlaces--;
                    _scheduleRepository.Update(booking.ClassSchedule);
                }

                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _bookingRepository.FindAsync(
                b => b.UserId == userId,
                b => b.ClassSchedule.Club,
                b => b.ClassSchedule.Trainer,
                b => b.ClassSchedule,
                b => b.User
            );

            var sortedBookings = bookings.OrderByDescending(b => b.ClassDate);
            return _mapper.Map<List<BookingDto>>(sortedBookings);
        }
    }
}