using CsvHelper;
using System.Globalization;
using backend.Data;
using System.Formats.Asn1;

namespace backend.Helpers
{
    public static class CsvReaderHelper
    {
        public static List<ContentRecRow> ReadCsv(string filePath)
        {
            var results = new List<ContentRecRow>();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            var records = csv.GetRecords<dynamic>();

            foreach (var record in records)
            {
                var dict = (IDictionary<string, object>)record;

                var recommendations = dict
                    .Where(kvp => kvp.Key.StartsWith("Recommendation"))
                    .Select(kvp => kvp.Value?.ToString())
                    .Where(val => !string.IsNullOrWhiteSpace(val))
                    .ToList();

                results.Add(new ContentRecRow
                {
                    ContentId = dict["ContentId"]?.ToString(),
                    IfYouLiked = dict["If you liked"]?.ToString(),
                    Recommendations = recommendations
                });
            }

            return results;
        }
        public static Dictionary<string, Dictionary<string, double>> ReadSimilarityMatrix(string filePath)
        {
            var result = new Dictionary<string, Dictionary<string, double>>();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            csv.Read();               // Read the first line (header row)
            csv.ReadHeader();         // Tell CsvHelper to treat it as the header
            var header = csv.HeaderRecord!;
            var contentIds = header.Skip(1).ToList(); // Now safe ✅
            while (csv.Read())
            {
                var rowId = csv.GetField(0); // Get row contentId
                var similarities = new Dictionary<string, double>();

                for (int i = 1; i < header.Length; i++)
                {
                    var value = csv.GetField(i);
                    if (double.TryParse(value, out double score))
                    {
                        similarities[contentIds[i - 1]] = score;
                    }
                }

                result[rowId] = similarities;
            }

            return result;
        }

    }
}

