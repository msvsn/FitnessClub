using System.Security.Claims;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Enums;
using FitnessClub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FitnessClub.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IClassScheduleService _scheduleService;

        public BookingController(IBookingService bookingService, IClassScheduleService scheduleService)
        {
            _bookingService = bookingService;
            _scheduleService = scheduleService;
        }

        public async Task<IActionResult> Create(int scheduleId, DateTime classDate)
        {
            if (classDate.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Неможливо забронювати заняття на минулу дату.";
                return RedirectToAction("Index", "Schedule");
            }

            var schedule = await _scheduleService.GetClassScheduleByIdAsync(scheduleId);
            if (schedule == null)
            {
                TempData["ErrorMessage"] = "Заняття не знайдено.";
                return RedirectToAction("Index", "Schedule");
            }

            if (schedule.BookedPlaces >= schedule.Capacity)
            {
                TempData["ErrorMessage"] = "На жаль, усі місця на це заняття вже заброньовані.";
                return RedirectToAction("Index", "Schedule"); 
            }

            var viewModel = new BookingViewModel
            {
                ClassScheduleId = scheduleId,
                Schedule = schedule,
                ClassDate = classDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) 
            {
                return Challenge();
            }

            var schedule = await _scheduleService.GetClassScheduleByIdAsync(model.ClassScheduleId);
            if (schedule == null)
            {
                 ModelState.AddModelError(string.Empty, "Обране заняття більше не існує.");
                 return View(model); 
            }
            model.Schedule = schedule; 

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                (BookingResult result, string? bookingId) = await _bookingService.BookClassAsync(userId.Value, model.ClassScheduleId, model.ClassDate);

                if (result == BookingResult.Success)
                {
                    TempData["SuccessMessage"] = $"Бронювання успішне! Ваш ID бронювання: {bookingId}";
                    return RedirectToAction("MyBookings");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, GetBookingErrorMessage(result));
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Виникла непередбачена помилка при бронюванні.";
            }

            return View(model);
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var bookings = await _bookingService.GetUserBookingsAsync(userId.Value);
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            try
            {
                bool cancelled = await _bookingService.CancelBookingAsync(bookingId, userId.Value);

                TempData[cancelled ? "SuccessMessage" : "ErrorMessage"] = cancelled
                    ? "Заняття успішно скасовано."
                    : "Не вдалося скасувати бронювання. Воно може не існувати, належати вам або заняття вже відбулося.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Виникла помилка при скасуванні бронювання.";
            }

            return RedirectToAction("MyBookings");
        }

        private int? GetCurrentUserId()
        {
             var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
             return int.TryParse(userIdStr, out var userId) ? userId : (int?)null;
        }

         private string GetBookingErrorMessage(BookingResult result)
         {
              return result switch
              {
                   BookingResult.InvalidScheduleOrDate => "Недійсне заняття або вибрана дата.",
                   BookingResult.NoAvailablePlaces => "Вибачте, на це заняття немає вільних місць.",
                   BookingResult.UserRequired => "Потрібна автентифікація користувача.",
                   BookingResult.BookingLimitExceeded => "Ви досягли ліміту активних бронювань.",
                   BookingResult.MembershipRequired => "Для бронювання потрібен активний абонемент. Будь ласка, придбайте його на сторінці Абонементи.",
                   BookingResult.MembershipClubMismatch => "Ваш основний абонемент не дійсний для цього клубу. Ви можете забронювати це заняття, якщо придбаєте разовий абонемент для цього клубу.",
                   BookingResult.AlreadyBooked => "Ви вже записані на це заняття.",
                   BookingResult.StrategyNotFound => "Не вдалося обробити запит бронювання (помилка конфігурації).",
                   BookingResult.UnknownError => "Сталася неочікувана помилка під час бронювання.",
                   _ => "Виникла невідома помилка при бронюванні."
              };
         }
    }
}