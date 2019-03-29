using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class Ep : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string url { get; set; }
        public int type { get; set; }
        public float sort { get; set; }
        public string name { get; set; }
        public string name_cn { get; set; }
        public string duration { get; set; }
        public string airdate { get; set; }
        public int comment { get; set; }
        public string desc { get; set; }
        private string _status { get; set; }
        public string status
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
    }

    public class Collection
    {
        public int wish { get; set; }
        public int collect { get; set; }
        public int doing { get; set; }
        public int on_hold { get; set; }
        public int dropped { get; set; }
    }

    public class Actor
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public Images images { get; set; }
    }

    public class Crt
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string name_cn { get; set; }
        public string role_name { get; set; }
        public Images images { get; set; }
        public List<Actor> actors { get; set; }
    }

    public class Staff
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string name_cn { get; set; }
        public string role_name { get; set; }
        public Images images { get; set; }
        public List<string> jobs { get; set; }
    }
    public class Count
    {

        [JsonProperty("10")]
        public int _10 { get; set; }

        [JsonProperty("9")]
        public int _9 { get; set; }

        [JsonProperty("8")]
        public int _8 { get; set; }

        [JsonProperty("7")]
        public int _7 { get; set; }

        [JsonProperty("6")]
        public int _6 { get; set; }

        [JsonProperty("5")]
        public int _5 { get; set; }

        [JsonProperty("4")]
        public int _4 { get; set; }

        [JsonProperty("3")]
        public int _3 { get; set; }

        [JsonProperty("2")]
        public int _2 { get; set; }

        [JsonProperty("1")]
        public int _1 { get; set; }
    }

    public class Rating
    {
        public int total { get; set; }
        public Count count { get; set; }
        public double score { get; set; }
    }

    public class Topic
    {
        public int id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public int main_id { get; set; }
        public int timestamp { get; set; }
        public int lastpost { get; set; }
        public int replies { get; set; }
        public User user { get; set; }
    }

    public class Blog
    {
        public int id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string image { get; set; }
        public int replies { get; set; }
        public int timestamp { get; set; }
        public string dateline { get; set; }
        public User user { get; set; }
    }

    public class Images
    {
        public string large { get; set; }
        public string common { get; set; }
        public string medium { get; set; }
        public string small { get; set; }
        public string grid { get; set; }
    }

    public class Subject
    {
        public int id { get; set; }
        public string url { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public string name_cn { get; set; }
        public string summary { get; set; }
        public int eps_count { get; set; }
        public string air_date { get; set; }
        public int air_weekday { get; set; }
        public int rank { get; set; }
        public Rating rating { get; set; }
        public Images images { get; set; }
        public Collection collection { get; set; }
        public List<Ep> eps { get; set; }
        public List<Crt> crt { get; set; }
        public List<Staff> staff { get; set; }
        public List<Topic> topic { get; set; }
        public List<Blog> blog { get; set; }
    }

    public class SearchResult
    {
        public int results { get; set; }
        public List<Subject> list { get; set; }
    }

    public class SList
    {
        public int subject_id { get; set; }
        public Subject subject { get; set; }
    }

    public class Collect
    {
        public SubjectStatus status { get; set; }
        public int count { get; set; }
        public List<SList> list { get; set; }
    }

    public class SubjectCollection
    {
        public int type { get; set; }
        public string name { get; set; }
        public string name_cn { get; set; }
        public List<Collect> collects { get; set; }
    }

}
