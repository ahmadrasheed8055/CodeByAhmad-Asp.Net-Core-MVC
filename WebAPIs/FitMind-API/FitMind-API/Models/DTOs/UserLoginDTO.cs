using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.DTOs
{
    public class UserLoginDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }

    }
}
