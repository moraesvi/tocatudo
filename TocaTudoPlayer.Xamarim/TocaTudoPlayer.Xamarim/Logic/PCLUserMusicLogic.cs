using System;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class PCLUserMusicLogic : IPCLUserMusicLogic
    {
        private readonly IPCLUserMusicDb _pclUserMusicDb;
        public PCLUserMusicLogic(IPCLUserMusicDb pclUserMusicDb)
        {
            _pclUserMusicDb = pclUserMusicDb;
        }
        public async Task LoadDb() //Redução de consumo de memória
        {
            await _pclUserMusicDb.LoadDb();
        }
        public void UnLoadDb() //Redução de consumo de memória
        {
            _pclUserMusicDb.UnLoadDb();
        }
        public async Task<bool> SaveOrUpdateMusicOnLocalDb(MusicModel music, (bool, byte[], object) tpMusic)
        {
            if (!ExistsOnLocalDb(music.VideoId))
                return await _pclUserMusicDb.SaveOrUpdateMusicOnLocalDb(tpMusic);

            return false;
        }
        public async Task<bool> UpdateMusicOnLocalDb(UserMusic userMusic)
        {
            if (ExistsOnLocalDb(userMusic.VideoId))
                return await _pclUserMusicDb.UpdateMusicOnLocalDb(userMusic);

            return false;
        }
        public async Task<UserMusic[]> GetMusics()
        {
            return (await _pclUserMusicDb.EnsureUniqueValidItemInQueue(_pclUserMusicDb.GetMusics()))
                                         .ToArray();
        }
        public async Task<(UserMusic, byte[])> GetMusicById(string videoId)
        {
            return await _pclUserMusicDb.GetMusicById(videoId);
        }
        public bool ExistsOnLocalDb(string videoId)
        {
            return _pclUserMusicDb.ExistsOnLocalDb(videoId);
        }
        public async Task RemoveMusicFromLocalDb(string videoId, Action musicRemoved)
        {
            await _pclUserMusicDb.RemoveMusicFromLocalDb(videoId, musicRemoved);
        }
    }
}
