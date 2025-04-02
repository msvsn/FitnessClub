namespace FitnessClub.DAL.Entities
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }

        public int ClubId { get; set; }
        public Club Club { get; set; }
    }
}