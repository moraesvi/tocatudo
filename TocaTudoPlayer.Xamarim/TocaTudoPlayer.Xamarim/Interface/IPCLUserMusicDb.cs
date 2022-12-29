using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLUserMusicDb
    {
        Task LoadDb();
        void UnLoadDb();
        Task<bool> SaveOrUpdateMusicOnLocalDb((bool, byte[], object) tpMusic);
        Task<bool> UpdateMusicOnLocalDb(UserMusic userMusic);
        List<UserMusic> GetMusics();
        Task<(UserMusic, byte[])> GetMusicById(string videoId);
        bool ExistsOnLocalDb(string videoId);
        Task<List<UserMusic>> EnsureUniqueValidItemInQueue(List<UserMusic> lstMusic);
        Task RemoveMusicFromLocalDb(string videoId, Action musicRemoved);
    }
}
