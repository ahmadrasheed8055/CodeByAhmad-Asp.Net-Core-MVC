using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class AppNotification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int TargetUserId { get; set; } // The user receiving the notification
        public virtual AppUsers? User { get; set; }

        public string? ActorName { get; set; } // e.g. "Ahmad Rasheed"
        public string? ActorImage { get; set; } // Base64 or URL

        [Required]
        [MaxLength(255)]
        public string NotificationType { get; set; } // 'post', 'poll', 'comment', 'reaction'

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } // "Ahmad Rasheed liked your post '...'"

        public int? TargetId { get; set; } // PostId or PollId

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public bool IsFollowingActor { get; set; } = false;
    }
}
