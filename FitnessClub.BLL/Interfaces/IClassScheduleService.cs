using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.BLL.Interfaces
{
    public interface IClassScheduleService
    {
        Task<IEnumerable<ClassScheduleDto>> GetSchedulesByClubAndDateAsync(int clubId, DateTime date);
        Task<ClassScheduleDto?> GetClassScheduleByIdAsync(int id);
        Task<IEnumerable<ClassScheduleDto>> GetAllClassSchedulesAsync();
        Task<ClassScheduleDto?> CreateClassScheduleAsync(ClassScheduleDto classScheduleDto);
        Task<bool> UpdateClassScheduleAsync(int id, ClassScheduleDto classScheduleDto);
        Task<bool> DeleteClassScheduleAsync(int id);
    }
}
