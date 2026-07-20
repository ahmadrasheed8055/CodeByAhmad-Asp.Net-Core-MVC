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
        // Fetch root comments along with their nested replies counters and metadata
        [HttpGet("getAll/{postId}")]
        public async Task<ActionResult<List<FitMind_API.Models.DTOs.PostComments>>> GetAllComments(int postId, [FromQuery] int? userId)
        {
            if (postId == 0)
                return BadRequest(new { message = "PostId is required" });

            // Only fetch root comments (where ParentCommentId is null) for the post
            var comments = await _context.PostComments
                .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Replies)
                .Include(c => c.CommentReactions)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new FitMind_API.Models.DTOs.PostComments
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    IsDeleted = c.IsDeleted,
                    ParentCommentId = c.ParentCommentId,

                    // Calculate metadata
                    RepliesCount = c.Replies.Count(r => !r.IsDeleted), // Exclude soft-deleted replies
                    LikeCount = c.CommentReactions.Count(r => r.IsLike == true),
                    DislikeCount = c.CommentReactions.Count(r => r.IsLike == false),
                    IsReactedByMe = userId.HasValue && userId != 0 
                        ? c.CommentReactions.Where(r => r.UserId == userId).Select(r => (bool?)r.IsLike).FirstOrDefault()
                        : (bool?)null,

                    // User things
                    UserName = c.User.Username,
                    UserImage = c.User.ProfilePhoto != null
                                        ? Convert.ToBase64String(c.User.ProfilePhoto)
                                        : null
                })
                .ToListAsync();

            return Ok(comments);
        }

        // GET: api/comments/getReplies/{commentId}?userId=optional
        // Fetch nested replies for a specific comment
        [HttpGet("getReplies/{commentId}")]
        public async Task<ActionResult<List<FitMind_API.Models.DTOs.PostComments>>> GetReplies(int commentId, [FromQuery] int? userId)
        {
            if (commentId == 0)
                return BadRequest(new { message = "CommentId is required" });

            // Fetch replies where ParentCommentId matches the requested commentId
            var replies = await _context.PostComments
                .Where(c => c.ParentCommentId == commentId && !c.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Replies)
                .Include(c => c.CommentReactions)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new FitMind_API.Models.DTOs.PostComments
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    IsDeleted = c.IsDeleted,
                    ParentCommentId = c.ParentCommentId,

                    // Calculate metadata for replies (in case replies have their own nested replies)
                    RepliesCount = c.Replies.Count(r => !r.IsDeleted),
                    LikeCount = c.CommentReactions.Count(r => r.IsLike == true),
                    DislikeCount = c.CommentReactions.Count(r => r.IsLike == false),
                    IsReactedByMe = userId.HasValue && userId != 0 
                        ? c.CommentReactions.Where(r => r.UserId == userId).Select(r => (bool?)r.IsLike).FirstOrDefault()
                        : (bool?)null,

                    // User things
                    UserName = c.User.Username,
                    UserImage = c.User.ProfilePhoto != null
                                        ? Convert.ToBase64String(c.User.ProfilePhoto)
                                        : null
                })
                .ToListAsync();

            return Ok(replies);
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
                    IsDeleted = c.IsDeleted,
                    ParentCommentId = c.ParentCommentId
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/comments/add-comment
        // Create a new comment or add a nested reply by specifying a ParentCommentId
        [HttpPost("add-comment")]
        public async Task<IActionResult> AddComment([FromBody] FitMind_API.Models.DTOs.PostComments commentDto)
        {
            if (commentDto == null)
                return BadRequest("Comment data is null.");

            if (commentDto.PostId == 0 || commentDto.UserId == 0)
                return BadRequest(new { message = "PostId and UserId are required." });

            if (string.IsNullOrWhiteSpace(commentDto.CommentContent))
                return BadRequest(new { message = "CommentContent is required." });

            if (commentDto.CommentContent.Length > 1000)
                return BadRequest(new { message = "CommentContent exceeds maximum length of 1000 characters." });

            var user = await _context.AppUsers.FindAsync(commentDto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var post = await _context.AddPosts.FindAsync(commentDto.PostId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            // If ParentCommentId is provided, ensure the parent comment exists
            if (commentDto.ParentCommentId.HasValue && commentDto.ParentCommentId != 0)
            {
                var parentComment = await _context.PostComments.FindAsync(commentDto.ParentCommentId.Value);
                if (parentComment == null || parentComment.IsDeleted)
                {
                    return NotFound(new { message = "Parent comment not found or has been deleted." });
                }
            }

            var comment = new Models.Entities.PostComments
            {
                PostId = commentDto.PostId,
                UserId = commentDto.UserId,
                CommentContent = commentDto.CommentContent,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                ParentCommentId = (commentDto.ParentCommentId.HasValue && commentDto.ParentCommentId != 0) ? commentDto.ParentCommentId : null
            };

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { CommentId = comment.CommentId, message = "Comment added successfully." });
        }


        // DELETE: api/comments/delete/{userId}/{commentId}
        // Soft-delete: only owner can delete their comment. Deleting a comment recursively soft-deletes its nested replies.
        [HttpDelete("delete/{userId}/{commentId}")]
        public async Task<IActionResult> DeleteOwnComment(int userId, int commentId)
        {
            if (userId == 0 || commentId == 0)
                return BadRequest(new { message = "IDs are required" });

            // Fetch the comment and its nested replies (to perform recursive soft-deletion)
            var comment = await _context.PostComments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            if (comment.UserId != userId)
                return Forbid();

            // Soft-delete the comment
            comment.IsDeleted = true;
            _context.PostComments.Update(comment);

            // Recursively soft-delete all nested replies
            await SoftDeleteReplies(comment.Replies);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment and its replies deleted successfully" });
        }

        // Helper method to recursively soft-delete replies
        private async Task SoftDeleteReplies(IEnumerable<Models.Entities.PostComments> replies)
        {
            foreach (var reply in replies.Where(r => !r.IsDeleted))
            {
                reply.IsDeleted = true;
                _context.PostComments.Update(reply);

                // Fetch nested replies for the current reply if any, and delete them
                var nestedReplies = await _context.PostComments
                    .Where(c => c.ParentCommentId == reply.CommentId && !c.IsDeleted)
                    .ToListAsync();

                if (nestedReplies.Any())
                {
                    await SoftDeleteReplies(nestedReplies);
                }
            }
        }


        // POST: api/comments/{id}/like
        // Toggle interaction logic: if already liked, clicking like again removes it.
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeComment(int id, [FromQuery] int userId)
        {
            if (userId == 0 || id == 0)
                return BadRequest(new { message = "CommentId and UserId are required" });

            var commentExists = await _context.PostComments.AnyAsync(c => c.CommentId == id && !c.IsDeleted);
            if (!commentExists)
                return NotFound(new { message = "Comment not found" });

            var existingReaction = await _context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == id && r.UserId == userId);

            if (existingReaction != null)
            {
                // If it's already a like, remove it (toggle off)
                if (existingReaction.IsLike == true)
                {
                    _context.CommentReactions.Remove(existingReaction);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Like removed" });
                }
                
                // If it was a dislike, switch to like
                existingReaction.IsLike = true;
                existingReaction.UpdatedAt = DateTime.Now;
                _context.CommentReactions.Update(existingReaction);
            }
            else
            {
                // Add new like reaction
                var reaction = new CommentReactions
                {
                    CommentId = id,
                    UserId = userId,
                    IsLike = true,
                    ReactedAt = DateTime.Now
                };
                _context.CommentReactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Comment liked successfully" });
        }

        // POST: api/comments/{id}/dislike
        // Toggle interaction logic: if already disliked, clicking dislike again removes it.
        [HttpPost("{id}/dislike")]
        public async Task<IActionResult> DislikeComment(int id, [FromQuery] int userId)
        {
            if (userId == 0 || id == 0)
                return BadRequest(new { message = "CommentId and UserId are required" });

            var commentExists = await _context.PostComments.AnyAsync(c => c.CommentId == id && !c.IsDeleted);
            if (!commentExists)
                return NotFound(new { message = "Comment not found" });

            var existingReaction = await _context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == id && r.UserId == userId);

            if (existingReaction != null)
            {
                // If it's already a dislike, remove it (toggle off)
                if (existingReaction.IsLike == false)
                {
                    _context.CommentReactions.Remove(existingReaction);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Dislike removed" });
                }
                
                // If it was a like, switch to dislike
                existingReaction.IsLike = false;
                existingReaction.UpdatedAt = DateTime.Now;
                _context.CommentReactions.Update(existingReaction);
            }
            else
            {
                // Add new dislike reaction
                var reaction = new CommentReactions
                {
                    CommentId = id,
                    UserId = userId,
                    IsLike = false,
                    ReactedAt = DateTime.Now
                };
                _context.CommentReactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Comment disliked successfully" });
        }
    }
}