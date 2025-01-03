using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineLibrarySystem_v1.Helpers;

namespace OnlineLibrarySystem_v1.Models.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; } // Foreign Key
        [BindNever]
        public Category? Category { get; set; } // Navigation property

        [Display(Name = "Copies Available")]
        public int CopiesAvailable { get; set; }

        [Display(Name = "Total Copies")]
        [Range(0, int.MaxValue, ErrorMessage = "Total copies must be a positive number")]
        public int TotalCopies { get; set; }

        /**
         * Navigation property - used to define and manage relationships between entities (classes)
         */
        [BindNever]
        public ICollection<BorrowedBook>? BorrowedBooks { get; set; }

        [BindNever]
        public string? TruncatedDescription => Description.TruncateWithEllipsis(64);
    }
}
