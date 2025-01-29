using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.Entities;
using OnlineLibrarySystem_v1.Models.ViewModels;
using static System.Reflection.Metadata.BlobBuilder;

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

        #region User Account

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            if (ViewData["Username"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLowerInvariant(),
                    nameof(AccountController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            if (ViewData["Role"]!.Equals("admin"))
            {
                return RedirectToAction(nameof(Admin));
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
                    nameof(AccountController.Login).ToLowerInvariant(),
                    nameof(AccountController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            int userId = Convert.ToInt32(ViewData["UserId"]);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Check if old password is correct
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                viewModel.OldPassword
            );

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Incorrect password");
            }

            // Check if new password length is at least 5 characters
            if (viewModel.NewPassword.Length < 5)
            {
                ModelState.AddModelError("NewPassword", "Must be at least 5 characters");
            }

            // Check if passwords match
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
        public async Task<IActionResult> ReturnBook(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ViewData["UserId"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLowerInvariant(),
                    nameof(AccountController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            int userId = Convert.ToInt32(ViewData["UserId"]);

            var borrowedBook = await _context.BorrowedBooks
                .Include(bb => bb.Book)
                .FirstOrDefaultAsync(bb => bb.Id == id && bb.User.Id == userId);

            if (borrowedBook == null)
            {
                return NotFound();
            }

            try
            {
                borrowedBook.Book.CopiesAvailable++;
                borrowedBook.ReturnDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowedBookExists(borrowedBook.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Profile/ExtendBorrow/5
        public async Task<IActionResult> ExtendBorrow(int? id)
        {
            if (ViewData["UserId"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLowerInvariant(),
                    nameof(AccountController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            int userId = Convert.ToInt32(ViewData["UserId"]);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var borrowedBook = await _context.BorrowedBooks
                .FirstOrDefaultAsync(bb => bb.Id == id && bb.UserId == userId);
            if (borrowedBook == null)
            {
                return NotFound();
            }

            try
            {
                borrowedBook.DueDate = borrowedBook.DueDate.AddDays(14);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowedBookExists(borrowedBook.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Admin Account

        // GET: Profile/Admin
        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            if (ViewData["Username"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLowerInvariant(),
                    nameof(AccountController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            if (ViewData["Role"]!.Equals("user"))
            {
                return RedirectToAction(
                    nameof(Index),
                    nameof(ProfileController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            var userId = Convert.ToInt32(ViewData["UserId"]);

            var admin = await _context.Users.FindAsync(userId);
            if (admin == null)
            {
                return NotFound();
            }

            var books = await _context.Books.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            var users = await _context.Users
                .Where(user => user.Role.Equals("user"))
                .ToListAsync();

            var borrowedBooks = await _context.BorrowedBooks
                .Where(borrowedBook => borrowedBook.ReturnDate == null)
                .ToListAsync();

            ProfileViewModel viewModel = new ProfileViewModel
            {
                User = admin,
                TotalBookCopiesFormatted = $"{books.Sum(book => book.TotalCopies):N0}",
                Categories = categories,
                Users = users,
                BorrowedBooks = borrowedBooks,
            };

            return View(viewModel);
        }

        // POST: Profile/EditAdminInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdminInfo(ProfileViewModel viewModel)
        {
            if (ViewData["UserId"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLowerInvariant(),
                    nameof(AccountController).Replace("Controller", "").ToLowerInvariant()
                );
            }

            int adminId = Convert.ToInt32(ViewData["UserId"]);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null)
            {
                return NotFound();
            }

            // Check if old password is correct
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                viewModel.OldPassword
            );

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Incorrect password");
            }

            // Check if new password length is at least 5 characters
            if (viewModel.NewPassword.Length < 5)
            {
                ModelState.AddModelError("NewPassword", "Must be at least 5 characters");
            }

            // Check if passwords match
            if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Must match with new password");
            }

            if (!ModelState.IsValid)
            {
                var books = await _context.Books.ToListAsync();
                var categories = await _context.Categories.ToListAsync();
                var users = await _context.Users
                    .Where(user => user.Role.Equals("user"))
                    .ToListAsync();
                var borrowedBooks = await _context.BorrowedBooks
                    .Where(borrowedBook => borrowedBook.ReturnDate == null)
                    .ToListAsync();

                viewModel.User = admin;
                viewModel.TotalBookCopiesFormatted = $"{books.Sum(book => book.TotalCopies):N0}";
                viewModel.Categories = categories;
                viewModel.Users = users;
                viewModel.BorrowedBooks = borrowedBooks;

                return View(nameof(Admin), viewModel);
            }

            // Update new admin info
            try
            {
                string hashedPassword = _passwordHasher.HashPassword(admin, viewModel.NewPassword);
                admin.PasswordHash = hashedPassword;

                _context.Users.Update(admin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Unable to save changes. Please try again.");

                var books = await _context.Books.ToListAsync();
                var categories = await _context.Categories.ToListAsync();
                var users = await _context.Users
                    .Where(user => user.Role.Equals("user"))
                    .ToListAsync();
                var borrowedBooks = await _context.BorrowedBooks
                    .Where(borrowedBook => borrowedBook.ReturnDate == null)
                    .ToListAsync();

                viewModel.User = admin;
                viewModel.TotalBookCopiesFormatted = $"{books.Sum(book => book.TotalCopies):N0}";
                viewModel.Categories = categories;
                viewModel.Users = users;
                viewModel.BorrowedBooks = borrowedBooks;

                return View(nameof(Admin), viewModel);
            }

            return RedirectToAction(nameof(Admin));
        }

        #endregion

        #region Helper Methods

        private bool BorrowedBookExists(int id)
        {
            return (_context.BorrowedBooks?.Any(bb => bb.Id == id)).GetValueOrDefault();
        }

        #endregion
    }
}
