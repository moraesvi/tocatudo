using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonFormDownloadViewModel
    {
        int TotalInProgress { get; set; }
        bool IsFormDownloadVisible { get; set; }
        bool MusicDownloadModeIsVisible { get; set; }
        bool AlbumDownloadModeIsVisible { get; set; }
        string MusicName { get; set; }
        ImageSource ImgMusic { get; set; }
        HttpDownload Download { get; set; }
        void SetDownloadInProgress(string videoId, string musicName, ImageSource imgMusic, HttpDownload download);
        Task UpdateDownloadQueue();
    }
}
