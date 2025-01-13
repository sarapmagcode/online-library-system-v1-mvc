using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineLibrarySystem_v1.Models.Entities;

namespace OnlineLibrarySystem_v1.Models.ViewModels
{
    public class ProfileViewModel
    {
        public User? User { get; set; }
    }
}
