using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineLibrarySystem_v1.Models.Entities;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class ProfileViewModel
    {
        [BindNever]
        public User? User { get; set; }

        [BindNever]
        public List<BorrowedBook>? BorrowedBooks { get; set; }

        #region Edit profile

        [Display(Name = "Old Password")]
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Display(Name = "New Password")]
        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Confirm New Password")]
        [Required]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        #endregion
    }
}
