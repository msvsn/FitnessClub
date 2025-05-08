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
    public class ClassScheduleService : IClassScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<ClassSchedule> _scheduleRepository;

        public ClassScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _scheduleRepository = _unitOfWork.GetRepository<ClassSchedule>();
        }

        public async Task<IEnumerable<ClassScheduleDto>> GetSchedulesByClubAndDateAsync(int clubId, DateTime date)
        {
            var schedules = await _scheduleRepository.FindAsync(
                cs => cs.ClubId == clubId && cs.DayOfWeek == date.DayOfWeek,
                cs => cs.Club,
                cs => cs.Trainer
            );

            var sortedSchedules = schedules.OrderBy(cs => cs.StartTime);
            return _mapper.Map<IEnumerable<ClassScheduleDto>>(sortedSchedules);
        }

        public async Task<ClassScheduleDto?> GetClassScheduleByIdAsync(int id)
        {
            var schedules = await _scheduleRepository.FindAsync(
                cs => cs.ClassScheduleId == id,
                cs => cs.Club,
                cs => cs.Trainer
            );
            var schedule = schedules.FirstOrDefault();
            if (schedule == null)
            {
                return null;
            }

            return _mapper.Map<ClassScheduleDto>(schedule);
        }
    }
}