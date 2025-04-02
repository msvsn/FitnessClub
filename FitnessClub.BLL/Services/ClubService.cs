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

        public async Task<ClubDto> CreateClubAsync(ClubDto clubDto)
        {
            _logger.LogInformation("Creating a new club: {ClubName}", clubDto.Name);
            if (clubDto == null) 
            { 
                 _logger.LogError("Club DTO is null.");
                 throw new ArgumentNullException(nameof(clubDto));
            } 

            var clubEntity = _mapper.Map<Club>(clubDto);

            await _clubRepository.AddAsync(clubEntity);

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Club created successfully with ID: {ClubId}", clubEntity.Id);

            return _mapper.Map<ClubDto>(clubEntity);
        }

        public async Task UpdateClubAsync(int id, ClubDto clubDto)
        {
             _logger.LogInformation("Updating club with ID: {ClubId}", id);
             if (id != clubDto.Id)
            {
                _logger.LogError("Club ID mismatch in update request.");
                throw new ArgumentException("ID mismatch");
            }
             if (clubDto == null)
            { 
                 _logger.LogError("Club DTO is null for update.");
                 throw new ArgumentNullException(nameof(clubDto));
            } 

            var existingClub = await _clubRepository.GetByIdAsync(id);
            if (existingClub == null)
            {
                _logger.LogWarning("Club with ID: {ClubId} not found for update.", id);
                throw new KeyNotFoundException($"Club with ID {id} not found");
            }

            _mapper.Map(clubDto, existingClub);

            _clubRepository.Update(existingClub);

            await _unitOfWork.SaveAsync();
             _logger.LogInformation("Club with ID: {ClubId} updated successfully.", id);
        }

        public async Task DeleteClubAsync(int id)
        {
            _logger.LogInformation("Deleting club with ID: {ClubId}", id);
            var clubToDelete = await _clubRepository.GetByIdAsync(id);
            if (clubToDelete == null)
            {
                 _logger.LogWarning("Club with ID: {ClubId} not found for deletion.", id);
                 throw new KeyNotFoundException($"Club with ID {id} not found");
            }

            await _clubRepository.DeleteByIdAsync(id);

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Club with ID: {ClubId} deleted successfully.", id);
        }
    }
}