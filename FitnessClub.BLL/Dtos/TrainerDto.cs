namespace FitnessClub.BLL.Dtos
{
    public class TrainerDto
    {
        public int TrainerId { get; set; }
        public string Name { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public int ClubId { get; set; }
    }
}