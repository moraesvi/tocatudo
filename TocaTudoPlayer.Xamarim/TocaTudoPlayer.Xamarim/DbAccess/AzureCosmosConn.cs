using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Model.DB.Cosmos;

namespace TocaTudoPlayer.Xamarim
{
    public class AzureCosmosConn : IAzureCosmosConn
    {
        private const string APP_EXCEPTION_CONTAINER = "AppException";
        private const string APP_CONFIG_CONTAINER = "AppConfig";
        private readonly CosmosClient _dbClient;
        private Container _container;
        private string _databaseName;
        public AzureCosmosConn(CosmosClient dbClient, string databaseName)
        {
            _dbClient = dbClient;
            _databaseName = databaseName;
        }
        public AppConfig GetAppConfig()
        {
            _container = _dbClient.GetContainer(_databaseName, APP_CONFIG_CONTAINER);
       
            FeedIterator<TbAppConfig> query = _container.GetItemLinqQueryable<TbAppConfig>(true)
                                                        .ToFeedIterator();

            while (query.HasMoreResults)
            {
                var task = query.ReadNextAsync();
                task.Wait();

                TbAppConfig currentResultSet = task.Result.FirstOrDefault();

                return new AppConfig()
                {
                    Id = currentResultSet.Id,
                    AdMob = new AppConfigAdMob()
                    {
                        AdsMusicBanner = currentResultSet.AdMob.AdsMusicBanner,
                        AdsIntersticialAlbum = currentResultSet.AdMob.AdsIntersticialAlbum,
                        AdsActiveProdMode = currentResultSet.AdMob.AdsActiveProdMode
                    }
                };
            }

            return null;
        }
        public async Task InsertAppException(TbAppException item)
        {
            item.Id = Guid.NewGuid().ToString();
            _container = _dbClient.GetContainer(_databaseName, APP_EXCEPTION_CONTAINER);

            await _container.CreateItemAsync(item, new PartitionKey(item.Id));
        }
        public async Task InsertAppConfig(TbAppConfig item)
        {
            item.Id = Guid.NewGuid().ToString();
            _container = _dbClient.GetContainer(_databaseName, APP_CONFIG_CONTAINER);

            await _container.CreateItemAsync(item, new PartitionKey(item.Id));
        }
    }
}
