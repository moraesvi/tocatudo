using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAlbumPlayerViewModel : INotifyPropertyChanged
    {
        event Action<Action> ShowInterstitial;
        AlbumModel Album { get; set; }
        ICommonPageViewModel CommonPageViewModel { get; }
        ICommonMusicPlayerViewModel MusicPlayerViewModel { get; }
        IMusicBottomAlbumPlayerViewModel BottomPlayerViewModel { get; }
        IAlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel { get; }
        string MusicSelectedName { get; set; }
        bool ViewModelLoadded { get; set; }
        bool PlayerLoaded { get; set; }
        bool ShowHideDownloadMusicOptions { get; set; }
        bool ShowDownloadingInfo { get; set; }
        bool ShowPlayingOfflineInfo { get; set; }
        bool IsSearching { get; set; }
        bool IsActionsEnabled { get; set; }
        ICommand PlayCommand { get; set; }
        ICommand PauseCommand { get; set; }
        ICommand ShowHideDownloadIconCommand { get; set; }
        ICommand DownloadMusicOptionsCommand { get; set; }
        ICommand StartDownloadMusicCommand { get; set; }
        string MusicSearchedName { get; set; }
        float MusicStreamProgress { get; set; }
        string ImgStartDownloadIcon { get; set; }
        bool ShowDownloadMusicStatusProgress { get; set; }
        HttpDownload Download { get; set; }
        void DbAccessEnabled(bool enabled);
        Task GetAlbum(AlbumParseType tpParse, string videoId);
        Task PlayMusic(PlaylistItem playlistItem);
        void ShowHideDownloadIcon();
        Task StartDownloadMusic(AlbumModel album);
        event Action<int, string> AppErrorEvent;
        bool IsNotInternetConnection { get; set; }
        bool IsInternetAvaiable { get; set; }
        bool IsWiFiConnection { get; set; }
    }
}
