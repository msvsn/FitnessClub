using System;

namespace FitnessClub.DAL.Entities
{
    public class Membership
    {
        public int MembershipId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int MembershipTypeId { get; set; }
        public MembershipType MembershipType { get; set; }
        public int? ClubId { get; set; }
        public Club? Club { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}