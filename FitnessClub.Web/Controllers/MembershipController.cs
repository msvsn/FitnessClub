using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Dtos;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using FitnessClub.Web.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;
using System.Collections.Generic;
using AutoMapper;
using FitnessClub.Core.Abstractions;
using FitnessClub.BLL.Enums;

namespace FitnessClub.Web.Controllers
{
    public class MembershipController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IClubService _clubService;
        private readonly IMapper _mapper;

        public MembershipController(IMembershipService membershipService, IClubService clubService, IMapper mapper)
        {
            _membershipService = membershipService;
            _clubService = clubService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var membershipTypeDtos = await _membershipService.GetAllMembershipTypesAsync();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            MembershipDto? activeMembership = null; 
            if (int.TryParse(userIdStr, out var userId))
            {
                 activeMembership = await _membershipService.GetActiveMembershipAsync(userId);
            }
            
            var viewModel = new MembershipViewModel
            {
                 MembershipTypes = membershipTypeDtos,
                 ActiveMembership = activeMembership,
                 IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false,
                 LoginUrlWithReturn = Url.Action("Login", "Account", new { returnUrl = Url.Action("Index", "Membership") })
            };

            return View(viewModel);
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out var userId) ? userId : (int?)null;
        }

        public async Task<IActionResult> Purchase(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Для придбання абонементу необхідно спочатку увійти в систему";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Purchase", "Membership", new { id }) });
            }

            var membershipType = (await _membershipService.GetAllMembershipTypesAsync()).FirstOrDefault(mt => mt.MembershipTypeId == id);

            if (membershipType == null)
            {
                TempData["ErrorMessage"] = "Вибраний тип абонементу не знайдено.";
                return RedirectToAction("Index");
            }

            var viewModel = new PurchaseMembershipViewModel
            {
                MembershipTypeId = id,
                MembershipType = membershipType,
                Clubs = null
            };

            if (membershipType != null && !membershipType.IsSingleVisit && !membershipType.IsNetwork)
            {
                var clubs = await _clubService.GetAllClubsAsync();
                viewModel.Clubs = new SelectList(clubs, "ClubId", "Name");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(PurchaseMembershipViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                 return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                 await PopulateViewModelForError(model);
                 return View(model);
            }

            try
            {
                var result = await _membershipService.PurchaseMembershipAsync(userId.Value, model.MembershipTypeId, model.ClubId);
                
                if (result == MembershipPurchaseResult.Success)
                {
                    TempData["SuccessMessage"] = "Абонемент успішно придбано!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, GetErrorMessageForResult(result));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Виникла невідома помилка при придбанні абонементу");
            }

            await PopulateViewModelForError(model);
            return View(model);
        }

        private async Task PopulateViewModelForError(PurchaseMembershipViewModel model)
        {
            model.MembershipType = (await _membershipService.GetAllMembershipTypesAsync()).FirstOrDefault(mt => mt.MembershipTypeId == model.MembershipTypeId);
            if (model.MembershipType != null && !model.MembershipType.IsNetwork)
            {
                var clubs = await _clubService.GetAllClubsAsync();
                model.Clubs = new SelectList(clubs, "ClubId", "Name", model.ClubId);
            }
            else
            {
                 model.Clubs = null;
            }
        }

        private string GetErrorMessageForResult(MembershipPurchaseResult result)
        {
            return result switch
            {
                MembershipPurchaseResult.InvalidMembershipType => "Невірний тип абонементу",
                MembershipPurchaseResult.ClubRequiredForSingleClub => "Для цього типу абонементу потрібно вибрати клуб",
                MembershipPurchaseResult.ClubNotNeededForNetwork => "Для мережевого абонементу не потрібно вибирати конкретний клуб",
                MembershipPurchaseResult.UserNotFound => "Користувач не знайдений",
                MembershipPurchaseResult.ClubNotFound => "Вибраний клуб не знайдений",
                MembershipPurchaseResult.AlreadyHasActiveMainMembership => "Ви вже маєте активний основний абонемент.",
                MembershipPurchaseResult.NetworkMembershipConflict => "Неможливо придбати одноразовий абонемент, маючи активний мережевий абонемент.",
                MembershipPurchaseResult.SameClubSingleVisitConflict => "Неможливо придбати одноразовий абонемент для клубу, де у вас вже є активний основний абонемент.",
                MembershipPurchaseResult.ClubRequiredForSingleVisit => "Для одноразового абонементу необхідно вибрати клуб.",
                MembershipPurchaseResult.NetworkMembershipIsExclusive => "Мережевий абонемент не може існувати одночасно з іншими абонементами, або ви намагаєтесь придбати не мережевий абонемент, маючи активний мережевий.",
                MembershipPurchaseResult.AlreadyHasActiveSingleVisitMembership => "Ви вже маєте активний разовий абонемент. Дозволено лише один активний разовий абонемент.",
                MembershipPurchaseResult.UnknownError => "Виникла невідома помилка",
                _ => "Виникла помилка при придбанні абонементу"
            };
        }
    }
}