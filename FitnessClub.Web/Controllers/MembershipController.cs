using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Dtos;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using FitnessClub.Web.ViewModels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;
using System.Collections.Generic;
using AutoMapper;
using FitnessClub.Core.Abstractions;

namespace FitnessClub.Web.Controllers
{
    [Authorize]
    public class MembershipController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IClubService _clubService;
        private readonly ILogger<MembershipController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MembershipController(IMembershipService membershipService, IClubService clubService, ILogger<MembershipController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _membershipService = membershipService;
            _clubService = clubService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Membership Index page accessed.");
            var userId = GetCurrentUserId();
            var isAuthenticated = userId.HasValue;
            MembershipDto? activeMembership = null;

            if (isAuthenticated)
            {
                _logger.LogDebug("User {UserId} is authenticated, fetching active membership.", userId.Value);
                activeMembership = await _membershipService.GetActiveMembershipAsync(userId.Value);
            }
            else
            {
                 _logger.LogDebug("User is not authenticated.");
            }

            var allTypes = await _membershipService.GetAllMembershipTypesAsync();
            var availableTypes = activeMembership == null ? allTypes : Enumerable.Empty<MembershipTypeDto>();

            var viewModel = new MembershipViewModel
            {
                MembershipTypes = allTypes,
                ActiveMembership = activeMembership,
                AvailableTypes = availableTypes,
                IsUserAuthenticated = isAuthenticated,
                 LoginUrlWithReturn = !isAuthenticated && availableTypes.Any() 
                                        ? Url.Action("Login", "Account", new { returnUrl = Url.Action("Index", "Membership") })
                                        : null
            };

            _logger.LogInformation("Returning Membership Index view. UserAuthenticated: {IsAuth}, HasActiveMembership: {HasActive}, AvailableTypesCount: {AvailableCount}", 
                isAuthenticated, activeMembership != null, availableTypes.Count());

            return View(viewModel);
        }

        public async Task<IActionResult> Purchase(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("Unauthorized attempt to access membership purchase page.");
                TempData["ErrorMessage"] = "Для придбання абонементу необхідно спочатку увійти в систему";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Purchase", "Membership", new { id }) });
            }

            var membershipType = (await _membershipService.GetAllMembershipTypesAsync()).FirstOrDefault(mt => mt.MembershipTypeId == id);

            if (membershipType == null)
            {
                _logger.LogWarning("Attempted to purchase non-existent MembershipType ID: {MembershipTypeId}", id);
                TempData["ErrorMessage"] = "Вибраний тип абонементу не знайдено.";
                return RedirectToAction("Index");
            }

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                var activeMembership = await _membershipService.GetActiveMembershipAsync(userId.Value);
                if (activeMembership != null)
                {
                    _logger.LogWarning("User {UserId} attempted to access purchase page for type {MembershipTypeId} but already has an active membership (ID: {ActiveMembershipId}).", 
                                     userId.Value, id, activeMembership.MembershipId);
                    TempData["ErrorMessage"] = $"Ви вже маєте активний абонемент (дійсний до {activeMembership.EndDate:yyyy-MM-dd}). Ви не можете придбати новий, поки діє поточний.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                _logger.LogError("User ID is null after successful authentication check in Purchase GET for MembershipType ID: {MembershipTypeId}", id);
                TempData["ErrorMessage"] = "Помилка визначення користувача. Спробуйте увійти знову.";
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new PurchaseMembershipViewModel
            {
                MembershipTypeId = id,
                MembershipType = membershipType,
                Clubs = null
            };

            if (!membershipType.IsNetwork)
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
                 _logger.LogError("Purchase POST failed: User ID is null, but user should be authenticated.");
                 return Unauthorized();
            }

            var activeMembership = await _membershipService.GetActiveMembershipAsync(userId.Value);
            if (activeMembership != null)
            {
                 _logger.LogWarning("User {UserId} attempted to purchase a membership but already has an active one (ID: {ActiveMembershipId}).", userId.Value, activeMembership.MembershipId);
                ModelState.AddModelError(string.Empty, "Ви вже маєте активний абонемент.");

                await PopulateViewModelForError(model);
                return View(model);
            }
            
            if (!ModelState.IsValid)
            {
                 _logger.LogWarning("Purchase POST failed due to ModelState invalid for User: {UserId}", userId.Value);
                 await PopulateViewModelForError(model);
                 return View(model);
            }

            try
            {
                _logger.LogInformation("Calling MembershipService.PurchaseMembershipAsync for User: {UserId}, MembershipTypeId: {MembershipTypeId}, ClubId: {ClubId}", 
                                         userId.Value, model.MembershipTypeId, model.ClubId);

                var result = await _membershipService.PurchaseMembershipAsync(userId.Value, model.MembershipTypeId, model.ClubId);
                
                _logger.LogInformation("MembershipService.PurchaseMembershipAsync returned: {Result}", result);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing membership for User: {UserId}, MembershipType: {MembershipTypeId}", userId.Value, model.MembershipTypeId);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
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
                MembershipPurchaseResult.ClubRequiredForSingleClub => "Для абонементу одного клубу потрібно вибрати клуб",
                MembershipPurchaseResult.ClubNotNeededForNetwork => "Для мережевого абонементу не потрібно вибирати конкретний клуб",
                MembershipPurchaseResult.UserNotFound => "Користувач не знайдений",
                MembershipPurchaseResult.ClubNotFound => "Вибраний клуб не знайдений",
                MembershipPurchaseResult.UnknownError => "Виникла невідома помилка",
                _ => "Виникла помилка при придбанні абонементу"
            };
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out var userId) ? userId : (int?)null;
        }
    }
}