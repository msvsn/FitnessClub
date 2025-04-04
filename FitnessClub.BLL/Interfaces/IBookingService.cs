using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Services;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingService
    {
        Task<(BookingResult Result, string? BookingId)> BookClassAsync(int? userId, string? guestName, int classScheduleId, DateTime classDate);
        Task<List<BookingDto>> GetUserBookingsAsync(int userId);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
    }
}