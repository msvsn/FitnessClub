using FitnessClub.BLL.Dtos;
using System.Collections.Generic;

namespace FitnessClub.Web.ViewModels
{
    public class UserProfileViewModel
    {
        public UserDto User { get; set; } = null!;
        public IEnumerable<MembershipDto> Memberships { get; set; } = new List<MembershipDto>();
    }
} 