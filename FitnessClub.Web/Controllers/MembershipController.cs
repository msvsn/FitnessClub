using FitnessClub.BLL.Services;
using FitnessClub.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitnessClub.BLL.Dtos;
using System.Collections.Generic;
using System;
using FitnessClub.BLL;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Web.Controllers
{
    public class MembershipController : Controller
    {
        private readonly MembershipService _membershipService;
        private readonly ClubService _clubService;

        public MembershipController(MembershipService membershipService, ClubService clubService)
        {
            _membershipService = membershipService;
            _clubService = clubService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var membershipTypes = new List<MembershipTypeDto>
            {
                new MembershipTypeDto 
                { 
                    MembershipTypeId = 1, 
                    Name = "Один клуб", 
                    Price = AppSettings.MembershipSettings.SingleClubPrice, 
                    DurationDays = AppSettings.MembershipSettings.SingleClubDurationDays, 
                    IsNetwork = false
                },
                new MembershipTypeDto 
                { 
                    MembershipTypeId = 2, 
                    Name = "Мережевий", 
                    Price = AppSettings.MembershipSettings.NetworkPrice, 
                    DurationDays = AppSettings.MembershipSettings.NetworkDurationDays, 
                    IsNetwork = true
                }
            };
            
            return View(membershipTypes);
        }

        public IActionResult Purchase(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Для придбання абонементу необхідно спочатку увійти в систему";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Purchase", "Membership", new { id }) });
            }

            ViewBag.MembershipTypeId = id;
            ViewBag.Clubs = _clubService.GetAllClubs();
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Purchase(int membershipTypeId, int? clubId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            var result = _membershipService.PurchaseMembership(userId, membershipTypeId, clubId);
            
            switch (result)
            {
                case MembershipPurchaseResult.Success:
                    TempData["SuccessMessage"] = "Абонемент успішно придбано!";
                    return RedirectToAction("Profile", "Account");
                
                case MembershipPurchaseResult.InvalidMembershipType:
                    TempData["ErrorMessage"] = "Невірний тип абонементу";
                    break;
                
                case MembershipPurchaseResult.ClubRequiredForSingleClub:
                    TempData["ErrorMessage"] = "Для абонементу одного клубу потрібно вибрати клуб";
                    break;
                
                case MembershipPurchaseResult.ClubNotNeededForNetwork:
                    TempData["ErrorMessage"] = "Для мережевого абонементу не потрібно вибирати конкретний клуб";
                    break;
                
                default:
                    TempData["ErrorMessage"] = "Виникла помилка при придбанні абонементу";
                    break;
            }
            
            return RedirectToAction("Purchase", new { id = membershipTypeId });
        }
    }
}