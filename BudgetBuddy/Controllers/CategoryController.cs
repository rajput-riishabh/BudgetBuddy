using System.Security.Claims;
using BudgetBuddy.Models;
using BudgetBuddy.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BudgetBuddy.Controllers
{

    [Authorize]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly HashSet<int> _predefinedCategoryIds = new HashSet<int> { 1, 2, 3, 4 };
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            var categories = await _context.Categories
                .Where(c => isAdmin || c.CreatedBy == 0 || !_predefinedCategoryIds.Contains(c.CategoryId) && c.CreatedBy == userId || _predefinedCategoryIds.Contains(c.CategoryId))
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    IsPredefined = (c.CreatedBy == 0),
                    ExpenseCount = _context.Expenses.Count(e => e.CategoryId == c.CategoryId && e.UserId == userId),
                    TotalExpenses = _context.Expenses
                        .Where(e => e.CategoryId == c.CategoryId && e.UserId == userId)
                        .Sum(e => e.Amount),
                    CanEdit = isAdmin || (!_predefinedCategoryIds.Contains(c.CategoryId) && c.CreatedBy == userId),
                    CanDelete = isAdmin || (!_predefinedCategoryIds.Contains(c.CategoryId) && c.CreatedBy == userId),
                    CategoryType = (c.CreatedBy == 0) ? "System" : "Custom",
                    CreatedByUser = (c.CreatedBy == 0) ? "System" : _context.Users.Where(u => u.UserId == c.CreatedBy).Select(u => u.UserName).FirstOrDefault()
                })
                .ToListAsync();

            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreatePredefined()
        {
            return View("Create", new CategoryViewModel { IsPredefined = true });
        }

        public IActionResult Create()
        {
            if (!User.IsInRole("Admin"))
            {
                // Regular users can only create custom categories
                return View(new CategoryViewModel { IsPredefined = false });
            }
            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var isAdmin = User.IsInRole("Admin");

                // Regular users cannot create predefined categories
                if (viewModel.IsPredefined && !isAdmin)
                {
                    return Forbid();
                }

                // Check if category name already exists
                if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == viewModel.Name.ToLower()))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(viewModel);
                }

                var category = new Category
                {
                    Name = viewModel.Name,
                    CreatedBy = viewModel.IsPredefined && isAdmin ? 0 : userId, // Set CreatedBy based on IsPredefined and Admin role
                    CreatedAt = DateTime.Now
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if user has permission to edit
            if (!isAdmin && (_predefinedCategoryIds.Contains(category.CategoryId) || category.CreatedBy != userId))
            {
                return Forbid();
            }

            var viewModel = new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                IsPredefined = (category.CreatedBy == 0)
            };
            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel viewModel)
        {
            if (id != viewModel.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var isAdmin = User.IsInRole("Admin");
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound();
                }

                // Check if user has permission to edit
                if (!isAdmin && (_predefinedCategoryIds.Contains(category.CategoryId) || category.CreatedBy != userId))
                {
                    return Forbid();
                }

                // Check if new name already exists (excluding current category)
                if (await _context.Categories.AnyAsync(c => c.CategoryId != id && c.Name.ToLower() == viewModel.Name.ToLower()))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(viewModel);
                }

                try
                {
                    category.Name = viewModel.Name;
                    category.UpdatedBy = userId;
                    category.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(viewModel.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(int id)

        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var isAdmin = User.IsInRole("Admin");



            var category = await _context.Categories.FindAsync(id);

            if (category == null)

            {

                return NotFound();

            }



            // Check if user has permission to delete

            if (!isAdmin && (_predefinedCategoryIds.Contains(category.CategoryId) || category.CreatedBy != userId))

            {

                return Forbid();

            }



            // Check if category has any expenses

            if (await _context.Expenses.AnyAsync(e => e.CategoryId == id))

            {

                TempData["Error"] = "Cannot delete category because it has associated expenses.";

                return RedirectToAction(nameof(Index));

            }

          


            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";

            return RedirectToAction(nameof(Index));

        }



        private bool CategoryExists(int id)

        {

            return _context.Categories.Any(e => e.CategoryId == id);

        }

    }

}