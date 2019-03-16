using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class Ep: INotifyPropertyChanged
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
        private void OnPropertyChanged([CallerMemberName]string propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Collection
    {
        public int doing { get; set; }
    }

    public class Rating
    {
        public int total { get; set; }
        public double score { get; set; }
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
