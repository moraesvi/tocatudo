using System.Threading;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IVideoModel
    {
        string VideoId { get; set; }
        Task LoadMusicImageInfo(ITocaTudoApi tocaTudoApi, CancellationToken cancellationToken = default);
    }
}
