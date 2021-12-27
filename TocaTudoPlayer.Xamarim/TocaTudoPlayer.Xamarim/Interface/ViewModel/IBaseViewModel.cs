using System;
using System.ComponentModel;

namespace TocaTudoPlayer.Xamarim
{
    public interface IBaseViewModel : INotifyPropertyChanged
    {
        event Action<int, string> AppErrorEvent;
        bool IsNotInternetConnection { get; set; }
        bool IsInternetAvaiable { get; set; }
        bool IsWiFiConnection { get; set; }
        string UserAlbumLocalSearchHistoryKey { get; }
        string UserMusicLocalSearchHistoryKey { get; }
    }
}
