using System.IO;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public abstract class MusicBaseViewModel : BaseViewModel, ICommonMusicModel
    {
        private readonly ICommonFormDownloadViewModel _formDownloadViewModel;
        private readonly ITocaTudoApi _tocaTudoApi;
        private ImageSource _imgMusicPlayingIcon;
        private IPCLUserMusicLogic _pclUserMusicLogic;
        private HttpDownload _download;
        private bool _isLoadded;
        private bool _isBufferingMusic;
        private bool _isPlaying;
        private bool _iconMusicDownloadVisible;
        private bool _isSelected;
        private bool _isSavedOnLocalDb;
        private bool _isPlayingImgChanged;
        public MusicBaseViewModel(ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _isLoadded = false;
            _tocaTudoApi = tocaTudoApi;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = false;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public MusicBaseViewModel(ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
        {
            _imgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _isLoadded = false;
            ShowMerchandisingAlert = true;
            IsHistoryPlayedSavedOnLocalDb = false;
            _isPlayingImgChanged = false;
            _isSavedOnLocalDb = false;

            _download = new HttpDownload(AppResource.MusicDownloadedLabel, ytClient);
            _download.DownloadComplete += DownloadMusicComplete;
        }
        public short Id { get; set; }
        public int[] AlbumTypeParse { get; set; }
        public string VideoId { get; set; }
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
            _isPlayingImgChanged = false;
        }
        public void UpdateMusicPlayingIcon()
        {
            if (_isPlayingImgChanged)
            {
                ImgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Play, 12, Color.Black);
                _isPlayingImgChanged = false;
            }
            else
            {
                ImgMusicPlayingIcon = AppHelper.FaviconImageSource(Icon.Stop, 12, Color.Black);
                _isPlayingImgChanged = true;
            }
        }
        public void SetDownload(IPCLUserMusicLogic pclUserMusicLogic)
        {
            _pclUserMusicLogic = pclUserMusicLogic;
        }
        public virtual async Task StartDownloadMusic()
        {
            MusicModel musicModel = new MusicModel()
            {
                VideoId = this.VideoId,
                MusicName = this.MusicName
            };

            await musicModel.LoadMusicImageInfo(_tocaTudoApi);

            _formDownloadViewModel.SetDownloadInProgress(VideoId, MusicName, ImageSource.FromStream(() => new MemoryStream(musicModel.MusicImage)), _download);
            await _download.StartDownloadMusic(musicModel);
        }
        public async Task StartDownloadMusic(byte[] imgMusic)
        {
            MusicModel musicModel = new MusicModel()
            {
                VideoId = this.VideoId,
                MusicName = this.MusicName,
                MusicImage = imgMusic
            };

            _formDownloadViewModel.SetDownloadInProgress(VideoId, MusicName, ImageSource.FromStream(() => new MemoryStream(imgMusic)), _download);
            await _download.StartDownloadMusic(musicModel);
        }
        private async void DownloadMusicComplete((bool, byte[]) tpMusic, object music)
        {
            await _pclUserMusicLogic.LoadDb();

            bool musicSaved = await _pclUserMusicLogic.SaveMusicOnLocalDb((MusicModel)music, tpMusic);

            //IconMusicStatusVisible = false;
            //IconMusicDownloadVisible = false;
            //IsDownloadMusicVisible = false;

            Download.IsDownloadEventEnabled = false;

            if (musicSaved)
            {
                IsSavedOnLocalDb = true;
                await _formDownloadViewModel.UpdateDownloadQueue();
            }

            _pclUserMusicLogic.UnLoadDb();
        }
    }
}
