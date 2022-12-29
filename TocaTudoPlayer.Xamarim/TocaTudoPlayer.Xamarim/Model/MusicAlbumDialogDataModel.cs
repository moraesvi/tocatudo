using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicAlbumDialogDataModel : NotifyPropertyChanged
    {
        private bool _isMusicOptionsVisible;
        private bool _isDownloadModelVisible;
        private bool _isSavedMusicDownloadModelVisible;
        private bool _normalModeIsVisible;
        private bool _albumModeIsVisible;
        private bool _albumModeDetailsIsVisible;
        private bool _savedAlbumModeIsVisible;
        private bool _formDownloadIsVisible;
        private bool _newAlbumIsVisible;
        private bool _existsAnyAlbumSaved;
        private bool _formAlbumIsVisible;
        private bool _selectAlbumIsVisible;
        private bool _addAlbumIsVisible;
        private bool _editFormIsVisible;
        private bool _musiscHasAlbumSaved;
        private bool _musicNameIsVisible;
        private FontAttributes _addAlbumFontAttr;
        private ImageSource _addAlbumIcon;
        private SelectModel _albumMusicSavedSelected;
        public MusicAlbumDialogDataModel()
        {
            MusicAlbumModel = new MusicAlbumModel();

            SetNormalMode();
        }
        public MusicAlbumDialogDataModel(bool musicHasAlbumSaved)
        {
            _musiscHasAlbumSaved = musicHasAlbumSaved;
            MusicAlbumModel = new MusicAlbumModel();

            if (_musiscHasAlbumSaved)
                SetAlbumMode();
            else
                SetNormalMode();
        }
        public MusicAlbumDialogDataModel(bool musicHasAlbumSaved, UserMusicAlbumSelect musicAlbumSelected)
        {
            _musiscHasAlbumSaved = musicHasAlbumSaved;
            MusicAlbumModel = new MusicAlbumModel();

            if (_musiscHasAlbumSaved)
                _albumMusicSavedSelected = new SelectModel(musicAlbumSelected.Id, musicAlbumSelected.AlbumName);

            if (_musiscHasAlbumSaved)
                SetAlbumMode();
            else
                SetNormalMode();
        }
        public FontAttributes AddAlbumFontAttr
        {
            get { return _addAlbumFontAttr; }
            set
            {
                _addAlbumFontAttr = value;
                OnPropertyChanged(nameof(AddAlbumFontAttr));
            }
        }
        public ImageSource AddAlbumIcon
        {
            get { return _addAlbumIcon; }
            set
            {
                _addAlbumIcon = value;
                OnPropertyChanged(nameof(AddAlbumIcon));
            }
        }
        public bool ExistsAnyAlbumSaved 
        {
            get { return _existsAnyAlbumSaved; }
            set
            {
                _existsAnyAlbumSaved = value;
                OnPropertyChanged(nameof(ExistsAnyAlbumSaved));
            }
        }
        public bool IsMusicOptionsVisible
        {
            get { return _isMusicOptionsVisible; }
            set
            {
                _isMusicOptionsVisible = value;

                NormalModeIsVisible = !_musiscHasAlbumSaved;
                AlbumModeIsVisible = _musiscHasAlbumSaved;

                OnPropertyChanged(nameof(IsMusicOptionsVisible));
            }
        }
        public bool IsDownloadModelVisible
        {
            get { return _isDownloadModelVisible; }
            set
            {
                _isDownloadModelVisible = value;
                OnPropertyChanged(nameof(IsDownloadModelVisible));
            }
        }
        public bool IsSavedMusicDownloadModelVisible
        {
            get { return _isSavedMusicDownloadModelVisible; }
            set
            {
                _isSavedMusicDownloadModelVisible = value;
                OnPropertyChanged(nameof(IsSavedMusicDownloadModelVisible));
            }
        }
        public bool NormalModeIsVisible
        {
            get { return _normalModeIsVisible; }
            set
            {
                _normalModeIsVisible = value;
                OnPropertyChanged(nameof(NormalModeIsVisible));
            }
        }
        public bool AlbumModeIsVisible
        {
            get { return _albumModeIsVisible; }
            set
            {
                _albumModeIsVisible = value;

                if (!_normalModeIsVisible)
                {
                    AlbumModeDetailsIsVisible = true;
                }

                OnPropertyChanged(nameof(AlbumModeIsVisible));
            }
        }
        public bool AlbumModeDetailsIsVisible
        {
            get { return _albumModeDetailsIsVisible; }
            set
            {
                _albumModeDetailsIsVisible = value;
                OnPropertyChanged(nameof(AlbumModeDetailsIsVisible));
            }
        }
        public bool SavedAlbumModeIsVisible
        {
            get { return _savedAlbumModeIsVisible; }
            set
            {
                _savedAlbumModeIsVisible = value;
                OnPropertyChanged(nameof(SavedAlbumModeIsVisible));
            }
        }
        public bool FormDownloadIsVisible
        {
            get { return _formDownloadIsVisible; }
            set
            {
                _formDownloadIsVisible = value;
                SelectAlbumIsVisible = false;
                AddAlbumIsVisible = false;

                OnPropertyChanged(nameof(FormDownloadIsVisible));
            }
        }
        public bool NewAlbumIsVisible
        {
            get { return _newAlbumIsVisible; }
            set
            {
                _newAlbumIsVisible = value;
                OnPropertyChanged(nameof(NewAlbumIsVisible));
            }
        }
        public bool FormAlbumIsVisible
        {
            get { return _formAlbumIsVisible; }
            set
            {
                _formAlbumIsVisible = value;

                //SelectAlbumIsVisible = _musiscHasAlbumSaved;
                FormDownloadIsVisible = false;

                OnPropertyChanged(nameof(FormAlbumIsVisible));
            }
        }
        public bool SelectAlbumIsVisible
        {
            get { return _selectAlbumIsVisible; }
            set
            {
                _selectAlbumIsVisible = value;
                OnPropertyChanged(nameof(SelectAlbumIsVisible));
            }
        }
        public bool AddAlbumIsVisible
        {
            get { return _addAlbumIsVisible; }
            set
            {
                _addAlbumIsVisible = value;
                OnPropertyChanged(nameof(AddAlbumIsVisible));
            }
        }
        public bool EditFormIsVisible
        {
            get { return _editFormIsVisible; }
            set
            {
                _editFormIsVisible = value;
                OnPropertyChanged(nameof(EditFormIsVisible));
            }
        }
        public bool MusicNameIsVisible
        {
            get { return _musicNameIsVisible; }
            set
            {
                _musicNameIsVisible = value;
                OnPropertyChanged(nameof(MusicNameIsVisible));
            }
        }
        public SelectModel AlbumMusicSavedSelected
        {
            get { return _albumMusicSavedSelected; }
            set
            {
                _albumMusicSavedSelected = value;
                OnPropertyChanged(nameof(AlbumMusicSavedSelected));
            }
        }
        public MusicAlbumModel MusicAlbumModel { get; set; }
        public void SetNormalMode()
        {
            IsMusicOptionsVisible = false;
            NormalModeIsVisible = true;
            AlbumModeIsVisible = false;
            AlbumModeDetailsIsVisible = false;
            FormAlbumIsVisible = false;
            MusicNameIsVisible = true;

            ReloadAddAlbumIcon();
        }
        public void SetAlbumMode(UserMusicAlbumSelect musicAlbumSelected)
        {
            AlbumMusicSavedSelected = new SelectModel(musicAlbumSelected.Id, musicAlbumSelected.AlbumName);

            SetAlbumMode();
        }
        public void SetAlbumMode(SelectModel musicAlbumSelected)
        {
            AlbumMusicSavedSelected = musicAlbumSelected;

            SetAlbumMode();
        }
        public void SetAlbumMode()
        {
            IsMusicOptionsVisible = false;
            NewAlbumIsVisible = false;
            NormalModeIsVisible = false;
            AlbumModeIsVisible = true;
            AlbumModeDetailsIsVisible = false;
            FormAlbumIsVisible = false;
            MusicNameIsVisible = false;

            ReloadAddAlbumIcon();
        }
        public void SetSavedAlbumMode()
        {
            IsMusicOptionsVisible = false;
            NewAlbumIsVisible = false;
            NormalModeIsVisible = false;
            AlbumModeIsVisible = false;
            AlbumModeDetailsIsVisible = false;
            SavedAlbumModeIsVisible = true;
            FormAlbumIsVisible = false;
            MusicNameIsVisible = true;

            ReloadAddAlbumIcon();
        }
        public void SetAlbumDetailsMode()
        {
            NormalModeIsVisible = false;
            AlbumModeIsVisible = false;
            FormAlbumIsVisible = false;
            AlbumModeDetailsIsVisible = true;

            ReloadAddAlbumIcon();
        }
        public void SetDownloadingMode()
        {
            MusicAlbumModel.IsAddAlbumMusicPlaylistVisible = false;

            if (!AlbumModeIsVisible)
                IsDownloadModelVisible = true;
            else
                IsSavedMusicDownloadModelVisible = true;
        }
        public void UpdateAddAlbumIconAndForm(bool reload = false)
        {
            if (_addAlbumFontAttr == FontAttributes.None && !reload)
            {
                AddAlbumFontAttr = FontAttributes.Bold;
                AddAlbumIcon = AppHelper.FaviconImageSource(Icon.Minus, 18, Color.FromHex("#D4420C"));

                NewAlbumIsVisible = !_existsAnyAlbumSaved;
                FormAlbumIsVisible = _existsAnyAlbumSaved;
                SelectAlbumIsVisible = _existsAnyAlbumSaved;
            }
            else
            {
                AddAlbumFontAttr = FontAttributes.None;
                AddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 18, Color.FromHex("#D4420C"));

                NewAlbumIsVisible = false;
                FormAlbumIsVisible = false;
                SelectAlbumIsVisible = false;
                AddAlbumIsVisible = false;
                EditFormIsVisible = false;
            }
        }
        public void SelectMusicAlbum(SelectModel selectModel)
        {
            if (selectModel == null)
                return;

            _albumMusicSavedSelected = selectModel;
        }
        public void ReloadAddAlbumIcon()
        {
            AddAlbumFontAttr = FontAttributes.None;
            AddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 18, Color.FromHex("#D4420C"));
            SelectAlbumIsVisible = false;
        }
        public void Reload() 
        {           
            ReloadAddAlbumIcon();
            UpdateAddAlbumIconAndForm(reload: true);
        }
    }
}
