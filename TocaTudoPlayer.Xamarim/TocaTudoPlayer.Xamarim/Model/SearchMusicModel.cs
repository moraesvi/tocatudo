using System.IO;
using Xamarin.Forms;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchMusicModel : MusicModelBase
    {
        private ImageSource _iconMusicStatus;
        private ImageSource _imgStartDownloadIcon;
        private ImageSource _musicDetailsAddAlbumIcon;
        private HttpDownload _download;
        private Color _musicSelectedColorPrimary;
        private Color _musicSelectedColorSecondary;
        private FontAttributes _musicFontAttr;
        private FontAttributes _musicDetailsAddAlbumFontAttr;
        private int _collectionMusicOptionSize;
        private bool _iconMusicStatusEnabled;
        private bool _musicIsPlaying;
        private bool _isDownloadMusicVisible;
        private bool _iconMusicStatusVisible;
        private bool _isBufferingMusic;
        private bool _isSelected;
        private string _whiteColor;
        public SearchMusicModel(ApiSearchMusicModel apiSearchMusic, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, MusicSearchType searchType, bool savedOnLocalDb)
            : base(apiSearchMusic, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            Id = apiSearchMusic.Id;
            SearchType = searchType;
            MusicName = apiSearchMusic.NomeAlbum;
            MusicTime = apiSearchMusic.MusicTime;
            MusicTimeTotalSeconds = apiSearchMusic.MusicTimeTotalSeconds;
            MusicImageUrl = apiSearchMusic.MusicImageUrl;
            TypeIcon = apiSearchMusic.Icon;
            VideoId = apiSearchMusic.VideoId;
            TipoParse = apiSearchMusic.TipoParse;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            Download.DownloadComplete += Download_DownloadComplete;
            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(ApiSearchMusicModel apiSearchMusic, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, MusicSearchType searchType, bool savedOnLocalDb)
            : base(apiSearchMusic, formDownloadViewModel, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            Id = apiSearchMusic.Id;
            SearchType = searchType;
            MusicName = apiSearchMusic.NomeAlbum;
            MusicTime = apiSearchMusic.MusicTime;
            MusicTimeTotalSeconds = apiSearchMusic.MusicTimeTotalSeconds;
            MusicImageUrl = apiSearchMusic.MusicImageUrl;
            TypeIcon = apiSearchMusic.Icon;
            VideoId = apiSearchMusic.VideoId;
            TipoParse = apiSearchMusic.TipoParse;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            Download.DownloadComplete += Download_DownloadComplete;
            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(ApiSearchMusicModel apiSearchMusic, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, UserMusicAlbumSelect musicAlbumSelected, MusicSearchType searchType, bool savedOnLocalDb)
            : base(apiSearchMusic, musicAlbumSelected, formDownloadViewModel, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            Id = apiSearchMusic.Id;
            SearchType = searchType;
            MusicName = apiSearchMusic.NomeAlbum;
            MusicTime = apiSearchMusic.MusicTime;
            MusicTimeTotalSeconds = apiSearchMusic.MusicTimeTotalSeconds;
            MusicImageUrl = apiSearchMusic.MusicImageUrl;
            TypeIcon = apiSearchMusic.Icon;
            VideoId = apiSearchMusic.VideoId;
            TipoParse = apiSearchMusic.TipoParse;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            Download.DownloadComplete += Download_DownloadComplete;
            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(SelectModel albumSelected, UserMusicSelect musicSelected, string icon, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, MusicSearchType searchType, bool savedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            Id = musicSelected.Id;
            SearchType = searchType;
            MusicName = musicSelected.MusicName;
            MusicTime = musicSelected.MusicTime;
            MusicTimeTotalSeconds = musicSelected.MusicTimeTotalSeconds;
            ByteMusicImage = musicSelected.MusicImage;
            ImgMusic = ImageSource.FromStream(() => new MemoryStream(musicSelected.MusicImage));
            TypeIcon = icon;
            VideoId = musicSelected.VideoId;
            TipoParse = new int[] { 1 };
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            Download.DownloadComplete += Download_DownloadComplete;

            MusicAlbumPopupModel = new MusicAlbumDialogDataModel();
            MusicAlbumPopupModel.SelectMusicAlbum(albumSelected);
            MusicAlbumPopupModel.SetSavedAlbumMode();

            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(UserMusicAlbumSelect musicAlbumSelected, UserMusicSelect musicSelected, string icon, MusicSearchType searchType, bool savedOnLocalDb)
        {
            Id = musicSelected.Id;
            SearchType = searchType;
            MusicName = musicSelected.MusicName;
            MusicTime = musicSelected.MusicTime;
            MusicTimeTotalSeconds = musicSelected.MusicTimeTotalSeconds;
            ByteMusicImage = musicSelected.MusicImage;
            ImgMusic = ImageSource.FromStream(() => new MemoryStream(musicSelected.MusicImage));
            TypeIcon = icon;
            VideoId = musicSelected.VideoId;
            TipoParse = new int[] { 1 };
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(savedOnLocalDb, musicAlbumSelected);

            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(UserMusic userMusic, string icon, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
           : base(userMusic, formDownloadViewModel, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            SearchType = MusicSearchType.SearchSavedMusic;
            MusicName = userMusic.MusicName;
            MusicTime = string.IsNullOrEmpty(userMusic.MusicTime) ? "00:00" : userMusic.MusicTime;
            MusicTimeTotalSeconds = userMusic.MusicTimeTotalSeconds;
            TypeIcon = icon;
            VideoId = userMusic.VideoId;
            ByteMusicImage = userMusic.MusicImage;
            ImgMusic = ImageSource.FromStream(() => new MemoryStream(userMusic.MusicImage));
            TipoParse = new int[] { 1 };
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(UserMusicAlbumSelect musicAlbumSelect, UserMusic userMusic, string icon, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
            : base(musicAlbumSelect, userMusic, formDownloadViewModel, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            SearchType = MusicSearchType.SearchSavedMusic;
            MusicName = userMusic.MusicName;
            MusicTime = string.IsNullOrEmpty(userMusic.MusicTime) ? "00:00" : userMusic.MusicTime;
            MusicTimeTotalSeconds = userMusic.MusicTimeTotalSeconds;
            TypeIcon = icon;
            VideoId = userMusic.VideoId;
            ByteMusicImage = userMusic.MusicImage;
            ImgMusic = ImageSource.FromStream(() => new MemoryStream(userMusic.MusicImage));
            TipoParse = new int[] { 1 };
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
            _whiteColor = "#ffffff";

            WhenNullSetMusicTimeMilliseconds();
        }
        public SearchMusicModel(UserAlbum userAlbum, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient, savedOnLocalDb)
        {
            SearchType = MusicSearchType.SearchSavedMusic;
            MusicName = userAlbum.Album;
            TypeIcon = Icon.ArrowDown;
            VideoId = userAlbum.VideoId;
            TipoParse = new int[] { 1 };
            IsSavedOnLocalDb = savedOnLocalDb;
            IsLoadded = true;
            TextColorMusic = "#121418";

            _collectionMusicOptionSize = 0;
            _musicSelectedColorPrimary = Color.FromHex("#d7dff6");
            _musicSelectedColorSecondary = Color.FromHex("#f5f7fa");
            _musicFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumFontAttr = FontAttributes.None;
            _musicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);

            WhenNullSetMusicTimeMilliseconds();
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
        public void UpdateIconMusicPlaying()
        {
            IconMusicStatus = _musicIsPlaying ? AppHelper.FaviconImageSource(Icon.PauseCircle, 35, Color.Black) : AppHelper.FaviconImageSource(Icon.PlayCircleO, 35, Color.Black);
            ImgStartDownloadIcon = _musicIsPlaying ? AppHelper.FaviconImageSource(Icon.PauseCircle, 35, Color.Coral) : null;
        }
        public void ReloadMusicUtilsAddAlbumIcon()
        {
            MusicDetailsAddAlbumFontAttr = FontAttributes.None;
            MusicDetailsAddAlbumIcon = AppHelper.FaviconImageSource(Icon.Plus, 22, Color.Green);
        }
        public void UpdMusicSelectedColor(bool isPlaying)
        {
            if (!isPlaying)
            {
                MusicSelectedColorPrimary = Color.FromHex("#d7dff6");
                MusicSelectedColorSecondary = Color.FromHex("#f5f7fa");
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
                TextColorMusic = "#121418";
                MusicFontAttr = FontAttributes.None;
            }
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
        private void Download_DownloadComplete(object sender, (bool, byte[], object) compressedMusic)
        {
            MusicAlbumPopupModel.MusicAlbumModel.IsAddAlbumMusicPlaylistVisible = true;
            MusicAlbumPopupModel.IsDownloadModelVisible = false;
            MusicAlbumPopupModel.IsSavedMusicDownloadModelVisible = false;
        }
        #endregion
    }
}
