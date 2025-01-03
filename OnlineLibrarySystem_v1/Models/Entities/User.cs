using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OnlineLibrarySystem_v1.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MinLength(4, ErrorMessage = "Username must be at least 4 characters")]
        [RegularExpression(@"^\S*$", ErrorMessage = "Username cannot contain spaces")]
        public string Username { get; set; }

        [BindNever]
        public string? PasswordHash { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        [BindNever]
        public ICollection<BorrowedBook>? BorrowedBooks { get; set; }
    }
}
