namespace FitnessClub.BLL.Dtos
{
    public class ClubDto
    {
        public int ClubId { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public bool HasPool { get; set; }
    }
}