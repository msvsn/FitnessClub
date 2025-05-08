using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Helpers;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.Entities;
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

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IEnumerable<IBookingStrategy> strategies)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _scheduleRepository = _unitOfWork.GetRepository<ClassSchedule>();
            _bookingRepository = _unitOfWork.GetRepository<Booking>();
        }

        public async Task<(BookingResult Result, string? BookingId)> BookClassAsync(int? userId, string? guestName, int classScheduleId, DateTime classDate)
        {
            if (!userId.HasValue && string.IsNullOrWhiteSpace(guestName))
            {
                return (BookingResult.UserOrGuestRequired, null);
            }
            if (userId.HasValue && !string.IsNullOrWhiteSpace(guestName))
            {
                guestName = null;
            }

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

            if (userId.HasValue)
            {
                var existingBooking = await _bookingRepository.FindAsync(b =>
                    b.UserId == userId.Value &&
                    b.ClassScheduleId == classScheduleId &&
                    b.ClassDate.Date == classDateOnly);

                if (existingBooking.Any())
                {
                    return (BookingResult.AlreadyBooked, null);
                }
            }

            IBookingStrategy? strategy = null;
            bool requiresMembershipCheck = false;

            if (userId.HasValue)
            {
                var userBookings = await _bookingRepository.FindAsync(b => b.UserId == userId.Value && b.ClassDate.Date >= DateTime.Today);
                int futureBookingsCount = userBookings.Count();

                int maxBookings = AppSettings.BookingSettings.MaxBookingsPerUser;
                if (futureBookingsCount >= maxBookings)
                {
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
                return (BookingResult.StrategyNotFound, null);
            }

            Membership? singleVisitMembershipToConsume = null;

            if (requiresMembershipCheck && userId.HasValue)
            {
                var membershipsValidOnClassDate = await _unitOfWork.GetRepository<Membership>().FindAsync(
                    m => m.UserId == userId.Value && 
                         m.StartDate.Date <= classDateOnly &&
                         m.EndDate.Date >= classDateOnly,
                    m => m.MembershipType, 
                    m => m.Club          
                );

                if (!membershipsValidOnClassDate.Any())
                {
                     return (BookingResult.MembershipRequired, null);
                }

                bool hasValidMainMembership = membershipsValidOnClassDate.Any(m => 
                    m.MembershipType != null && !m.MembershipType.IsSingleVisit && 
                    (m.MembershipType.IsNetwork || m.ClubId == classSchedule.ClubId)
                );

                if (!hasValidMainMembership)
                {
                    singleVisitMembershipToConsume = membershipsValidOnClassDate
                        .Where(m => m.MembershipType != null && m.MembershipType.IsSingleVisit && !m.IsUsed)
                        .OrderByDescending(m => m.EndDate)
                        .FirstOrDefault();

                    if (singleVisitMembershipToConsume == null)
                    {
                        bool hasAnyMembershipForClub = membershipsValidOnClassDate.Any(m => m.ClubId == classSchedule.ClubId);
                        if (hasAnyMembershipForClub) 
                            return (BookingResult.MembershipRequired, null);
                        else 
                            return (BookingResult.MembershipClubMismatch, null);
                    }
                }
            }

            try
            {
                var booking = strategy.CreateBooking(userId, guestName, classScheduleId, classDateOnly);

                classSchedule.BookedPlaces++;
                _scheduleRepository.Update(classSchedule);
                await _bookingRepository.AddAsync(booking);

                if (singleVisitMembershipToConsume != null)
                {
                    singleVisitMembershipToConsume.IsUsed = true;
                    _unitOfWork.GetRepository<Membership>().Update(singleVisitMembershipToConsume);
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

            try
            {
                if (booking.ClassSchedule.BookedPlaces > 0)
                {
                    booking.ClassSchedule.BookedPlaces--;
                    _scheduleRepository.Update(booking.ClassSchedule);
                }
                _bookingRepository.Delete(booking);
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
                b => b.ClassSchedule
            );

            var sortedBookings = bookings.OrderByDescending(b => b.ClassDate);

            return _mapper.Map<List<BookingDto>>(sortedBookings);
        }
    }
}