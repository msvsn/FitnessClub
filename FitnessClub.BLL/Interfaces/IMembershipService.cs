using FitnessClub.BLL.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Interfaces
{
    public interface IMembershipService
    {
        Task<MembershipDto> PurchaseMembershipAsync(int userId, string type, int months);
        Task<List<MembershipDto>> GetAllMembershipsWithUsersAsync();
        Task<MembershipDto> GetMembershipWithUserLazyAsync(int membershipId);
    }
}