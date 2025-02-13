using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.Entities;
using OnlineLibrarySystem_v1.Models.ViewModels;

namespace OnlineLibrarySystem_v1.Controllers
{
    public class BooksController : BaseController
    {
        private readonly LibraryDbContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const int _maxFileSize = 5 * 1024 * 1024; // 5MB

        public BooksController(LibraryDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Books
                .Include(book => book.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(book =>
                    book.Title.Contains(search) ||
                    book.Author.Contains(search) ||
                    book.Description.Contains(search)
                );
            }

            var books = await query
                .Select(book => new BookListViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    ImagePath = book.ImagePath,
                    Description = book.Description,
                    Author = book.Author,
                    CategoryName = book.Category.Name,
                    CopiesAvailable = book.CopiesAvailable,
                    TotalCopies = book.TotalCopies
                })
                .ToListAsync();

            ViewData["search"] = search;

            return View(books);
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

            var userId = ViewData["UserId"] != null ? Convert.ToInt32(ViewData["UserId"]) : -1;

            var currentBorrowing = userId != -1 ?
                await _context.BorrowedBooks
                    .Where(bb => bb.BookId == id && bb.UserId == userId && bb.ReturnDate == null)
                    .FirstOrDefaultAsync() : null;

            var viewModel = new BookListViewModel
            {
                Id = book.Id,
                ImagePath= book.ImagePath,
                Title = book.Title,
                Description = book.Description,
                Author = book.Author,
                CategoryName = book.Category.Name,
                CopiesAvailable = book.CopiesAvailable,
                TotalCopies = book.TotalCopies,
                IsCurrentlyBorrowed = currentBorrowing != null,
                DueDate = currentBorrowing?.DueDate
            };

            return View(viewModel);
        }

        // POST: Books/Details/5
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsAction(int? id)
        {
            if (ViewData["UserId"] == null || ViewData["Username"] == null)
            {
                return RedirectToAction(
                    nameof(AccountController.Login).ToLower(),
                    nameof(AccountController).Replace("Controller", "").ToLower());
            }

            if (id == null)
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

            if (book.CopiesAvailable <= 0)
            {
                TempData["ErrorMessage"] = "Sorry, this book is currently not available for borrowing.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = ViewData["UserId"] != null ? Convert.ToInt32(ViewData["UserId"]) : -1;
            if (userId == -1)
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            var existingBorrow = await _context.BorrowedBooks
                .AnyAsync(bb => bb.BookId == id && bb.UserId == userId && bb.ReturnDate == null);
            if (existingBorrow)
            {
                TempData["ErrorMessage"] = "You already have this book borrowed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var borrowedBook = new BorrowedBook
            {
                BookId = book.Id,
                UserId = userId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14) // 2-week borrowing period
            };

            book.CopiesAvailable--;

            _context.BorrowedBooks.Add(borrowedBook);

            await _context.SaveChangesAsync();

            TempData["SuccesMessage"] = "Book borrowed successfully! Please return it by " + borrowedBook.DueDate.ToString("MMMM dd, yyyy");
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ViewData["UserId"] == null)
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            int userId = Convert.ToInt32(ViewData["UserId"]);

            var borrowedBook = await _context.BorrowedBooks
                .FirstOrDefaultAsync(bb => bb.BookId == id && bb.UserId == userId && bb.ReturnDate == null);

            if (borrowedBook == null)
            {
                return NotFound();
            }

            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book != null)
                {
                    book.CopiesAvailable++;
                }

                borrowedBook.ReturnDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DBConcurrencyException)
            {
                if(!BorrowedBookExists(borrowedBook.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            var viewModel = new BookViewModel();
            await RefreshCategoryList(viewModel);

            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            if (!ValidateBookImage(viewModel.BookImage))
            {
                await RefreshCategoryList(viewModel);
                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = await SaveBookImage(viewModel.BookImage);

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
                    viewModel.ImagePath = "/uploads/" + uniqueFileName;
                    viewModel.CopiesAvailable = viewModel.TotalCopies;

                    _context.Add(viewModel.ToEntity());
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while saving the book. Please try again.");
                }
            }

            await RefreshCategoryList(viewModel);
            return View(viewModel);
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
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookViewModel(book);
            await RefreshCategoryList(viewModel);

            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, BookViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    if (viewModel.BookImage != null)
                    {
                        if (!ValidateBookImage(viewModel.BookImage))
                        {
                            await RefreshCategoryList(viewModel);
                            return View(viewModel);
                        }

                        if (!string.IsNullOrEmpty(existingBook.ImagePath))
                        {
                            DeleteImage(existingBook.ImagePath);
                        }

                        // Save new image
                        string uniqueFileName = await SaveBookImage(viewModel.BookImage);
                        existingBook.ImagePath = "/uploads/" + uniqueFileName;
                    }

                    existingBook.Title = viewModel.Title;
                    existingBook.Description = viewModel.Description;
                    existingBook.Author = viewModel.Author;
                    existingBook.CategoryId = viewModel.CategoryId;
                    existingBook.TotalCopies = viewModel.TotalCopies;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DBConcurrencyException)
                {
                    if (!BookExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            await RefreshCategoryList(viewModel);
            return View(viewModel);
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

            var viewModel = new BookListViewModel
            {
                Id = book.Id,
                ImagePath = book.ImagePath,
                Title = book.Title,
                Description = book.Description,
                Author = book.Author,
                CategoryName = book.Category.Name,
                CopiesAvailable = book.CopiesAvailable,
                TotalCopies = book.TotalCopies
            };

            return View(viewModel);
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
                if (!string.IsNullOrEmpty(book.ImagePath))
                {
                    DeleteImage(book.ImagePath);
                }

                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private bool BookExists(int id)
        {
            return (_context.Books?.Any(book => book.Id == id)).GetValueOrDefault();
        }

        private bool BorrowedBookExists(int bookId)
        {
            return (_context.BorrowedBooks?.Any(borrowedBook => borrowedBook.BookId == bookId)).GetValueOrDefault();
        }

        private async Task RefreshCategoryList(BookViewModel viewModel)
        {
            var categories = await _context.Categories.ToListAsync();
            viewModel.CategoryList = new SelectList(
                categories,
                "Id",
                "Name",
                viewModel.CategoryId
            );
        }

        private bool ValidateBookImage(IFormFile? bookImage)
        {
            if (bookImage == null || bookImage.Length == 0)
            {
                ModelState.AddModelError("BookImage", "Please provide an image of the book");
                return false;
            }

            var extension = Path.GetExtension(bookImage.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("BookImage", "Only .jpg, .jpeg, and .png files are allowed");
                return false;
            }

            if (bookImage.Length > _maxFileSize)
            {
                ModelState.AddModelError("BookImage", "File size cannot exceed 5MB");
                return false;
            }

            return true;
        }

        private async Task<string> SaveBookImage(IFormFile? image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var uploadFolder = Path.Combine("wwwroot", "uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_bookImage";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string? imagePath)
        {
            // Delete the old image if it exists
            if (!string.IsNullOrEmpty(imagePath))
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
        }

        #endregion
    }
}
