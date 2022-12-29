using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLUserMusicLogic
    {
        Task LoadDb();
        void UnLoadDb();
        Task<bool> SaveOrUpdateMusicOnLocalDb(MusicModel music, (bool, byte[], object) tpMusic);
        Task<bool> UpdateMusicOnLocalDb(UserMusic userMusic);
        Task<UserMusic[]> GetMusics();
        Task<(UserMusic, byte[])> GetMusicById(string videoId);
        bool ExistsOnLocalDb(string videoId);
        Task RemoveMusicFromLocalDb(string videoId, Action musicRemoved);
    }
}
