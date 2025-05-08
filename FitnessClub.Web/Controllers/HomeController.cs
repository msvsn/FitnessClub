using FitnessClub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using FitnessClub.BLL.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace FitnessClub.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClubService _clubService;
        private readonly ITrainerService _trainerService;

        public HomeController(IClubService clubService, ITrainerService trainerService)
        {
            _clubService = clubService;
            _trainerService = trainerService;
        }

        public async Task<IActionResult> Index()
        {
            var clubs = await _clubService.GetAllClubsAsync();
            var trainers = await _trainerService.GetAllTrainersAsync();

            var viewModel = new HomeViewModel
            {
                Clubs = clubs.ToList(),
                Trainers = trainers.ToList()
            };
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}