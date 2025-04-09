namespace FitMind_API.Models.DTOs
{
    public class UpdateAppUserDTO
    {
        public int Id { get; set; }

        public required string Username { get; set; }
        public string? UniqueName { get; set; }
        public int? UserVisibility { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }
    }
}
