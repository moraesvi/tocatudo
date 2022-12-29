using System;
using System.ComponentModel;

namespace TocaTudoPlayer.Xamarim
{
    public interface IBaseViewModel
    {
        event EventHandler<string> AppErrorEvent;
        event EventHandler ShowInterstitial;
        event EventHandler<Action> ActionShowInterstitial;
        bool IsNotInternetConnection { get; set; }
        bool IsInternetAvaiable { get; set; }
        int IsInternetAvaiableGridSize { get; set; }
        bool IsWiFiConnection { get; set; }
        string UserAlbumLocalSearchHistoryKey { get; }
        string UserMusicLocalSearchHistoryKey { get; }
    }
}
