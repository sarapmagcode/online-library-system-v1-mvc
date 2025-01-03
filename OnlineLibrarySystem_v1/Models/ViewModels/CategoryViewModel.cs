using System.ComponentModel.DataAnnotations;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Category Name field is required.")]
        [StringLength(255, ErrorMessage = "The Name field cannot exceed 255 characters.")]
        public string Name { get; set; }
    }
}
