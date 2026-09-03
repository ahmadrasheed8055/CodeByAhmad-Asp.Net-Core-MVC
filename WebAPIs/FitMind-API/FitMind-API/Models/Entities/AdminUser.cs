using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class AdminUser
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        [EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [StringLength(100)]
        public string? DisplayName { get; set; }

        public DateTime FirstLoginAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
