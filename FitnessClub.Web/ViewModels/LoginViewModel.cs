using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Ім\'я користувача")]
        public string Username { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;
        [Display(Name = "Запам\'ятати мене")]
        public bool RememberMe { get; set; }
    }
}
