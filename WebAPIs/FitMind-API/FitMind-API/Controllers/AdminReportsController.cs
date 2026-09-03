using FitMind_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitMind_API.Controllers
{
    [Route("api/admin/reports")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminReportsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminReportsController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetReportsSummary()
        {
            var total = await _context.Reports.CountAsync();
            var pending = await _context.Reports.CountAsync(r => r.Status.ToLower() == "pending");
            var underReview = await _context.Reports.CountAsync(r => r.Status.ToLower() == "under review");
            var resolved = await _context.Reports.CountAsync(r => r.Status.ToLower() == "resolved");
            var dismissed = await _context.Reports.CountAsync(r => r.Status.ToLower() == "dismissed");

            return Ok(new
            {
                Total = total,
                Pending = pending,
                UnderReview = underReview,
                Resolved = resolved,
                Dismissed = dismissed
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string status = "", [FromQuery] string type = "")
        {
            var query = _context.Reports.Include(r => r.ReporterUser).Include(r => r.ReviewedByAdmin).AsQueryable();

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                var lowerStatus = status.ToLower();
                query = query.Where(r => r.Status.ToLower() == lowerStatus);
            }

            if (!string.IsNullOrEmpty(type) && type.ToLower() != "all")
            {
                var lowerType = type.ToLower();
                query = query.Where(r => r.TargetType.ToLower() == lowerType);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.ReportId,
                    r.TargetType,
                    r.TargetId,
                    r.Reason,
                    r.Status,
                    r.CreatedAt,
                    ReporterName = r.ReporterUser.Username,
                    ReviewedBy = r.ReviewedByAdmin != null ? r.ReviewedByAdmin.Email : null
                })
                .ToListAsync();

            return Ok(new { data = reports, totalItems, totalPages, currentPage = page });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportDetails(int id)
        {
            var report = await _context.Reports
                .Include(r => r.ReporterUser)
                .Include(r => r.ReviewedByAdmin)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null) return NotFound();

            object targetDetails = null;

            if (report.TargetType.ToLower() == "post")
            {
                var post = await _context.AddPosts
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PostId == report.TargetId);
                
                if (post != null)
                {
                    targetDetails = new
                    {
                        post.PostId,
                        post.Title,
                        post.Description,
                        post.CreatedAt,
                        HasMedia = post.PostImage != null,
                        PostImageBase64 = post.PostImage != null ? Convert.ToBase64String(post.PostImage) : null,
                        AuthorId = post.UserId,
                        AuthorName = post.User.Username
                    };
                }
            }
            else if (report.TargetType.ToLower() == "poll")
            {
                var poll = await _context.Polls
                    .Include(p => p.Post)
                    .ThenInclude(post => post.User)
                    .Include(p => p.Options)
                    .FirstOrDefaultAsync(p => p.PollId == report.TargetId);

                if (poll != null)
                {
                    targetDetails = new
                    {
                        poll.PollId,
                        poll.Question,
                        poll.CreatedAt,
                        AuthorId = poll.Post.UserId,
                        AuthorName = poll.Post.User.Username,
                        Options = poll.Options.Select(o => new { o.OptionId, o.OptionText })
                    };
                }
            }
            else if (report.TargetType.ToLower() == "profile")
            {
                var profile = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Id == report.TargetId);

                if (profile != null)
                {
                    targetDetails = new
                    {
                        profile.Id,
                        profile.Username,
                        profile.Email,
                        profile.Bio,
                        profile.JoinedDate,
                        profile.Status
                    };
                }
            }

            return Ok(new
            {
                Report = new
                {
                    report.ReportId,
                    report.TargetType,
                    report.TargetId,
                    report.Reason,
                    report.Status,
                    report.CreatedAt,
                    Reporter = new { report.ReporterUser.Id, report.ReporterUser.Username, report.ReporterUser.Email },
                    ReviewedBy = report.ReviewedByAdmin != null ? new { report.ReviewedByAdmin.AdminId, report.ReviewedByAdmin.Email } : null
                },
                TargetDetails = targetDetails
            });
        }

        public class UpdateStatusDTO
        {
            public string Status { get; set; } = string.Empty;
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] UpdateStatusDTO request)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            var validStatuses = new[] { "Pending", "Under Review", "Resolved", "Dismissed" };
            if (!validStatuses.Any(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status.");
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(adminIdClaim, out int adminId))
            {
                report.ReviewedByAdminId = adminId;
            }

            // Keep the original casing from validStatuses array
            string previousStatus = report.Status;
            report.Status = validStatuses.First(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            
            // Add notification when report is resolved or dismissed
            if (report.Status == "Resolved" || report.Status == "Dismissed")
            {
                var notification = new FitMind_API.Models.Entities.AppNotification
                {
                    TargetUserId = report.ReporterUserId,
                    ActorName = "FitJoin Admin",
                    ActorImage = null,
                    NotificationType = "report_update",
                    Message = $"Your report regarding a {report.TargetType.ToLower()} has been marked as {report.Status}.",
                    TargetId = report.ReportId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AppNotifications.Add(notification);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Report status updated to {report.Status}." });
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveReport(int id)
        {
            return await UpdateReportStatus(id, new UpdateStatusDTO { Status = "Resolved" });
        }

        [HttpPut("{id}/dismiss")]
        public async Task<IActionResult> DismissReport(int id)
        {
            return await UpdateReportStatus(id, new UpdateStatusDTO { Status = "Dismissed" });
        }
    }
}

