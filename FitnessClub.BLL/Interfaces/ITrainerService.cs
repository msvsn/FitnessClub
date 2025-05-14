using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.BLL.Interfaces
{
    public interface ITrainerService
    {
        Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
        Task<IEnumerable<TrainerDto>> GetTrainersByClubAsync(int clubId);
        Task<TrainerDto?> GetTrainerByIdAsync(int id);
        Task<TrainerDto?> CreateTrainerAsync(TrainerDto trainerDto);
        Task<bool> UpdateTrainerAsync(int id, TrainerDto trainerDto);
        Task<bool> DeleteTrainerAsync(int id);
    }
}
