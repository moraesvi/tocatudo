using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class PCLUserMusicDb : IPCLUserMusicDb
    {
        private readonly IPCLStorageDb _pclStorage;
        private List<UserMusic> _lstUserMusic;
        public PCLUserMusicDb(IPCLStorageDb pclStorage)
        {
            _pclStorage = pclStorage;
        }
        public async Task LoadDb()
        {
            _lstUserMusic = await _pclStorage.GetJson<List<UserMusic>>(UserMusic.UserMusicSavedLocalKey) ?? new List<UserMusic> { };
        }
        public void UnLoadDb() //Redução de consumo de memória
        {
            _lstUserMusic?.Clear();
        }
        public async Task<bool> SaveMusicOnLocalDb(MusicModel music, (bool, byte[]) tpMusic)
        {
            if (music == null)
                return false;

            bool musicSaved = false;

            UserMusic userMusic = null;

            await LoadDb();

            userMusic = _lstUserMusic.Where(lu => string.Equals(lu.VideoId, ((MusicModel)music).VideoId))
                                     .FirstOrDefault();
            if (userMusic != null)
            {
                userMusic.DateTimeIn = DateTimeOffset.UtcNow.ToString();
                _lstUserMusic.Add(userMusic);
            }
            else
            {
                userMusic = new UserMusic((MusicModel)music, tpMusic.Item1);
                userMusic.MusicPath = await userMusic.GetFileNameLocalPath();

                _lstUserMusic.Add(userMusic);
            }

            try
            {
                musicSaved = await _pclStorage.SaveFile(UserMusic.UserMusicSavedLocalKey, _lstUserMusic);
                await File.WriteAllBytesAsync(userMusic.MusicPath, tpMusic.Item2);
            }
            catch
            {
                musicSaved = false;
            }

            UnLoadDb();

            return musicSaved;
        }
        public List<UserMusic> GetMusics()
        {
            return _lstUserMusic;
        }
        public async Task<(UserMusic, byte[])> GetMusicById(string videoId)
        {
            if (_lstUserMusic == null)
                return (null, null);

            UserMusic userMusic = _lstUserMusic.Where(lu => string.Equals(lu.VideoId, videoId))
                                               .FirstOrDefault();

            if (userMusic == null)
                return (null, null);

            byte[] music = await File.ReadAllBytesAsync(userMusic.MusicPath);

            if (userMusic.IsMusicCompressed)
            {
                music = await HttpDownload.Descompress(music);
            }

            return (userMusic, music);
        }
        public bool ExistsOnLocalDb(string videoId)
        {
            if (_lstUserMusic == null)
                return false;

            return _lstUserMusic.Exists(lu => string.Equals(lu.VideoId, videoId));
        }
    }
}
