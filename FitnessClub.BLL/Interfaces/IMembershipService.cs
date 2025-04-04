using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Services;

namespace FitnessClub.BLL.Interfaces
{
    public interface IMembershipService
    {
        Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync();
        Task<MembershipPurchaseResult> PurchaseMembershipAsync(int userId, int membershipTypeId, int? clubId);
        Task<MembershipDto?> GetActiveMembershipAsync(int userId);
    }
}