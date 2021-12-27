using System.IO;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);
    public delegate void DownloadCompleteHandler(byte[] compressedMusic);
    public interface IHttpClientDownload
    {
        event ProgressChangedHandler ProgressChanged;
        event DownloadCompleteHandler DownloadComplete;
        Task Download(string downloadUrl);
    }
}
