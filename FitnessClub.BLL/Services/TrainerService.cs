using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using System.Collections.Generic;

namespace FitnessClub.BLL.Services
{
    public class TrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<TrainerDto> GetAllTrainers() => _mapper.Map<IEnumerable<TrainerDto>>(_unitOfWork.Trainers.GetAll());
    }
}