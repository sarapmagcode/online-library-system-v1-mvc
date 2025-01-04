using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.ViewModels;

namespace OnlineLibrarySystem_v1.Controllers
{
    public class BooksController : BaseController
    {
        private readonly LibraryDbContext _context;

        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const int maxFileSize = 5 * 1024 * 1024; // 5MB

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();

            BookViewModel bookViewModel = new BookViewModel
            {
                Books = books
            };

            return View(bookViewModel);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var bookViewModel = new BookViewModel { Book = book };

            return View(bookViewModel);
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            var categories = await _context.Categories.ToListAsync();

            BookViewModel bookViewModel = new BookViewModel
            {
                CategoryList = new SelectList(categories, "Id", "Name")
            };

            return View(bookViewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel bookViewModel, IFormFile? bookImage)
        {
            // Initial validation
            if (bookImage == null || bookImage.Length == 0)
            {
                ModelState.AddModelError("Book.ImagePath", "Please provide an image of the book");
                var categories = await _context.Categories.ToListAsync();
                bookViewModel.CategoryList = new SelectList(categories, "Id", "Name", bookViewModel.Book.CategoryId);
                return View(bookViewModel);
            }

            // File type validation
            var extension = Path.GetExtension(bookImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Book.ImagePath", "Only .jpg, .jpeg, and .png files are allowed");
                var categories = await _context.Categories.ToListAsync();
                bookViewModel.CategoryList = new SelectList(categories, "Id", "Name", bookViewModel.Book.CategoryId);
                return View(bookViewModel);
            }

            // File size validation
            if (bookImage.Length > maxFileSize)
            {
                ModelState.AddModelError("Book.ImagePath", "File size cannot exceed 5MB");
                var categories = await _context.Categories.ToListAsync();
                bookViewModel.CategoryList = new SelectList(categories, "Id", "Name", bookViewModel.Book.CategoryId);
                return View(bookViewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    var uploadFolder = Path.Combine("wwwroot", "uploads");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + bookImage.FileName;
                    var filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookImage.CopyToAsync(fileStream);
                    }

                    /*
                     * Note:
                     * 1. The physical file is saved in the "wwwroot/uploads" folder on your server
                     * 2. But when accessing the file through a web browser, "wwwroot" is treated as the 
                     * web root directory and is not included in the URL path
                     * 
                     * So:
                     * - Physical path on server: wwwroot/uploads/guid_image.jpg
                     * - URL path in database/browser: /uploads/guid_image.jpg
                     * 
                     * This works because:
                     * - ASP.NET Core treats "wwwroot" as the root directory for static files
                     * - When a browser requests /uploads/image.jpg, the server automatically looks in wwwroot/uploads/image.jpg
                     * - You never include "wwwroot" in URLs or links because it's just the container for public files
                     */
                    bookViewModel.Book.ImagePath = "/uploads/" + uniqueFileName;

                    bookViewModel.Book.CopiesAvailable = bookViewModel.Book.TotalCopies;

                    _context.Add(bookViewModel.Book);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while saving the book. Please try again.");
                }
            }

            var categoriesList = await _context.Categories.ToListAsync();

            // 4th parameter below: Selected value
            bookViewModel.CategoryList = new SelectList(categoriesList, "Id", "Name", bookViewModel.Book?.CategoryId);
            return View(bookViewModel);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            var categories = await _context.Categories.ToListAsync();

            if (book == null || categories == null)
            {
                return NotFound();
            }

            var bookViewModel = new BookViewModel
            {
                Book = book,
                CategoryList = new SelectList(categories, "Id", "Name", book.CategoryId)
            };

            return View(bookViewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, BookViewModel bookViewModel)
        {
            if (id != bookViewModel.Book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookViewModel.Book);
                    await _context.SaveChangesAsync();
                }
                catch (DBConcurrencyException)
                {
                    if (!BookExists(bookViewModel.Book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            bookViewModel.CategoryList = new SelectList(categories, "Id", "Name", bookViewModel.Book.CategoryId);
            return View(bookViewModel);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var bookViewModel = new BookViewModel
            {
                Book = book
            };

            return View(bookViewModel);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'LibraryDbContext.Books' is null.");
            }

            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return (_context.Books?.Any(book => book.Id == id)).GetValueOrDefault();
        }
    }
}
