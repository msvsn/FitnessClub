using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Enums;

namespace FitnessClub.BLL.Interfaces
{
    public interface IMembershipService
    {
        Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync();
        Task<MembershipPurchaseResult> PurchaseMembershipAsync(int userId, int membershipTypeId, int? clubId);
        Task<MembershipDto?> GetActiveMembershipAsync(int userId);
        Task<IEnumerable<MembershipDto>> GetAllUserMembershipsAsync(int userId);
        Task<MembershipTypeDto?> GetMembershipTypeByIdAsync(int id);
        Task<MembershipTypeDto?> CreateMembershipTypeAsync(MembershipTypeDto membershipTypeDto);
        Task<bool> UpdateMembershipTypeAsync(int id, MembershipTypeDto membershipTypeDto);
        Task<bool> DeleteMembershipTypeAsync(int id);
    }
}