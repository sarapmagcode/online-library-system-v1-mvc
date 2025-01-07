using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineLibrarySystem_v1.Models.Entities;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Book Image")]
        [Required(ErrorMessage = "Please select an image file")]
        public IFormFile? BookImage { get; set; }
        public string? ImagePath { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Display(Name = "Author")]
        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author name cannot be longer than 100 characters")]
        public string Author { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Display(Name = "Available Copies")]
        [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be negative")]
        public int CopiesAvailable { get; set; }

        [Display(Name = "Total Copies")]
        [Required(ErrorMessage = "Total copies is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1")]
        public int TotalCopies { get; set; }

        [BindNever]
        public SelectList? CategoryList { get; set; }

        public BookViewModel() { }

        public BookViewModel(Book book)
        {
            Id = book.Id;
            ImagePath = book.ImagePath;
            Title = book.Title;
            Description = book.Description;
            Author = book.Author;
            CategoryId = book.CategoryId;
            CopiesAvailable = book.CopiesAvailable;
            TotalCopies = book.TotalCopies;
        }

        public Book ToEntity()
        {
            return new Book
            {
                Id = Id,
                ImagePath = ImagePath,
                Title = Title,
                Description = Description,
                Author = Author,
                CategoryId = CategoryId,
                CopiesAvailable = TotalCopies,
                TotalCopies = TotalCopies,
            };
        }
    }
}
