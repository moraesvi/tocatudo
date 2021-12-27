using System.Collections.Generic;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLUserMusicDb
    {
        Task LoadDb();
        void UnLoadDb();
        Task<bool> SaveMusicOnLocalDb(MusicModel music, (bool, byte[]) tpMusic);
        List<UserMusic> GetMusics();
        Task<(UserMusic, byte[])> GetMusicById(string videoId);
        bool ExistsOnLocalDb(string videoId);
    }
}
