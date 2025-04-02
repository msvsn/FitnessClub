using System;

namespace FitnessClub.BLL.Dtos
{
    public class ClassScheduleDto
    {
        public int ClassScheduleId { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ClassType { get; set; }
        public int TrainerId { get; set; }
        public string TrainerName { get; set; }
        public int Capacity { get; set; }
        public int BookedPlaces { get; set; }
        public int AvailablePlaces => Capacity - BookedPlaces;
    }
}