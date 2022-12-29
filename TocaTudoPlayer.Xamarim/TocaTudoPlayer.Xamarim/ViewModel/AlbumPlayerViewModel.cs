using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TocaTudoPlayer.Xamarim.Resources;
using TocaTudoPlayer.Xamarim.ViewModel;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumPlayerViewModel : BaseViewModel
    {
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserAlbumLogic _pclUserAlbumLogic;
        private readonly CommonMusicPlayerViewModel _musicPlayerViewModel;
        private readonly AlbumPlayedHistoryViewModel _albumPlayedHistoryViewModel;
        private readonly MusicBottomAlbumPlayerViewModel _bottomPlayerViewModel;
        private readonly CommonPageViewModel _commonPageViewModel;
        private bool _playerIsFullLoaded;
        private bool _showHideDownloadMusicOptions;
        private bool _isDownloadComplete;
        private bool _showDownloadMusicStatusProgress;
        private bool _showDownloadingInfo;
        private bool _showPlayingOfflineInfo;
        private bool _dbAccesEnabled;
        private bool _isSearching;
        private bool _isAlbumHistorySaved;
        private float _mStreamProgress;
        private string _musicSelectedName;
        private int _albumFrameSize;
        private AlbumModel _album;
        private AlbumModel _albumModel;
        private readonly YoutubeClient _ytClient;
        private HttpDownload _download;
        private string _imgStartDownloadIcon;

        private readonly WeakEventManager _playerReady;

        private const string USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY = "ap_history.json";
        public AlbumPlayerViewModel(IPCLStorageDb pclStorageDb, IPCLUserAlbumLogic pclUserAlbumLogic, ITocaTudoApi tocaTudoApi, CommonMusicPlayerViewModel musicPlayerViewModel, MusicBottomAlbumPlayerViewModel bottomPlayerViewModel, AlbumPlayedHistoryViewModel searchHistoryAlbumViewModel, CommonPageViewModel commonPageViewModel, YoutubeClient ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _audioPlayer = DependencyService.Get<IAudio>();
            _ytClient = ytClient;
            _pclStorageDb = pclStorageDb;
            _pclUserAlbumLogic = pclUserAlbumLogic;
            _albumFrameSize = 0;
            _album = new AlbumModel();
            _albumModel = new AlbumModel();
            _musicPlayerViewModel = musicPlayerViewModel;
            _bottomPlayerViewModel = bottomPlayerViewModel;
            _albumPlayedHistoryViewModel = searchHistoryAlbumViewModel;
            _commonPageViewModel = commonPageViewModel;
            DownloadMusicOptionsCommand = new ShowHideDownloadMusicOptionsCommand(this);
            StartDownloadMusicCommand = new PlayerStartDownloadMusicCommand(this);

            _playerReady = new WeakEventManager();

            _audioPlayer.PlayerReady -= AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering -= AudioPlayer_PlayerReadyBuffering;
            _audioPlayer.PlayerAlbumPlaylistChanged -= AudioPlayer_PlayerPlaylistChanged;
            _audioPlayer.PlayerException -= AudioPlayer_PlayerException;

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering += AudioPlayer_PlayerReadyBuffering;
            _audioPlayer.PlayerAlbumPlaylistChanged += AudioPlayer_PlayerPlaylistChanged;
            _audioPlayer.PlayerException += AudioPlayer_PlayerException;
        }
        public event EventHandler PlayerReady
        {
            add => _playerReady.AddEventHandler(value);
            remove => _playerReady.RemoveEventHandler(value);
        }
        public AlbumModel Album
        {
            get { return _album; }
            set { _album = value; }
        }
        public int AlbumFrameSize
        {
            get { return _albumFrameSize; }
            set
            {
                _albumFrameSize = value;
                OnPropertyChanged(nameof(AlbumFrameSize));
            }
        }
        public CommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public CommonMusicPlayerViewModel MusicPlayerViewModel
        {
            get { return _musicPlayerViewModel; }
        }
        public MusicBottomAlbumPlayerViewModel BottomPlayerViewModel
        {
            get { return _bottomPlayerViewModel; }
        }
        public AlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel
        {
            get { return _albumPlayedHistoryViewModel; }
        }
        public string MusicSelectedName
        {
            get { return _musicSelectedName; }
            set
            {
                _musicSelectedName = value;
                OnPropertyChanged(nameof(MusicSelectedName));
            }
        }
        public bool PlayerLoaded
        {
            get { return _playerIsFullLoaded; }
            set
            {
                _playerIsFullLoaded = value;
                OnPropertyChanged(nameof(PlayerLoaded));
            }
        }
        public bool ShowHideDownloadMusicOptions
        {
            get { return _showHideDownloadMusicOptions; }
            set
            {
                _showHideDownloadMusicOptions = value;
                OnPropertyChanged(nameof(ShowHideDownloadMusicOptions));
            }
        }
        public bool ShowDownloadingInfo
        {
            get { return _showDownloadingInfo; }
            set
            {
                _showDownloadingInfo = value;
                OnPropertyChanged(nameof(ShowDownloadingInfo));
            }
        }
        public bool ShowPlayingOfflineInfo
        {
            get { return _showPlayingOfflineInfo; }
            set
            {
                _showPlayingOfflineInfo = value;
                OnPropertyChanged(nameof(ShowPlayingOfflineInfo));
            }
        }
        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                _isSearching = value;
                OnPropertyChanged(nameof(IsSearching));
            }
        }
        public IAsyncCommand<PlaylistItem> PlayCommand => PlayEventCommand();
        public ICommand PauseCommand { get; set; }
        public ICommand ShowHideDownloadIconCommand { get; set; }
        public ICommand DownloadMusicOptionsCommand { get; set; }
        public ICommand StartDownloadMusicCommand { get; set; }
        public string MusicSearchedName { get; set; }
        public float MusicStreamProgress
        {
            get { return _mStreamProgress; }
            set
            {
                _mStreamProgress = value;
                OnPropertyChanged(nameof(MusicStreamProgress));
            }
        }
        public string ImgStartDownloadIcon
        {
            get { return _imgStartDownloadIcon; }
            set
            {
                _imgStartDownloadIcon = value;
                OnPropertyChanged(nameof(ImgStartDownloadIcon));
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
        public HttpDownload Download
        {
            get { return _download; }
            set
            {
                _download = value;
                OnPropertyChanged(nameof(Download));
            }
        }
        public void DbAccessEnabled(bool enabled)
        {
            _dbAccesEnabled = enabled;
        }
        public async Task GetAlbum(AlbumParseType tpParse, string videoId)
        {
            AlbumFrameSize = 0;
            PlayerLoaded = false;
            IsSearching = false;
            ShowDownloadMusicStatusProgress = false;
            ShowDownloadingInfo = false;
            ShowPlayingOfflineInfo = false;

            _isDownloadComplete = false;
            _isAlbumHistorySaved = false;

            IsSearching = true;

            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            ImgStartDownloadIcon = "showDownloadIcon.png";

            Download = new HttpDownload(AppResource.AlbumDownloadedLabel, _ytClient);

            Download.DownloadComplete += DownloadAlbumComplete;
            Download.DownloadStarted += DownloadAlbumStarted;

            if (tpParse == AlbumParseType.NaoDefinido || string.IsNullOrWhiteSpace(videoId))
                return;

            _bottomPlayerViewModel.Init(MusicPlayerViewModel);

            CommonMusicPlayerManager.StopAllMusicBottomPlayers();

            if (_dbAccesEnabled)
            {
                await _pclUserAlbumLogic.LoadDb();

                bool albumInDb = AlbumInDb(videoId);

                if (!albumInDb)
                    await GetAlbumFromApi(tpParse, videoId);
                else
                    await GetAlbumFromDb(videoId);
            }
            else
            {
                await GetAlbumFromApi(tpParse, videoId);
            }

            _bottomPlayerViewModel.SetAlbumPlaylist(_albumModel.Album, _albumModel.VideoId, _albumModel.Playlist.ToArray());
            _commonPageViewModel.InitAlbumPlayingGrid(_albumModel.Album, _albumModel.ImgAlbum);

            _pclUserAlbumLogic.UnLoadDb();
        }
        public async Task PlayMusic(PlaylistItem playlistItem)
        {
            MusicSelectedName = string.Empty;

            if (!_playerIsFullLoaded)
                return;

            PlaylistItem pItem = null;

            Album.Playlist.ToList()
                          .ForEach(plist =>
            {
                if (plist.Id != playlistItem.Id)
                    plist.IsPlaying = false;
                else
                    pItem = plist;
            });

            if (playlistItem.IsPlaying)
            {
                _audioPlayer.Pause();
                playlistItem.IsPlaying = false;
            }
            else
            {
                playlistItem.IsActiveMusic = true;

                MusicStreamProgress = 0;
                MusicSelectedName = playlistItem.NomeMusica;

                _bottomPlayerViewModel.PlayBottomPlayer(_album, pItem);
            }

            if (!_isAlbumHistorySaved)
            {
                await _albumPlayedHistoryViewModel.SaveLocalHistory(_album, _album.ParseType, _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage)
                                                  .OnError($"{nameof(AlbumPlayerViewModel)}_SaveLocalHistory", () => RaiseDefaultAppErrorEvent());
                _isAlbumHistorySaved = true;
            }

            App.EventTracker.SendEvent("AlbumPlayMusic", new Dictionary<string, string>()
            {
                { "AlbumName", _albumModel.Album },
                { "VideoId", _albumModel.VideoId },
            });
        }
        public void ShowHideDownloadIcon()
        {
            foreach (PlaylistItem plist in Album.Playlist)
            {
                plist.ShowHideDownloadIcon = !plist.ShowHideDownloadIcon;
                plist.ShowHideDownloadMusicOptions = false;
            }
        }
        public async Task StartDownloadMusic(AlbumModel album)
        {
            await _download.StartDownloadMusic(album);
        }

        #region Metodos Privados
        private IAsyncCommand<PlaylistItem> PlayEventCommand()
        {
            return new AsyncCommand<PlaylistItem>(
                execute: async (item) =>
                {
                    await PlayMusic(item);
                });
        }
        private async Task GetAlbumFromApi(AlbumParseType tpParse, string videoId)
        {
            Task<StreamManifest> taskUrl = _ytClient.Videos.Streams.GetManifestAsync(videoId).AsTask();
            Task<ApiAlbumModel> taskAlbumModel = _tocaTudoApi.AlbumEndpoint(tpParse, videoId);
            Task<List<UserAlbumPlayedHistory>> taskAlbumPlayer = _pclStorageDb.GetJson<List<UserAlbumPlayedHistory>>(USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY).AsTask();

            AudioOnlyStreamInfo streamInfo = null;

            await Task.WhenAll(taskUrl, taskAlbumModel, taskAlbumPlayer)
                      .ContinueWith(tsk =>
            {
                if (!taskUrl.IsCanceled && !taskUrl.IsFaulted)
                {
                    try
                    {
                        streamInfo = taskUrl.Result
                                            .GetAudioOnlyStreams()
                                            .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                            .FirstOrDefault();
                    }
                    catch (Exception)
                    {

                    }
                }
            });

            if (taskUrl.IsFaulted || taskAlbumModel.IsFaulted)
            {
                if (taskUrl.Exception == null || taskUrl.Exception == null)
                    return;

                if (taskUrl.Exception.Message.IndexOf("410") >= 0 || taskUrl.Exception.Message.IndexOf("is not") >= 0)
                {
                    RaiseAppErrorEvent(AppResource.AlbumIsNotPlayable);
                    return;
                }
            }

            if (streamInfo == null || taskAlbumModel.Result == null)
            {
                RaiseAppErrorEvent(AppResource.AlbumIsNotPlayable);
                return;
            }

            byte[] imgAlbum = Convert.FromBase64String(taskAlbumModel.Result.ImgAlbum);

            _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(imgAlbum);

            if (taskAlbumModel.Result.Playlist.Count() == 0)
            {
                RaiseAppErrorEvent(AppResource.AlbumIsNotPlayable);
                return;
            }

            _albumModel.Album = taskAlbumModel.Result.Album;
            _albumModel.ByteImgAlbum = imgAlbum;
            _albumModel.ParseType = tpParse;
            _albumModel.AlbumTime = taskAlbumModel.Result.MusicTime;
            _albumModel.MusicTimeTotalSeconds = taskAlbumModel.Result.MusicTimeTotalSeconds;
            _albumModel.ImgAlbum = _bottomPlayerViewModel.MusicStatusBottomModel.MusicImage;
            _albumModel.VideoId = videoId;
            _albumModel.Playlist.Clear();

            for (int index = 0; index < taskAlbumModel.Result.Playlist.Count(); index++)
            {
                ApiPlaylist apiPlaylist = taskAlbumModel.Result.Playlist[index];
                _albumModel.Playlist.Add(new PlaylistItem(apiPlaylist, (short)(index + 1)));
            }

            AlbumModelServicePlayer albumServicePlayer = new AlbumModelServicePlayer(_albumModel.VideoId, _albumModel.Album, _albumModel.ByteImgAlbum, _albumModel.Playlist.ToArray());

            _audioPlayer.Source(streamInfo.Url, videoId, _bottomPlayerViewModel.MusicStatusBottomModel, albumServicePlayer);

            ShowDownloadingInfo = true;
            IsInternetAvaiable = true;
        }
        private async Task GetAlbumFromDb(string uAlbumId)
        {
            if (!_dbAccesEnabled)
                return;

            _download.IsDownloadEventEnabled = false;
            _download.PercentDesc = AppResource.AlbumPlayingOfflineLabel;

            Task<(UserAlbum, byte[])> taskAlbumMusic = _pclUserAlbumLogic.GetAlbumById(uAlbumId);
            Task<List<UserAlbumPlayedHistory>> taskAlbumPlayedHist = _pclStorageDb.GetJson<List<UserAlbumPlayedHistory>>(USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY).AsTask();

            await Task.WhenAll(taskAlbumMusic, taskAlbumPlayedHist);

            if (taskAlbumMusic.Result.Item1 == null || taskAlbumMusic.Result.Item2 == null)
                return;

            ShowHideDownloadMusicOptions = true;
            ShowPlayingOfflineInfo = true;

            _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(taskAlbumMusic.Result.Item1.ImgAlbum);

            _albumModel.Album = taskAlbumMusic.Result.Item1.Album;
            _albumModel.AlbumTime = taskAlbumMusic.Result.Item1.AlbumTime;
            _albumModel.MusicTimeTotalSeconds = taskAlbumMusic.Result.Item1.MusicTimeTotalSeconds;
            _albumModel.VideoId = taskAlbumMusic.Result.Item1.VideoId;
            _albumModel.ByteImgAlbum = taskAlbumMusic.Result.Item1.ImgAlbum;
            _albumModel.GenerateImageMusic(taskAlbumMusic.Result.Item1.ImgAlbum);
            _albumModel.Playlist.Clear();

            foreach (UserAlbumPlaylist item in taskAlbumMusic.Result.Item1.Playlist.ToArray())
            {
                _albumModel.Playlist.Add(new PlaylistItem()
                {
                    Id = item.Id,
                    Number = item.Number,
                    NomeMusica = item.MusicName,
                    TempoSegundos = item.TimeSeconds,
                    TempoSegundosInicio = item.SecondsStartTime,
                    TempoSegundosFim = item.SecondsEndTime,
                    TempoDesc = item.DescTime,
                });
            }

            AlbumModelServicePlayer albumServicePlayer = new AlbumModelServicePlayer(_albumModel.VideoId, _albumModel.Album, _albumModel.ByteImgAlbum, _albumModel.Playlist.ToArray());
            _audioPlayer.Source(taskAlbumMusic.Result.Item2, albumServicePlayer);
        }
        private void DownloadAlbumStarted(object sender, EventArgs e)
        {
            if (_download.IsDownloading)
                UpdDownloadMusicImg();
            //_download.IsDownloadEventEnabled = true;
        }
        private void DownloadAlbumComplete(object sender, (bool, byte[], object) tpMusic)
        {
            Task.Run(async () =>
            {
                AlbumModel albumModel = (AlbumModel)tpMusic.Item3;

                try
                {
                    await _pclUserAlbumLogic.SaveAlbumOnLocalDb(albumModel, tpMusic);
                }
                catch
                {
                }

                _isDownloadComplete = true;

                UpdDownloadMusicImg();

                AlbumPlayedHistoryViewModel.PlayedHistory.ToList()
                                                         .ForEach(ph =>
                {
                    if (string.Equals(ph.VideoId, albumModel.VideoId))
                    {
                        ph.IsSavedOnLocalDb = true;
                    }
                });

                RaiseShowInterstitial();
            });
        }
        private bool AlbumInDb(string videoId)
        {
            if (!_dbAccesEnabled)
                return false;

            return _pclUserAlbumLogic.ExistsOnLocalDb(videoId);
        }
        private void UpdDownloadMusicImg()
        {
            if (_isDownloadComplete)
            {
                ImgStartDownloadIcon = "done.png";
                ShowDownloadMusicStatusProgress = false;
                return;
            }

            ImgStartDownloadIcon = !_download.IsDownloading ? "showDownloadIcon.png" : "showDownloadIconClicked.png";
            ShowDownloadMusicStatusProgress = true;
        }
        private void AudioPlayer_PlayerReady(object sender, ICommonMusicModel musicModel)
        {
            Task.Run(() =>
            {
                if (!PlayerLoaded)
                    ViewModel_ShowInterstitial();

                if (Album.Playlist.Count == 0)
                {
                    Album.Album = _albumModel.Album;
                    Album.AlbumTime = _albumModel.AlbumTime;
                    Album.MusicTimeTotalSeconds = _albumModel.MusicTimeTotalSeconds;
                    Album.ByteImgAlbum = _albumModel.ByteImgAlbum;
                    Album.ImgAlbum = _albumModel.ImgAlbum;
                    Album.ParseType = _albumModel.ParseType;
                    Album.VideoId = _albumModel.VideoId;

                    for (int index = 0; index < _albumModel.Playlist.Count(); index++)
                    {
                        Album.Playlist.Add(_albumModel.Playlist[index]);
                    }

                    AlbumFrameSize = 140;

                    _playerReady.RaiseEvent(sender, null, nameof(PlayerReady));
                }

                PlayerLoaded = true;
                IsSearching = false;
            }).ConfigureAwait(false);
        }
        private void AudioPlayer_PlayerReadyBuffering(object sender, ICommonMusicModel e)
        {
        }
        private void AudioPlayer_PlayerPlaylistChanged(object sender, ItemServicePlayer obj)
        {
            for (int index = 0; index < Album.Playlist.Count; index++)
            {
                PlaylistItem item = Album.Playlist[index];
                if (item.Id == obj.Id)
                {
                    if (item.IsPlaying)
                        break;

                    item.IsPlaying = true;

                    MusicStreamProgress = 0;
                    MusicSelectedName = item.NomeMusica;

                    _bottomPlayerViewModel.PlayBottomPlayer(_album, item);
                }
                else
                    item.IsPlaying = false;
            }
        }
        private void AudioPlayer_PlayerException(object sender, string e)
        {
            RaiseDefaultAppErrorEvent();
        }
        private void ViewModel_ShowInterstitial()
        {
            if (!App.IsSleeping)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsAlbumIntersticial, () =>
                    {

                    }, () =>
                    {
                        MusicPlayerViewModel.Pause();
                    });
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppHelper.HasInterstitialToShow = true;
                    MusicPlayerViewModel.Pause();
                });
            }
        }

        #endregion
    }
}
