using FitMind_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FitMind_API.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminDashboardController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var totalUsers = await _context.AppUsers.CountAsync();
                var activeUsers = await _context.AppUsers.CountAsync(u => u.Status == 1 && !u.IsDeleted);
                var totalPosts = await _context.AddPosts.CountAsync(p => !p.IsDeleted);
                var totalComments = await _context.PostComments.CountAsync(c => !c.IsDeleted);
                var pendingReports = await _context.Reports.CountAsync(r => r.Status == "Pending");
                var activePolls = await _context.Polls.CountAsync();
                var totalCategories = await _context.Categories.CountAsync();
                var totalAdmins = await _context.AdminUsers.CountAsync(a => a.IsActive);

                var recentPosts = await _context.AddPosts
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .Select(p => new
                    {
                        p.PostId,
                        p.Title,
                        AuthorName = p.User != null ? p.User.Username : "Member",
                        CategoryName = p.Category != null ? p.Category.Name : "General",
                        p.IsPublished,
                        p.ViewCount,
                        p.CreatedAt
                    })
                    .ToListAsync();

                var recentUsers = await _context.AppUsers
                    .OrderByDescending(u => u.JoinedDate)
                    .Take(8)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.JoinedDate,
                        u.Status
                    })
                    .ToListAsync();

                var recentReports = await _context.Reports
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(6)
                    .Select(r => new
                    {
                        r.ReportId,
                        r.TargetType,
                        r.TargetId,
                        r.Reason,
                        r.Status,
                        r.CreatedAt
                    })
                    .ToListAsync();

                // Live Database & System Diagnostics
                var canConnect = await _context.Database.CanConnectAsync();
                var process = Process.GetCurrentProcess();

                return Ok(new
                {
                    summary = new
                    {
                        totalUsers,
                        activeUsers,
                        totalPosts,
                        totalComments,
                        pendingReports,
                        activePolls,
                        totalCategories,
                        totalAdmins
                    },
                    recentActivity = new
                    {
                        posts = recentPosts,
                        users = recentUsers,
                        reports = recentReports
                    },
                    liveDb = new
                    {
                        status = canConnect ? "Online" : "Degraded",
                        databaseName = "FitMindDB",
                        serverTime = DateTime.UtcNow,
                        uptimeMinutes = Math.Round((DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMinutes, 1),
                        allocatedMemoryMb = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading dashboard metrics", error = ex.Message });
            }
        }
    }
}
