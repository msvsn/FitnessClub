using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Services
{
    public class ClubService : IClubService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClubService> _logger;
        private readonly IRepository<Club> _clubRepository;

        public ClubService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClubService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clubRepository = _unitOfWork.GetRepository<Club>();
        }

        public async Task<IEnumerable<ClubDto>> GetAllClubsAsync()
        {
            _logger.LogInformation("Fetching all clubs.");
            var clubs = await _clubRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {ClubCount} clubs from repository.", clubs.Count());
            return _mapper.Map<IEnumerable<ClubDto>>(clubs);
        }

        public async Task<ClubDto?> GetClubByIdAsync(int id)
        {
            _logger.LogInformation("Fetching club by ID: {ClubId}", id);
            var club = await _clubRepository.GetByIdAsync(id);

            if (club == null)
            {
                _logger.LogWarning("Club with ID: {ClubId} not found.", id);
                return null;
            }
            return _mapper.Map<ClubDto>(club);
        }

        // CreateClubAsync, UpdateClubAsync, DeleteClubAsync removed as clubs are considered static.
    }
}