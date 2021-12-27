using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class PCLUserAlbumDb : IPCLUserAlbumDb
    {
        private readonly IPCLStorageDb _pclStorage;
        private List<UserAlbum> _lstUserAlbum;
        public PCLUserAlbumDb(IPCLStorageDb pclStorage)
        {
            _pclStorage = pclStorage;
        }
        public async Task LoadDb() //Redução de consumo de memória
        {
            _lstUserAlbum = await _pclStorage.GetJson<List<UserAlbum>>(UserAlbum.UserAlbumSavedLocalKey) ?? new List<UserAlbum> { };
        }
        public void UnLoadDb() //Redução de consumo de memória
        {
            _lstUserAlbum?.Clear();
        }
        public async Task<bool> SaveAlbumOnLocalDb(AlbumModel album, (bool, byte[]) tpMusic)
        {
            if (album == null)
                return false;

            bool musicSaved = false;

            await LoadDb();

            UserAlbum userAlbum = _lstUserAlbum.Where(al => string.Equals(al.UAlbumlId, album.UAlbumlId))
                                               .FirstOrDefault();
            if (userAlbum != null)
            {
                userAlbum.DateTimeIn = DateTimeOffset.UtcNow.ToString();
                _lstUserAlbum.Add(userAlbum);
            }
            else
            {
                userAlbum = new UserAlbum(album);

                _lstUserAlbum.Add(userAlbum);
            }

            try
            {
                musicSaved = await _pclStorage.SaveFile(UserMusic.UserMusicSavedLocalKey, _lstUserAlbum);
                await File.WriteAllBytesAsync(userAlbum.MusicPath, tpMusic.Item2);
            }
            catch
            {
                musicSaved = false;
            }

            UnLoadDb();

            return musicSaved;
        }
        public List<UserAlbum> GetAlbums()
        {
            return _lstUserAlbum;
        }
        public async Task<(UserAlbum, byte[])> GetAlbumById(string uAlbumlId)
        {
            if (_lstUserAlbum == null)
                return (null, null);

            UserAlbum userAlbum = _lstUserAlbum.Where(lu => string.Equals(lu.UAlbumlId, uAlbumlId))
                                               .FirstOrDefault();

            if (userAlbum == null)
                return (null, null);

            byte[] music = await File.ReadAllBytesAsync(userAlbum.MusicPath);

            if (userAlbum.IsMusicCompressed)
            {
                music = await HttpDownload.Descompress(music);
            }

            return (userAlbum, music);
        }
        public bool ExistsOnLocalDb(string uAlbumId)
        {
            if (_lstUserAlbum == null)
                return false;

            return _lstUserAlbum.Exists(lu => string.Equals(lu.UAlbumlId, uAlbumId));
        }
    }
}
