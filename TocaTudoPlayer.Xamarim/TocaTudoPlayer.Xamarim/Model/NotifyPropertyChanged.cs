using System.ComponentModel;
using Xamarin.CommunityToolkit.Helpers;

namespace TocaTudoPlayer.Xamarim
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        protected readonly DelegateWeakEventManager _propertyChangedEventManager;
        public NotifyPropertyChanged()
        {
            _propertyChangedEventManager = new DelegateWeakEventManager();
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }
        public void OnPropertyChanged(string propertyName)
        {
            _propertyChangedEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(PropertyChanged));
        }
    }
}
