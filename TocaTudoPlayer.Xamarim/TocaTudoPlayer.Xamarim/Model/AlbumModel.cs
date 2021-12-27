using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumModel : BaseViewModel, IVideoModel
    {
        private ObservableCollection<PlaylistItem> _playlist;
        private ImageSource _imgAlbum;
        public AlbumModel()
        {
            _playlist = new ObservableCollection<PlaylistItem>();
        }
        public string Album { get; set; }
        public string VideoId { get; set; }
        public string UAlbumlId { get; set; }
        public AlbumParseType ParseType { get; set; }
        public ImageSource ImgAlbum 
        {
            get { return _imgAlbum; }
            set { _imgAlbum = value; }
        }
        public byte[] ByteImgAlbum { get; set; }       
        public ObservableCollection<PlaylistItem> Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                OnPropertyChanged(nameof(Playlist));
            }
        }
        public void GenerateImageMusic(byte[] imgData) 
        {
            _imgAlbum = ImageSource.FromStream(() => new MemoryStream(imgData));
        }
        public async Task<string> GetFileNameLocalPath()
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            bool hasLocalPermissionToStorage = (statusRead == PermissionStatus.Granted && statusWrite == PermissionStatus.Granted);

            if (!hasLocalPermissionToStorage)
                return string.Empty;

            if (string.IsNullOrEmpty(VideoId))
                return string.Empty;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{VideoId}.txt");
            return path;
        }
        public async Task LoadMusicImageInfo(ITocaTudoApi tocaTudoApi, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(VideoId))
                return;

            ByteImgAlbum = await tocaTudoApi.PlayerImageEndpoint(VideoId);
        }
    }
    public class PlaylistItem : BaseViewModel
    {
        private string _downloadMusicStatusText;
        private string _imgLogo;
        private string _imgDownloadOptions;
        private string _imgStartDownloadIcon;
        private Color _musicSelectedColorPrimary;
        private Color _musicSelectedColorSecondary;
        private FontAttributes _musicPlayingFontAttr;
        private string _textColorMusicPlaying;
        private bool _showHideDownloadIcon;
        private bool _showHideDownloadMusicOptions;
        private bool _showDownloadMusicStatusProgress;
        private bool _downloadMusicOptionsIconCliked;
        private bool _isPlaying;
        private bool _isDownloadMusicStarted;
        public PlaylistItem()
        {
            _downloadMusicStatusText = "iniciar...";
            _imgLogo = Icon.Play;
            _imgStartDownloadIcon = "showDownloadIcon.png";
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicPlayingFontAttr = FontAttributes.None;
            _textColorMusicPlaying = "#374149";
            _isPlaying = false;
            _isDownloadMusicStarted = false;
            _showHideDownloadIcon = false;
            _showHideDownloadMusicOptions = false;
            _showDownloadMusicStatusProgress = false;
            _downloadMusicOptionsIconCliked = false;
        }
        public PlaylistItem(ApiPlaylist apiPlaylist, short number)
        {
            Id = apiPlaylist.Id;
            Number = number;
            NomeMusica = apiPlaylist.NomeMusica;
            TempoSegundos = apiPlaylist.TempoSegundos;
            TempoSegundosInicio = apiPlaylist.TempoSegundosInicio;
            TempoSegundosFim = apiPlaylist.TempoSegundosFim;
            TempoDesc = apiPlaylist.TempoDesc;
            _downloadMusicStatusText = "iniciar...";
            _imgLogo = Icon.Play;
            _imgStartDownloadIcon = "showDownloadIcon.png";
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicPlayingFontAttr = FontAttributes.None;
            _textColorMusicPlaying = "#374149";
            _isPlaying = false;
            _isDownloadMusicStarted = false;
            _showHideDownloadIcon = false;
            _showHideDownloadMusicOptions = false;
            _showDownloadMusicStatusProgress = false;
            _downloadMusicOptionsIconCliked = false;
        }
        public short Id { get; set; }
        public short Number { get; set; }
        public string NomeMusica { get; set; }
        public int TempoSegundos { get; set; }
        public int TempoSegundosInicio { get; set; }
        public int TempoSegundosFim { get; set; }
        public string TempoDesc { get; set; }
        public bool IsPaused { get; set; }
        public string DownloadMusicStatusText
        {
            get { return _downloadMusicStatusText; }
            set
            {
                _downloadMusicStatusText = value;
                OnPropertyChanged(nameof(DownloadMusicStatusText));
            }
        }
        public bool IsDownloadMusicStarted
        {
            get { return _isDownloadMusicStarted; }
            set
            {
                _isDownloadMusicStarted = value;
                UpdImgDownloadMusic();
            }
        }
        public bool ShowHideDownloadIcon
        {
            get { return _showHideDownloadIcon; }
            set
            {
                _showHideDownloadIcon = value;
                OnPropertyChanged(nameof(ShowHideDownloadIcon));
            }
        }
        public bool ShowHideDownloadMusicOptions
        {
            get { return _showHideDownloadMusicOptions; }
            set
            {
                _showHideDownloadMusicOptions = value;
                _downloadMusicOptionsIconCliked = _showHideDownloadMusicOptions;
                OnPropertyChanged(nameof(ShowHideDownloadMusicOptions));
                UpdImgDownloadOptions();
            }
        }
        public bool ShowDownloadMusicStatusProgress
        {
            get { return _showDownloadMusicStatusProgress; }
            set
            {
                _showDownloadMusicStatusProgress = value;
                OnPropertyChanged(nameof(ShowDownloadMusicStatusProgress));
            }
        }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                UpdImgLogo();
                UpdMusicSelectedColor();
                UpdFontMusicPlaying();
                UpdTextColorMusicPlaying();

                OnPropertyChanged(nameof(IsPlaying));
            }
        }
        public string ImgLogo
        {
            get { return _imgLogo; }
            private set
            {
                _imgLogo = value;
                OnPropertyChanged(nameof(ImgLogo));
            }
        }
        public string ImgDownloadOptions
        {
            get { return _imgDownloadOptions; }
            private set
            {
                _imgDownloadOptions = value;
                OnPropertyChanged(nameof(ImgDownloadOptions));
            }
        }
        public string ImgStartDownloadIcon
        {
            get { return _imgStartDownloadIcon; }
            private set
            {
                _imgStartDownloadIcon = value;
                OnPropertyChanged(nameof(ImgStartDownloadIcon));
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
        public FontAttributes MusicPlayingFontAttr
        {
            get { return _musicPlayingFontAttr; }
            private set
            {
                _musicPlayingFontAttr = value;
                OnPropertyChanged(nameof(MusicPlayingFontAttr));
            }
        }
        public string TextColorMusicPlaying
        {
            get { return _textColorMusicPlaying; }
            private set
            {
                _textColorMusicPlaying = value;
                OnPropertyChanged(nameof(TextColorMusicPlaying));
            }
        }
        private void UpdMusicSelectedColor() 
        {
            if (IsPlaying)
            {
                MusicSelectedColorPrimary = Color.FromHex("#4987E5");
                MusicSelectedColorSecondary = Color.FromHex("#7C97E3");
            }
            else
            {
                MusicSelectedColorPrimary = Color.FromHex("#F7F9FC");
                MusicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            }
        }
        private void UpdImgLogo() => ImgLogo = IsPlaying ? Icon.Stop : Icon.Play;
        private void UpdImgDownloadOptions() => ImgDownloadOptions = !_downloadMusicOptionsIconCliked ? "downloadIcon.png" : "downloadIconClicked.png";
        private void UpdImgDownloadMusic()
        {
            ImgStartDownloadIcon = !_isDownloadMusicStarted ? "showDownloadIcon.png" : "showDownloadIconClicked.png";
            DownloadMusicStatusText = "baixando: 0%";
            ShowDownloadMusicStatusProgress = true;
        }
        private void UpdFontMusicPlaying() => MusicPlayingFontAttr = IsPlaying ? FontAttributes.Bold : FontAttributes.None;
        private void UpdTextColorMusicPlaying() => TextColorMusicPlaying = IsPlaying ? "White" : "#374149";
    }
}
