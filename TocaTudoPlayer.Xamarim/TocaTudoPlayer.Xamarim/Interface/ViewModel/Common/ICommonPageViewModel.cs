using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonPageViewModel : INotifyPropertyChanged
    {
        string AlbumName { get; }
        ImageSource AlbumImage { get; }
        int AlbumPlayingGridSize { get; set; }
        AlbumPlayer SelectedAlbum { get; set; }
        CommonFormDownloadViewModel FormDownloadViewModel { get; }
        void InitAlbumPlayingGrid(string albumName, ImageSource albumImage);
    }
}
