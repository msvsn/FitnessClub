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
        private readonly IClassScheduleService _classScheduleService;
        private readonly IEnumerable<IBookingStrategy> _strategies;
        private readonly IRepository<ClassSchedule> _scheduleRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IMembershipService _membershipService;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IClassScheduleService classScheduleService, IEnumerable<IBookingStrategy> strategies, IMembershipService membershipService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _classScheduleService = classScheduleService ?? throw new ArgumentNullException(nameof(classScheduleService));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _scheduleRepository = _unitOfWork.GetRepository<ClassSchedule>();
            _bookingRepository = _unitOfWork.GetRepository<Booking>();
            _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
        }

        public async Task<(BookingResult Result, string? BookingId)> BookClassAsync(int userId, int classScheduleId, DateTime classDate)
        {
            var classScheduleDto = await _classScheduleService.GetAndValidateClassScheduleAsync(classScheduleId, classDate);
            if (classScheduleDto == null)
            {
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            var classScheduleEntity = (await _scheduleRepository.FindAsync(cs => cs.ClassScheduleId == classScheduleId)).FirstOrDefault();
            if (classScheduleEntity == null) 
            {
                return (BookingResult.InvalidScheduleOrDate, null);
            }

            var availabilityCheck = await CheckAvailabilityAndExistingBookingAsync(userId, classScheduleId, classDate.Date, classScheduleEntity);
            if (availabilityCheck != BookingResult.Success)
            {
                return (availabilityCheck, null);
            }

            var bookingLimitCheck = await ValidateUserBookingLimitAsync(userId);
            if (bookingLimitCheck != BookingResult.Success)
            {
                return (bookingLimitCheck, null);
            }

            var (membershipCheck, oneTimePass) = await VerifyMembershipAndClubAccessAsync(userId, classScheduleDto.ClubId);
            if (membershipCheck != BookingResult.Success)
            {
                return (membershipCheck, null);
            }

            return await PerformBookingCreationAsync(userId, classScheduleId, classDate.Date, classScheduleEntity, oneTimePass);
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await GetAndValidateBookingForCancellationAsync(bookingId, userId);
            if (booking == null)
            {
                return false;
            }

            return await PerformBookingCancellationAsync(booking);
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

        private async Task<BookingResult> CheckAvailabilityAndExistingBookingAsync(int userId, int classScheduleId, DateTime classDateOnly, ClassSchedule classSchedule)
        {
            if (classSchedule.BookedPlaces >= classSchedule.Capacity)
            {
                return BookingResult.NoAvailablePlaces;
            }

            var existingBooking = await _bookingRepository.FindAsync(b =>
                b.UserId == userId &&
                b.ClassScheduleId == classScheduleId &&
                b.ClassDate.Date == classDateOnly &&
                !b.IsCancelled);

            if (existingBooking.Any())
            {
                return BookingResult.AlreadyBooked;
            }

            return BookingResult.Success;
        }

        private async Task<BookingResult> ValidateUserBookingLimitAsync(int userId)
        {
            var userBookings = await _bookingRepository.FindAsync(b => 
                b.UserId == userId && 
                b.ClassDate.Date >= DateTime.Today && 
                !b.IsCancelled
            );
            int futureBookingsCount = userBookings.Count();
            int maxBookings = AppSettings.BookingSettings.MaxBookingsPerUser;

            if (futureBookingsCount >= maxBookings)
            {
                return BookingResult.BookingLimitExceeded;
            }

            return BookingResult.Success;
        }

        private async Task<(BookingResult Result, MembershipDto? OneTimePass)> VerifyMembershipAndClubAccessAsync(int userId, int targetClubId)
        {
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
            MembershipDto? oneTimePass = null;

            if (!isNetwork && membershipClubId.HasValue && membershipClubId.Value != targetClubId)
            {
                oneTimePass = await _membershipService.GetActiveOneTimePassAsync(userId);
                if (oneTimePass == null)
                {
                    return (BookingResult.MembershipClubMismatch, null);
                }
            }

            return (BookingResult.Success, oneTimePass);
        }

        private async Task<(BookingResult Result, string? BookingId)> PerformBookingCreationAsync(int userId, int classScheduleId, DateTime classDateOnly, ClassSchedule classSchedule, MembershipDto? oneTimePass)
        {
             IBookingStrategy? strategy = _strategies.OfType<MembershipBookingStrategy>().FirstOrDefault();
            if (strategy == null) return (BookingResult.StrategyNotFound, null); 

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

        private async Task<Booking?> GetAndValidateBookingForCancellationAsync(int bookingId, int userId)
        {
             var bookings = await _bookingRepository.FindAsync(
                b => b.BookingId == bookingId,
                b => b.ClassSchedule
            );
            var booking = bookings.FirstOrDefault();

            if (booking == null || booking.UserId != userId || booking.IsCancelled)
            {
                return null;
            }
            
            if (booking.ClassSchedule == null) {
                 return null; 
            }

            var classDateTime = booking.ClassDate.Date + booking.ClassSchedule.StartTime;
            if (classDateTime < DateTime.UtcNow)
            {
                return null; 
            }

            return booking;
        }

        private async Task<bool> PerformBookingCancellationAsync(Booking booking)
        {
            try
            {
                booking.IsCancelled = true;
                _bookingRepository.Update(booking);

                if (booking.ClassSchedule != null && booking.ClassSchedule.BookedPlaces > 0)
                {
                    booking.ClassSchedule.BookedPlaces--;
                    _scheduleRepository.Update(booking.ClassSchedule);
                }
                else if (booking.ClassSchedule == null)
                {
                    return false;
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
    }
}