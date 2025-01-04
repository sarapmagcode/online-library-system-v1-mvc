using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Data;
using OnlineLibrarySystem_v1.Models.Entities;
using OnlineLibrarySystem_v1.Models.ViewModels;

namespace OnlineLibrarySystem_v1.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly LibraryDbContext _context;

        public CategoriesController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'OnlineLibrarySystem_v1.Categories' is null.");
            }

            var categoriesQuery = from c in _context.Categories
                                  select c;

            var categories = await categoriesQuery.ToListAsync();

            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            // Use a ViewModel to exclude navigation properties in model validation

            if (ModelState.IsValid)
            {
                var newCategory = new Category
                {
                    Id = model.Id,
                    Name = model.Name
                };

                _context.Add(newCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult >Edit(int? id)
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryViewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(categoryViewModel);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = new Category
                    {
                        Id = model.Id,
                        Name = model.Name
                    };

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(model.Id))
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

            return View(model);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (ViewData["Role"] == null || !ViewData["Role"].Equals("admin"))
            {
                return RedirectToAction(nameof(AccountController.Login), nameof(AccountController).Replace("Controller", ""));
            }

            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryViewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(categoryViewModel);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'OnlineLibrarySystem_v1.Categories' is null.");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(c => c.Id == id)).GetValueOrDefault();
        }
    }
}
