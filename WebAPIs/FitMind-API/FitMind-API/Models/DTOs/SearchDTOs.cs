using System.Collections.Generic;
using FitMind_API.Models.Entities;

namespace FitMind_API.Models.DTOs
{
    public class SearchGroup<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }

    public class SearchUserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UniqueName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePhoto { get; set; }
    }

    public class SearchResultDTO
    {
        public SearchGroup<SearchUserDTO>? Users { get; set; }
        public SearchGroup<GetAllPostsDTO>? Posts { get; set; }
        public SearchGroup<Categories>? Categories { get; set; }
        public SearchGroup<PollDTO>? Polls { get; set; }
        public string? DetectedIntent { get; set; }
        public string? IntentDisplayName { get; set; }
        public double? ConfidenceScore { get; set; }
    }
}
