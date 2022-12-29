using System;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class TocaTudoApi : ITocaTudoApi
    {
        public async Task<AppConfig> AppConfigEndpoint() 
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoAppConfigEndpoint()}";
            AppConfig appConfig = await HttpApiHelper.Get<AppConfig>(urlFormat);

            return appConfig;
        }
        public async Task<string[]> SearchEndpoint(string term)
        {
            string urlFormat = $"{AppApiUri.TocaTudoGetSearchEndpoint(term)}{term}";
            ApiResultCommon<string[]> searchResult = await HttpApiHelper.Get<ApiResultCommon<string[]>>(urlFormat);

            return searchResult?.Result;
        }
        public async Task<ApiSearchMusicModel[]> SearchPlaylistEndpoint(string term)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoGetPlaylistEndpoint(term)}";
            ApiSearchMusicModel[] playlistResult = await HttpApiHelper.Get<ApiSearchMusicModel[]>(urlFormat);

            return playlistResult;
        }
        public async Task<ApiSearchMusicModel[]> SearchMusicEndpoint(string term)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoGetMusicEndpoint(term)}";
            ApiSearchMusicModel[] playlistResult = await HttpApiHelper.Get<ApiSearchMusicModel[]>(urlFormat);

            return playlistResult;
        }
        public async Task<ApiAlbumModel> AlbumEndpoint(AlbumParseType tpParse, string videoId)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoGetAlbumEndpoint(videoId, (int)tpParse)}";
            ApiAlbumModel albumResult = await HttpApiHelper.Get<ApiAlbumModel>(urlFormat);

            return albumResult;
        }
        public async Task<string> StreamUrlEndpoint(string videoId)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoGetStreamUrlEndpoint(videoId)}";
            string result = await HttpApiHelper.Get<string>(urlFormat);

            return result;
        }
        public async Task<byte[]> PlayerImageEndpoint(string videoId)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoGetPlayerImageEndpoint(videoId)}";           
            byte[] result = await HttpApiHelper.Get<byte[]>(urlFormat);

            return result;
        }
        public async Task<byte[]> PlayerImageWidescreenEndpoint(string videoId)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoGetPlayerImageWidescreenEndpoint(videoId)}";
            byte[] result = await HttpApiHelper.Get<byte[]>(urlFormat);

            return result;
        }
        public async Task<bool> AppExceptionEndpoint(AppException exception)
        {
            string urlFormat = $"{AppApiUri.TOCA_TUDO_URL}{AppApiUri.TocaTudoAppExceptionEndpoint()}";
            bool ok = await HttpApiHelper.Post<bool>(urlFormat, exception);

            return ok;
        }
    }
}
