using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.Entities;
using OnlineLibrarySystem_v1.Models.ViewModels;

namespace OnlineLibrarySystem_v1.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly LibraryDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public ProfileController(LibraryDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            if (ViewData["Username"] == null)
            {
                return RedirectToAction(nameof(AccountController.Login).ToLower(), nameof(AccountController).Replace("Controller", "").ToLower());
            }

            var userId = Convert.ToInt32(ViewData["UserId"]);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var borrowedBooks = await _context.BorrowedBooks
                .Include(bb => bb.Book)
                .Where(bb => bb.UserId == user.Id)
                .ToListAsync();

            ProfileViewModel viewModel = new ProfileViewModel
            {
                User = user,
                BorrowedBooks = borrowedBooks
            };

            return View(viewModel);
        }

        // POST: Profile/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileViewModel viewModel)
        {
            if (ViewData["UserId"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLower(),
                    nameof(AccountController).Replace("Controller", "").ToLower()
                );
            }

            int userId = Convert.ToInt32(ViewData["UserId"]);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Checks if old password is correct
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                viewModel.OldPassword
            );

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Incorrect password");
            }

            // Checks if new password length is at least 5 characters
            if (viewModel.NewPassword.Length < 5)
            {
                ModelState.AddModelError("NewPassword", "Must be at least 5 characters");
            }

            // Checks if passwords match
            if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Must match with New Password");
            }

            if (!ModelState.IsValid)
            {
                viewModel.User = user;
                viewModel.BorrowedBooks = await _context.BorrowedBooks
                    .Include(bb => bb.Book)
                    .Where(bb => bb.UserId == user.Id)
                    .ToListAsync();

                return View(nameof(Index), viewModel);
            }

            // Update new user details
            try
            {
                string hashedPassword = _passwordHasher.HashPassword(user, viewModel.NewPassword);
                user.PasswordHash = hashedPassword;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Unable to save changes. Please try again.");

                viewModel.User = user;
                viewModel.BorrowedBooks = await _context.BorrowedBooks
                    .Include(bb => bb.Book)
                    .Where(bb => bb.UserId == user.Id)
                    .ToListAsync();

                return View(nameof(Index), viewModel);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Profile/ReturnBook/5

    }
}
