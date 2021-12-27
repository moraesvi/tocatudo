using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchMusicModel : MusicBaseViewModel
    {
        private ImageSource _iconMusicStatus;
        private ImageSource _imgStartDownloadIcon;
        private ImageSource _musicDetailsFormAlbumIcon;
        private ImageSource _musicDetailsAddAlbumIcon;
        private HttpDownload _download;
        private Color _musicSelectedColorPrimary;
        private Color _musicSelectedColorSecondary;
        private FontAttributes _musicFontAttr;
        private FontAttributes _musicDetailsAddAlbumFontAttr;
        private SelectModel _albumMusicSavedSelected;
        private int _collectionMusicOptionSize;
        private bool _iconMusicStatusEnabled;
        private bool _musicIsPlaying;
        private bool _isDownloadMusicVisible;
        private bool _iconMusicDownloadVisible;
        private bool _musicAlbumEditFormIsVisible;
        private bool _iconMusicStatusVisible;
        private bool _isBufferingMusic;
        private bool _isMusicOptionsVisible;
        private bool _normalModeIsVisible;
        private bool _albumModeIsVisible;
        private bool _albumModeDetailsIsVisible;
        private bool _modelNewAlbumIsVisible;
        private bool _musicDetailsFormAlbumIsVisible;
        private bool _musicDetailsFormDownloadIsVisible;
        private bool _musicDetailsSelectAlbumIsVisible;
        private bool _musicDetailsAddAlbumIsVisible;
        private bool _existsAnyAlbumSaved;
        private bool _musiscHasAlbumSaved;
        private bool _isSelected;
        private bool _isDownloadModelVisible;
        private bool _isViewCellPlusMusicPlaylistVisible;
        private string _textColorMusic;
        public SearchMusicModel(ApiSearchMusicModel apiSearchMusic, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, MusicSearchType searchType, bool savedOnLocalDb)
            : base(tocaTudoApi, ytClient)
        {
            Id = apiSearchMusic.Id;
            SearchType = searchType;
            MusicName = apiSearchMusic.NomeAlbum;
            TypeIcon = apiSearchMusic.Icon;
            VideoId = apiSearchMusic.VideoId;
            TipoParse = apiSearchMusic.TipoParse;
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.DownloadComplete += Download_DownloadComplete;

            _collectionMusicOptionSize = 0;
            _iconMusicStatusEnabled = false;
            _isDownloadMusicVisible = false;
            _iconMusicDownloadVisible = false;
            _musicAlbumEditFormIsVisible = false;
            _iconMusicStatusVisible = false;
            _isBufferingMusic = false;
            _isSelected = false;
            _musicDetailsAddAlbumIsVisible = false;
            _isDownloadModelVisible = false;
            _isViewCellPlusMusicPlaylistVisible = true;
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 15, Color.Black);
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _textColorMusic = "#374149";
            _musiscHasAlbumSaved = apiSearchMusic.HasAlbum;

            if (apiSearchMusic.HasAlbum)
            {
                SetAlbumMode();
            }
            else
            {
                SetNormalMode();
            }
        }
        public SearchMusicModel(ApiSearchMusicModel apiSearchMusic, ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, MusicSearchType searchType, bool savedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient)
        {
            Id = apiSearchMusic.Id;
            SearchType = searchType;
            MusicName = apiSearchMusic.NomeAlbum;
            TypeIcon = apiSearchMusic.Icon;
            VideoId = apiSearchMusic.VideoId;
            TipoParse = apiSearchMusic.TipoParse;
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.DownloadComplete += Download_DownloadComplete;

            _collectionMusicOptionSize = 0;
            _iconMusicStatusEnabled = false;
            _isDownloadMusicVisible = false;
            _iconMusicDownloadVisible = false;
            _musicAlbumEditFormIsVisible = false;
            _iconMusicStatusVisible = false;
            _isBufferingMusic = false;
            _isSelected = false;
            _musicDetailsAddAlbumIsVisible = false;
            _isDownloadModelVisible = false;
            _isViewCellPlusMusicPlaylistVisible = true;
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 15, Color.Black);
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _textColorMusic = "#374149";
            _musiscHasAlbumSaved = apiSearchMusic.HasAlbum;

            if (apiSearchMusic.HasAlbum)
            {
                SetAlbumMode();
            }
            else
            {
                SetNormalMode();
            }
        }
        public SearchMusicModel(ApiSearchMusicModel apiSearchMusic, ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, UserMusicAlbumSelect musicAlbumSelected, MusicSearchType searchType, bool savedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient)
        {
            Id = apiSearchMusic.Id;
            SearchType = searchType;
            MusicName = apiSearchMusic.NomeAlbum;
            TypeIcon = apiSearchMusic.Icon;
            VideoId = apiSearchMusic.VideoId;
            TipoParse = apiSearchMusic.TipoParse;
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            MusicModel = new MusicModel();

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.DownloadComplete += Download_DownloadComplete;

            _albumMusicSavedSelected = new SelectModel(musicAlbumSelected.Id, musicAlbumSelected.AlbumName);

            _collectionMusicOptionSize = 0;
            _iconMusicStatusEnabled = false;
            _isDownloadMusicVisible = false;
            _iconMusicDownloadVisible = false;
            _musicAlbumEditFormIsVisible = false;
            _iconMusicStatusVisible = false;
            _isBufferingMusic = false;
            _isSelected = false;
            _musicDetailsAddAlbumIsVisible = false;
            _isDownloadModelVisible = false;
            _isViewCellPlusMusicPlaylistVisible = true;
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 15, Color.Black);
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _textColorMusic = "#374149";
            _musiscHasAlbumSaved = apiSearchMusic.HasAlbum;

            if (apiSearchMusic.HasAlbum)
            {
                SetAlbumMode();
            }
            else
            {
                SetNormalMode();
            }
        }
        public SearchMusicModel(UserMusicSelect musicSelected, string icon, ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, MusicSearchType searchType, bool savedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient)
        {
            Id = musicSelected.Id;
            SearchType = searchType;
            MusicName = musicSelected.MusicName;
            TypeIcon = icon;
            VideoId = musicSelected.VideoId;
            TipoParse = new int[] { 1 };
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            MusicModel = new MusicModel();

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.DownloadComplete += Download_DownloadComplete;

            _collectionMusicOptionSize = 0;
            _iconMusicStatusEnabled = false;
            _isDownloadMusicVisible = false;
            _iconMusicDownloadVisible = false;
            _musicAlbumEditFormIsVisible = false;
            _iconMusicStatusVisible = false;
            _isBufferingMusic = false;
            _isSelected = false;
            _musicDetailsAddAlbumIsVisible = false;
            _isDownloadModelVisible = false;
            _isViewCellPlusMusicPlaylistVisible = true;
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 15, Color.Black);
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _textColorMusic = "#374149";
            //_musiscHasAlbumSaved = true;

            SetNormalMode();
        }
        public SearchMusicModel(UserMusic userMusic, ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient)
        {
            SearchType = MusicSearchType.SearchSavedMusic;
            MusicName = userMusic.MusicName;
            TypeIcon = Icon.ArrowDown;
            VideoId = userMusic.VideoId;
            TipoParse = new int[] { 1 };
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            MusicModel = new MusicModel();

            _collectionMusicOptionSize = 0;
            _iconMusicStatusEnabled = false;
            _isDownloadMusicVisible = false;
            _iconMusicDownloadVisible = false;
            _musicAlbumEditFormIsVisible = false;
            _iconMusicStatusVisible = false;
            _isBufferingMusic = false;
            _isSelected = false;
            _musicDetailsAddAlbumIsVisible = false;
            _isDownloadModelVisible = false;
            _isViewCellPlusMusicPlaylistVisible = true;
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 15, Color.Black);
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _textColorMusic = "#374149";

            SetNormalMode();
        }
        public int CollectionMusicOptionSize
        {
            get { return _collectionMusicOptionSize; }
            set
            {
                _collectionMusicOptionSize = value;
                OnPropertyChanged(nameof(CollectionMusicOptionSize));
            }
        }
        public string TypeIcon { get; set; }
        public int[] TipoParse { get; set; }
        public ImageSource IconMusicStatus
        {
            get { return _iconMusicStatus; }
            set
            {
                _iconMusicStatus = value;
                OnPropertyChanged(nameof(IconMusicStatus));
            }
        }
        public ImageSource ImgStartDownloadIcon
        {
            get { return _imgStartDownloadIcon; }
            set
            {
                _imgStartDownloadIcon = value;
                OnPropertyChanged(nameof(ImgStartDownloadIcon));
            }
        }
        public ImageSource MusicDetailsFormAlbumIcon
        {
            get { return _musicDetailsFormAlbumIcon; }
            set
            {
                _musicDetailsFormAlbumIcon = value;
                OnPropertyChanged(nameof(MusicDetailsFormAlbumIcon));
            }
        }
        public ImageSource MusicDetailsAddAlbumIcon
        {
            get { return _musicDetailsAddAlbumIcon; }
            set
            {
                _musicDetailsAddAlbumIcon = value;
                OnPropertyChanged(nameof(MusicDetailsAddAlbumIcon));
            }
        }
        public FontAttributes MusicDetailsAddAlbumFontAttr
        {
            get { return _musicDetailsAddAlbumFontAttr; }
            set
            {
                _musicDetailsAddAlbumFontAttr = value;
                OnPropertyChanged(nameof(MusicDetailsAddAlbumFontAttr));
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
        public override bool IsSelected 
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                ReloadMusicPlayingIcon();
                SetPlayingMode(base.IsSelected);
            }
        }
        public bool IconMusicStatusEnabled
        {
            get { return _iconMusicStatusEnabled; }
            set
            {
                _iconMusicStatusEnabled = value;
                OnPropertyChanged(nameof(IconMusicStatusEnabled));
            }
        }
        public bool IsMusicOptionsVisible
        {
            get { return _isMusicOptionsVisible; }
            set
            {
                _isMusicOptionsVisible = value;
                UpdateMusicDetailsFormAlbumIcon(_isMusicOptionsVisible);

                NormalModeIsVisible = !_musiscHasAlbumSaved;
                AlbumModeIsVisible = _musiscHasAlbumSaved;

                //MusicDetailsAddAlbumIsVisible = !_hasAlbumNameSaved;
                //MusicDetailsFormAlbumIsVisible = _hasAlbumNameSaved;

                OnPropertyChanged(nameof(IsMusicOptionsVisible));
            }
        }
        public bool NormalModeIsVisible
        {
            get { return _normalModeIsVisible; }
            set
            {
                _normalModeIsVisible = value;

                //MusicDetailsAddAlbumIsVisible = !_hasAlbumNameSaved;
                //MusicDetailsFormAlbumIsVisible = _hasAlbumNameSaved;

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
                    //MusicDetailsSelectAlbumIsVisible = _albumModeIsVisible;
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
                //MusicDetailsAddAlbumIsVisible = !_albumModeIsVisible;

                OnPropertyChanged(nameof(AlbumModeDetailsIsVisible));
            }
        }
        public bool MusicDetailsFormAlbumIsVisible
        {
            get { return _musicDetailsFormAlbumIsVisible; }
            set
            {
                _musicDetailsFormAlbumIsVisible = value;

                MusicDetailsSelectAlbumIsVisible = _musiscHasAlbumSaved;
                MusicDetailsFormDownloadIsVisible = false;
                //MusicDetailsAddAlbumIsVisible = !_musiscHasAlbumSaved;

                OnPropertyChanged(nameof(MusicDetailsFormAlbumIsVisible));
            }
        }
        public bool ModelNewAlbumIsVisible
        {
            get { return _modelNewAlbumIsVisible; }
            set
            {
                _modelNewAlbumIsVisible = value;
                OnPropertyChanged(nameof(ModelNewAlbumIsVisible));
            }
        }
        public bool MusicAlbumEditFormIsVisible
        {
            get { return _musicAlbumEditFormIsVisible; }
            set
            {
                _musicAlbumEditFormIsVisible = value;
                OnPropertyChanged(nameof(MusicAlbumEditFormIsVisible));
            }
        }
        public bool MusicDetailsFormDownloadIsVisible
        {
            get { return _musicDetailsFormDownloadIsVisible; }
            set
            {
                _musicDetailsFormDownloadIsVisible = value;
                MusicDetailsSelectAlbumIsVisible = false;
                MusicDetailsAddAlbumIsVisible = false;

                OnPropertyChanged(nameof(MusicDetailsFormDownloadIsVisible));
            }
        }
        public bool MusicDetailsSelectAlbumIsVisible
        {
            get { return _musicDetailsSelectAlbumIsVisible; }
            set
            {
                _musicDetailsSelectAlbumIsVisible = value;
                OnPropertyChanged(nameof(MusicDetailsSelectAlbumIsVisible));
            }
        }
        public bool MusicDetailsAddAlbumIsVisible
        {
            get { return _musicDetailsAddAlbumIsVisible; }
            set
            {
                _musicDetailsAddAlbumIsVisible = value;
                OnPropertyChanged(nameof(MusicDetailsAddAlbumIsVisible));
            }
        }
        public bool IsDownloadMusicVisible
        {
            get { return _isDownloadMusicVisible; }
            set
            {
                _isDownloadMusicVisible = value;
                OnPropertyChanged(nameof(IsDownloadMusicVisible));
            }
        }
        public bool IconMusicStatusVisible
        {
            get { return _iconMusicStatusVisible; }
            set
            {
                _iconMusicStatusVisible = value;
                OnPropertyChanged(nameof(IconMusicStatusVisible));
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
        public bool IsViewCellPlusMusicPlaylistVisible
        {
            get { return _isViewCellPlusMusicPlaylistVisible; }
            set
            {
                _isViewCellPlusMusicPlaylistVisible = value;
                OnPropertyChanged(nameof(IsViewCellPlusMusicPlaylistVisible));
            }
        }
        public Color MusicSelectedColorPrimary
        {
            get { return _musicSelectedColorPrimary; }
            set
            {
                _musicSelectedColorPrimary = value;
                OnPropertyChanged(nameof(MusicSelectedColorPrimary));
            }
        }
        public Color MusicSelectedColorSecondary
        {
            get { return _musicSelectedColorSecondary; }
            set
            {
                _musicSelectedColorSecondary = value;
                OnPropertyChanged(nameof(MusicSelectedColorSecondary));
            }
        }
        public FontAttributes MusicFontAttr
        {
            get { return _musicFontAttr; }
            private set
            {
                _musicFontAttr = value;
                OnPropertyChanged(nameof(MusicFontAttr));
            }
        }
        public string TextColorMusic
        {
            get { return _textColorMusic; }
            private set
            {
                _textColorMusic = value;
                OnPropertyChanged(nameof(TextColorMusic));
            }
        }
        public void SelectMusicAlbum(SelectModel selectModel)
        {
            if (selectModel == null)
                return;

            _albumMusicSavedSelected = selectModel;
        }
        public void UpdateIconMusicPlaying()
        {
            IconMusicStatus = _musicIsPlaying ? AppHelper.FaviconImageSource(Icon.PauseCircle, 35, Color.Black) : AppHelper.FaviconImageSource(Icon.PlayCircleO, 35, Color.Black);
            ImgStartDownloadIcon = _musicIsPlaying ? AppHelper.FaviconImageSource(Icon.PauseCircle, 35, Color.Coral) : null;
        }
        public void UpdateMusicDetailsFormAlbumIcon(bool isTrigged)
        {
            if (isTrigged)
            {
                MusicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Minus, 15, Color.Red);
            }
            else
            {
                MusicDetailsFormAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 15, Color.Black);
            }
        }
        public void InitFormMusicUtils(bool existsAnyAlbumSaved)
        {
            _existsAnyAlbumSaved = existsAnyAlbumSaved;

            ModelNewAlbumIsVisible = !_existsAnyAlbumSaved;
            MusicDetailsFormAlbumIsVisible = _existsAnyAlbumSaved;
            MusicDetailsSelectAlbumIsVisible = _existsAnyAlbumSaved;
        }
        public void ReloadMusicUtilsAddAlbumIcon()
        {
            MusicDetailsAddAlbumFontAttr = FontAttributes.None;
            MusicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
        }
        public void UpdateMusicUtilsAddAlbumIconAndForm()
        {
            if (_musicDetailsAddAlbumFontAttr == FontAttributes.None)
            {
                MusicDetailsAddAlbumFontAttr = FontAttributes.Bold;
                MusicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Minus, 22, Color.Red);

                ModelNewAlbumIsVisible = !_existsAnyAlbumSaved;
                MusicDetailsFormAlbumIsVisible = _existsAnyAlbumSaved;
                MusicDetailsSelectAlbumIsVisible = _existsAnyAlbumSaved;

                CollectionMusicOptionSize = 110;
            }
            else
            {
                MusicDetailsAddAlbumFontAttr = FontAttributes.None;
                MusicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
                ModelNewAlbumIsVisible = false;
                MusicDetailsFormAlbumIsVisible = false;
                MusicDetailsSelectAlbumIsVisible = false;
                MusicDetailsAddAlbumIsVisible = false;
                CollectionMusicOptionSize = 50;
            }
        }
        public void UpdMusicSelectedColor(bool isPlaying)
        {
            if (!isPlaying)
            {
                MusicSelectedColorPrimary = Color.FromHex("#F7F9FC");
                MusicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            }
            else
            {
                MusicSelectedColorPrimary = Color.FromHex("#4987E5");
                MusicSelectedColorSecondary = Color.FromHex("#7C97E3");
            }
        }
        public void UpdMusicFontColor(bool isPlaying)
        {
            if (isPlaying)
            {
                TextColorMusic = "White";
                MusicFontAttr = FontAttributes.Bold;
            }
            else
            {
                TextColorMusic = "#374149";
                MusicFontAttr = FontAttributes.None;
            }
        }
        public void SetNormalMode()
        {
            IsMusicOptionsVisible = false;
            NormalModeIsVisible = true;
            AlbumModeIsVisible = false;
            AlbumModeDetailsIsVisible = false;
            MusicDetailsFormAlbumIsVisible = false;
            CollectionMusicOptionSize = 0;

            ReloadMusicUtilsAddAlbumIcon();
        }
        public void SetAlbumMode(UserMusicAlbumSelect musicAlbumSelected)
        {
            AlbumMusicSavedSelected = new SelectModel(musicAlbumSelected.Id, musicAlbumSelected.AlbumName);
            SetAlbumMode();
        }
        public void SetAlbumMode()
        {
            IsMusicOptionsVisible = false;
            ModelNewAlbumIsVisible = false;
            NormalModeIsVisible = false;
            AlbumModeIsVisible = true;
            AlbumModeDetailsIsVisible = true;
            MusicDetailsFormAlbumIsVisible = false;
            CollectionMusicOptionSize = 0;

            ReloadMusicUtilsAddAlbumIcon();
        }
        public void SetAlbumDetailsMode()
        {
            NormalModeIsVisible = false;
            AlbumModeIsVisible = false;
            MusicDetailsFormAlbumIsVisible = false;
            AlbumModeDetailsIsVisible = true;

            ReloadMusicUtilsAddAlbumIcon();
        }
        public void SetDownloadingMode()
        {
            IsViewCellPlusMusicPlaylistVisible = false;
            IsDownloadModelVisible = true;
        }

        #region Private Methods
        private void SetPlayingMode(bool playing)
        {
            if (playing)
            {
                IconMusicStatusEnabled = true;
                IconMusicStatusVisible = true;

                UpdMusicSelectedColor(playing);
                UpdMusicFontColor(playing);
            }
            else
            {
                IsBufferingMusic = false;
                IsLoadded = true;
                IconMusicStatusVisible = false;
                IconMusicDownloadVisible = false;
                IsDownloadMusicVisible = false;

                UpdMusicSelectedColor(playing);
                UpdMusicFontColor(playing);
            }
        }
        private void Download_DownloadComplete((bool, byte[]) compressedMusic, object model)
        {
            IsViewCellPlusMusicPlaylistVisible = true;
            IsDownloadModelVisible = false;
        }
        #endregion
    }
}
