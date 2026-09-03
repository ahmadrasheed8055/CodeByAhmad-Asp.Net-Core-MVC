using FitMind_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminUsersController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminUsersController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "", [FromQuery] int? status = null)
        {
            var query = _context.AppUsers.Include(u => u.postReactions).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(lowerSearch) || 
                                         u.Email.ToLower().Contains(lowerSearch) ||
                                         (u.UniqueName != null && u.UniqueName.ToLower().Contains(lowerSearch)));
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.JoinedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.UniqueName,
                    u.JoinedDate,
                    u.Status,
                    u.IsDeleted,
                    PostCount = _context.AddPosts.Count(p => p.UserId == u.Id),
                    u.ProfilePhoto // Note: This might be large if stored as byte array, consider removing if it slows down list
                })
                .ToListAsync();

            return Ok(new { data = users, totalItems, totalPages, currentPage = page });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.AppUsers
                .Select(u => new {
                    u.Id, u.Username, u.Email, u.UniqueName, u.Bio, u.Phone, u.Location, u.Country,
                    u.JoinedDate, u.Status, u.IsDeleted,
                    PostCount = _context.AddPosts.Count(p => p.UserId == u.Id),
                    FollowersCount = _context.UserFollowers.Count(f => f.FollowingId == u.Id),
                    FollowingCount = _context.UserFollowers.Count(f => f.FollowerId == u.Id)
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] int status)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            // Check if user is an admin
            var isAdmin = await _context.AdminUsers.AnyAsync(a => a.Email.ToLower() == user.Email.ToLower() && a.IsActive);
            if (isAdmin && (status == 2 || status == 3)) // Prevent suspending or banning admins
            {
                return BadRequest("Cannot suspend or ban a whitelisted admin account.");
            }

            user.Status = status; // 1 = Active, 2 = Suspended, 3 = Banned
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Status updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            var isAdmin = await _context.AdminUsers.AnyAsync(a => a.Email.ToLower() == user.Email.ToLower() && a.IsActive);
            if (isAdmin)
            {
                return BadRequest("Cannot delete a whitelisted admin account.");
            }

            // Note: DB relationships have restrictive deletes for AppUser.
            // Soft delete might be better:
            user.IsDeleted = true;
            user.Status = 3; // Banned/Deactivated
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
