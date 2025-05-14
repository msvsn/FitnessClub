using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<Trainer> _trainerRepository;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _trainerRepository = _unitOfWork.GetRepository<Trainer>();
        }

        public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
        {
            var trainers = await _trainerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TrainerDto>>(trainers);
        }

        public async Task<IEnumerable<TrainerDto>> GetTrainersByClubAsync(int clubId)
        {
            var trainers = await _trainerRepository.FindAsync(t => t.ClubId == clubId);
            return _mapper.Map<IEnumerable<TrainerDto>>(trainers);
        }

        public async Task<TrainerDto?> GetTrainerByIdAsync(int id)
        {
            var trainer = await _trainerRepository.GetByIdAsync(id);
            if (trainer == null)
            {
                return null;
            }
            return _mapper.Map<TrainerDto>(trainer);
        }

        public async Task<TrainerDto?> CreateTrainerAsync(TrainerDto trainerDto)
        {
            if (trainerDto == null) throw new ArgumentNullException(nameof(trainerDto));
            var trainer = _mapper.Map<Trainer>(trainerDto);
            await _trainerRepository.AddAsync(trainer);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<TrainerDto>(trainer);
        }

        public async Task<bool> UpdateTrainerAsync(int id, TrainerDto trainerDto)
        {
            if (trainerDto == null) throw new ArgumentNullException(nameof(trainerDto));
            if (id != trainerDto.TrainerId) return false;
            var existingTrainer = await _trainerRepository.GetByIdAsync(id);
            if (existingTrainer == null) return false;
            _mapper.Map(trainerDto, existingTrainer);
            _trainerRepository.Update(existingTrainer);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteTrainerAsync(int id)
        {
            var trainer = await _trainerRepository.GetByIdAsync(id);
            if (trainer == null) return false;
            _trainerRepository.Delete(trainer);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}