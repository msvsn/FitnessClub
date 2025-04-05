using FitnessClub.BLL.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessClub.Web.ViewModels
{
    public class ScheduleViewModel
    {
        public IEnumerable<ClassScheduleDto> Schedules { get; set; } = Enumerable.Empty<ClassScheduleDto>();
        public SelectList? Clubs { get; set; }
        public int? SelectedClubId { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string SelectedClubName { get; set; } = "Усі клуби";
        public string? InfoMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
