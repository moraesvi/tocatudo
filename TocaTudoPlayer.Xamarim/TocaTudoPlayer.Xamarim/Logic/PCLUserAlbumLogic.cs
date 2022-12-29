using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class PCLUserAlbumLogic : IPCLUserAlbumLogic
    {
        private readonly IPCLUserAlbumDb _pclUserAlbumDb;
        public PCLUserAlbumLogic(IPCLUserAlbumDb pclUserAlbumDb)
        {
            _pclUserAlbumDb = pclUserAlbumDb;
        }
        public async Task LoadDb()
        {
            await _pclUserAlbumDb.LoadDb();
        }
        public void UnLoadDb()
        {
            _pclUserAlbumDb.UnLoadDb();
        }
        public bool ExistsOnLocalDb(string uAlbumId)
        {
            return _pclUserAlbumDb.ExistsOnLocalDb(uAlbumId);
        }
        public async Task<(UserAlbum, byte[])> GetAlbumById(string videoId)
        {
            return await _pclUserAlbumDb.GetAlbumById(videoId);
        }
        public UserAlbum[] GetAlbums()
        {
            return _pclUserAlbumDb.GetAlbums()
                                  .ToArray();
        }
        public async Task<bool> SaveAlbumOnLocalDb(AlbumModel album, (bool, byte[], object) tpAlbum)
        {
            return await _pclUserAlbumDb.SaveAlbumOnLocalDb(album, tpAlbum);
        }
    }
}
