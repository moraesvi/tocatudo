using System.Net.Http;

namespace TocaTudoPlayer.Xamarim
{
    public interface IHTTPClientHandlerCreationService
    {
        HttpClientHandler GetInsecureHandler();
    }
}
