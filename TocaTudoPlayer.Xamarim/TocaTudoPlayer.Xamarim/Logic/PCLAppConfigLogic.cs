using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class PCLAppConfigLogic : IPCLAppConfigLogic
    {
        private readonly IPCLStorageDb _pclStorageDb;
        public PCLAppConfigLogic(IPCLStorageDb pclStorageDb)
        {
            _pclStorageDb = pclStorageDb;
        }
        public async Task<AppConfig> Get()
        {
            return await _pclStorageDb.GetJson<AppConfig>(AppConfig.UserAppConfigLocalKey);
        }
        public async Task<bool> SaveOrUpdate(AppConfig app)
        {
            return await _pclStorageDb.SaveFile(AppConfig.UserAppConfigLocalKey, app);
        }
    }
}
