using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using YoutubeParse.ExplodeV2;
using YoutubeParse.ExplodeV2.Videos.Streams;

namespace TocaTudoPlayer.Xamarim
{
    public class HttpDownload : BaseViewModel, INotifyPropertyChanged
    {
        private HttpClientDownloader _http;
        private object _model;
        private int _percent;
        private string _percentDesc;
        private string _downloadMusicCompleteTextDesc;
        private float _progress;
        private bool _isDownloading;
        private bool _isDownloadEventEnabled;

        private readonly YoutubeClient _ytClient;

        public delegate void DownloadStartedHandler();
        public event DownloadStartedHandler DownloadStarted;

        public delegate void DownloadCompleteHandler((bool, byte[]) compressedMusic, object model);
        public event DownloadCompleteHandler DownloadComplete;

        public HttpDownload()
        {
            _http = new HttpClientDownloader();
            _percentDesc = AppResource.AlbumDownloadLabel;
            _isDownloading = false;
            _isDownloadEventEnabled = true;
            _http.DownloadStarted += Http_DownloadStarted;
            _http.ProgressChanged += Http_ProgressChanged;
            _http.DownloadComplete += Http_DownloadComplete;
        }
        public HttpDownload(string downloadMusicCompleteTextDesc, YoutubeClient ytClient)
        {
            _http = new HttpClientDownloader();
            _downloadMusicCompleteTextDesc = downloadMusicCompleteTextDesc;
            _percentDesc = AppResource.AlbumDownloadLabel;
            _isDownloading = false;
            _isDownloadEventEnabled = true;
            _ytClient = ytClient;
            _http.DownloadStarted += Http_DownloadStarted;
            _http.ProgressChanged += Http_ProgressChanged;
            _http.DownloadComplete += Http_DownloadComplete;
        }
        public int Percent
        {
            get { return _percent; }
            set
            {
                _percent = value;
                OnPropertyChanged(nameof(Percent));
            }
        }
        public string PercentDesc
        {
            get { return _percentDesc; }
            set
            {
                _percentDesc = value;
                OnPropertyChanged(nameof(PercentDesc));
            }
        }
        public float Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public bool IsDownloadEventEnabled
        {
            get { return _isDownloadEventEnabled; }
            set
            {
                _isDownloadEventEnabled = value;
                OnPropertyChanged(nameof(IsDownloadEventEnabled));
            }
        }
        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                _isDownloading = value;
                OnPropertyChanged(nameof(IsDownloading));
            }
        }
        public async Task StartDownloadMusic<TModel>(TModel music) where TModel : class, IVideoModel
        {
            await Task.Run(async () =>
            {
                IsDownloadEventEnabled = IsDownloadEventEnabled ? false : true;
                IsDownloading = true;
                PercentDesc = AppResource.AlbumDownloadInitLabel;

                StreamManifest streamMusicUrl = await _ytClient.Videos.Streams.GetManifestAsync(music.VideoId).AsTask();

                AudioOnlyStreamInfo streamInfo = streamMusicUrl.GetAudioOnlyStreams()
                                                               .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                               .FirstOrDefault();

                await Download(streamInfo.Url, music);
            }).ConfigureAwait(false);
        }
        public async Task Download<TModel>(string url, TModel album) where TModel : class, IVideoModel
        {
            _model = album;
            await _http.Download(url);
        }
        public static async Task<byte[]> Descompress(byte[] data)
        {
            return await HttpClientDownloader.GZipDescompress(data);
        }

        #region Metodos Privados
        private void Http_DownloadStarted()
        {
            PercentDesc = $"{AppResource.AlbumDownloadInProgressLabel}: 0%";

            if (DownloadStarted != null)
                DownloadStarted();
        }
        private void Http_DownloadComplete((bool, byte[]) compressedMusic)
        {
            PercentDesc = $"{AppResource.AlbumDownloadInProgressLabel}: 100%";
            Progress = 1;

            DownloadComplete(compressedMusic, _model);

            IsDownloading = false;
            PercentDesc = _downloadMusicCompleteTextDesc;

            _http.Dispose();
        }
        private void Http_ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            _percent = (int)progressPercentage.Value;

            if (_percent > 0)
            {
                Percent = _percent;
                PercentDesc = string.Concat(AppResource.AlbumDownloadInProgressLabel, ": ", _percent, "%");
            }

            Progress = (float)(progressPercentage.Value / 100);
        }
        #endregion
    }
}
