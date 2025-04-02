using FitnessClub.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClubService _clubService;
        private readonly TrainerService _trainerService;

        public HomeController(ClubService clubService, TrainerService trainerService)
        {
            _clubService = clubService;
            _trainerService = trainerService;
        }

        public IActionResult Index() => View();
        
        public IActionResult Clubs() => View(_clubService.GetAllClubs());
        
        public IActionResult Trainers() => View(_trainerService.GetAllTrainers());
        
        [Route("Error")]
        [Route("Home/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
} 