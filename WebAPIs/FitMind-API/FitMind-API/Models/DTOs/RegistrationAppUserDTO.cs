namespace FitMind_API.Models.DTOs
{
    public class RegistrationAppUserDTO
    {
        public int Id { get; set; }
        public required string  Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
