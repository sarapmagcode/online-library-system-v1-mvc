using System.ComponentModel.DataAnnotations;
using OnlineLibrarySystem_v1.Helpers;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class BookListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "ImagePath")]
        public string ImagePath { get; set; }

        [Display(Name = "Book Image")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Author")]
        public string Author { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Available Copies")]
        public int CopiesAvailable { get; set; }

        [Display(Name = "Total Copies")]
        public int TotalCopies { get; set; }

        // Extras
        public string TruncatedDescription => StringExtensions.TruncateWithEllipsis(Description, 64);
        public bool? IsCurrentlyBorrowed { get; set; }

        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }
    }
}
