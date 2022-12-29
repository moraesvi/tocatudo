using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLAppConfigLogic
    {
        Task<AppConfig> Get();
        Task<bool> SaveOrUpdate(AppConfig app);
    }
}
