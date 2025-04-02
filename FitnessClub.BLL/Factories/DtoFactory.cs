using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL.Entities;

namespace FitnessClub.BLL.Services
{
    public class DtoFactory : IDtoFactory
    {
        private readonly IMapper _mapper;

        public DtoFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public BookingDto CreateBookingDto(Booking booking) => _mapper.Map<BookingDto>(booking);
    }
}