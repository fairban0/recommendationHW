using Microsoft.AspNetCore.Mvc;
using backend.Helpers;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollaborativeFilteringController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetRecommendations()
        {
            var filePath = "recommendations_output.csv";
            var results = CsvReaderHelper.ReadCsv(filePath);
            return Ok(results);
        }
    }
}


