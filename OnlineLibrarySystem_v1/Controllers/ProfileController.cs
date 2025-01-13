using Microsoft.AspNetCore.Mvc;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.ViewModels;

namespace OnlineLibrarySystem_v1.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly LibraryDbContext _context;

        public ProfileController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            if (ViewData["Username"] == null)
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            var userId = Convert.ToInt32(ViewData["UserId"]);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // TODO: Get other details

            ProfileViewModel viewModel = new ProfileViewModel
            {
                User = user
            };

            return View(viewModel);
        }
    }
}
