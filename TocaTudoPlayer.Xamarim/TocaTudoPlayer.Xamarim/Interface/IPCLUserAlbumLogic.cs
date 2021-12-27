using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLUserAlbumLogic
    {
        Task LoadDb();
        void UnLoadDb();
        Task<bool> SaveAlbumOnLocalDb(AlbumModel album, (bool, byte[]) tpAlbum);
        List<UserAlbum> GetAlbums();
        Task<(UserAlbum, byte[])> GetAlbumById(string uAlbumId);
        bool ExistsOnLocalDb(string uAlbumId);
    }
}
