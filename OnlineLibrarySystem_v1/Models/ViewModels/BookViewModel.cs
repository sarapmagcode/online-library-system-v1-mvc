using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineLibrarySystem_v1.Models.Entities;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class BookViewModel
    {
        public Book Book { get; set; }

        [BindNever]
        public List<Book>? Books { get; set; }

        [BindNever]
        public SelectList? CategoryList { get; set; }
    }
}
