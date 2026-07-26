using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class PollOption
    {
        [Key]
        public int OptionId { get; set; }

        [Required]
        [ForeignKey("Poll")]
        public int PollId { get; set; }

        public virtual Poll Poll { get; set; }

        [Required]
        [StringLength(255)]
        public string OptionText { get; set; }

        [Required]
        [StringLength(10)]
        public string OptionLetter { get; set; } // e.g., A, B, C...

        public ICollection<PollVote> Votes { get; set; }
    }
}
