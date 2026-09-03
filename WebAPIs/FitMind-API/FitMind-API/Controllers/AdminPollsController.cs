using FitMind_API.Data;
using FitMind_API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Controllers
{
    [Route("api/admin/polls")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminPollsController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AdminPollsController(FMDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPolls([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
        {
            var query = _context.Polls.Include(p => p.Post).Include(p => p.Options).ThenInclude(o => o.Votes).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(p => p.Question.ToLower().Contains(lowerSearch) || p.Post.Title.ToLower().Contains(lowerSearch));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var polls = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.PollId,
                    ParentPostTitle = p.Post.Title,
                    p.Question,
                    TotalVotes = p.Options.Sum(o => o.Votes.Count),
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(new { data = polls, totalItems, totalPages, currentPage = page });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoll(int id)
        {
            var poll = await _context.Polls
                .Include(p => p.Options).ThenInclude(o => o.Votes)
                .Include(p => p.Post)
                .FirstOrDefaultAsync(p => p.PollId == id);

            if (poll == null) return NotFound();

            var totalVotes = poll.Options.Sum(o => o.Votes?.Count ?? 0);

            var result = new
            {
                poll.PollId,
                poll.PostId,
                ParentPostTitle = poll.Post.Title,
                poll.Question,
                TotalVotes = totalVotes,
                poll.CreatedAt,
                Options = poll.Options.Select(o => new
                {
                    o.OptionId,
                    o.OptionText,
                    VoteCount = o.Votes?.Count ?? 0,
                    Percentage = totalVotes > 0 ? (double)(o.Votes?.Count ?? 0) / totalVotes * 100 : 0
                })
            };

            return Ok(result);
        }

        public class UpdatePollDTO
        {
            public string Question { get; set; } = string.Empty;
            public List<UpdatePollOptionDTO> Options { get; set; } = new();
        }

        public class UpdatePollOptionDTO
        {
            public int OptionId { get; set; }
            public string OptionText { get; set; } = string.Empty;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePoll(int id, [FromBody] UpdatePollDTO request)
        {
            var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.PollId == id);
            if (poll == null) return NotFound();

            poll.Question = request.Question;
            foreach (var optDto in request.Options)
            {
                var option = poll.Options.FirstOrDefault(o => o.OptionId == optDto.OptionId);
                if (option != null)
                {
                    option.OptionText = optDto.OptionText;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Poll updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll == null) return NotFound();

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Poll deleted successfully." });
        }
    }
}
