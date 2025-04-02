using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessClub.BLL.Services
{
    public class ClassScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<ClassScheduleDto> GetSchedulesByClubAndDate(int clubId, DateTime date) =>
            _mapper.Map<IEnumerable<ClassScheduleDto>>(
                _unitOfWork.ClassSchedules.Query()
                    .Include(cs => cs.Club)
                    .Include(cs => cs.Trainer)
                    .Where(cs => cs.ClubId == clubId && cs.DayOfWeek == date.DayOfWeek));

        public ClassScheduleDto GetClassScheduleById(int id)
        {
            var schedule = _unitOfWork.ClassSchedules.Query()
                .Include(cs => cs.Club)
                .Include(cs => cs.Trainer)
                .FirstOrDefault(cs => cs.ClassScheduleId == id);
            return _mapper.Map<ClassScheduleDto>(schedule);
        }
    }
}