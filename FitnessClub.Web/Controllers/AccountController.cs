using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Services;
using FitnessClub.DAL.Entities;
using System;
using Microsoft.AspNetCore.Authorization;
using FitnessClub.Web.ViewModels;

namespace FitnessClub.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly MembershipService _membershipService;

        public AccountController(UserService userService, MembershipService membershipService)
        {
            _userService = userService;
            _membershipService = membershipService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _userService.Register(model.FirstName, model.LastName, model.Username, model.Password);
                    var user = _userService.Login(model.Username, model.Password);
                    SignInUser(user);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            var user = _userService.Login(username, password);
            
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, "User")
                };
                
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = System.DateTimeOffset.UtcNow.AddDays(7)
                };
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                
                return RedirectToAction("Index", "Home");
            }
            
            ViewData["ErrorMessage"] = "Невірний логін або пароль";
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = _userService.GetUserById(userId);
            var membership = _membershipService.GetActiveMembership(userId);
            ViewBag.Membership = membership;
            return View(user);
        }

        private void SignInUser(BLL.Dtos.UserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)).Wait();
        }
    }
}