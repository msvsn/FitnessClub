using System;

namespace FitnessClub.BLL.Dtos
{
    public class MembershipDto
    {
        public int MembershipId { get; set; }
        public int UserId { get; set; }
        public int MembershipTypeId { get; set; }
        public string MembershipTypeName { get; set; }
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
        public bool IsNetworkMembership => ClubId == null;
    }
}