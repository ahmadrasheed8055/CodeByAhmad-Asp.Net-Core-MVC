using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMind_API.Data;
using FitMind_API.Models.Entities;
using FitMind_API.Models.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public PollsController(FMDBContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<ActionResult<PollDTO>> CreatePoll([FromBody] CreatePollDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.Options == null || dto.Options.Count < 2 || dto.Options.Count > 10)
            {
                return BadRequest(new { message = "Valid title and between 2 to 10 options are required." });
            }

            var user = await _context.AppUsers.FindAsync(dto.UserId);
            if (user == null) return NotFound("User not found");

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return NotFound("Category not found");

            var post = new AddPost
            {
                Title = dto.Title,
                Description = "Poll",
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                PublishAt = DateTime.UtcNow,
                IsPublished = true,
                IsDeleted = false,
                ViewCount = 0
            };

            _context.AddPosts.Add(post);
            await _context.SaveChangesAsync();

            var poll = new Poll
            {
                PostId = post.PostId,
                Question = dto.Title,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                AllowUserOptions = dto.AllowUserOptions,
                IsMultipleChoice = dto.IsMultipleChoice,
                AllowVoteEdit = dto.AllowVoteEdit,
                ShowResultsBeforeVoting = dto.ShowResultsBeforeVoting,
                IsPinned = false,
                IsClosed = false
            };
            
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            char letter = 'A';
            foreach (var optText in dto.Options)
            {
                if (!string.IsNullOrWhiteSpace(optText))
                {
                    _context.PollOptions.Add(new PollOption
                    {
                        PollId = poll.PollId,
                        OptionText = optText.Trim(),
                        OptionLetter = letter.ToString()
                    });
                    letter++;
                }
            }
            await _context.SaveChangesAsync();

            // Broadcast "New Poll" notification to all other users
            var allOtherUserIds = await _context.AppUsers
                .Where(u => u.Id != dto.UserId)
                .Select(u => u.Id)
                .ToListAsync();

            var notifications = allOtherUserIds.Select(id => new AppNotification
            {
                TargetUserId = id,
                ActorName = user.Username ?? "Member",
                ActorImage = user.ProfilePhoto != null ? "data:image/jpeg;base64," + Convert.ToBase64String(user.ProfilePhoto) : null,
                NotificationType = "poll",
                Message = $"{user.Username ?? "Member"} published a new poll: \"{dto.Title}\"",
                TargetId = post.PostId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            _context.AppNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Poll created successfully", pollId = poll.PollId, postId = post.PostId });
        }

        [HttpPost("vote")]
        public async Task<ActionResult<PollDTO>> VotePoll([FromBody] VotePollDTO dto)
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .ThenInclude(o => o.Votes)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.PollId == dto.PollId);

            if (poll == null) return NotFound(new { message = "Poll not found" });

            if (poll.IsClosed) return BadRequest(new { message = "Poll is closed for voting." });

            if (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Poll has expired." });
            }

            if (dto.OptionIds == null || !dto.OptionIds.Any())
            {
                return BadRequest(new { message = "At least one option must be selected." });
            }

            if (poll.IsMultipleChoice)
            {
                if (dto.OptionIds.Count > 2)
                    return BadRequest(new { message = "You can only select up to 2 options." });
            }
            else
            {
                if (dto.OptionIds.Count > 1)
                    return BadRequest(new { message = "You can only select 1 option for this poll." });
            }

            // Verify all selected options exist in this poll
            foreach (var optId in dto.OptionIds)
            {
                if (!poll.Options.Any(o => o.OptionId == optId))
                {
                    return BadRequest(new { message = $"Option {optId} not found in this poll." });
                }
            }

            var existingVotes = await _context.PollVotes
                .Where(v => v.PollId == dto.PollId && v.UserId == dto.UserId)
                .ToListAsync();
            
            if (existingVotes.Any())
            {
                if (poll.AllowVoteEdit)
                {
                    _context.PollVotes.RemoveRange(existingVotes);
                }
                else
                {
                    return BadRequest(new { message = "You have already voted on this poll and editing is disabled." });
                }
            }

            foreach (var optId in dto.OptionIds)
            {
                _context.PollVotes.Add(new PollVote
                {
                    PollId = dto.PollId,
                    OptionId = optId,
                    UserId = dto.UserId,
                    VotedAt = DateTime.UtcNow
                });
            }
            
            await _context.SaveChangesAsync();

            // Notify poll author about the vote
            var post = await _context.AddPosts.FindAsync(poll.PostId);
            if (post != null && post.UserId != dto.UserId)
            {
                var actor = await _context.AppUsers.FindAsync(dto.UserId);
                _context.AppNotifications.Add(new AppNotification
                {
                    TargetUserId = post.UserId,
                    ActorName = actor?.Username ?? "Member",
                    ActorImage = actor?.ProfilePhoto != null ? "data:image/jpeg;base64," + Convert.ToBase64String(actor.ProfilePhoto) : null,
                    NotificationType = "reaction", // or "poll_vote" but frontend uses reaction for icons
                    Message = $"{actor?.Username ?? "A member"} voted on your poll \"{poll.Question}\"",
                    TargetId = poll.PostId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // Refetch
            var allPollVotes = await _context.PollVotes.Where(v => v.PollId == dto.PollId).ToListAsync();
            var totalVotes = allPollVotes.Count;

            var resultOptions = poll.Options.Select(o => {
                var optionVotesCount = allPollVotes.Count(v => v.OptionId == o.OptionId);
                return new PollOptionResultDTO
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText,
                    OptionLetter = o.OptionLetter,
                    VoteCount = optionVotesCount,
                    VotePercentage = totalVotes == 0 ? 0 : (optionVotesCount * 100.0 / totalVotes)
                };
            }).ToList();

            var result = new PollDTO
            {
                PollId = poll.PollId,
                PostId = poll.PostId,
                Question = poll.Question,
                ExpiresAt = poll.ExpiresAt,
                IsExpired = poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow,
                TotalVotes = totalVotes,
                AllowUserOptions = poll.AllowUserOptions,
                IsMultipleChoice = poll.IsMultipleChoice,
                AllowVoteEdit = poll.AllowVoteEdit,
                ShowResultsBeforeVoting = poll.ShowResultsBeforeVoting,
                IsPinned = poll.IsPinned,
                IsClosed = poll.IsClosed,
                UserVotedOptionIds = dto.OptionIds,
                Options = resultOptions
            };

            return Ok(result);
        }

        public class CustomOptionDTO
        {
            public int PollId { get; set; }
            public int UserId { get; set; }
            public string OptionText { get; set; }
        }

        [HttpPost("add-option")]
        public async Task<ActionResult> AddCustomOption([FromBody] CustomOptionDTO dto)
        {
            var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.PollId == dto.PollId);
            if (poll == null) return NotFound(new { message = "Poll not found." });

            if (!poll.AllowUserOptions) return BadRequest(new { message = "This poll does not allow custom options." });
            if (poll.IsClosed || (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow))
            {
                return BadRequest(new { message = "Poll is closed or expired." });
            }
            if (string.IsNullOrWhiteSpace(dto.OptionText)) return BadRequest(new { message = "Option text cannot be empty." });

            if (poll.Options.Count >= 20) return BadRequest(new { message = "Maximum options reached." });

            char nextLetter = (char)('A' + poll.Options.Count);
            
            var newOpt = new PollOption
            {
                PollId = dto.PollId,
                OptionText = dto.OptionText.Trim(),
                OptionLetter = nextLetter > 'Z' ? "?" : nextLetter.ToString()
            };

            _context.PollOptions.Add(newOpt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Option added successfully", optionId = newOpt.OptionId });
        }

        [HttpPost("toggle-pin/{pollId}")]
        public async Task<ActionResult> TogglePin(int pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return NotFound(new { message = "Poll not found." });

            poll.IsPinned = !poll.IsPinned;
            await _context.SaveChangesAsync();

            return Ok(new { message = poll.IsPinned ? "Poll pinned" : "Poll unpinned" });
        }

        [HttpPost("toggle-close/{pollId}")]
        public async Task<ActionResult> ToggleClose(int pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return NotFound(new { message = "Poll not found." });

            poll.IsClosed = !poll.IsClosed;
            await _context.SaveChangesAsync();

            return Ok(new { message = poll.IsClosed ? "Poll closed" : "Poll reopened" });
        }

        [HttpDelete("{pollId}")]
        public async Task<ActionResult> DeletePoll(int pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return NotFound(new { message = "Poll not found." });

            var post = await _context.AddPosts.FindAsync(poll.PostId);
            if (post != null)
            {
                _context.AddPosts.Remove(post); // Deleting the post triggers cascade delete for poll, options, votes
            }
            else
            {
                _context.Polls.Remove(poll);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Poll deleted successfully." });
        }
    }
}
