using System;
using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class Membership
    {
        public int MembershipId { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public int MembershipTypeId { get; set; }
        public virtual MembershipType MembershipType { get; set; } = null!;
        public int? ClubId { get; set; }
        public virtual Club? Club { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}