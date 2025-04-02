using FitnessClub.BLL.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Interfaces
{
    public interface IClassService
    {
        Task<ClassScheduleDto> RegisterForClassAsync(int userId, int classScheduleId);
        Task<List<ClassScheduleDto>> GetAllClassesWithLocationAsync();
        Task<ClassScheduleDto> GetClassWithVisitsLazyAsync(int classId);
    }
}