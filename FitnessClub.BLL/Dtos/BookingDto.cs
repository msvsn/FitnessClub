using System;

namespace FitnessClub.BLL.Dtos
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int? UserId { get; set; }
        public string? GuestName { get; set; }
        public int ClassScheduleId { get; set; }
        public string ClassType { get; set; }
        public string ClubName { get; set; }
        public string TrainerName { get; set; }
        public DateTime ClassDate { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsMembershipBooking { get; set; }
    }
}