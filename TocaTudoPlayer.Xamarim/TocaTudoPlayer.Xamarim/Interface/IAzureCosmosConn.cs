using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Model.DB.Cosmos;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAzureCosmosConn
    {
        AppConfig GetAppConfig();
        Task InsertAppException(TbAppException item);
        Task InsertAppConfig(TbAppConfig item);
    }
}
