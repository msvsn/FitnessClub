using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;

namespace FitnessClub.BLL.Services
{
    public class ClubService : IClubService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<Club> _clubRepository;

        public ClubService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _clubRepository = _unitOfWork.GetRepository<Club>();
        }

        public async Task<IEnumerable<ClubDto>> GetAllClubsAsync()
        {
            var clubs = await _clubRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ClubDto>>(clubs);
        }

        public async Task<ClubDto?> GetClubByIdAsync(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
            {
                return null;
            }
            return _mapper.Map<ClubDto>(club);
        }
    }
}