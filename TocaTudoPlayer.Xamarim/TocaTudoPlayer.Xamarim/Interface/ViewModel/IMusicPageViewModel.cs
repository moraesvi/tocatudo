using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicPageViewModel : IMusicAlbumPageBaseViewModel
    {
        string MusicSearchedName { get; set; }
        ICommonMusicPageViewModel CommonMusicPageViewModel { get; }
        ICommonMusicPlayerViewModel MusicPlayerViewModel { get; }
        IMusicPlayedHistoryViewModel MusicPlayedHistoryViewModel { get; }
        ICommonPageViewModel CommonPageViewModel { get; }
        ObservableCollection<SearchMusicModel> MusicPlaylist { get; set; }
        ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection { get; set; }
        UserMusicPlayedHistory LastMusicHistorySelected { get; set; }
        ICommand SearchMusicCommand { get; set; }
        ICommand DownloadMusicCommand { get; set; }
        AsyncCommand<SearchMusicModel> SelectMusicCommand { get; }
        AsyncCommand<SearchMusicModel> StartDownloadMusicCommand { get; }
        AsyncCommand<SelectModel> MusicHistoryAlbumSelectedCommand { get; }
        AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormCommand { get; }
        AsyncCommand<HistoryMusicModel> MusicHistoryFormDownloadStartCommand { get; }
        Command<HistoryMusicModel> MusicHistoryPlayCommand { get; }
        Task PlayMusic(ICommonMusicModel musicModel, CancellationToken cancellationToken);
        Task MusicPlaylistSearch();
        void ClearPlaylistLoaded();
        void StopMusicHistoryIsPlaying();
    }
}
