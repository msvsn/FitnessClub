using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<ClassScheduleDto>> GetAllClassSchedulesAsync()
        {
            var query = _scheduleRepository.Query()
                .Include(cs => cs.Club)
                .Include(cs => cs.Trainer)
                .OrderBy(cs => cs.DayOfWeek);
            var schedulesFromDb = await query.ToListAsync();
            var clientSortedSchedules = schedulesFromDb
                                          .OrderBy(cs => cs.DayOfWeek)
                                          .ThenBy(cs => cs.StartTime);
            return _mapper.Map<IEnumerable<ClassScheduleDto>>(clientSortedSchedules);
        }

        public async Task<ClassScheduleDto?> CreateClassScheduleAsync(ClassScheduleDto classScheduleDto)
        {
            if (classScheduleDto == null) throw new ArgumentNullException(nameof(classScheduleDto));
            var schedule = _mapper.Map<ClassSchedule>(classScheduleDto);
            await _scheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<ClassScheduleDto>(schedule);
        }

        public async Task<bool> UpdateClassScheduleAsync(int id, ClassScheduleDto classScheduleDto)
        {
            if (classScheduleDto == null) throw new ArgumentNullException(nameof(classScheduleDto));
            if (id != classScheduleDto.ClassScheduleId) return false;
            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null) return false;
            _mapper.Map(classScheduleDto, existingSchedule);
            _scheduleRepository.Update(existingSchedule);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteClassScheduleAsync(int id)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null) return false;
            _scheduleRepository.Delete(schedule);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}