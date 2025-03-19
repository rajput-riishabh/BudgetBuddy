using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BudgetBuddy.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using BudgetBuddy.Models.ViewModels;
using BudgetBuddy.Services;
using System;
using System.Linq;
using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HashSet<int> _predefinedCategoryIds = new HashSet<int> { 1, 2, 3, 4 };

        public CategoryApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/category/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var statistics = await _context.Categories
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    IsPredefined = _predefinedCategoryIds.Contains(c.CategoryId),
                    ExpenseCount = _context.Expenses
                        .Count(e => e.CategoryId == c.CategoryId && e.UserId == userId),
                    MonthlyTotal = _context.Expenses
                        .Where(e => e.CategoryId == c.CategoryId &&
                                  e.UserId == userId &&
                                  e.Date >= startOfMonth &&
                                  e.Date <= endOfMonth)
                        .Sum(e => e.Amount),
                    AllTimeTotal = _context.Expenses
                        .Where(e => e.CategoryId == c.CategoryId && e.UserId == userId)
                        .Sum(e => e.Amount)
                })
                .ToListAsync();

            return Ok(statistics);
        }

        // GET: api/category/validate-name?name=xyz
        [HttpGet("validate-name")]
        public async Task<IActionResult> ValidateName([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Ok(new { isValid = false, message = "Category name is required." });
            }

            var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excludeId.Value);
            }

            var exists = await query.AnyAsync();

            return Ok(new
            {
                isValid = !exists,
                message = exists ? "A category with this name already exists." : null
            });
        }

        // GET: api/category/search?term=xyz
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(c => c.Name.ToLower().Contains(term.ToLower()));
            }

            if (!isAdmin)
            {
                // Regular users can only see predefined categories and their own custom categories
                query = query.Where(c => _predefinedCategoryIds.Contains(c.CategoryId) || c.CreatedBy == userId);
            }

            var categories = await query
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    IsPredefined = _predefinedCategoryIds.Contains(c.CategoryId),
                    CanEdit = isAdmin || (!_predefinedCategoryIds.Contains(c.CategoryId) && c.CreatedBy == userId)
                })
                .Take(10)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/category/monthly-trends
        [HttpGet("monthly-trends")]
        public async Task<IActionResult> GetMonthlyTrends([FromQuery] int months = 6)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Now;
            var startDate = currentDate.AddMonths(-months + 1).Date;

            var trends = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate)
                .GroupBy(e => new
                {
                    Year = e.Date.Year,
                    Month = e.Date.Month,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.Name
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var results = trends
                .GroupBy(t => new { t.CategoryId, t.CategoryName })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    MonthlyData = g.Select(m => new
                    {
                        Date = $"{m.Year}-{m.Month:D2}",
                        Total = m.Total
                    }).ToList()
                })
                .ToList();

            return Ok(results);
        }

        // POST: api/category/batch-update
        [HttpPost("batch-update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BatchUpdate([FromBody] List<CategoryBatchUpdateModel> updates)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Now;

            foreach (var update in updates)
            {
                var category = await _context.Categories.FindAsync(update.CategoryId);
                if (category != null)
                {
                    category.Name = update.Name;
                    category.UpdatedBy = userId;
                    category.UpdatedAt = currentDate;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Categories updated successfully" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "An error occurred while updating categories" });
            }
        }
    }

    public class CategoryBatchUpdateModel
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}