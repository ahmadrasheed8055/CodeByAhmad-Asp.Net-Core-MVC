using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMind_API.Data;
using FitMind_API.Models.Entities;
using FitMind_API.Models.DTOs;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostReactionsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public PostReactionsController(FMDBContext context)
        {
            _context = context;
        }


        //get post likes & dislikes
        [HttpGet("postReactionsCount/{postId}")]
        public async Task<IActionResult> GetPostReactionsCount(int postId)
        {
            if (postId == 0)
            {
                return BadRequest(new { message = "Post ID is required" });
            }
            var postLikes = await LikesCount(postId);
            var postDislikes = await DislikesCount(postId);
            var postReactionsCount = new PostReactionsCountDTO
            {
                PostId = postId,
                TotalLikes = postLikes,
                TotalDislikes = postDislikes
            };
            return Ok(postReactionsCount);
        }

        //get post dislikes
        [HttpGet("postDislikesCount/{postId}")]
        public async Task<int> DislikesCount( int postId)
        {
            var dislikesCount = await _context.PostReactions
                .Where(r => r.PostId == postId  && r.IsLike == false)
                .CountAsync();
            return dislikesCount;
        }

        //get post likes
        [HttpGet("postLikesCount/{postId}")]
        public async Task<int> LikesCount( int postId)
        {
            var likesCount = await _context.PostReactions
                .Where(r => r.PostId == postId  && r.IsLike == true)
                .CountAsync();
            return likesCount;
        }

        //post a like
        [HttpPost("addPostReaction")]
        public async Task<IActionResult> AddPostReaction(PostReactionsDTO postReaction)
        {
            if (postReaction.PostId == 0 || postReaction.UserId == 0)
            {
                return NotFound(new { message = "ids are reqired" });
            }

            if (!await PostAndUserExists(postReaction.UserId, postReaction.PostId))
            {
                return NotFound(new { message = "Post or User not found" });
            }


            // Check if like already exists
            var exists = await _context.PostReactions
                .AnyAsync(p => p.UserId == postReaction.UserId && p.PostId == postReaction.PostId);


            if (exists)
                return BadRequest(new { message = "Already liked" });

            var reaction = new PostReactions
            {
                UserId = postReaction.UserId,
                PostId = postReaction.PostId,
                IsLike = postReaction.IsLike,
                ReactedAt = DateTime.Now // if you have this field
            };

            _context.PostReactions.Add(reaction);
            await _context.SaveChangesAsync();

            var postLikes = await LikesCount( postReaction.PostId);
            var postDislikes = await DislikesCount(postReaction.PostId);

            return Ok(new { Message = "Post reaction added",
                            PostLikes = postLikes,
                            PostDislikes= postDislikes
                            });
        }

        [HttpPut("updateReaction")]
        public async Task<IActionResult> UpdatePostReaction(PostReactionsDTO postReaction)
        {
            if (postReaction.PostId == 0 || postReaction.UserId == 0)
                return BadRequest(new { message = "IDs are required" });

            if (!await PostAndUserExists(postReaction.UserId, postReaction.PostId))
            {
                return NotFound(new { message = "Post or User not found" });
            }

            // Fetch existing reaction
            var existingReaction = await _context.PostReactions
                .FirstOrDefaultAsync(p => p.UserId == postReaction.UserId && p.PostId == postReaction.PostId);

            if (existingReaction == null)
                return NotFound(new { message = "Reaction not found" });

            // Update fields
            existingReaction.IsLike = postReaction.IsLike;
            existingReaction.UpdatedAt = DateTime.Now;

            _context.PostReactions.Update(existingReaction);
            await _context.SaveChangesAsync();

            var postLikes = await LikesCount(postReaction.PostId);
            var postDislikes = await DislikesCount(postReaction.PostId);

            return Ok(new
            {
                Message = "Reaction updated.",
                PostLikes = postLikes,
                PostDislikes = postDislikes
            });
        }


        // DELETE: api/PostReactions/5
        [HttpDelete("removePostReaction/{userId}/{postId}")]
        public async Task<IActionResult> RemovePostReaction(int userId, int postId)
        {
            if (!await PostAndUserExists(userId, postId))
            {
                return NotFound(new { message = "Post or User not found" });
            }

            var existingReaction = await _context.PostReactions
                             .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);

            if (existingReaction == null)
                return NotFound(new { message = "Reaction not found" });

            _context.PostReactions.Remove(existingReaction);
            await _context.SaveChangesAsync();

            var postLikes = await LikesCount(postId);
            var postDislikes = await DislikesCount(postId);

            return Ok(new
            {
                Message = "Reaction deleted.",
                PostLikes = postLikes,
                PostDislikes = postDislikes
            });
        }

       


        private async Task<bool> PostAndUserExists(int userId, int postId)
        {
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            var postExists = await _context.AddPosts.AnyAsync(p => p.PostId == postId);

            return postExists && userExists;
        }

    }
}
