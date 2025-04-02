using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TrainerService> _logger;
        private readonly IRepository<Trainer> _trainerRepository;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TrainerService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trainerRepository = _unitOfWork.GetRepository<Trainer>();
        }

        public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
        {
            _logger.LogInformation("Fetching all trainers.");
            var trainers = await _trainerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TrainerDto>>(trainers);
        }

        public async Task<IEnumerable<TrainerDto>> GetTrainersByClubAsync(int clubId)
        {
            _logger.LogInformation("Fetching trainers for Club ID: {ClubId}.", clubId);
            var trainers = await _trainerRepository.FindAsync(t => t.ClubId == clubId);
            return _mapper.Map<IEnumerable<TrainerDto>>(trainers);
        }

        public async Task<TrainerDto?> GetTrainerByIdAsync(int id)
        {
            _logger.LogInformation("Fetching trainer by ID: {TrainerId}", id);
            var trainer = await _trainerRepository.GetByIdAsync(id);
            if (trainer == null)
            {
                _logger.LogWarning("Trainer with ID: {TrainerId} not found.", id);
                return null;
            }
            return _mapper.Map<TrainerDto>(trainer);
        }
    }
}