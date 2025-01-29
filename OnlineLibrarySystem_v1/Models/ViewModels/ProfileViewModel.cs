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

        [BindNever]
        public List<Book>? Books { get; set; }

        [BindNever]
        public List<Category>? Categories { get; set; }

        [BindNever]
        public List<User>? Users { get; set; }

        [BindNever]
        public string? TotalBookCopiesFormatted { get; set; }

        #region Edit profile

        [Display(Name = "Old Password")]
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; } = string.Empty;

        [Display(Name = "New Password")]
        [Required(ErrorMessage = "New password is required")]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Confirm New Password")]
        [Required(ErrorMessage = "Confirm new password is required")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        #endregion
    }
}
