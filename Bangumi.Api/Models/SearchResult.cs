using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    public class SearchResult
    {
        [JsonPropertyName("results")]
        public int ResultCount { get; set; }

        [JsonPropertyName("list")]
        public List<SubjectForSearch> Results { get; set; }
    }
}
