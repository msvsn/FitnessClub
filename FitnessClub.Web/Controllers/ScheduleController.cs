using FitnessClub.BLL.Interfaces;
using FitnessClub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessClub.Web.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IClubService _clubService;
        private readonly IClassScheduleService _scheduleService;

        public ScheduleController(IClubService clubService, IClassScheduleService scheduleService)
        {
            _clubService = clubService;
            _scheduleService = scheduleService;
        }

        public async Task<IActionResult> Index(int? clubId, DateTime? date)
        {
            var currentDate = date?.Date ?? DateTime.Today;

            var clubs = Enumerable.Empty<FitnessClub.BLL.Dtos.ClubDto>();
            try
            {
                 clubs = await _clubService.GetAllClubsAsync();
            }
            catch (Exception)
            {
                 ViewBag.ErrorMessage = "Не вдалося завантажити список клубів.";
            }

            var schedules = Enumerable.Empty<FitnessClub.BLL.Dtos.ClassScheduleDto>();
            if (clubId.HasValue)
            {
                try
                {
                    schedules = await _scheduleService.GetSchedulesByClubAndDateAsync(clubId.Value, currentDate);
                     var selectedClub = clubs.FirstOrDefault(c => c.ClubId == clubId.Value);
                     ViewBag.SelectedClubName = selectedClub?.Name ?? "Обраний клуб";
                }
                catch (Exception)
                {
                    ViewBag.ErrorMessage = "Не вдалося завантажити розклад для вибраного клубу.";
                }
            }

            var viewModel = new ScheduleViewModel
            {
                Clubs = new SelectList(clubs.ToList(), "ClubId", "Name", clubId),
                Schedules = schedules.ToList(),
                SelectedClubId = clubId,
                SelectedDate = currentDate
            };

            if (clubId.HasValue)
            {
                var selectedClub = clubs.FirstOrDefault(c => c.ClubId == clubId.Value);
                viewModel.SelectedClubName = selectedClub?.Name ?? "Обраний клуб";
            }

            return View(viewModel);
        }
    }
}