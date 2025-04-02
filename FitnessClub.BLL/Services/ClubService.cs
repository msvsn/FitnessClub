using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using System.Collections.Generic;
using System.Linq;

namespace FitnessClub.BLL.Services
{
    public class ClubService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClubService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<ClubDto> GetAllClubs() => _mapper.Map<IEnumerable<ClubDto>>(_unitOfWork.Clubs.GetAll());
        
        public ClubDto GetClubById(int id)
        {
            var club = _unitOfWork.Clubs.Query().FirstOrDefault(c => c.ClubId == id);
            return _mapper.Map<ClubDto>(club);
        }
    }
}