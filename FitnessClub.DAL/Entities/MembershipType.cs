namespace FitnessClub.DAL.Entities
{
    public class MembershipType
    {
        public int MembershipTypeId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsNetwork { get; set; }
    }
}