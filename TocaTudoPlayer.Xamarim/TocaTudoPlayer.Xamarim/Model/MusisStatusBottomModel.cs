using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Interface;
using TocaTudoPlayer.Xamarim.ViewModel.CustomView;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicStatusBottomModel : BaseViewModel, INotifyPropertyChanged
    {
        private bool _musicStreamComplete;
        private bool _bottomStatusIsVisible;
        private bool _bottomPlayerLoadingIsVisible;
        private bool _bottomPlayerIsVisible;
        private string _albumName;
        private string _musicName;
        private string _musicTotalTimeDesc;
        private string _musicPartTimeDesc;
        private float _musicStreamProgress;
        private ImageSource _iconButtonMusic;
        private ImageSource _musicImage;
        private ITocaTudoApi _tocaTudoApi;
        private MusicBottomPlayerViewModelBase _vm;
        public MusicStatusBottomModel(ITocaTudoApi tocaTudoApi, MusicBottomPlayerViewModelBase vm)
        {
            _tocaTudoApi = tocaTudoApi;
            _musicStreamComplete = false;
            _vm = vm;

            _vm.MusicIsPlayingEvent += VM_MusicIsPlayingEvent;
        }
        public MusicStatusBottomModel(ITocaTudoApi tocaTudoApi, IMusicBottomPlayerViewModel vm)
        {
            _tocaTudoApi = tocaTudoApi;
            _musicStreamComplete = false;
            //_vm = vm;

            //_vm.MusicIsPlayingEvent += VM_MusicIsPlayingEvent;
        }
        public MusicStatusBottomModel(ITocaTudoApi tocaTudoApi, IMusicBottomAlbumPlayerViewModel vm)
        {
            _tocaTudoApi = tocaTudoApi;
            _musicStreamComplete = false;
            //_vm = vm;

            //_vm.MusicIsPlayingEvent += VM_MusicIsPlayingEvent;
        }
        public bool BottomStatusIsVisible
        {
            get { return _bottomStatusIsVisible; }
            set
            {
                _bottomStatusIsVisible = value;
                OnPropertyChanged(nameof(BottomStatusIsVisible));
            }
        }
        public bool BottomPlayerIsVisible
        {
            get { return _bottomPlayerIsVisible; }
            set
            {
                _bottomPlayerIsVisible = value;

                if (_bottomPlayerIsVisible)
                    BottomPlayerLoadingIsVisible = false;
                else
                    BottomPlayerLoadingIsVisible = true;

                OnPropertyChanged(nameof(BottomPlayerIsVisible));
            }
        }
        public bool BottomPlayerLoadingIsVisible
        {
            get { return _bottomPlayerLoadingIsVisible; }
            set
            {
                _bottomPlayerLoadingIsVisible = value;
                OnPropertyChanged(nameof(BottomPlayerLoadingIsVisible));
            }
        }
        public string MusicTotalTimeDesc
        {
            get { return _musicTotalTimeDesc; }
            set
            {
                _musicTotalTimeDesc = value;
                OnPropertyChanged(nameof(MusicTotalTimeDesc));
            }
        }
        public string MusicPartTimeDesc
        {
            get { return _musicPartTimeDesc; }
            set
            {
                _musicPartTimeDesc = value;
                OnPropertyChanged(nameof(MusicPartTimeDesc));
            }
        }
        public string AlbumName
        {
            get { return _albumName; }
            set
            {
                _albumName = value;
                OnPropertyChanged(nameof(AlbumName));
            }
        }
        public string MusicName
        {
            get { return _musicName; }
            set
            {
                _musicName = value;
                OnPropertyChanged(nameof(MusicName));
            }
        }
        public ImageSource MusicImage
        {
            get { return _musicImage; }
            set
            {
                _musicImage = value;
                OnPropertyChanged(nameof(MusicImage));
            }
        }
        public byte[] ByteMusicImage { get; set; }
        public float MusicStreamProgress
        {
            get { return _musicStreamProgress; }
            set
            {
                _musicStreamProgress = value;
                _musicStreamComplete = _musicStreamProgress >= 1;

                OnPropertyChanged(nameof(MusicStreamProgress));
            }
        }
        public bool MusicStreamComplete
        {
            get { return _musicStreamComplete; }
        }
        public ImageSource IconButtonMusic
        {
            get { return _iconButtonMusic; }
            set
            {
                _iconButtonMusic = value;
                OnPropertyChanged(nameof(IconButtonMusic));
            }
        }
        public void Init()
        {
            _albumName = string.Empty;
            _musicName = string.Empty;
            _musicTotalTimeDesc = string.Empty;
            _musicPartTimeDesc = "00:00";
            _musicStreamProgress = 0;
            _musicStreamComplete = false;
            _bottomPlayerIsVisible = false;
            _bottomPlayerLoadingIsVisible = false;
        }
        private void VM_MusicIsPlayingEvent(bool playing)
        {
            IconButtonMusic = !playing ? AppHelper.FaviconImageSource(Icon.PlayCircle, 35, Color.Black) : AppHelper.FaviconImageSource(Icon.PauseCircle, 35, Color.Black);
        }
        public async Task LoadMusicImageInfo(string videoId, CancellationToken cancellationToken = default)
        {
            BottomPlayerIsVisible = false;

            if (cancellationToken.IsCancellationRequested)
                return;

            byte[] mData = await _tocaTudoApi.PlayerImageEndpoint(videoId);

            if (mData != null)
            {
                ByteMusicImage = mData;
                MusicImage = ImageSource.FromStream(() => new MemoryStream(mData));
            }
        }
        public void LoadMusicImageInfo(byte[] byteImg)
        {
            ByteMusicImage = byteImg;
            MusicImage = ImageSource.FromStream(() => new MemoryStream(byteImg));
        }
    }
}
