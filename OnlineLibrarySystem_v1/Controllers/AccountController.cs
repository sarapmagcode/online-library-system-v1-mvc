using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.Entities;
using OnlineLibrarySystem_v1.Models.ViewModels;

namespace OnlineLibrarySystem_v1.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly LibraryDbContext _context;

        public AccountController(IPasswordHasher<User> passwordHasher, LibraryDbContext context)
        {
            _passwordHasher = passwordHasher;
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction(nameof(BooksController.Index), nameof(BooksController).Replace("Controller", ""));
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginViewModel.Username);
                if (user == null)
                {
                    ModelState.AddModelError(nameof(loginViewModel.Password), "Username/password is incorrect");
                    return View(loginViewModel);
                }

                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash, // Hashed password from database
                    loginViewModel.Password // plain text password from login form
                );

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    return RedirectToAction(nameof(Index), "Books");
                }
                else
                {
                    ModelState.AddModelError(nameof(loginViewModel.Password), "Username/password is incorrect");
                }
            }

            return View(loginViewModel);
        }

        // POST: Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/Signup
        public IActionResult Signup()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction(nameof(BooksController.Index), nameof(BooksController).Replace("Controller", ""));
            }

            return View();
        }

        // POST: Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel signupViewModel)
        {
            if (!signupViewModel.Password.Equals(signupViewModel.ConfirmPassword))
            {
                ModelState.AddModelError(nameof(signupViewModel.ConfirmPassword), "Must match password");
                return View(signupViewModel);
            }

            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username == signupViewModel.User.Username))
            {
                ModelState.AddModelError("User.Username", "Username already exists");
                return View(signupViewModel);
            }

            // Check if email exists
            if (await _context.Users.AnyAsync(u => u.Email == signupViewModel.User.Email))
            {
                ModelState.AddModelError("User.Email", "Email is already registered");
                return View(signupViewModel);
            }

            if (ModelState.IsValid)
            {
                // Hash password
                string hashedPassword = _passwordHasher.HashPassword(signupViewModel.User, signupViewModel.Password);
                signupViewModel.User.PasswordHash = hashedPassword;

                _context.Users.Add(signupViewModel.User);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Login));
            }
            
            return View(signupViewModel);
        }
    }
}
