using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using System;
using System.ComponentModel;
using System.Linq;

namespace TocaTudoPlayer.Xamarim
{
    public class BaseViewModel : IBaseViewModel
    {
        private const string USER_ALBUM_LOCAL_SEARCH_HISTORY_KEY = "a_history.json";
        private const string USER_MUSIC_LOCAL_SEARCH_HISTORY_KEY = "m_history.json";
        private bool _isInternetAvaiable;
        private bool _isNotInternetConnection;
        private bool _isWiFiConnection;
        public event PropertyChangedEventHandler PropertyChanged;
        private event Action<int, string> _appErrorEvent;
        public BaseViewModel()
        {
            CheckIfExistsInternetConn();
            CheckInternetConnEver();
        }
        public event Action<int, string> AppErrorEvent
        {
            add => _appErrorEvent += value;
            remove => _appErrorEvent -= value;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                OnPropertyChanged(nameof(IsInternetAvaiable));
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
        protected void RaiseAppErrorEvent(int level, string msg) 
        {
            _appErrorEvent(level, msg);
        }
        protected void CheckIfExistsInternetConn() 
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
