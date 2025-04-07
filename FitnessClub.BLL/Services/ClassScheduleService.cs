using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
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
            var dayOfWeek = date.DayOfWeek;
            var schedules = await _scheduleRepository.FindAsync(
                s => s.ClubId == clubId && s.DayOfWeek == dayOfWeek,
                s => s.Club,
                s => s.Trainer
            );
            return _mapper.Map<IEnumerable<ClassScheduleDto>>(schedules);
        }

        public async Task<ClassScheduleDto?> GetClassScheduleByIdAsync(int id)
        {
            var schedules = await _scheduleRepository.FindAsync(
                s => s.ClassScheduleId == id,
                s => s.Club, 
                s => s.Trainer
            );
            var schedule = schedules.FirstOrDefault();

            if (schedule == null)
            {
                return null;
            }
            return _mapper.Map<ClassScheduleDto>(schedule);
        }

        public async Task<ClassScheduleDto?> GetAndValidateClassScheduleAsync(int classScheduleId, DateTime classDate)
        {
            var schedules = await _scheduleRepository.FindAsync(
                cs => cs.ClassScheduleId == classScheduleId,
                cs => cs.Club
            );
            var classSchedule = schedules.FirstOrDefault();

            if (classSchedule == null) return null;

            var classDateOnly = classDate.Date;
            if (classDateOnly.DayOfWeek != classSchedule.DayOfWeek || classDateOnly < DateTime.Today)
            {
                return null;
            }

            return _mapper.Map<ClassScheduleDto>(classSchedule);
        }
    }
}