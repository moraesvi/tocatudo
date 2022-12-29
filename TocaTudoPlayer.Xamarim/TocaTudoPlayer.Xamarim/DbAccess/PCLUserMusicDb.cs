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
        public async Task<bool> SaveOrUpdateMusicOnLocalDb((bool, byte[], object) tpMusic)
        {
            if (tpMusic.Item2 == null || tpMusic.Item3 == null)
                return false;

            bool musicSaved = false;

            UserMusic userMusic = null;

            await LoadDb();

            userMusic = _lstUserMusic.Where(lu => string.Equals(lu.VideoId, ((MusicModel)tpMusic.Item3).VideoId))
                                     .FirstOrDefault();
            if (userMusic != null)
            {
                userMusic.DateTimeIn = DateTimeOffset.UtcNow.ToString();
                _lstUserMusic.Add(userMusic);
            }
            else
            {
                userMusic = new UserMusic((MusicModel)tpMusic.Item3, tpMusic.Item1);
                userMusic.MusicPath = userMusic.GetFileNameLocalPath();

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
        public async Task<bool> UpdateMusicOnLocalDb(UserMusic userMusic)
        {
            if (userMusic == null)
                return false;

            await LoadDb();
            await EnsureUniqueValidItemInQueue(_lstUserMusic);

            UserMusic uMusic = _lstUserMusic.Where(lu => string.Equals(lu.VideoId, userMusic.VideoId))
                                            .FirstOrDefault();

            if (userMusic == null)
                return false;

            bool musicSaved = false;

            try
            {
                uMusic.DateTimeIn = DateTimeOffset.UtcNow.ToString();

                musicSaved = await _pclStorage.SaveFile(UserMusic.UserMusicSavedLocalKey, _lstUserMusic);
                await File.WriteAllBytesAsync(uMusic.MusicPath, userMusic.MusicImage);
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

            if (userMusic == null || string.IsNullOrEmpty(userMusic?.MusicPath))
                return (null, null);

            if (!await _pclStorage.FileExists(userMusic.MusicPath))
            {
                UserMusic musicToRemove = _lstUserMusic?.Where(um => string.Equals(um.VideoId, userMusic.VideoId))
                                                       ?.FirstOrDefault();
                await RemoveMusicFromLocalDb(musicToRemove, _lstUserMusic);

                return (null, null);
            }

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
        public async Task<List<UserMusic>> EnsureUniqueValidItemInQueue(List<UserMusic> lstMusic)
        {
            UserMusic musicToRemove = lstMusic?.GroupBy(um => um.VideoId)
                                              ?.Where(um => um.Count() > 1)
                                              ?.SelectMany(um => um.ToArray())
                                              ?.FirstOrDefault();
            if (musicToRemove != null)
            {
                await RemoveMusicFromLocalDb(musicToRemove, lstMusic);
            }

            return _lstUserMusic;
        }
        public async Task RemoveMusicFromLocalDb(string videoId, Action musicRemoved)
        {
            await LoadDb();

            if (_lstUserMusic == null || _lstUserMusic.Count == 0)
                return;

            UserMusic userMusicRemove = _lstUserMusic.Where(um => string.Equals(um.VideoId, videoId))
                                                     .FirstOrDefault();

            if (userMusicRemove == null)
                return;

            _lstUserMusic.Remove(userMusicRemove);

            await _pclStorage.RemoveFile(userMusicRemove.MusicPath);
            await _pclStorage.SaveFile(UserMusic.UserMusicSavedLocalKey, _lstUserMusic);

            await LoadDb();

            musicRemoved();
        }

        #region Private Methods
        private async Task RemoveMusicFromLocalDb(UserMusic musicToRemove, List<UserMusic> lstMusic)
        {
            lstMusic.Remove(musicToRemove);

            await _pclStorage.SaveFile(UserMusic.UserMusicSavedLocalKey, lstMusic);
            await _pclStorage.RemoveFile(musicToRemove.MusicPath);

            await LoadDb();
        }
        #endregion
    }
}
