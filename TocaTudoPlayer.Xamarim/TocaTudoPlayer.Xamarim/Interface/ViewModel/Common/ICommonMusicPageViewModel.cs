using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonMusicPageViewModel
    {
        bool ExistsSavedAnyAlbum { get; }
        UserMusicAlbumSelect[] MusicAlbumPlaylistSelected { get; }
        ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection { get; set; }
        void InitFormMusicUtils(SearchMusicModel music);
        Task LoadMusicAlbumPlaylistSelected();
        void LoadAlbumMusicSavedSelect();
        Task<bool> ExistsMusicAlbumPlaylist(string albumName, SearchMusicModel music);
        Task InsertMusicAlbumPlaylistSelected(string albumName, SearchMusicModel music);
        Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel musicModel);
        Task DeleteMusicAlbumPlaylistSelected(SearchMusicModel musicModel);
    }
}
