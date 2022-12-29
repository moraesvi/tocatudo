using AngleSharp.Io;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonFormDownloadViewModel : BaseViewModel
    {
        private const int FORM_DOWNLOAD_SIZE = 40;
        private const int DOWNLOAD_MAX_QUEUE_COUNT = 5;

        private float _formDownloadSize;
        private bool _isFormDownloadVisible;
        private bool _albumDownloadModeIsVisible;
        private Queue<DownloadMusicModel> _lstDownload;
        private ObservableCollection<DownloadMusicModel> _downloadQueue;
        public CommonFormDownloadViewModel()
        {
            _lstDownload = new Queue<DownloadMusicModel>();
            _downloadQueue = new ObservableCollection<DownloadMusicModel>();
        }
        public float FormDownloadSize
        {
            get { return _formDownloadSize; }
            set
            {
                _formDownloadSize = value;
                OnPropertyChanged(nameof(FormDownloadSize));
            }
        }
        public bool IsFormDownloadVisible
        {
            get { return _isFormDownloadVisible; }
            set
            {
                _isFormDownloadVisible = value;

                float countFormSize = _downloadQueue.Count >= 4 ? 4 : _downloadQueue.Count;

                FormDownloadSize = _isFormDownloadVisible ? FORM_DOWNLOAD_SIZE * countFormSize : 0;

                OnPropertyChanged(nameof(IsFormDownloadVisible));
            }
        }
        public bool AlbumDownloadModeIsVisible
        {
            get { return _albumDownloadModeIsVisible; }
            set
            {
                _albumDownloadModeIsVisible = value;
                OnPropertyChanged(nameof(AlbumDownloadModeIsVisible));
            }
        }
        public ObservableCollection<DownloadMusicModel> DownloadQueue
        {
            get { return _downloadQueue; }
            set
            {
                _downloadQueue = value;
                OnPropertyChanged(nameof(DownloadQueue));
            }
        }
        public DownloadQueueStatus SetDownloadInProgress(DownloadMusicModel downloadMusicModel)
        {
            if (_lstDownload.Count >= DOWNLOAD_MAX_QUEUE_COUNT)
                return DownloadQueueStatus.AchievedMaxQueue;

            _lstDownload.Enqueue(downloadMusicModel);
            _downloadQueue.Add(downloadMusicModel);

            IsFormDownloadVisible = true;

            downloadMusicModel.Download.DownloadComplete += (s, e) =>
            {
                UpdateDownloadQueue((MusicModel)e.Item3);
            };

            return DownloadQueueStatus.MusicQueued;
        }

        #region Private Methods
        private void UpdateDownloadQueue(MusicModel musicModel)
        {
            if (_lstDownload.Count > 0)
            {
                _lstDownload.Dequeue();
            }

            DownloadMusicModel itemToRemove = _downloadQueue.Where(d => string.Equals(d.VideoId, musicModel.VideoId))
                                                            .FirstOrDefault();
            _downloadQueue.Remove(itemToRemove);

            DownloadMusicModel[] downloadedMusics = _lstDownload.Where(d => !d.Download.IsDownloading)
                                                                .ToArray() ?? new DownloadMusicModel[] { };
            DownloadMusicModel[] downloadedQueueMusics = _downloadQueue.Where(d => !d.Download.IsDownloading)
                                                                       .ToArray() ?? new DownloadMusicModel[] { };

            if (downloadedMusics.Count() > 0)
            {
                _lstDownload = new Queue<DownloadMusicModel>(downloadedMusics);
            }
            if (downloadedQueueMusics.Count() > 0)
            {
                foreach (DownloadMusicModel item in downloadedQueueMusics)
                    _downloadQueue.Remove(item);
            }

            if (_downloadQueue.Count() == 0)
                _lstDownload.Clear();


            IsFormDownloadVisible = _downloadQueue.Count > 0;
        }
        #endregion
    }
    public class DownloadMusicModel : BaseViewModel
    {
        private string _musicName;
        private HttpDownload _download;
        private ImageSource _imgMusic;
        public DownloadMusicModel(ICommonMusicModel musicModel, HttpDownload download)
        {
            VideoId = musicModel.VideoId;
            MusicName = musicModel.MusicName;
            ImgMusic = musicModel.ImgMusic;
            Download = download;
        }
        public string VideoId { get; set; }
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
    }
}
