namespace FitnessClub.BLL.Dtos
{
    public class LocationDto
    {
        public int LocationId { get; set; }
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
    }
}