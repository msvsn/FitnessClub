using System;
using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class ClassSchedule
    {
        public int ClassScheduleId { get; set; }
        public int ClubId { get; set; }
        public Club Club { get; set; }
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }
        public string ClassType { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public int BookedPlaces { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}