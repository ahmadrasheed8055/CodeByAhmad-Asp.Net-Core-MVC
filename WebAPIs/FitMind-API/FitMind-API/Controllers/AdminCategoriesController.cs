using FitMind_API.Data;
using FitMind_API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Controllers
{
    [Route("api/admin/categories")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminCategoriesController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.ImageUrl,
                    c.Description,
                    PostCount = _context.AddPosts.Count(p => p.CategoryId == c.Id)
                })
                .ToListAsync();

            return Ok(categories);
        }

        public class CategoryDTO
        {
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO request)
        {
            var category = new Categories
            {
                Name = request.Name,
                Slug = string.IsNullOrEmpty(request.Slug) ? GenerateSlug(request.Name) : request.Slug,
                ImageUrl = request.ImageUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDTO request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = request.Name;
            category.Slug = string.IsNullOrEmpty(request.Slug) ? GenerateSlug(request.Name) : request.Slug;
            category.ImageUrl = request.ImageUrl;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var postCount = await _context.AddPosts.CountAsync(p => p.CategoryId == id);
            if (postCount > 0)
            {
                return BadRequest($"Cannot delete category. There are {postCount} posts attached to it.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted successfully." });
        }

        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}
