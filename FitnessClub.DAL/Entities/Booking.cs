using System;

namespace FitnessClub.DAL.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string? GuestName { get; set; }
        public int ClassScheduleId { get; set; }
        public ClassSchedule ClassSchedule { get; set; }
        public DateTime ClassDate { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsMembershipBooking { get; set; }
    }
}