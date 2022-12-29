using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonMusicPageViewModel
    {
        List<UserMusicAlbumSelect> MusicAlbumPlaylistSelected { get; }
        ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection { get; set; }
        ObservableCollection<SelectModel> AlbumMusicSavedSelectFilteredCollection { get; set; }
        void InitFormMusicUtils(MusicAlbumDialogDataModel music);
        bool HasAlbumSaved(string videoId);
        UserMusicAlbumSelect[] GetAlbumSaved(string videoId);
        Task LoadMusicAlbumPlaylistSelected();
        void LoadAlbumMusicSavedSelect();
        Task<bool> ExistsMusicAlbumPlaylist(string albumName, ICommonMusicModel music);
        Task InsertOrUpdateMusicAlbumPlaylistSelected(string albumName, UserMusic music, List<UserMusicAlbumSelect> musicAlbumPlaylist);
        Task InsertOrUpdateMusicAlbumPlaylistSelected(string albumName, ICommonMusicModel music);
        Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, ICommonMusicModel musicModel);
        Task DeleteMusicAlbumPlaylistSelected(ICommonMusicModel musicModel);
    }
}
