using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    public class Calendar
    {
        [JsonPropertyName("weekday")]
        public Weekday Weekday { get; set; }

        [JsonPropertyName("items")]
        public List<SubjectForCalendar> Items { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Calendar b = (Calendar)obj;
            return Weekday.EqualsExT(b.Weekday) &&
                   Items.SequenceEqualExT(b.Items);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Items.Count;
        }
    }
}
