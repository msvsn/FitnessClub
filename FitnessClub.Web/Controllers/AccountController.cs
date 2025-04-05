using FitnessClub.BLL.Services;
using FitnessClub.DAL.Entities;
using FitnessClub.BLL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Web.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FitnessClub.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMembershipService _membershipService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, IMembershipService membershipService, IMapper mapper, ILogger<AccountController> logger)
        {
            _userService = userService;
            _membershipService = membershipService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.RegisterAsync(model.FirstName, model.LastName, model.Username, model.Password);
                    var userDto = await _userService.LoginAsync(model.Username, model.Password);
                    if (userDto != null)
                    {
                        await SignInUser(userDto);
                        return RedirectToAction("Index", "Home");
                    }
                    ModelState.AddModelError(string.Empty, "Registration successful, but failed to log in automatically.");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userDto = await _userService.LoginAsync(model.Username, model.Password);
                if (userDto != null)
                {
                    await SignInUser(userDto);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                _logger.LogWarning("Could not parse UserId from claims.");
                 return Challenge();
            }

            var user = await _userService.GetUserByIdAsync(userId);
            var membership = await _membershipService.GetActiveMembershipAsync(userId);

            var profileViewModel = new UserProfileViewModel
            {
                User = user!,
                ActiveMembership = membership
            };

            if (profileViewModel.User == null)
            {
                _logger.LogWarning("User data not found for logged in user ID: {UserId}", userId);
                return RedirectToAction("Login");
            }

            return View(profileViewModel);
        }

        private async Task SignInUser(BLL.Dtos.UserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}