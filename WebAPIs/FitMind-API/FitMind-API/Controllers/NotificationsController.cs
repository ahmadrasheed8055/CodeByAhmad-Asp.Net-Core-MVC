using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMind_API.Data;
using FitMind_API.Models.Entities;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public NotificationsController(FMDBContext context)
        {
            _context = context;
        }

        // GET: api/Notifications/getMyNotifications/{userId}
        [HttpGet("getMyNotifications/{userId}")]
        public async Task<ActionResult<IEnumerable<AppNotification>>> GetMyNotifications(int userId)
        {
            if (userId == 0)
                return BadRequest("Invalid user ID");

            var notifications = await _context.AppNotifications
                .Where(n => n.TargetUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Ok(notifications);
        }

        // PUT: api/Notifications/markAsRead/{id}
        [HttpPut("markAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notif = await _context.AppNotifications.FindAsync(id);
            if (notif == null)
                return NotFound();

            notif.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/Notifications/markAllAsRead/{userId}
        [HttpPut("markAllAsRead/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var notifications = await _context.AppNotifications
                .Where(n => n.TargetUserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Notifications/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notif = await _context.AppNotifications.FindAsync(id);
            if (notif == null)
                return NotFound();

            _context.AppNotifications.Remove(notif);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
