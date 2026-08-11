using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class UserFollower
    {
        [Required]
        public int FollowerId { get; set; }
        
        [ForeignKey("FollowerId")]
        public virtual AppUsers? Follower { get; set; }

        [Required]
        public int FollowingId { get; set; }

        [ForeignKey("FollowingId")]
        public virtual AppUsers? Following { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
