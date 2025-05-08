using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Services;
using FitnessClub.BLL.Dtos;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using FitnessClub.BLL.Interfaces;
using System.Linq;
using FitnessClub.Web.ViewModels;

namespace FitnessClub.Web.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IClassScheduleService _scheduleService;
        private readonly IClubService _clubService;

        public ScheduleController(IClassScheduleService scheduleService, IClubService clubService)
        {
            _scheduleService = scheduleService;
            _clubService = clubService;
        }

        public async Task<IActionResult> Index(int? clubId, DateTime? date)
        {
            DateTime selectedDate = date ?? DateTime.Today;

            var viewModel = new ScheduleViewModel
            {
                SelectedClubId = clubId,
                SelectedDate = selectedDate
            };

            IEnumerable<ClubDto> clubs = new List<ClubDto>();
            try
            {
                clubs = await _clubService.GetAllClubsAsync();
                viewModel.Clubs = new SelectList(clubs, "ClubId", "Name", clubId);
            }
            catch (Exception)
            {
                viewModel.ErrorMessage = "Помилка завантаження списку клубів.";
            }

            IEnumerable<ClassScheduleDto> schedules = new List<ClassScheduleDto>();
            if (clubId.HasValue && string.IsNullOrEmpty(viewModel.ErrorMessage))
            {
                try
                {
                     schedules = await _scheduleService.GetSchedulesByClubAndDateAsync(clubId.Value, selectedDate);
                    viewModel.SelectedClubName = clubs.FirstOrDefault(c => c.ClubId == clubId)?.Name ?? "Невідомий клуб";
                    viewModel.Schedules = schedules;

                    if (!schedules.Any())
                    {
                        viewModel.InfoMessage = $"Немає занять у клубі \'{viewModel.SelectedClubName}\' на {selectedDate:dd.MM.yyyy}.";
                    }
                }
                 catch (Exception)
                {
                    viewModel.ErrorMessage = (viewModel.ErrorMessage ?? "") + " Помилка завантаження розкладу.";
                }
            }
            else if (!clubId.HasValue && string.IsNullOrEmpty(viewModel.ErrorMessage))
            {
                viewModel.InfoMessage = "Будь ласка, оберіть клуб для перегляду розкладу.";
            }

            if (TempData["ErrorMessage"] != null) viewModel.ErrorMessage = (viewModel.ErrorMessage ?? "") + TempData["ErrorMessage"]?.ToString();
            if (TempData["SuccessMessage"] != null) viewModel.SuccessMessage = TempData["SuccessMessage"]?.ToString();

            return View(viewModel);
        }
    }
}