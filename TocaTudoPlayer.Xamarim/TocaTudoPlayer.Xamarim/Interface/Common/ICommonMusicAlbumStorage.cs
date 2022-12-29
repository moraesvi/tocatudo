using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonMusicAlbumStorage<TModel> where TModel : MusicModelBase
    {
        Task<bool> ExistsMusicAlbumPlaylist(string albumName, TModel music);
        Task InsertMusicAlbumPlaylistSelected(string albumName, TModel music);
        Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, TModel music);
        Task DeleteMusicAlbumPlaylistSelected(TModel music);
    }
}
