using System;
using System.ComponentModel.DataAnnotations;
using FitnessClub.BLL.Dtos;

namespace FitnessClub.Web.ViewModels
{
    public class BookingViewModel
    {
        [Required]
        public int ClassScheduleId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ClassDate { get; set; }
        public string? GuestName { get; set; }
        public string? ScheduleInfo { get; set; }
        public ClassScheduleDto? Schedule { get; set; }
    }
}
