using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonPageViewModel : ICommonPageViewModel
    {
        private readonly ICommonFormDownloadViewModel _formDownloadViewModel;
        private ImageSource _albumImage;
        private int _albumPlayingGridSize;
        private string _albumName;
        public event PropertyChangedEventHandler PropertyChanged;
        public CommonPageViewModel(ICommonFormDownloadViewModel formDownloadViewModel) 
        {
            _albumPlayingGridSize = 0;
            _formDownloadViewModel = formDownloadViewModel;
        }
        public string AlbumName
        {
            get { return _albumName; }
            private set
            {

                _albumName = value;
                OnPropertyChanged(nameof(AlbumName));
            }
        }
        public ImageSource AlbumImage
        {
            get { return _albumImage; }
            private set
            {

                _albumImage = value;
                OnPropertyChanged(nameof(AlbumImage));
            }
        }
        public int AlbumPlayingGridSize
        {
            get { return _albumPlayingGridSize; }
            private set
            {

                _albumPlayingGridSize = value;
                OnPropertyChanged(nameof(AlbumPlayingGridSize));
            }
        }
        public NavigationPage SelectedMusic { get; set; }
        public ICommonFormDownloadViewModel FormDownloadViewModel
        {
            get { return _formDownloadViewModel; }
        }
        public void InitAlbumPlayingGrid(string albumName, ImageSource albumImage) 
        {
            AlbumName = albumName;
            AlbumImage = albumImage;
            AlbumPlayingGridSize = 150;
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
