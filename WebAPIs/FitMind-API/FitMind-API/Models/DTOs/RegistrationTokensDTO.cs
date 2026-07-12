using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.DTOs
{
    public class RegistrationTokensDTO
    {
        public string Email { get; set; } = string.Empty;

        
        public string Token { get; set; } = string.Empty;
        public int? TokenType { get; set; }

       
        public DateTime Expiry { get; set; }
    }
}
