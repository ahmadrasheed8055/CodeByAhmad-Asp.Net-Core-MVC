using FitMind_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Controllers
{
    [Route("api/admin/posts")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminPostsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminPostsController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "", [FromQuery] bool? isPublished = null)
        {
            var query = _context.AddPosts.Include(p => p.User).Include(p => p.Category).Include(p => p.Comments).Include(p => p.postReactions).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(lowerSearch) || p.User.Username.ToLower().Contains(lowerSearch));
            }

            if (isPublished.HasValue)
            {
                query = query.Where(p => p.IsPublished == isPublished.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.PostId,
                    p.Title,
                    Snippet = p.Description.Length > 100 ? p.Description.Substring(0, 100) + "..." : p.Description,
                    AuthorName = p.User.Username,
                    CategoryName = p.Category.Name,
                    p.CreatedAt,
                    ReactionCount = p.postReactions.Count,
                    CommentCount = p.Comments.Count,
                    p.IsPublished,
                    p.IsDeleted
                })
                .ToListAsync();

            return Ok(new { data = posts, totalItems, totalPages, currentPage = page });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _context.AddPosts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.postReactions)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            var result = new
            {
                post.PostId,
                post.Title,
                post.Description,
                AuthorName = post.User.Username,
                CategoryName = post.Category.Name,
                post.CreatedAt,
                post.IsPublished,
                ReactionCount = post.postReactions?.Count ?? 0,
                Comments = post.Comments?.Select(c => new
                {
                    c.CommentId,
                    c.CommentContent,
                    c.CreatedAt,
                    AuthorName = c.User.Username,
                    c.IsDeleted
                })
            };

            return Ok(result);
        }

        [HttpPut("{id}/toggle-visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var post = await _context.AddPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.IsPublished = !post.IsPublished;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post visibility toggled successfully.", isPublished = post.IsPublished });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.AddPosts.FindAsync(id);
            if (post == null) return NotFound();

            _context.AddPosts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post deleted successfully." });
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.PostComments.FindAsync(commentId);
            if (comment == null) return NotFound();

            _context.PostComments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Comment deleted successfully." });
        }
    }
}
