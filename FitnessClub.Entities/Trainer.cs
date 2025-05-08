namespace FitnessClub.Entities
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public int ClubId { get; set; }
        public virtual Club Club { get; set; } = null!;
    }
} 