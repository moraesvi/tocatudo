using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using System;
using System.ComponentModel;
using System.Linq;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private const string USER_ALBUM_LOCAL_SEARCH_HISTORY_KEY = "a_history.json";
        private const string USER_MUSIC_LOCAL_SEARCH_HISTORY_KEY = "m_history.json";
        private bool _isInternetAvaiable;
        private bool _isNotInternetConnection;
        private bool _isWiFiConnection;
        private int _isInternetAvaiableGridSize;
        protected readonly WeakEventManager<string> _appErrorEvent;
        protected readonly WeakEventManager _showInterstitial;
        protected readonly WeakEventManager<Action> _actionShowInterstitial;
        protected readonly DelegateWeakEventManager _propertyChangedEventManager;
        public event PropertyChangedEventHandler PropertyChanged 
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }
        public BaseViewModel()
        {
            _appErrorEvent = new WeakEventManager<string>();
            _showInterstitial = new WeakEventManager();
            _actionShowInterstitial = new WeakEventManager<Action>();
            _propertyChangedEventManager = new DelegateWeakEventManager();

            CheckInternetConnection();
            CheckInternetConnEver();
        }
        public event EventHandler<string> AppErrorEvent
        {
            add => _appErrorEvent.AddEventHandler(value);
            remove => _appErrorEvent.RemoveEventHandler(value);
        }
        public event EventHandler ShowInterstitial
        {
            add => _showInterstitial.AddEventHandler(value);
            remove => _showInterstitial.RemoveEventHandler(value);
        }
        public event EventHandler<Action> ActionShowInterstitial
        {
            add => _actionShowInterstitial.AddEventHandler(value);
            remove => _actionShowInterstitial.RemoveEventHandler(value);
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChangedEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(PropertyChanged));
        }
        public bool IsNotInternetConnection
        {
            get { return _isNotInternetConnection; }
            set
            {
                
                _isNotInternetConnection = value;
                OnPropertyChanged(nameof(IsNotInternetConnection));
            }
        }
        public bool IsInternetAvaiable 
        {
            get { return _isInternetAvaiable; }
            set 
            { 
                _isInternetAvaiable = value;

                IsNotInternetConnection = !_isInternetAvaiable;
                IsInternetAvaiableGridSize = _isInternetAvaiable ? 0 : 40;

                OnPropertyChanged(nameof(IsInternetAvaiable));
            }
        }
        public int IsInternetAvaiableGridSize
        {
            get { return _isInternetAvaiableGridSize; }
            set
            {
                _isInternetAvaiableGridSize = value;
                OnPropertyChanged(nameof(IsInternetAvaiableGridSize));
            }
        }
        public bool IsWiFiConnection
        {
            get { return _isWiFiConnection; }
            set 
            { 
                _isWiFiConnection = value;
                OnPropertyChanged(nameof(IsWiFiConnection));
            }
        }
        public string UserAlbumLocalSearchHistoryKey { get => USER_ALBUM_LOCAL_SEARCH_HISTORY_KEY; }
        public string UserMusicLocalSearchHistoryKey { get => USER_MUSIC_LOCAL_SEARCH_HISTORY_KEY; }
        protected void RaiseAppErrorEvent(string msg) 
        {
            _appErrorEvent.RaiseEvent(this, msg, nameof(AppErrorEvent));
        }
        protected void RaiseDefaultAppErrorEvent()
        {
            _appErrorEvent.RaiseEvent(this, AppResource.AppDefaultError, nameof(AppErrorEvent));
        }
        public void RaiseShowInterstitial()
        {
            _showInterstitial.RaiseEvent(this, null, nameof(ShowInterstitial));
        }
        protected void RaiseActionShowInterstitial(Action action)
        {
            _actionShowInterstitial.RaiseEvent(this, action, nameof(ActionShowInterstitial));
        }
        protected void RaiseActionShowInterstitial(Action actionBefore, Action actionAfter)
        {
            actionBefore();
            _actionShowInterstitial.RaiseEvent(this, actionAfter, nameof(ActionShowInterstitial));
        }
        public void CheckInternetConnection() 
        {
            IsInternetAvaiable = CrossConnectivity.Current.IsConnected;
        }
        protected void CheckInternetConnEver()
        {
            CrossConnectivity.Current.ConnectivityChanged += (sender, args) => 
            {
                IsInternetAvaiable = args.IsConnected;
            };
        }
        protected bool CheckIfWiFiConn()
        {
            ConnectionType wifi = ConnectionType.WiFi;
            return CrossConnectivity.Current.ConnectionTypes.Contains(wifi);
        }
    }
}
