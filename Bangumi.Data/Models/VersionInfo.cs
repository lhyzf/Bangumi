using System;
using System.Text.Json;

namespace Bangumi.Data.Models
{
    internal class VersionInfo
    {
        public string Version { get; set; }
        public bool AutoCheck { get; set; } = false;
        public bool AutoUpdate { get; set; } = false;
        public DateTimeOffset LastUpdate { get; set; }
        public int CheckInterval { get; set; } = 7;
        public string[] SitesEnabledOrder { get; set; }

        public string ToJson() =>
            JsonSerializer.Serialize(this);

        public static VersionInfo FromJson(string json) =>
            JsonSerializer.Deserialize<VersionInfo>(json);
    }
}
