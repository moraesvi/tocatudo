using System.Collections.Generic;
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
        public async Task<bool> SaveMusicOnLocalDb(MusicModel music, (bool, byte[]) tpMusic)
        {
            return await  _pclUserMusicDb.SaveMusicOnLocalDb(music, tpMusic);
        }
        public UserMusic[] GetMusics() 
        {
            return _pclUserMusicDb.GetMusics()
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
    }
}
