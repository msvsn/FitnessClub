using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.BLL.Interfaces
{
    public interface IClubService
    {
        Task<IEnumerable<ClubDto>> GetAllClubsAsync();
        Task<ClubDto?> GetClubByIdAsync(int id);
        Task<ClubDto> CreateClubAsync(ClubDto clubDto);
        Task<bool> UpdateClubAsync(int clubId, ClubDto clubDto);
        Task<bool> DeleteClubAsync(int clubId);
    }
}
