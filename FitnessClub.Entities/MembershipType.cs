using System.Collections.Generic;

namespace FitnessClub.Entities
{
    public class MembershipType
    {
        public int MembershipTypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsNetwork { get; set; }
        public bool IsSingleVisit { get; set; }
        public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    }
} 