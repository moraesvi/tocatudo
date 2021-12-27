using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IDbLogic
    {
        Task<ApiSearchMusicModel[]> GetAlbums();
        Task<ApiSearchMusicModel[]> GetMusics();
        Task<bool> ExistsAlbumAsync(string videoId);
        Task<bool> ExistsMusicAsync(string videoId);
        Task<int> InsertOrUpdateAsync(string uAlbumlId, AlbumModel album, (bool, byte[]) musicData);
        Task<int> InsertOrUpdateAsync(string uMusicId, MusicModel musicModel, (bool, byte[]) musicData);
        Task<(AlbumModel, byte[])> GetAlbumByVideoIdAsync(string videoId);
        Task<(MusicModel, byte[])> GetMusicByVideoIdAsync(string videoId);
        Task<bool> DeleteAlbum(string videoId);
        Task<bool> DeleteMusic(string videoId);
    }
}
