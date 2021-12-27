using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim.Interface
{
    public interface ISearchPlaylistViewModel : INotifyPropertyChanged
    {
        event Action PlayerReady;
        event Action<Action> ShowInterstitial;
        event Action<int, string> AppErrorEvent;
        string MusicSearchedName { get; set; }
        string AlbumSearchedName { get; set; }
        bool MenuActionsEnabled { get; set; }
        bool SearchAlbumVisible { get; set; }
        bool AlbumPlaylistIsVisible { get; set; }
        bool PlaylistIsVisible { get; set; }
        bool MusicPlaylistIsVisible { get; set; }
        bool IsSearching { get; set; }
        bool SearchSavedForm { get; set; }
        bool PlayerLoaded { get; set; }
        bool IsNotInternetConnection { get; set; }
        bool IsInternetAvaiable { get; set; }
        bool IsWiFiConnection { get; set; }     
        bool MusicDetailsChangeAlbumIsVisible { get; set; }
        bool MusicDetailsAddAlbumIsVisible { get; set; }
        ICommonMusicPlayerViewModel MusicPlayer { get; }
        IAlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel { get; }
        IMusicPlayedHistoryViewModel MusicPlayedHistoryViewModel { get; }
        ICommand SearchAlbumCommand { get; set; }
        ICommand SearchMusicCommand { get; set; }
        ICommand MusicSavedCommand { get; set; }
        ICommand DownloadMusicVisibleCommand { get; set; }
        ICommand DownloadMusicCommand { get; set; }
        Command<UserAlbumPlayedHistory> AlbumHistoryFormCommand { get; }
        AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormCommand { get; }
        Command<HistoryMusicModel> MusicHistoryPlayCommand { get; }
        ObservableCollection<SearchMusicModel> AlbumPlaylist { get; set; }
        ObservableCollection<SearchMusicModel> Playlist { get; set; }        
        ObservableCollection<SearchMusicModel> MusicPlaylist { get; set; }
        ObservableCollection<SearchMusicModel> SavedAlbumPlaylist { get; set; }
        ObservableCollection<SearchMusicModel> SavedMusicPlaylist { get; set; }
        ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection { get; set; }
        UserMusicPlayedHistory LastMusicHistorySelected { get; set; }
        //Task<string[]> SearchTerm(string term);
        //void MusicIconType(SearchMusicModel searchMusic);
        Task AlbumPlaylistSearch();
        Task MusicPlaylistSearch();
        Task AlbumPlaylistSearchFromDb();
        Task MusicPlaylistSearchFromDb();
        void DownloadMusicVisible(SearchMusicModel searchMusic);
        void ClearPlaylistLoaded();
        void ClearSavedAlbumPlaylistLoaded();
        void ClearSavedMusicPlaylistLoaded();
        Task InsertMusicAlbumPlaylistSelected(string albumName, SearchMusicModel music);
        Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel musicModel);
        Task DeleteMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel musicModel);
        Task<bool> ExistsMusicAlbumPlaylist(string albumName, SearchMusicModel music);
        Task LoadMusicAlbumPlaylistSelected();
        void LoadAlbumMusicSavedSelect();
    }
}
