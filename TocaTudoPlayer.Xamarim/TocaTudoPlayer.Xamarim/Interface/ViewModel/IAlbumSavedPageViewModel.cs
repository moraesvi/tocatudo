using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAlbumSavedPageViewModel : IMusicAlbumPageBaseViewModel
    {
        ObservableCollection<SearchMusicModel> SavedAlbumPlaylist { get; set; }
        ICommonPageViewModel CommonPageViewModel { get; }
        IAlbumSavedPlayedHistoryViewModel AlbumSavedPlayedHistoryViewModel { get; }
        Task AlbumPlaylistSearchFromDb();
        void ClearSavedAlbumPlaylistLoaded();
    }
}
