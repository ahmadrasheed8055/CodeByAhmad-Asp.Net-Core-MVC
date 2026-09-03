using FitMind_API.Data;
using FitMind_API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Controllers
{
    [Route("api/admin/admins")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminManagementController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminManagementController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _context.AdminUsers
                .OrderByDescending(a => a.FirstLoginAt)
                .Select(a => new
                {
                    a.AdminId,
                    a.Email,
                    a.DisplayName,
                    a.FirstLoginAt,
                    a.LastLoginAt,
                    a.IsActive
                })
                .ToListAsync();

            return Ok(admins);
        }

        public class CreateAdminDTO
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin([FromBody] CreateAdminDTO request)
        {
            var email = request.Email.ToLower().Trim();
            if (await _context.AdminUsers.AnyAsync(a => a.Email.ToLower() == email))
            {
                return BadRequest("Admin with this email already exists.");
            }

            var newAdmin = new AdminUser
            {
                Email = email,
                FirstLoginAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AdminUsers.Add(newAdmin);
            await _context.SaveChangesAsync();
            return Ok(newAdmin);
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleAdminStatus(int id)
        {
            var admin = await _context.AdminUsers.FindAsync(id);
            if (admin == null) return NotFound();

            var currentAdminEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (currentAdminEmail != null && currentAdminEmail.Equals(admin.Email, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You cannot deactivate your own admin account.");
            }

            admin.IsActive = !admin.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Admin status updated.", isActive = admin.IsActive });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.AdminUsers.FindAsync(id);
            if (admin == null) return NotFound();

            var currentAdminEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (currentAdminEmail != null && currentAdminEmail.Equals(admin.Email, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You cannot delete your own admin account.");
            }

            _context.AdminUsers.Remove(admin);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Admin deleted successfully." });
        }
    }
}
