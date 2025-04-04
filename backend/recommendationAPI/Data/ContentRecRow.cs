namespace backend.Data
{
    public class ContentRecRow
    {
        public string? ContentId { get; set; } = string.Empty;
        public string? IfYouLiked { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }
}