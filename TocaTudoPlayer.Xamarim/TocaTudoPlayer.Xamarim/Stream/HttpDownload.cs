using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
//using YoutubeParse.ExplodeV2;
//using YoutubeParse.ExplodeV2.Videos.Streams;

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

        public WeakEventManager _downloadStarted;
        public WeakEventManager<(bool, byte[], object)> _downloadComplete;

        public HttpDownload()
        {
            _http = new HttpClientDownloader();
            _downloadStarted = new WeakEventManager();
            _downloadComplete = new WeakEventManager<(bool, byte[], object)>();
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
            _downloadStarted = new WeakEventManager();
            _downloadComplete = new WeakEventManager<(bool, byte[], object)>();
            _downloadMusicCompleteTextDesc = downloadMusicCompleteTextDesc;
            _percentDesc = AppResource.AlbumDownloadLabel;
            _isDownloading = false;
            _isDownloadEventEnabled = true;
            _ytClient = ytClient;
            _http.DownloadStarted += Http_DownloadStarted;
            _http.ProgressChanged += Http_ProgressChanged;
            _http.DownloadComplete += Http_DownloadComplete;
        }
        public event EventHandler DownloadStarted
        {
            add => _downloadStarted.AddEventHandler(value);
            remove => _downloadStarted.RemoveEventHandler(value);
        }
        public event EventHandler<(bool, byte[], object)> DownloadComplete
        {
            add => _downloadComplete.AddEventHandler(value);
            remove => _downloadComplete.RemoveEventHandler(value);
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
            if (!_isDownloadEventEnabled)
                return;

            await Task.Run(async () =>
            {
                try
                {
                    IsDownloadEventEnabled = false;
                    IsDownloading = true;
                    PercentDesc = AppResource.AlbumDownloadInitLabel;

                    StreamManifest streamMusicUrl = await _ytClient.Videos.Streams.GetManifestAsync(music.VideoId).AsTask();

                    AudioOnlyStreamInfo streamInfo = streamMusicUrl.GetAudioOnlyStreams()
                                                                   .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                                   .FirstOrDefault();

                    await Download(streamInfo.Url, music);

                    App.EventTracker.SendEvent("StartDownloadMusic", new Dictionary<string, string>()
                    {
                        { "VideoId", music.VideoId },
                    });
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("410") >= 0 || ex.Message.IndexOf("is not") >= 0)
                    {
                        RaiseAppErrorEvent(AppResource.MusicIsNotPlayable);
                        PercentDesc = AppResource.MusicCouldNotDownload;
                        IsDownloadEventEnabled = false;
                    }
                    else
                    {
                        RaiseAppErrorEvent(AppResource.MusicCouldNotDownloadTryAgain);
                        PercentDesc = AppResource.TryAgainLabel;
                        IsDownloadEventEnabled = true;
                    }

                    IsDownloading = false;
                    _downloadComplete.RaiseEvent(this, (false, null, _model), nameof(DownloadComplete));
                }
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

            if (_downloadStarted != null)
                _downloadStarted.RaiseEvent(this, null, nameof(DownloadStarted));
        }
        private void Http_DownloadComplete((bool, byte[]) compressedMusic)
        {
            PercentDesc = $"{AppResource.AlbumDownloadInProgressLabel}: 100%";
            Progress = 1;

            _downloadComplete.RaiseEvent(this, (compressedMusic.Item1, compressedMusic.Item2, _model), nameof(DownloadComplete));

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
