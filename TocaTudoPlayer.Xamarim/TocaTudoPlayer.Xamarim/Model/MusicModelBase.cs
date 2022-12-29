using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Forms;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicModelBase : NotifyPropertyChanged, ICommonMusicModel
    {
        private readonly CommonFormDownloadViewModel _formDownloadViewModel;
        private readonly ITocaTudoApi _tocaTudoApi;
        private MusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private ImageSource _imgMusicPlayingIcon;
        private IPCLUserMusicLogic _pclUserMusicLogic;
        private HttpDownload _download;
        private Action _downloadComplete;
        private bool _isLoadded;
        private bool _isBufferingMusic;
        private bool _isPlaying;
        private bool _isAnimated;
        private bool _iconMusicDownloadVisible;
        private bool _isSelected;
        private bool _isSavedOnLocalDb;
        private bool _isPlayingImgChanged;
        private string _textColorMusic;
        private string _musicImageUrl;
        private string _musicTime;
        private long _musicTimeTotalSeconds;
        public MusicModelBase()
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            IsSavedOnLocalDb = false;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel();
        }
        public MusicModelBase(ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _isLoadded = false;
            _tocaTudoApi = tocaTudoApi;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel();

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel();

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool hasAlbumSaved, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: hasAlbumSaved);

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(ApiSearchMusicModel apiSearchMusic, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _isLoadded = false;
            _tocaTudoApi = tocaTudoApi;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: apiSearchMusic.HasAlbum);

            MusicModel.MusicTimeTotalSeconds = apiSearchMusic.MusicTimeTotalSeconds;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(UserMusic userMusic, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel();

            MusicModel.MusicTimeTotalSeconds = userMusic.MusicTimeTotalSeconds;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(UserMusicAlbumSelect musicAlbumSelected, UserMusic userMusic, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: userMusic.HasAlbum, musicAlbumSelected);

            MusicModel.MusicTimeTotalSeconds = userMusic.MusicTimeTotalSeconds;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(ApiSearchMusicModel apiSearchMusic, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: apiSearchMusic.HasAlbum);

            MusicModel.MusicTimeTotalSeconds = apiSearchMusic.MusicTimeTotalSeconds;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(ApiSearchMusicModel apiSearchMusic, UserMusicAlbumSelect musicAlbumSelected, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: apiSearchMusic.HasAlbum, musicAlbumSelected);

            MusicModel.MusicTimeTotalSeconds = apiSearchMusic.MusicTimeTotalSeconds;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicModelBase(UserMusicAlbumSelect musicAlbumSelected, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool hasAlbumSaved, bool savedOnLocalDb)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = savedOnLocalDb;

            MusicModel = new MusicModel();
            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: hasAlbumSaved, musicAlbumSelected);

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.IsDownloadEventEnabled = !savedOnLocalDb;
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public short Id { get; set; }
        public int[] AlbumTypeParse { get; set; }
        public string VideoId { get; set; }
        public string MusicTime 
        { 
            get { return _musicTime; } 
            set 
            {
                _musicTime = value;
                OnPropertyChanged(nameof(MusicTime));
            }
        }
        public long MusicTimeTotalSeconds 
        { 
            get { return _musicTimeTotalSeconds; }
            set 
            {
                _musicTimeTotalSeconds = value;
            }
        }
        public string MusicImageUrl
        {
            get { return _musicImageUrl; }
            set
            {
                _musicImageUrl = value;
                OnPropertyChanged(nameof(MusicImageUrl));
            }
        }
        public byte[] ByteMusicImage { get; set; }
        public ImageSource ImgMusic { get; set; }
        public bool IsLoadded
        {
            get { return _isLoadded; }
            set
            {
                _isLoadded = value;
                OnPropertyChanged(nameof(IsLoadded));
            }
        }
        public bool IsActiveMusic { get; set; }
        public bool ShowMerchandisingAlert { get; set; }
        public bool IsSavedOnLocalDb
        {
            get { return _isSavedOnLocalDb; }
            set
            {
                _isSavedOnLocalDb = value;

                if (_download != null)
                    _download.IsDownloadEventEnabled = _isSavedOnLocalDb ? false : true;

                OnPropertyChanged(nameof(IsSavedOnLocalDb));
            }
        }
        public bool IsHistoryPlayedSavedOnLocalDb { get; set; }
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }
        public bool IsAnimated 
        {
            get { return _isAnimated; }
            set
            {
                _isAnimated = value;
                OnPropertyChanged(nameof(IsAnimated));
            }
        }
        public bool IsBufferingMusic
        {
            get { return _isBufferingMusic; }
            set
            {
                _isBufferingMusic = value;
                OnPropertyChanged(nameof(IsBufferingMusic));
            }
        }
        public bool IconMusicDownloadVisible
        {
            get { return _iconMusicDownloadVisible; }
            set
            {
                _iconMusicDownloadVisible = value;
                OnPropertyChanged(nameof(IconMusicDownloadVisible));
            }
        }
        public string MusicName { get; set; }
        public string TextColorMusic
        {
            get { return _textColorMusic; }
            set
            {
                _textColorMusic = value;
                OnPropertyChanged(nameof(TextColorMusic));
            }
        }
        public MusicSearchType SearchType { get; set; }
        public ImageSource ImgMusicPlayingIcon
        {
            get { return _imgMusicPlayingIcon; }
            set
            {
                _imgMusicPlayingIcon = value;
                OnPropertyChanged(nameof(ImgMusicPlayingIcon));
            }
        }
        public MusicAlbumDialogDataModel MusicAlbumPopupModel { get; set; }
        public MusicModel MusicModel { get; set; }
        public HttpDownload Download
        {
            get { return _download; }
            set
            {
                _download = value;
                OnPropertyChanged(nameof(Download));
            }
        }
        public void ReloadMusicPlayingIcon()
        {
            ImgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            IsBufferingMusic = false;
            _isPlaying = false;
            _isPlayingImgChanged = false;
        }
        public void UpdateMusicPlayingIcon()
        {
            if (_isPlayingImgChanged)
            {
                ImgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);

                _isPlayingImgChanged = false;
                _isPlaying = false;
            }
            else
            {
                ImgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Stop, 12, Color.Black);

                _isPlayingImgChanged = true;
                _isPlaying = true;
            }
        }
        public void SetDownload(IPCLUserMusicLogic pclUserMusicLogic)
        {
            _pclUserMusicLogic = pclUserMusicLogic;
        }
        public void SetDownloadingMode(bool force = true)
        {
            if (force)
            {
                MusicAlbumPopupModel.SetDownloadingMode();
            }
            else if (Download.IsDownloading)
            {
                MusicAlbumPopupModel.SetDownloadingMode();
            }
        }
        public virtual async Task<DownloadQueueStatus> StartDownloadMusic(MusicPlayedHistoryViewModel musicPlayedHistoryViewModel)
        {
            MusicModel musicModel = new MusicModel()
            {
                VideoId = this.VideoId,
                MusicName = this.MusicName,
                MusicTime = this.MusicTime,
                MusicImage = this.ByteMusicImage,
                MusicTimeTotalSeconds = this.MusicTimeTotalSeconds
            };

            await musicModel.LoadMusicImageInfo(MusicImageUrl);

            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;

            DownloadQueueStatus downloadEnqueued = _formDownloadViewModel.SetDownloadInProgress(new DownloadMusicModel(this, _download));
            if (downloadEnqueued != DownloadQueueStatus.MusicQueued)
                return downloadEnqueued;

            await _download.StartDownloadMusic(musicModel);
            return downloadEnqueued;
        }
        public async Task<DownloadQueueStatus> StartDownloadMusic(MusicPlayedHistoryViewModel musicPlayedHistoryViewModel, Action downloadComplete)
        {
            _downloadComplete = downloadComplete;
            return await StartDownloadMusic(musicPlayedHistoryViewModel);
        }
        public async Task<DownloadQueueStatus> StartDownloadMusic(byte[] imgMusic, MusicPlayedHistoryViewModel musicPlayedHistoryViewModel)
        {
            MusicModel musicModel = new MusicModel()
            {
                VideoId = this.VideoId,
                MusicName = this.MusicName,
                MusicTime = this.MusicTime,
                MusicImage = this.ByteMusicImage,
                MusicTimeTotalSeconds = this.MusicTimeTotalSeconds
            };

            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;

            DownloadQueueStatus downloadEnqueued = _formDownloadViewModel.SetDownloadInProgress(new DownloadMusicModel(this, _download));
            if (downloadEnqueued != DownloadQueueStatus.MusicQueued)
                return downloadEnqueued;

            await _download.StartDownloadMusic(musicModel);
            return downloadEnqueued;            
        }
        protected void WhenNullSetMusicTimeMilliseconds()
        {
            if (MusicTimeTotalSeconds == 0 && !string.IsNullOrEmpty(MusicTime))
                MusicTimeTotalSeconds = AppHelper.HourOrSecondsToMilliseconds(MusicTime);
        }
        private async void DownloadMusicComplete(object sender, (bool, byte[], object) tpMusic)
        {
            if (tpMusic.Item2 == null || tpMusic.Item3 == null)
                return;

            bool musicSaved = false;

            try
            {
                musicSaved = await _pclUserMusicLogic.SaveOrUpdateMusicOnLocalDb((MusicModel)tpMusic.Item3, tpMusic);

                await App.Services.GetRequiredService<MusicPlayedHistoryViewModel>().LoadPlayedHistory();
                await App.Services.GetRequiredService<SavedMusicPageViewModel>().MusicPlaylistSearchFromDb();
            }
            catch
            {
            }

            if (musicSaved)
            {
                IsSavedOnLocalDb = true;
            }

            _downloadComplete?.Invoke();

            MusicAlbumPopupModel.IsDownloadModelVisible = false;
            Download.IsDownloadEventEnabled = false;
        }
    }
}
