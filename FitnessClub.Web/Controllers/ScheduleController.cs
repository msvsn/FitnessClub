using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Services;
using FitnessClub.BLL.Dtos;
using System;
using System.Collections.Generic;

namespace FitnessClub.Web.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ClassScheduleService _scheduleService;
        private readonly ClubService _clubService;

        public ScheduleController(ClassScheduleService scheduleService, ClubService clubService)
        {
            _scheduleService = scheduleService;
            _clubService = clubService;
        }

        public IActionResult Index()
        {
            ViewBag.Clubs = _clubService.GetAllClubs();
            return View();
        }

        [HttpPost]
        public IActionResult ViewSchedule(int clubId, DateTime selectedDate)
        {
            var schedules = _scheduleService.GetSchedulesByClubAndDate(clubId, selectedDate);
            ViewBag.SelectedDate = selectedDate;
            ViewBag.ClubId = clubId;
            ViewBag.ClubName = _clubService.GetClubById(clubId)?.Location;
            return View("Schedule", schedules);
        }

        [HttpGet]
        public IActionResult ViewSchedule(int? clubId, DateTime? selectedDate)
        {
            if (!clubId.HasValue || !selectedDate.HasValue)
            {
                return RedirectToAction("Index");
            }
            
            var schedules = _scheduleService.GetSchedulesByClubAndDate(clubId.Value, selectedDate.Value);
            ViewBag.SelectedDate = selectedDate.Value;
            ViewBag.ClubId = clubId.Value;
            ViewBag.ClubName = _clubService.GetClubById(clubId.Value)?.Location;
            return View("Schedule", schedules);
        }
        
        public IActionResult Book(int id, DateTime date)
        {
            return RedirectToAction("Create", "Booking", new { id, date });
        }
    }
}