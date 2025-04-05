using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class MembershipType
    {
        public int MembershipTypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsNetwork { get; set; }
        public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    }
}