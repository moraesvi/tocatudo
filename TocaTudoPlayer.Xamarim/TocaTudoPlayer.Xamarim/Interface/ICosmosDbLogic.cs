using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICosmosDbLogic
    {
        AppConfig GetAppConfig();
        Task InsertAppException(AppException item);
        Task InsertAppConfig(AppConfig item);
    }
}
