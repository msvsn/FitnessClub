using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FitnessClub.BLL.Services
{
    public class ClassScheduleService : IClassScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassScheduleService> _logger;

        public ClassScheduleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClassScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ClassScheduleDto>> GetSchedulesByClubAndDateAsync(int clubId, DateTime date)
        {
            _logger.LogInformation("Fetching schedules for Club ID: {ClubId}, Date: {Date}", clubId, date.ToString("yyyy-MM-dd"));
            var schedules = await _unitOfWork.ClassSchedules.Query()
                .Include(cs => cs.Club)
                .Include(cs => cs.Trainer)
                .Where(cs => cs.ClubId == clubId && cs.DayOfWeek == date.DayOfWeek)
                .OrderBy(cs => cs.StartTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClassScheduleDto>>(schedules);
        }

        public async Task<ClassScheduleDto?> GetClassScheduleByIdAsync(int id)
        {
            _logger.LogInformation("Fetching class schedule by ID: {ScheduleId}", id);
            var schedule = await _unitOfWork.ClassSchedules.Query()
                .Include(cs => cs.Club)
                .Include(cs => cs.Trainer)
                .FirstOrDefaultAsync(cs => cs.ClassScheduleId == id);

            if (schedule == null)
            {
                _logger.LogWarning("Class schedule with ID: {ScheduleId} not found.", id);
                return null;
            }

            return _mapper.Map<ClassScheduleDto>(schedule);
        }
    }
}