using System.Collections.Generic;
using FitnessClub.BLL.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Web.ViewModels
{
    public class PurchaseMembershipViewModel
    {
        [Required]
        public int MembershipTypeId { get; set; }
        public MembershipTypeDto? MembershipType { get; set; }
        public int? ClubId { get; set; }
        public SelectList? Clubs { get; set; }
    }
} 