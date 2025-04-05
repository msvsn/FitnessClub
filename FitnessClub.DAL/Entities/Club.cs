using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class Club
    {
        public int ClubId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public bool HasPool { get; set; }
        public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
    }
}