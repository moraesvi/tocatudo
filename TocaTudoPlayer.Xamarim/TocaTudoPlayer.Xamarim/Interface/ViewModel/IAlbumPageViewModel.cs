using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAlbumPageViewModel : IMusicAlbumPageBaseViewModel
    {
        bool AlbumPlaylistIsVisible { get; set; }
        string AlbumSearchedName { get; set; }
        ICommonMusicPlayerViewModel MusicPlayer { get; }
        IAlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel { get; }
        ICommonPageViewModel CommonPageViewModel { get; }
        ObservableCollection<SearchMusicModel> AlbumPlaylist { get; set; }
        ICommand SearchAlbumCommand { get; set; }
        Command<UserAlbumPlayedHistory> AlbumHistoryFormCommand { get; }
        Task AlbumPlaylistSearch();
    }
}
