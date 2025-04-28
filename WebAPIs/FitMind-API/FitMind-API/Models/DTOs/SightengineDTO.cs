namespace FitMind_API.Models.DTOs
{
    public class SightengineDTO
    {

        public string Status { get; set; }
        public RequestData Request { get; set; }
        public NudityData Nudity { get; set; }
        public MediaData Media { get; set; }
        public OffensiveData Offensive { get; set; }
        public WadData Wad { get; set; }
    }
  

    public class RequestData
    {
        public string Id { get; set; }
        public double Timestamp { get; set; }
        public int Operations { get; set; }
    }

    public class NudityData
    {
        public decimal Raw { get; set; }
        public decimal Safe { get; set; }
        public decimal Partial { get; set; }
    }

    public class MediaData
    {
        public string Id { get; set; }
        public string Uri { get; set; }
    }


    public class WadData
    {
        public decimal Weapon { get; set; }
        public decimal Alcohol { get; set; }
        public decimal Drugs { get; set; }
        public decimal Weapons { get; set; }

    }

    public class OffensiveData
    {
        public decimal Prob { get; set; }
        public string Label { get; set; } // example: "gore", "nudity", "offensive", etc.
    }
}
