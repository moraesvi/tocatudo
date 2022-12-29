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
        public async Task<bool> SaveAlbumOnLocalDb(AlbumModel album, (bool, byte[], object) tpMusic)
        {
            if (album == null)
                return false;

            bool musicSaved = false;

            await LoadDb();

            UserAlbum userAlbum = _lstUserAlbum.Where(al => string.Equals(al.VideoId, album.VideoId))
                                               .FirstOrDefault();
            if (userAlbum != null)
            {
                userAlbum.DateTimeIn = DateTimeOffset.UtcNow.ToString();
                _lstUserAlbum.Add(userAlbum);
            }
            else
            {
                userAlbum = new UserAlbum(album, tpMusic.Item1);
                userAlbum.DateTimeIn = DateTimeOffset.UtcNow.ToString();

                _lstUserAlbum.Add(userAlbum);
            }

            try
            {
                userAlbum.MusicPath = userAlbum.GetFileNameLocalPath();
                musicSaved = await _pclStorage.SaveFile(UserAlbum.UserAlbumSavedLocalKey, _lstUserAlbum);

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
        public async Task<(UserAlbum, byte[])> GetAlbumById(string videoId)
        {
            if (_lstUserAlbum == null)
                return (null, null);

            UserAlbum userAlbum = _lstUserAlbum.Where(lu => string.Equals(lu.VideoId, videoId))
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
        public bool ExistsOnLocalDb(string videoId)
        {
            if (_lstUserAlbum == null)
                return false;

            return _lstUserAlbum.Exists(lu => string.Equals(lu.VideoId, videoId));
        }
    }
}
