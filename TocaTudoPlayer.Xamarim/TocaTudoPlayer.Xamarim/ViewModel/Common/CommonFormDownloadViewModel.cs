using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonFormDownloadViewModel : BaseViewModel, ICommonFormDownloadViewModel
    {
        private int _totalInProgress;
        private bool _isFormDownloadVisible;
        private string _musicName;
        private HttpDownload _download;
        private ImageSource _imgMusic;
        private Queue<(string VideoId, string Music, ImageSource Img, HttpDownload Download)> _lstDownload;
        public CommonFormDownloadViewModel()
        {
            _isFormDownloadVisible = false;
            _lstDownload = new Queue<(string VideoId, string Music, ImageSource Img, HttpDownload Download)>();
        }
        public int TotalInProgress
        {
            get { return _totalInProgress; }
            set
            {
                _totalInProgress = value;
                OnPropertyChanged(nameof(TotalInProgress));
            }
        }
        public bool IsFormDownloadVisible
        {
            get { return _isFormDownloadVisible; }
            set
            {
                _isFormDownloadVisible = value;
                OnPropertyChanged(nameof(IsFormDownloadVisible));
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
        public ImageSource ImgMusic
        {
            get { return _imgMusic; }
            set
            {
                _imgMusic = value;
                OnPropertyChanged(nameof(ImgMusic));
            }
        }
        public HttpDownload Download
        {
            get { return _download; }
            set
            {
                _download = value;
                OnPropertyChanged(nameof(Download));
            }
        }
        public void SetDownloadInProgress(string videoId, string musicName, ImageSource imgMusic, HttpDownload download)
        {
            bool musicExists = _lstDownload.Any(music => string.Equals(music.VideoId, videoId));
            if (musicExists)
                return;

            _lstDownload.Enqueue((videoId, musicName, imgMusic, download));

            BindDownloadInProgressFromQueue();
        }
        public async Task UpdateDownloadQueue()
        {
            _lstDownload.Dequeue();

            if (_lstDownload.Count == 0)
            {
                await Task.Delay(1000);
                IsFormDownloadVisible = false;

                return;
            }

            await Task.Delay(500);

            BindDownloadInProgressFromQueue();
        }

        #region Private Methods
        private void BindDownloadInProgressFromQueue()
        {
            (string VideoId, string Music, ImageSource Img, HttpDownload Download) tpDownload = _lstDownload.Peek();

            TotalInProgress = _lstDownload.Count;
            MusicName = tpDownload.Music;
            ImgMusic = tpDownload.Img;
            Download = tpDownload.Download;
            IsFormDownloadVisible = true;
        }
        #endregion
    }
}
