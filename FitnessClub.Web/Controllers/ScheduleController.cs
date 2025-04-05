using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Services;
using FitnessClub.BLL.Dtos;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using FitnessClub.BLL.Interfaces;
using System.Linq;

namespace FitnessClub.Web.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IClassScheduleService _scheduleService;
        private readonly IClubService _clubService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IClassScheduleService scheduleService, IClubService clubService, ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _clubService = clubService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? clubId, DateTime? date)
        {
            _logger.LogInformation("Fetching schedule for ClubId: {ClubId}, Date: {Date}", clubId, date);
            DateTime selectedDate = date ?? DateTime.Today;
            
            IEnumerable<ClubDto> clubs = new List<ClubDto>();
            try
            {
                clubs = await _clubService.GetAllClubsAsync();
                _logger.LogInformation("Retrieved {ClubCount} clubs.", clubs.Count());
                foreach (var club in clubs)
                {
                    _logger.LogDebug("Club ID: {ClubId}, Name: {ClubName}", club.ClubId, club.Name); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching clubs.");
                ViewBag.ErrorMessage = "Error loading club list.";
            }
            
            var clubsSelectList = new SelectList(clubs, "ClubId", "Name", clubId);

            IEnumerable<ClassScheduleDto> schedules = new List<ClassScheduleDto>();
            string selectedClubName = "All Clubs";
            if (clubId.HasValue)
            {
                try
                {
                     schedules = await _scheduleService.GetSchedulesByClubAndDateAsync(clubId.Value, selectedDate);
                    _logger.LogInformation("Service returned {ScheduleCount} schedules for ClubId: {ClubId}, Date: {Date}", 
                                             schedules.Count(), clubId.Value, selectedDate.ToString("yyyy-MM-dd"));
                    selectedClubName = clubs.FirstOrDefault(c => c.ClubId == clubId)?.Name ?? "Unknown Club";
                    _logger.LogInformation("Retrieved {ScheduleCount} schedules for ClubId: {ClubId}", schedules.Count(), clubId.Value);
                }
                 catch (Exception ex)
                {
                     _logger.LogError(ex, "Error fetching schedules for ClubId: {ClubId}", clubId.Value);
                    ViewBag.ErrorMessage = "Error loading schedule.";
                }
            }

            ViewBag.Clubs = clubsSelectList;
            ViewBag.SelectedClubId = clubId;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.SelectedClubName = selectedClubName;

            if (!schedules.Any() && clubId.HasValue)
            {
                ViewBag.InfoMessage = $"Немає занять у клубі '{selectedClubName}' на {selectedDate:dd.MM.yyyy}." + (ViewBag.ErrorMessage != null ? " " + ViewBag.ErrorMessage : "");
            }
            else if (!clubId.HasValue && ViewBag.ErrorMessage == null)
            {
                ViewBag.InfoMessage = "Будь ласка, оберіть клуб для перегляду розкладу.";
            }
            else if(ViewBag.ErrorMessage != null)
            {
                 ViewBag.InfoMessage = ViewBag.ErrorMessage;
            }

            return View(schedules);
        }
    }
}