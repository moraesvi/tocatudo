using System;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Model.DB.Cosmos;

namespace TocaTudoPlayer.Xamarim.Logic
{
    public class CosmosDbLogic : ICosmosDbLogic
    {
        private readonly IAzureCosmosConn _db;
        public CosmosDbLogic(IAzureCosmosConn db) 
        {
            _db = db;
        }
        public AppConfig GetAppConfig() 
        {
            return _db.GetAppConfig();
        }
        public async Task InsertAppException(AppException item)
        {
            TbAppException exception = new TbAppException(item);
            exception.Id = Guid.NewGuid().ToString();

            await _db.InsertAppException(exception);
        }
        public async Task InsertAppConfig(AppConfig item)
        {
            TbAppConfig appConfig = new TbAppConfig(item);
            appConfig.Id = Guid.NewGuid().ToString();

            await _db.InsertAppConfig(appConfig);
        }

    }
}
