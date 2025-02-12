using OnlineLibrarySystem_v1.Models.Entities;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class CategoryDetailsViewModel
    {
        public Category? Category { get; set; }
        public IEnumerable<BookListViewModel>? Books { get; set; }
    }
}
