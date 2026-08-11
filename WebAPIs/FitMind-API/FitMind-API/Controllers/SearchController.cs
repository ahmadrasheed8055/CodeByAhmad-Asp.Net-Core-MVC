using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMind_API.Data;
using FitMind_API.Models.DTOs;
using FitMind_API.Models.Entities;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly FMDBContext _context;

        public SearchController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<SearchResultDTO>> GlobalSearch(
            [FromQuery] string q, 
            [FromQuery] string? type = null, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 5)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { message = "Search query cannot be empty." });
            }

            q = q.Trim();
            if (q.Length < 2)
            {
                return BadRequest(new { message = "Search query must be at least 2 characters long." });
            }
            if (q.Length > 100)
            {
                return BadRequest(new { message = "Search query is too long." });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;

            var result = new SearchResultDTO();
            int userId = 0;
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimsUserId))
            {
                int.TryParse(claimsUserId, out userId);
            }

            var typeLower = type?.ToLowerInvariant();
            bool searchAll = string.IsNullOrEmpty(typeLower) || typeLower == "all";

            if (searchAll || typeLower == "users")
            {
                var query = _context.AppUsers
                    .Where(u => !u.IsDeleted && (
                        EF.Functions.Like(u.Username, $"%{q}%") || 
                        EF.Functions.Like(u.UniqueName, $"%{q}%") || 
                        EF.Functions.Like(u.Email, $"%{q}%") ||
                        EF.Functions.Like(u.Bio, $"%{q}%"))); // Added Bio based on requirements

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(u => u.JoinedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new SearchUserDTO
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        UniqueName = u.UniqueName,
                        Bio = u.Bio,
                        ProfilePhoto = u.ProfilePhoto != null ? Convert.ToBase64String(u.ProfilePhoto) : null
                    })
                    .ToListAsync();
                
                result.Users = new SearchGroup<SearchUserDTO> { Items = items, TotalCount = totalCount };
            }

            if (searchAll || typeLower == "posts")
            {
                var query = _context.AddPosts
                    .Where(p => !p.IsDeleted && p.IsPublished && p.PublishAt != null && 
                                !_context.HiddenPosts.Any(hp => hp.PostId == p.PostId) &&
                                (EF.Functions.Like(p.Title, $"%{q}%") || 
                                 EF.Functions.Like(p.Description, $"%{q}%") || 
                                 EF.Functions.Like(p.User.Username, $"%{q}%")));

                var totalCount = await query.CountAsync();
                var items = await query
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.postReactions)
                    .Include(p => p.Poll).ThenInclude(p => p.Options).ThenInclude(o => o.Votes)
                    .Include(p => p.Poll).ThenInclude(p => p.Votes)
                    .OrderByDescending(p => p.PublishAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(post => new GetAllPostsDTO
                    {
                        PostId = post.PostId,
                        Title = post.Title,
                        Description = post.Description,
                        CreatedAt = post.CreatedAt,
                        UpdatedAt = post.UpdatedAt,
                        PublishAt = post.PublishAt,
                        IsPublished = post.IsPublished,
                        UserId = post.UserId,
                        UserName = post.User.Username,
                        UserImage = post.User.ProfilePhoto != null ? Convert.ToBase64String(post.User.ProfilePhoto) : null,
                        CategoryId = post.CategoryId,
                        CategoryName = post.Category.Name,
                        PostImage = post.PostImage != null ? Convert.ToBase64String(post.PostImage) : null,
                        ViewCount = post.ViewCount,
                        LikeCount = post.postReactions.Count(r => r.IsLike == true) == 0 ? (int?)null : post.postReactions.Count(r => r.IsLike == true),
                        DislikeCount = post.postReactions.Count(r => r.IsLike == false) == 0 ? (int?)null : post.postReactions.Count(r => r.IsLike == false),
                        IsReactedByMe = userId != 0 ? post.postReactions.Where(r => r.UserId == userId).Select(r => (bool?)r.IsLike).FirstOrDefault() : (bool?)null,
                        IsSavedByMe = userId != 0 ? _context.SavedPosts.Any(sp => sp.UserId == userId && sp.PostId == post.PostId) : (bool?)null,
                        IsHidden = false,
                        IsFollowingAuthor = userId != 0 ? _context.UserFollowers.Any(f => f.FollowerId == userId && f.FollowingId == post.UserId) : (bool?)null,
                        Poll = post.Poll == null ? null : new PollDTO
                        {
                            PollId = post.Poll.PollId,
                            PostId = post.Poll.PostId,
                            Question = post.Poll.Question,
                            ExpiresAt = post.Poll.ExpiresAt,
                            IsExpired = post.Poll.ExpiresAt.HasValue && post.Poll.ExpiresAt.Value < DateTime.UtcNow,
                            TotalVotes = post.Poll.Votes.Count,
                            AllowUserOptions = post.Poll.AllowUserOptions,
                            IsMultipleChoice = post.Poll.IsMultipleChoice,
                            AllowVoteEdit = post.Poll.AllowVoteEdit,
                            ShowResultsBeforeVoting = post.Poll.ShowResultsBeforeVoting,
                            IsPinned = post.Poll.IsPinned,
                            IsClosed = post.Poll.IsClosed,
                            Options = post.Poll.Options.Select(o => new PollOptionResultDTO
                            {
                                OptionId = o.OptionId,
                                OptionText = o.OptionText,
                                VoteCount = o.Votes.Count,
                                VotePercentage = post.Poll.Votes.Count > 0 ? (double)o.Votes.Count / post.Poll.Votes.Count * 100 : 0
                            }).ToList()
                        }
                    })
                    .ToListAsync();
                
                result.Posts = new SearchGroup<GetAllPostsDTO> { Items = items, TotalCount = totalCount };
            }

            if (searchAll || typeLower == "categories")
            {
                var query = _context.Categories
                    .Where(c => c.IsActive && (EF.Functions.Like(c.Name, $"%{q}%") || EF.Functions.Like(c.Description, $"%{q}%")));

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                result.Categories = new SearchGroup<Categories> { Items = items, TotalCount = totalCount };
            }

            if (searchAll || typeLower == "polls")
            {
                var query = _context.Polls
                    .Include(p => p.Post)
                    .Where(p => !p.Post.IsDeleted && p.Post.IsPublished && EF.Functions.Like(p.Question, $"%{q}%"));

                var totalCount = await query.CountAsync();
                var items = await query
                    .Include(p => p.Options).ThenInclude(o => o.Votes)
                    .Include(p => p.Votes)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PollDTO
                    {
                        PollId = p.PollId,
                        PostId = p.PostId,
                        Question = p.Question,
                        ExpiresAt = p.ExpiresAt,
                        IsExpired = p.ExpiresAt.HasValue && p.ExpiresAt.Value < DateTime.UtcNow,
                        TotalVotes = p.Votes.Count,
                        AllowUserOptions = p.AllowUserOptions,
                        IsMultipleChoice = p.IsMultipleChoice,
                        AllowVoteEdit = p.AllowVoteEdit,
                        ShowResultsBeforeVoting = p.ShowResultsBeforeVoting,
                        IsPinned = p.IsPinned,
                        IsClosed = p.IsClosed,
                        Options = p.Options.Select(o => new PollOptionResultDTO
                        {
                            OptionId = o.OptionId,
                            OptionText = o.OptionText,
                            VoteCount = o.Votes.Count,
                            VotePercentage = p.Votes.Count > 0 ? (double)o.Votes.Count / p.Votes.Count * 100 : 0
                        }).ToList()
                    })
                    .ToListAsync();

                result.Polls = new SearchGroup<PollDTO> { Items = items, TotalCount = totalCount };
            }

            return Ok(result);
        }
    }
}
