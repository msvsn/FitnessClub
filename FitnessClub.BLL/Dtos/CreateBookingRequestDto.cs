using System;

namespace FitnessClub.BLL.Dtos
{
    public class CreateBookingRequestDto
    {
        public int? UserId { get; set; }
        public string? GuestName { get; set; }
        public int ClassScheduleId { get; set; }
        public DateTime ClassDate { get; set; }
    }
} 