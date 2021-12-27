using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface ITocaTudoApi
    {
        Task<string[]> SearchEndpoint(string term);
        Task<ApiSearchMusicModel[]> SearchPlaylistEndpoint(string term);
        Task<ApiSearchMusicModel[]> SearchMusicEndpoint(string term);
        Task<ApiAlbumModel> AlbumEndpoint(AlbumParseType tpParse, string videoId);
        Task<string> StreamUrlEndpoint(string videoId);
        Task<byte[]> PlayerImageEndpoint(string videoId);
    }
}
