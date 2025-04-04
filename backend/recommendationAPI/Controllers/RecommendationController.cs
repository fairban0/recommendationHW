using Microsoft.AspNetCore.Mvc;
using backend.Helpers;
using backend.Data;
using CsvHelper.Configuration;
using System.Globalization;
using System.Linq;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly List<ContentRecRow> _collabData;
        private readonly Dictionary<string, Dictionary<string, double>> _similarityMatrix;

        public RecommendationController()
        {
            _collabData = CsvReaderHelper.ReadCsv("collabrecommendations_output.csv");
            _similarityMatrix = CsvReaderHelper.ReadSimilarityMatrix("similarity_matrix.csv");
        }

        // GET: api/Recommendation/contentIds
        [HttpGet("contentIds")]
        public IActionResult GetValidContentIds()
        {
            var collabIds = _collabData.Select(r => r.ContentId).ToHashSet();
            var similarityIds = _similarityMatrix.Keys.ToHashSet();
            var validIds = collabIds.Intersect(similarityIds).ToList();
            return Ok(validIds);
        }

        // GET: api/Recommendation/{contentId}
        [HttpGet("{contentId}")]
        public IActionResult GetRecommendations(string contentId)
        {
            var collabMap = _collabData.ToDictionary(r => r.ContentId);
            if (!collabMap.ContainsKey(contentId)) return NotFound("ID not in collaborative data");
            if (!_similarityMatrix.ContainsKey(contentId)) return NotFound("ID not in similarity matrix");

            var collabRow = collabMap[contentId];

            // Top 5 most similar (excluding self) with valid titles
            var contentRecommendations = _similarityMatrix[contentId]
                .Where(kvp => kvp.Key != contentId && collabMap.ContainsKey(kvp.Key))
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp =>
                {
                    var title = collabMap[kvp.Key].IfYouLiked;
                    var cleanTitle = title.Split("(ID:")[0].Trim(); // 👈 removes old ID part if present
                    return $"{cleanTitle} (ID: {kvp.Key})";
                })

                .ToList();

            var result = new RecommendationComparison
            {
                ContentId = contentId,
                IfYouLiked = collabRow.IfYouLiked,
                CollaborativeRecommendations = collabRow.Recommendations,
                ContentRecommendations = contentRecommendations
            };

            return Ok(result);
        }
    }
}