using System;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class TocaTudoApi : ITocaTudoApi
    {
        public async Task<string[]> SearchEndpoint(string term)
        {
            string urlFormat = string.Concat(AppApiUri.TocaTudoGetSearchEndpoint(term), term);
            ApiResultCommon<string[]> searchResult = await HttpApiHelper.Get<ApiResultCommon<string[]>>(urlFormat);

            return searchResult?.Result;
        }
        public async Task<ApiSearchMusicModel[]> SearchPlaylistEndpoint(string term)
        {
            string urlFormat = string.Concat(AppApiUri.TOCA_TUDO_URL, AppApiUri.TOCA_TUDO_BUSCAV2_PLAYLIST_ENDPOINT, term);
            ApiResultCommon<ApiSearchMusicModel[]> playlistResult = await HttpApiHelper.Get<ApiResultCommon<ApiSearchMusicModel[]>>(urlFormat);

            return playlistResult?.Result;
        }
        public async Task<ApiSearchMusicModel[]> SearchMusicEndpoint(string term)
        {
            string urlFormat = string.Concat(AppApiUri.TOCA_TUDO_URL, AppApiUri.TocaTudoGetMusicEndpoint(term));

            ApiResultCommon<ApiSearchMusicModel[]> playlistResult = await HttpApiHelper.Get<ApiResultCommon<ApiSearchMusicModel[]>>(urlFormat);

            return playlistResult?.Result;
        }
        public async Task<ApiAlbumModel> AlbumEndpoint(AlbumParseType tpParse, string videoId)
        {
            string urlFormat = string.Concat(AppApiUri.TOCA_TUDO_URL, AppApiUri.TOCA_TUDO_PLAYLISTV2_ENDPOINT, videoId, "/", (int)tpParse);
            ApiResultCommon<ApiAlbumModel> albumResult = await HttpApiHelper.Get<ApiResultCommon<ApiAlbumModel>>(urlFormat);

            return albumResult?.Result;
        }
        public async Task<string> StreamUrlEndpoint(string videoId)
        {
            string urlFormat = string.Concat(AppApiUri.TOCA_TUDO_URL, AppApiUri.TocaTudoGetStreamUrlEndpoint(videoId));
            ApiResultCommon<string> result = await HttpApiHelper.Get<ApiResultCommon<string>>(urlFormat);

            return result?.Result;
        }
        public async Task<byte[]> PlayerImageEndpoint(string videoId)
        {
            string urlFormat = string.Concat(AppApiUri.TOCA_TUDO_URL, AppApiUri.TocaTudoGetPlayerImageEndpoint(videoId));
            ApiResultCommon<byte[]> result = await HttpApiHelper.Get<ApiResultCommon<byte[]>>(urlFormat);

            return result?.Result;
        }
    }
}
