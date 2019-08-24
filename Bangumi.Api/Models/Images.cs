using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Images
    {
        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("common")]
        public string Common { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("grid")]
        public string Grid { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Images i = (Images)obj;
            return (Large == null ? Large == i.Large : Large.Equals(i.Large)) &&
                   (Common == null ? Common == i.Common : Common.Equals(i.Common)) &&
                   (Medium == null ? Medium == i.Medium : Medium.Equals(i.Medium)) &&
                   (Small == null ? Small == i.Small : Small.Equals(i.Small)) &&
                   (Grid == null ? Grid == i.Grid : Grid.Equals(i.Grid));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Common.Length;
        }
    }
}
