using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonPageViewModel : INotifyPropertyChanged
    {
        string AlbumName { get; }
        ImageSource AlbumImage { get; }
        int AlbumPlayingGridSize { get; }
        NavigationPage SelectedMusic { get; set; }
        ICommonFormDownloadViewModel FormDownloadViewModel { get; }
        void InitAlbumPlayingGrid(string albumName, ImageSource albumImage);
    }
}
