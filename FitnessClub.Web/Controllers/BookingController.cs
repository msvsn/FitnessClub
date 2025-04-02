using FitnessClub.BLL.Services;
using FitnessClub.DAL.Entities;
using FitnessClub.BLL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;

namespace FitnessClub.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly ClassScheduleService _scheduleService;
        private readonly UserService _userService;

        public BookingController(BookingService bookingService, ClassScheduleService scheduleService, UserService userService)
        {
            _bookingService = bookingService;
            _scheduleService = scheduleService;
            _userService = userService;
        }

        public IActionResult Create(int id, DateTime date)
        {
            var classSchedule = _scheduleService.GetClassScheduleById(id);
            if (classSchedule == null)
                return NotFound();
                
            ViewBag.ClassSchedule = classSchedule;
            ViewBag.ClassDate = date;
            return View();
        }

        [HttpPost]
        public IActionResult Book(BookingDto model)
        {
            if (ModelState.IsValid)
            {
                int? userId = null;
                if (User.Identity.IsAuthenticated && User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                else if (string.IsNullOrEmpty(model.GuestName))
                {
                    TempData["ErrorMessage"] = "Будь ласка, увійдіть в систему або вкажіть ім'я гостя";
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Book", "Booking") });
                }

                var (result, bookingId) = _bookingService.BookClass(userId, model.GuestName, model.ClassScheduleId, model.ClassDate);
                
                if (result == BookingResult.Success)
                {
                    TempData["SuccessMessage"] = "Заняття успішно заброньовано";
                    return RedirectToAction("BookingConfirmation");
                }
                else
                {
                    switch (result)
                    {
                        case BookingResult.InvalidScheduleOrDate:
                            TempData["ErrorMessage"] = "Неправильний розклад або дата заняття";
                            break;
                        case BookingResult.NoAvailablePlaces:
                            TempData["ErrorMessage"] = "На жаль, всі місця на це заняття вже заброньовані";
                            break;
                        case BookingResult.UserOrGuestRequired:
                            TempData["ErrorMessage"] = "Будь ласка, увійдіть в систему або вкажіть ім'я гостя";
                            break;
                        default:
                            TempData["ErrorMessage"] = "Сталася помилка при бронюванні";
                            break;
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            
            TempData["ErrorMessage"] = "Будь ласка, перевірте правильність заповнення форми";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult BookingConfirmation()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }
        
        [Authorize]
        public IActionResult MyBookings()
        {
            if (User.Identity.IsAuthenticated && User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var bookings = _bookingService.GetUserBookings(userId);
                return View(bookings);
            }
            return RedirectToAction("Login", "Account");
        }
    }
}