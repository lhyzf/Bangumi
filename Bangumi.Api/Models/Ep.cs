using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Ep : INotifyPropertyChanged
    {
        public Ep()
        {
            Url = string.Empty;
            Name = string.Empty;
            NameCn = string.Empty;
            Duration = string.Empty;
            AirDate = string.Empty;
            Desc = string.Empty;
            Status = string.Empty;
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("sort")]
        public double Sort { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("airdate")]
        public string AirDate { get; set; }

        [JsonProperty("comment")]
        public int Comment { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        private string _status { get; set; }

        [JsonProperty("status")]
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Ep e = (Ep)obj;
            return Id == e.Id &&
                   Type == e.Type &&
                   Sort == e.Sort &&
                   Comment == e.Comment &&
                   Url.Equals(e.Url) &&
                   Name.Equals(e.Name) &&
                   NameCn.Equals(e.NameCn) &&
                   Duration.Equals(e.Duration) &&
                   AirDate.Equals(e.AirDate) &&
                   Desc.Equals(e.Desc) &&
                   Status.Equals(e.Status);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
