using FitnessClub.BLL.Dtos;

namespace FitnessClub.Web.ViewModels
{
    public class UserProfileViewModel
    {
        public UserDto User { get; set; } = null!;
        public MembershipDto? ActiveMembership { get; set; }
    }
} 