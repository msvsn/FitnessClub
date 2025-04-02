namespace FitnessClub.BLL.Dtos
{
    public class VisitDto
    {
        public int UserId { get; set; }
        public int ClassScheduleId { get; set; }
        // public DateTime VisitTime { get; set; } // Maybe track when they attended?
        // public bool IsOneTime { get; set; } // This seems redundant if booking type is tracked
    }
}