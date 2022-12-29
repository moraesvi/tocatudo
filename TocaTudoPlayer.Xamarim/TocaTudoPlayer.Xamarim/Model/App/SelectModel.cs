using System;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class SelectModel : NotifyPropertyChanged
    {
        private string _value;
        public SelectModel(int id, string value)
        {
            Id = id;
            _value = value;
        }
        public int Id { get; }
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        public bool Equals(SelectModel other)
        {
            if (other == null) return false;
            return Id == other.Id;
        }
    }
}
