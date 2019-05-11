using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class WatchingStatus : INotifyPropertyChanged
    {
        public string name { get; set; }
        public string name_cn { get; set; }
        public int subject_id { get; set; }
        private string _watched_eps { get; set; }
        private string _eps_count { get; set; }
        public int lasttouch { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public List<SimpleEp> eps { get; set; }
        private string _ep_color { get; set; }

        public string ep_color
        {
            get { return _ep_color; }
            set
            {
                _ep_color = value;
                OnPropertyChanged();
            }
        }
        public string watched_eps
        {
            get { return _watched_eps; }
            set
            {
                _watched_eps = value;
                OnPropertyChanged();
            }
        }
        public string eps_count
        {
            get { return _eps_count; }
            set
            {
                _eps_count = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SimpleEp
    {
        public int id { get; set; }
        public int type { get; set; }
        public string sort { get; set; }
        public string status { get; set; }

    }
}
