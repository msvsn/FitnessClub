using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ClassScheduleService> _logger;
        private readonly IRepository<ClassSchedule> _scheduleRepository;

        public ClassScheduleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClassScheduleService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scheduleRepository = _unitOfWork.GetRepository<ClassSchedule>();
        }

        public async Task<IEnumerable<ClassScheduleDto>> GetSchedulesByClubAndDateAsync(int clubId, DateTime date)
        {
            _logger.LogInformation("Fetching schedules for Club ID: {ClubId}, Date: {Date}", clubId, date.ToString("yyyy-MM-dd"));
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
            _logger.LogInformation("Fetching class schedule by ID: {ScheduleId}", id);
            var schedules = await _scheduleRepository.FindAsync(
                cs => cs.ClassScheduleId == id,
                cs => cs.Club,
                cs => cs.Trainer
            );
            var schedule = schedules.FirstOrDefault();

            if (schedule == null)
            {
                _logger.LogWarning("Class schedule with ID: {ScheduleId} not found.", id);
                return null;
            }

            return _mapper.Map<ClassScheduleDto>(schedule);
        }
    }
}