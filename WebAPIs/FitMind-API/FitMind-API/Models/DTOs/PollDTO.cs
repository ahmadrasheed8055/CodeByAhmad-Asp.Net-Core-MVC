using System;
using System.Collections.Generic;

namespace FitMind_API.Models.DTOs
{
    public class CreatePollDTO
    {
        public string Title { get; set; }
        public int CategoryId { get; set; }
        public List<string> Options { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int UserId { get; set; }

        public bool AllowUserOptions { get; set; }
        public bool IsMultipleChoice { get; set; }
        public bool AllowVoteEdit { get; set; }
    }

    public class VotePollDTO
    {
        public int PollId { get; set; }
        public List<int> OptionIds { get; set; } = new List<int>();
        public int UserId { get; set; }
    }

    public class PollOptionResultDTO
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public string OptionLetter { get; set; }
        public int VoteCount { get; set; }
        public double VotePercentage { get; set; }
    }

    public class PollDTO
    {
        public int PollId { get; set; }
        public int PostId { get; set; }
        public string Question { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsExpired { get; set; }
        public int TotalVotes { get; set; }
        public List<int>? UserVotedOptionIds { get; set; }
        
        public bool AllowUserOptions { get; set; }
        public bool IsMultipleChoice { get; set; }
        public bool AllowVoteEdit { get; set; }
        public bool IsPinned { get; set; }
        public bool IsClosed { get; set; }
        
        public List<PollOptionResultDTO> Options { get; set; }
    }
}
