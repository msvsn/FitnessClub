using FitnessClub.BLL.Dtos;
using System.Collections.Generic;

namespace FitnessClub.Web.ViewModels
{
    public class HomeViewModel
    {
        public List<ClubDto> Clubs { get; set; } = new List<ClubDto>();
        public List<TrainerDto> Trainers { get; set; } = new List<TrainerDto>();
    }
} 