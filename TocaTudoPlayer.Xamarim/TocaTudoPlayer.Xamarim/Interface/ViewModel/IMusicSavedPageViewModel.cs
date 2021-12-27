using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicSavedPageViewModel : IMusicAlbumPageBaseViewModel
    {
        ObservableCollection<SearchMusicModel> SavedMusicPlaylist { get; set; }
        ICommonPageViewModel CommonPageViewModel { get; }
        ICommonMusicPageViewModel CommonMusicPageViewModel { get; }
        IMusicSavedPlayedHistoryViewModel MusicSavedPlayedHistoryViewModel { get; }
        AsyncCommand<SearchMusicModel> SelectMusicCommand { get; }
        Task MusicPlaylistSearchFromDb();
        void ClearSavedMusicPlaylistLoaded();
    }
}
