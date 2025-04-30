using Newtonsoft.Json;

namespace FitMind_API.Models.DTOs
{
    public class SightengineTextModerationDTO
    {
        public string Status { get; set; }
        public Profanity Profanity { get; set; }
        public Personal Personal { get; set; }
        public Link Link { get; set; }
    }

    public class Profanity
    {
        public List<Match> Matches { get; set; }
    }

    public class Personal
    {
        public List<Match> Matches { get; set; }
    }

    public class Link
    {
        public List<Match> Matches { get; set; }
    }

    public class Match
    {
        public string Type { get; set; }
        public string Intensity { get; set; }
        public string MatchText { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        [JsonProperty("match")]
        public string MatchRaw { get; set; }
    }
}
