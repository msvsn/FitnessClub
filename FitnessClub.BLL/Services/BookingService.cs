using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.BLL.Services
{
    public enum BookingResult
    {
        Success,
        InvalidScheduleOrDate,
        NoAvailablePlaces,
        UserOrGuestRequired
    }

    public class BookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, UserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public (BookingResult Result, string BookingId) BookClass(int? userId, string guestName, int classScheduleId, DateTime classDate)
        {
            var classSchedule = _unitOfWork.ClassSchedules.Query().Include(cs => cs.Club).FirstOrDefault(cs => cs.ClassScheduleId == classScheduleId);
            
            if (classSchedule == null || classDate.DayOfWeek != classSchedule.DayOfWeek || classDate <= DateTime.Now)
                return (BookingResult.InvalidScheduleOrDate, null);
            
            if (classSchedule.BookedPlaces >= classSchedule.Capacity)
                return (BookingResult.NoAvailablePlaces, null);
            
            if (!userId.HasValue && string.IsNullOrEmpty(guestName))
                return (BookingResult.UserOrGuestRequired, null);

            bool isMembershipBooking = userId.HasValue && _userService.HasValidMembershipForClub(userId.Value, classSchedule.ClubId, classDate);
            var booking = new Booking
            {
                UserId = userId,
                GuestName = guestName,
                ClassScheduleId = classScheduleId,
                ClassDate = classDate,
                BookingDate = DateTime.Now,
                IsMembershipBooking = isMembershipBooking
            };

            classSchedule.BookedPlaces++;
            _unitOfWork.Bookings.Add(booking);
            _unitOfWork.Save();
            return (BookingResult.Success, booking.BookingId.ToString());
        }
    
        public List<BookingDto> GetUserBookings(int userId)
        {
            var bookings = _unitOfWork.Bookings.Query()
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Club)
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Trainer)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.ClassDate)
                .ToList();
                
            return _mapper.Map<List<BookingDto>>(bookings);
        }
    }
}