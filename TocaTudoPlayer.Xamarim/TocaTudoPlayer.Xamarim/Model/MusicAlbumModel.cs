using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicAlbumModel : NotifyPropertyChanged
    {
        private int _formAlbumMusicIconSize;
        private bool _isAddAlbumMusicPlaylistVisible;
        private bool _isFormAlbumMusicDetailsOpen;
        private ImageSource _formAlbumMusicDetailsIcon;
        public MusicAlbumModel()
        {
            _formAlbumMusicIconSize = 15;
            _isAddAlbumMusicPlaylistVisible = true;
            _isFormAlbumMusicDetailsOpen = false;
            _formAlbumMusicDetailsIcon = AppHelper.FaviconImageSource(Icon.Plus, _formAlbumMusicIconSize, Color.Black);
        }
        public MusicAlbumModel(int formAlbumMusicIconSize)
        {
            _formAlbumMusicIconSize = formAlbumMusicIconSize;
            _isAddAlbumMusicPlaylistVisible = true;
            _formAlbumMusicDetailsIcon = AppHelper.FaviconImageSource(Icon.Plus, _formAlbumMusicIconSize, Color.Black);
        }
        public bool IsAddAlbumMusicPlaylistVisible
        {
            get { return _isAddAlbumMusicPlaylistVisible; }
            set
            {
                _isAddAlbumMusicPlaylistVisible = value;
                OnPropertyChanged(nameof(IsAddAlbumMusicPlaylistVisible));
            }
        }
        public ImageSource FormAlbumMusicDetailsIcon
        {
            get { return _formAlbumMusicDetailsIcon; }
            set
            {
                _formAlbumMusicDetailsIcon = value;
                OnPropertyChanged(nameof(FormAlbumMusicDetailsIcon));
            }
        }
        public Command ShowFormAlbumMusicDetailsCommand => ShowFormAlbumMusicDetailsEventCommand();

        #region Private Methods
        private Command ShowFormAlbumMusicDetailsEventCommand()
        {
            return new Command(
                execute: () =>
                {
                    UpdateFormAlbumMusicDetailsIcon();
                });
        }
        private void UpdateFormAlbumMusicDetailsIcon()
        {
            if (!_isFormAlbumMusicDetailsOpen)
            {
                FormAlbumMusicDetailsIcon = AppHelper.FaviconImageSource(Icon.Minus, _formAlbumMusicIconSize, Color.Red);
                _isFormAlbumMusicDetailsOpen = true;
            }
            else
            {
                FormAlbumMusicDetailsIcon = AppHelper.FaviconImageSource(Icon.Plus, _formAlbumMusicIconSize, Color.Black);
                _isFormAlbumMusicDetailsOpen = false;
            }
        }
        #endregion
    }
}
