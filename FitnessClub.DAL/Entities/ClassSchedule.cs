using System;
using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class ClassSchedule
    {
        public int ClassScheduleId { get; set; }
        public int ClubId { get; set; }
        public virtual Club Club { get; set; } = null!;
        public int TrainerId { get; set; }
        public virtual Trainer Trainer { get; set; } = null!;
        public string ClassType { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public int BookedPlaces { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}