using FitnessClub.BLL.Dtos;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessClub.Web.ViewModels
{
    public class MembershipViewModel
    {
        public IEnumerable<MembershipTypeDto> MembershipTypes { get; set; } = new List<MembershipTypeDto>();
        public MembershipDto? ActiveMembership { get; set; }
        public IEnumerable<MembershipTypeDto> AvailableTypes { get; set; } = Enumerable.Empty<MembershipTypeDto>();
        public bool IsUserAuthenticated { get; set; }
        public string? LoginUrlWithReturn { get; set; }
    }
} 