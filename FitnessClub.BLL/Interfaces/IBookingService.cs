using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Services;
using static FitnessClub.BLL.Services.BookingService;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingService
    {
        Task<(BookingResult Result, string? BookingId)> BookClassAsync(int userId, int classScheduleId, DateTime classDate);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<List<BookingDto>> GetUserBookingsAsync(int userId);
    }
}