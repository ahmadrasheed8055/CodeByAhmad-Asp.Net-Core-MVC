using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.Entities
{
    public class AppUsers
    {
        [Key]
        public int Id { get; set; }

        [ StringLength(50)]
        public required string Username { get; set; }

        [EmailAddress, StringLength(100)]
        public required string Email { get; set; }

        //[Required]
        public required string PasswordHash { get; set; }  // Store Hashed Password

        public bool EmailConfirmed { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int Status { get; set; } = 1;

        // Navigation Property for Tokens
        public ICollection<UserRT>? UserTokens { get; set; } = new List<UserRT>();
    }
}
