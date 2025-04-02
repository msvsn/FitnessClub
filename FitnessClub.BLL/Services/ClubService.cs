using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.DAL;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FitnessClub.BLL.Services
{
    public class ClubService : IClubService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClubService> _logger;

        public ClubService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClubService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ClubDto>> GetAllClubsAsync()
        {
            _logger.LogInformation("Fetching all clubs.");
            var clubs = await _unitOfWork.Clubs.GetAllAsync();
            return _mapper.Map<IEnumerable<ClubDto>>(clubs);
        }

        public async Task<ClubDto?> GetClubByIdAsync(int id)
        {
            _logger.LogInformation("Fetching club by ID: {ClubId}", id);
            var club = await _unitOfWork.Clubs.GetByIdAsync(id);

             if (club == null)
            {
                _logger.LogWarning("Club with ID: {ClubId} not found.", id);
                return null;
            }
            return _mapper.Map<ClubDto>(club);
        }
    }
}