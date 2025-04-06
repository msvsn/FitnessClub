using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Dtos;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using FitnessClub.Web.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using AutoMapper;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;

namespace FitnessClub.Web.Controllers
{
    [Authorize]
    public class MembershipController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IClubService _clubService;
        private readonly IUserService _userService;

        public MembershipController(IMembershipService membershipService, IClubService clubService, IUserService userService)
        {
            _membershipService = membershipService;
            _clubService = clubService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            MembershipDto? activeMembership = null;
            bool isAuthenticated = int.TryParse(userIdStr, out var userId);

            if (isAuthenticated)
            {
                activeMembership = await _membershipService.GetActiveMembershipAsync(userId);
            }

            var availableTypes = await _membershipService.GetAllMembershipTypesAsync();
            if (activeMembership != null)
            {
                 availableTypes = availableTypes.Where(t => t.MembershipTypeId != activeMembership.MembershipTypeId);
            }

            var viewModel = new MembershipViewModel
            {
                ActiveMembership = activeMembership,
                MembershipTypes = availableTypes.ToList(),
                IsUserAuthenticated = isAuthenticated
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Purchase(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Challenge();
            }

            var activeMembership = await _membershipService.GetActiveMembershipAsync(userId);
            if (activeMembership != null && activeMembership.IsNetworkMembership)
            {
                TempData["ErrorMessage"] = "У вас вже є активний мережевий абонемент, який діє у всіх клубах.";
                return RedirectToAction("Index");
            }

            var membershipType = (await _membershipService.GetAllMembershipTypesAsync()).FirstOrDefault(mt => mt.MembershipTypeId == id);
            if (membershipType == null)
            {
                TempData["ErrorMessage"] = "Вибраний тип абонементу не існує.";
                return RedirectToAction("Index");
            }

            var viewModel = new PurchaseMembershipViewModel
            {
                MembershipTypeId = id,
                MembershipType = membershipType
            };

            if (membershipType != null && !membershipType.IsNetwork)
            {
                viewModel.Clubs = new SelectList(await _clubService.GetAllClubsAsync(), "ClubId", "Name");
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(PurchaseMembershipViewModel model)
        {
             var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (!int.TryParse(userIdStr, out var userId))
             {
                 return Challenge(); 
             }

             var activeMembership = await _membershipService.GetActiveMembershipAsync(userId);
             if (activeMembership != null && activeMembership.IsNetworkMembership)
             {
                 TempData["ErrorMessage"] = "У вас вже є активний мережевий абонемент, який діє у всіх клубах.";
                 return RedirectToAction("Index");
             }

            if (!ModelState.IsValid)
            {
                 if (model.MembershipType != null && !model.MembershipType.IsNetwork && model.Clubs == null)
                 {
                      model.Clubs = new SelectList(await _clubService.GetAllClubsAsync(), "ClubId", "Name", model.ClubId);
                 }
                return View(model);
            }

            try
            {
                if (model.MembershipType == null) 
                {
                    model.MembershipType = (await _membershipService.GetAllMembershipTypesAsync()).FirstOrDefault(mt => mt.MembershipTypeId == model.MembershipTypeId);
                    if (model.MembershipType == null)
                    {
                        ModelState.AddModelError(string.Empty, "Не вдалося визначити тип абонементу.");
                        return View(model);
                    }
                }

                var result = await _membershipService.PurchaseMembershipAsync(userId, model.MembershipTypeId, model.MembershipType.IsNetwork ? null : model.ClubId);

                if (result == BLL.Enums.MembershipPurchaseResult.Success)
                {
                    TempData["SuccessMessage"] = "Абонемент успішно придбано!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, GetPurchaseErrorMessage(result));
                     if (model.MembershipType != null && !model.MembershipType.IsNetwork && model.Clubs == null)
                     {
                          model.Clubs = new SelectList((await _clubService.GetAllClubsAsync()).ToList(), "ClubId", "Name", model.ClubId);
                     }
                    return View(model);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Виникла непередбачена помилка при спробі придбання абонементу.");
                if (model.MembershipType != null && !model.MembershipType.IsNetwork && model.Clubs == null)
                 {
                      model.Clubs = new SelectList((await _clubService.GetAllClubsAsync()).ToList(), "ClubId", "Name", model.ClubId);
                 }
                return View(model);
            }
        }

        private string GetPurchaseErrorMessage(BLL.Enums.MembershipPurchaseResult result)
        {
            return result switch
            {
                BLL.Enums.MembershipPurchaseResult.InvalidMembershipType => "Недійсний тип абонементу.",
                BLL.Enums.MembershipPurchaseResult.ClubRequiredForSingleClub => "Необхідно вибрати клуб для цього типу абонементу.",
                BLL.Enums.MembershipPurchaseResult.ClubNotNeededForNetwork => "Мережевий абонемент не потребує вибору клубу.",
                BLL.Enums.MembershipPurchaseResult.ClubNotFound => "Обраний клуб не знайдено.",
                BLL.Enums.MembershipPurchaseResult.AlreadyHasActiveMembership => "Ви вже маєте активний абонемент.",
                BLL.Enums.MembershipPurchaseResult.MembershipAlreadyCoversClub => "Ваш поточний абонемент вже діє у цьому клубі. Разовий абонемент тут не потрібен.",
                BLL.Enums.MembershipPurchaseResult.CannotPurchaseWithNetwork => "Ви не можете придбати додаткові абонементи, маючи активний мережевий.",
                BLL.Enums.MembershipPurchaseResult.AlreadyHasActiveOneTimePass => "Ви вже маєте активний невикористаний разовий абонемент.",
                _ => "Виникла невідома помилка."
            };
        }
    }
}