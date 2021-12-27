using System.Collections.Generic;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLUserMusicLogic
    {
        Task LoadDb();
        void UnLoadDb();
        Task<bool> SaveMusicOnLocalDb(MusicModel music, (bool, byte[]) tpMusic);
        UserMusic[] GetMusics();
        Task<(UserMusic, byte[])> GetMusicById(string videoId);
        bool ExistsOnLocalDb(string videoId);
    }
}
