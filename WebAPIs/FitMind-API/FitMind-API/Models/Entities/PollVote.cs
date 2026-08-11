using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class PollVote
    {
        [Key]
        public int VoteId { get; set; }

        [Required]
        [ForeignKey("Poll")]
        public int PollId { get; set; }
        public virtual Poll Poll { get; set; }

        [Required]
        [ForeignKey("Option")]
        public int OptionId { get; set; }
        public virtual PollOption Option { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual AppUsers User { get; set; }

        public DateTime VotedAt { get; set; }
    }
}
