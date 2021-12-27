using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class HistoryAlbumModel : BaseViewModel
    {
        public string AlbumName { get; set; }
        public ImageSource AlbumImg { get; set; }
        public string VideoId { get; set; }
        public int ParseType { get; set; }
    }
}
