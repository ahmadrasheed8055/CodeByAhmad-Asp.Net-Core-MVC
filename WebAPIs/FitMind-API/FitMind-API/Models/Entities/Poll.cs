using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class Poll
    {
        [Key]
        public int PollId { get; set; }

        [Required]
        [ForeignKey("Post")]
        public int PostId { get; set; }
        
        // Navigation to the parent Post
        public virtual AddPost Post { get; set; }

        [Required]
        [StringLength(255)]
        public string Question { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        // Configuration Flags
        public bool AllowUserOptions { get; set; } = false;
        public bool IsMultipleChoice { get; set; } = false;
        public bool AllowVoteEdit { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public bool IsClosed { get; set; } = false;

        // Navigation properties
        public ICollection<PollOption> Options { get; set; }
        public ICollection<PollVote> Votes { get; set; }
    }
}
