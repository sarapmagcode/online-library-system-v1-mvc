using System.ComponentModel.DataAnnotations;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
