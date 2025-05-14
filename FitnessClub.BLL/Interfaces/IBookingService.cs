using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Enums;

namespace FitnessClub.BLL.Interfaces
{
    public interface IBookingService
    {
        Task<(BookingResult Result, string? BookingId)> BookClassAsync(int? userId, string? guestName, int classScheduleId, DateTime classDate);
        Task<List<BookingDto>> GetUserBookingsAsync(int userId);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
        Task<BookingDto?> GetBookingByIdAsync(int id);
        Task<bool> UpdateBookingAsync(int id, BookingDto bookingDto);
    }
}