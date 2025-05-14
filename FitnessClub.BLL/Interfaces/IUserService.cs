using System;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.BLL.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(string firstName, string lastName, string username, string password);
        Task<UserDto?> LoginAsync(string username, string password);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> HasValidMembershipForClubAsync(int userId, int clubId, DateTime date);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> CreateUserAsync(UserDto userDto);
        Task<bool> UpdateUserAsync(int id, UserDto userDto);
        Task<bool> DeleteUserAsync(int id);
    }
}