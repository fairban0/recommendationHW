using Microsoft.AspNetCore.Mvc;
using backend.Helpers;
using backend.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        [HttpGet("contentIds")]
        public IActionResult GetValidContentIds()
        {
            var collabIds = _collabData.Select(r => r.ContentId).ToHashSet();
            var similarityIds = _similarityMatrix.Keys.ToHashSet();
            var validIds = collabIds.Intersect(similarityIds).ToList();
            return Ok(validIds);
        }

        [HttpGet("{contentId}")]
        public async Task<IActionResult> GetRecommendations(string contentId)
        {
            var collabMap = _collabData.ToDictionary(r => r.ContentId);
            if (!collabMap.ContainsKey(contentId)) return NotFound("ID not in collaborative data");
            if (!_similarityMatrix.ContainsKey(contentId)) return NotFound("ID not in similarity matrix");

            var collabRow = collabMap[contentId];

            var contentRecommendations = _similarityMatrix[contentId]
                .Where(kvp => kvp.Key != contentId && collabMap.ContainsKey(kvp.Key))
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp =>
                {
                    var title = collabMap[kvp.Key].IfYouLiked;
                    var cleanTitle = title.Split("(ID:")[0].Trim();
                    return $"{cleanTitle} (ID: {kvp.Key})";
                })
                .ToList();

            var azureRecommendations = await GetAzureRecommendations(contentId);

            var result = new RecommendationComparison
            {
                ContentId = contentId,
                IfYouLiked = collabRow.IfYouLiked,
                CollaborativeRecommendations = collabRow.Recommendations,
                ContentRecommendations = contentRecommendations,
                AzureRecommendations = azureRecommendations
            };

            return Ok(result);
        }

        // 🔧 Private helper for Azure ML integration — no route attribute here
        private async Task<List<string>> GetAzureRecommendations(string contentId)
        {
            var requestData = new
            {
                Inputs = new
                {
                    WebServiceInput0 = new[]
                    {
                        new
                        {
                            contentId = Convert.ToInt64(contentId),
                            personId = -8854698371283742000,
                            rating = 1
                        }
                    }
                }
            };

            const string apiKey = "bCtQXvxBBKJYJ6HatTvgrjER7iJeL4BV";
            var endpoint = "http://16ce6c74-f99d-4807-b722-015be68d5a86.eastus2.azurecontainer.io/score";

            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var json = System.Text.Json.JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(endpoint, content);
                var resultString = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"\n🟡 Azure ML Response Status Code: {response.StatusCode}");
                Console.WriteLine("🟡 Full Raw Response:");
                Console.WriteLine(resultString);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("🔴 Azure ML request failed!");
                    return new List<string> { "Azure model unavailable" };
                }

                var result = System.Text.Json.JsonDocument.Parse(resultString);

                if (result.RootElement.TryGetProperty("Results", out var results) &&
                    results.TryGetProperty("WebServiceOutput0", out var outputArray))
                {
                    var recommendationRow = outputArray.EnumerateArray().FirstOrDefault();

                    if (recommendationRow.ValueKind != System.Text.Json.JsonValueKind.Object)
                        return new List<string> { "No recommendation object found" };

                    var azureRecs = new List<string>();

                    foreach (var property in recommendationRow.EnumerateObject())
                    {
                        if (property.Name.StartsWith("Recommended Item"))
                        {
                            azureRecs.Add(property.Value.ToString());
                        }
                    }

                    return azureRecs.Count > 0 ? azureRecs : new List<string> { "No items returned" };
                }

                return new List<string> { "Unexpected Azure response format" };
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔴 Azure ML call failed:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                return new List<string> { "Azure model unavailable" };
            }
        }
    }
}
