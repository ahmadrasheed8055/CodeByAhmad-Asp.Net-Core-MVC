using FitMind_API.Data;
using FitMind_API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitMind_API.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    public class PublicReportsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public PublicReportsController(FMDBContext context)
        {
            _context = context;
        }

        public class ReportRequestDTO
        {
            public string Reason { get; set; } = string.Empty;
        }

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var reports = await _context.Reports
                .Where(r => r.ReporterUserId == userId && r.Status == "Pending")
                .Select(r => new { r.TargetType, r.TargetId })
                .ToListAsync();

            var reportedPosts = reports.Where(r => r.TargetType == "post").Select(r => r.TargetId).ToList();
            var reportedPolls = reports.Where(r => r.TargetType == "poll").Select(r => r.TargetId).ToList();
            var reportedProfiles = reports.Where(r => r.TargetType == "profile").Select(r => r.TargetId).ToList();

            return Ok(new
            {
                reportedPosts,
                reportedPolls,
                reportedProfiles
            });
        }

        [HttpPost("posts/{id}")]
        public async Task<IActionResult> ReportPost(int id, [FromBody] ReportRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var post = await _context.AddPosts.FindAsync(id);
            if (post == null) return NotFound("Post not found.");

            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.TargetType == "post" && r.TargetId == id && r.ReporterUserId == userId);
            if (existingReport != null)
                return BadRequest(new { message = "You have already reported this post." });

            var report = new Report
            {
                TargetType = "post",
                TargetId = id,
                ReporterUserId = userId,
                Reason = request.Reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Report submitted successfully. Thank you for helping keep FitJoin safe." });
        }

        [HttpPost("polls/{id}")]
        public async Task<IActionResult> ReportPoll(int id, [FromBody] ReportRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var poll = await _context.Polls.FindAsync(id);
            if (poll == null) return NotFound("Poll not found.");

            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.TargetType == "poll" && r.TargetId == id && r.ReporterUserId == userId);
            if (existingReport != null)
                return BadRequest(new { message = "You have already reported this poll." });

            var report = new Report
            {
                TargetType = "poll",
                TargetId = id,
                ReporterUserId = userId,
                Reason = request.Reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Report submitted successfully. Thank you for helping keep FitJoin safe." });
        }

        [HttpPost("profiles/{id}")]
        public async Task<IActionResult> ReportProfile(int id, [FromBody] ReportRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            if (userId == id) return BadRequest(new { message = "You cannot report your own profile." });

            var profile = await _context.AppUsers.FindAsync(id);
            if (profile == null) return NotFound("Profile not found.");

            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.TargetType == "profile" && r.TargetId == id && r.ReporterUserId == userId);
            if (existingReport != null)
                return BadRequest(new { message = "You have already reported this profile." });

            var report = new Report
            {
                TargetType = "profile",
                TargetId = id,
                ReporterUserId = userId,
                Reason = request.Reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Report submitted successfully. Thank you for helping keep FitJoin safe." });
        }
    }
}
