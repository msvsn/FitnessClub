using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class Club
    {
        public int ClubId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool HasPool { get; set; }
        public List<Trainer> Trainers { get; set; } = new List<Trainer>();
        public List<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
        public List<Membership> Memberships { get; set; } = new List<Membership>();
    }
}