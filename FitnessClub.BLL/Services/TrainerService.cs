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
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TrainerService> _logger;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TrainerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
        {
             _logger.LogInformation("Fetching all trainers.");
            var trainers = await _unitOfWork.Trainers.GetAllAsync();
            return _mapper.Map<IEnumerable<TrainerDto>>(trainers);
        }

        public async Task<IEnumerable<TrainerDto>> GetTrainersByClubAsync(int clubId)
        {
             _logger.LogInformation("Fetching trainers for Club ID: {ClubId}.", clubId);
            var trainers = await _unitOfWork.Trainers.Query()
                .Where(t => t.ClubId == clubId)
                .ToListAsync();

             return _mapper.Map<IEnumerable<TrainerDto>>(trainers);
        }


        public async Task<TrainerDto?> GetTrainerByIdAsync(int id)
        {
            _logger.LogInformation("Fetching trainer by ID: {TrainerId}", id);
            var trainer = await _unitOfWork.Trainers.GetByIdAsync(id);
            if (trainer == null)
            {
                 _logger.LogWarning("Trainer with ID: {TrainerId} not found.", id);
                return null;
            }
            return _mapper.Map<TrainerDto>(trainer);
        }
    }
}