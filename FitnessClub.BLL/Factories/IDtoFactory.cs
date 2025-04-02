using FitnessClub.BLL.Dtos;
using FitnessClub.DAL.Entities;

namespace FitnessClub.BLL.Services
{
    public interface IDtoFactory
    {
        BookingDto CreateBookingDto(Booking booking);
    }
}