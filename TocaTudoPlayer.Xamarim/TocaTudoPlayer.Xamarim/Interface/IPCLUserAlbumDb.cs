using System.Collections.Generic;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLUserAlbumDb
    {
        Task LoadDb();
        void UnLoadDb();
        Task<bool> SaveAlbumOnLocalDb(AlbumModel album, (bool, byte[], object) tpMusic);
        List<UserAlbum> GetAlbums();
        Task<(UserAlbum, byte[])> GetAlbumById(string videoId);
        bool ExistsOnLocalDb(string uAlbumId);
    }
}
