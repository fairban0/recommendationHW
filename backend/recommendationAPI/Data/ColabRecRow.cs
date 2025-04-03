namespace backend.Data
{
    public class ColabRecRow
    {
        public string? ContentId { get; set; }
        public string? IfYouLiked { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }
}

