using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrarySystem_v1.Models.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Book image is required")]
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author name cannot be longer than 100 characters")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; } // Foreign Key
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } // Navigation property

        [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be negative")]
        public int CopiesAvailable { get; set; }

        [Required(ErrorMessage = "Total copies is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1")]
        public int TotalCopies { get; set; }

        public ICollection<BorrowedBook>? BorrowedBooks { get; set; } // Navigation property
    }
}
