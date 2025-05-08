namespace FitnessClub.BLL.Dtos
{
    public class MembershipTypeDto
    {
        public int MembershipTypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsNetwork { get; set; }
        public string? Description { get; set; }
        public bool IsSingleVisit { get; set; }
    }
}