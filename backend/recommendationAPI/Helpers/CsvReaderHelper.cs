using CsvHelper;
using System.Globalization;
using backend.Data;
using System.Formats.Asn1;

namespace backend.Helpers
{
    public static class CsvReaderHelper
    {
        public static List<ColabRecRow> ReadCsv(string filePath)
        {
            var results = new List<ColabRecRow>();

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

                results.Add(new ColabRecRow
                {
                    ContentId = dict["ContentId"]?.ToString(),
                    IfYouLiked = dict["If you liked"]?.ToString(),
                    Recommendations = recommendations
                });
            }

            return results;
        }
    }
}

