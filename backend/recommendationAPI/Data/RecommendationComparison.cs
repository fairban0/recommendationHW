namespace backend.Data
{
    public class RecommendationComparison
    {
        public string ContentId { get; set; } = string.Empty;
        public string IfYouLiked { get; set; } = string.Empty;
        public List<string> CollaborativeRecommendations { get; set; } = new();
        public List<string> ContentRecommendations { get; set; } = new();
    }
}
