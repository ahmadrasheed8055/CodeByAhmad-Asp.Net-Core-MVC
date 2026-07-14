using FitMind_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMind_API.Models.Entities;
using FitMind_API.Models.DTOs;
using PostComments = FitMind_API.Models.DTOs.PostComments;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly Data.FMDBContext _context;

        public CommentsController(FMDBContext context)
        {
            _context = context;
        }

        // GET: api/comments/getAll/{postId}?userId=optional
        [HttpGet("getAll/{postId}")]
        public async Task<ActionResult<List<FitMind_API.Models.DTOs.PostComments>>> GetAllComments(int postId, [FromQuery] int? userId)
        {
            if (postId == 0)
                return BadRequest(new { message = "PostId is required" });

            var comments = await _context.PostComments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new FitMind_API.Models.DTOs.PostComments
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            return Ok(comments);
        }

        // GET: api/comments/getUserComments/{userId}
        [HttpGet("getUserComments/{userId}")]
        public async Task<ActionResult<List<PostComments>>> GetUserComments(int userId)
        {
            if (userId == 0)
                return BadRequest(new { message = "UserId is required" });

            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound(new { message = "User not found" });

            var comments = await _context.PostComments
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new PostComments
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            return Ok(comments);
        }

        // DELETE: api/comments/delete/{userId}/{commentId}
        // Soft-delete: only owner can delete their comment
        [HttpDelete("delete/{userId}/{commentId}")]
        public async Task<IActionResult> DeleteOwnComment(int userId, int commentId)
        {
            if (userId == 0 || commentId == 0)
                return BadRequest(new { message = "IDs are required" });

            var comment = await _context.PostComments.FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);
            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            if (comment.UserId != userId)
                return Forbid();

            comment.IsDeleted = true;
            _context.PostComments.Update(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment deleted" });
        }

        // POST: api/comments/react
        // Body: CommentReactionDTO { CommentId, UserId, IsLike }
        [HttpPost("react")]
        public async Task<IActionResult> ReactToComment([FromBody] CommentReactionDTO dto)
        {
            if (dto == null || dto.CommentId == 0 || dto.UserId == 0)
                return BadRequest(new { message = "CommentId and UserId are required" });

            var commentExists = await _context.PostComments.AnyAsync(c => c.CommentId == dto.CommentId && !c.IsDeleted);
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == dto.UserId);
            if (!commentExists || !userExists)
                return NotFound(new { message = "Comment or User not found" });

            var existing = await _context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == dto.CommentId && r.UserId == dto.UserId);

            if (existing != null)
            {
                // If same reaction again -> remove (toggle off)
                if (existing.IsLike == dto.IsLike)
                {
                    _context.CommentReactions.Remove(existing);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Reaction removed" });
                }

                // Update reaction
                existing.IsLike = dto.IsLike;
                existing.UpdatedAt = DateTime.Now;
                _context.CommentReactions.Update(existing);
            }
            else
            {
                var reaction = new CommentReactions
                {
                    CommentId = dto.CommentId,
                    UserId = dto.UserId,
                    IsLike = dto.IsLike,
                    ReactedAt = DateTime.Now
                };
                _context.CommentReactions.Add(reaction);
            }

            await _context.SaveChangesAsync();

            var likes = await _context.CommentReactions.CountAsync(r => r.CommentId == dto.CommentId && r.IsLike == true);
            var dislikes = await _context.CommentReactions.CountAsync(r => r.CommentId == dto.CommentId && r.IsLike == false);

            return Ok(new { message = "Reaction recorded", likes, dislikes });
        }

        // DELETE: api/comments/removeReaction/{userId}/{commentId}
        [HttpDelete("removeReaction/{userId}/{commentId}")]
        public async Task<IActionResult> RemoveCommentReaction(int userId, int commentId)
        {
            if (userId == 0 || commentId == 0)
                return BadRequest(new { message = "IDs are required" });

            var existing = await _context.CommentReactions.FirstOrDefaultAsync(r => r.UserId == userId && r.CommentId == commentId);
            if (existing == null)
                return NotFound(new { message = "Reaction not found" });

            _context.CommentReactions.Remove(existing);
            await _context.SaveChangesAsync();

            var likes = await _context.CommentReactions.CountAsync(r => r.CommentId == commentId && r.IsLike == true);
            var dislikes = await _context.CommentReactions.CountAsync(r => r.CommentId == commentId && r.IsLike == false);

            return Ok(new { message = "Reaction removed", likes, dislikes });
        }
    }
}