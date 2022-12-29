using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonPageViewModel : INotifyPropertyChanged
    {
        private readonly CommonFormDownloadViewModel _formDownloadViewModel;
        private ImageSource _albumImage;
        private int _internetConnectionAlertGridSize;
        private int _albumPlayingGridSize;
        private string _albumName;
        public event PropertyChangedEventHandler PropertyChanged;
        public CommonPageViewModel(CommonFormDownloadViewModel formDownloadViewModel) 
        {
            _internetConnectionAlertGridSize = 0;
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
        public int InternetConnectionAlertGridSize
        {
            get { return _internetConnectionAlertGridSize; }
            set
            {

                _internetConnectionAlertGridSize = value;
                OnPropertyChanged(nameof(InternetConnectionAlertGridSize));
            }
        }
        public int AlbumPlayingGridSize
        {
            get { return _albumPlayingGridSize; }
            set
            {

                _albumPlayingGridSize = value;
                OnPropertyChanged(nameof(AlbumPlayingGridSize));
            }
        }
        public AlbumPlayer SelectedAlbum { get; set; }
        public CommonFormDownloadViewModel FormDownloadViewModel
        {
            get { return _formDownloadViewModel; }
        }
        public void InitAlbumPlayingGrid(string albumName, ImageSource albumImage) 
        {
            AlbumName = albumName;
            AlbumImage = albumImage;
            AlbumPlayingGridSize = 80;
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
