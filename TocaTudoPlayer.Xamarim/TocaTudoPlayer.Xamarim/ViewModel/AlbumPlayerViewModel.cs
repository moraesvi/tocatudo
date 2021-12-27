using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TocaTudoPlayer.Xamarim.Resources;
using TocaTudoPlayer.Xamarim.ViewModel;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;
using YoutubeParse.ExplodeV2.Videos.Streams;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumPlayerViewModel : BaseViewModel, IAlbumPlayerViewModel
    {
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly IDbLogic _dbLogic;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly ICommonMusicPlayerViewModel _musicPlayerViewModel;
        private readonly IAlbumPlayedHistoryViewModel _albumPlayedHistoryViewModel;
        private readonly IMusicBottomAlbumPlayerViewModel _bottomPlayerViewModel;
        private readonly ICommonPageViewModel _commonPageViewModel;
        private bool _playerIsFullLoaded;
        private bool _viewModelLoadded;
        private bool _showHideDownloadMusicOptions;
        private bool _isActionsEnabled;
        private bool _isDownloadComplete;
        private bool _showDownloadMusicStatusProgress;
        private bool _showDownloadingInfo;
        private bool _showPlayingOfflineInfo;
        private bool _dbAccesEnabled;
        private bool _isSearching;
        private bool _isAlbumHistorySaved;
        private float _mStreamProgress;
        private string _musicSelectedName;
        private AlbumModel _album;
        private AlbumModel _albumModel;
        private PlaylistItem _playlistItemLastSelected;
        private readonly YoutubeClient _ytClient;
        private HttpDownload _download;
        private string _imgStartDownloadIcon;

        private event Action<Action> _showInterstitial;
        private const string USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY = "ap_history.json";
        public AlbumPlayerViewModel(IDbLogic dbLogic, IPCLStorageDb pclStorageDb, ITocaTudoApi tocaTudoApi, ICommonMusicPlayerViewModel musicPlayerViewModel, IMusicBottomAlbumPlayerViewModel bottomPlayerViewModel, IAlbumPlayedHistoryViewModel searchHistoryAlbumViewModel, ICommonPageViewModel commonPageViewModel, YoutubeClient ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _audioPlayer = DependencyService.Get<IAudio>();
            _ytClient = ytClient;
            _dbLogic = dbLogic;
            _pclStorageDb = pclStorageDb;
            _album = new AlbumModel();
            _albumModel = new AlbumModel();
            _musicPlayerViewModel = musicPlayerViewModel;
            _bottomPlayerViewModel = bottomPlayerViewModel;
            _albumPlayedHistoryViewModel = searchHistoryAlbumViewModel;
            _commonPageViewModel = commonPageViewModel;
            PlayCommand = new PlayerPlayCommand(this);
            DownloadMusicOptionsCommand = new ShowHideDownloadMusicOptionsCommand(this);
            StartDownloadMusicCommand = new PlayerStartDownloadMusicCommand(this);

            _audioPlayer.Start();

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering += (searchMusic) => { };
            _audioPlayer.PlayerPlaylistChanged += AudioPlayer_PlayerPlaylistChanged;
        }
        public event Action<Action> ShowInterstitial
        {
            add
            {
                _showInterstitial += value;
            }
            remove
            {
                _showInterstitial -= value;
            }
        }
        public AlbumModel Album
        {
            get { return _album; }
            set
            {
                _album = value;
            }
        }
        public ICommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public ICommonMusicPlayerViewModel MusicPlayerViewModel
        {
            get { return _musicPlayerViewModel; }
        }
        public IMusicBottomAlbumPlayerViewModel BottomPlayerViewModel
        {
            get { return _bottomPlayerViewModel; }
        }
        public IAlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel
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
        public bool ViewModelLoadded
        {
            get { return _viewModelLoadded; }
            set { _viewModelLoadded = value; }
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
        public bool IsActionsEnabled
        {
            get { return _isActionsEnabled; }
            set
            {
                _isActionsEnabled = value;
                OnPropertyChanged(nameof(IsActionsEnabled));
            }
        }
        public ICommand PlayCommand { get; set; }
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
            PlayerLoaded = false;
            IsSearching = false;
            IsActionsEnabled = true;
            ShowDownloadMusicStatusProgress = false;
            ShowDownloadingInfo = false;
            ShowPlayingOfflineInfo = false;

            _isDownloadComplete = false;
            _isAlbumHistorySaved = false;

            IsSearching = true;
            IsActionsEnabled = false;

            AppHelper.MusicPlayerInterstitialWasShowed = false;

            ImgStartDownloadIcon = "showDownloadIcon.png";

            Download = new HttpDownload(AppResource.AlbumDownloadedLabel, _ytClient);

            Download.DownloadComplete += DownloadAlbumComplete;
            Download.DownloadStarted += DownloadAlbumStarted;

            if (tpParse == AlbumParseType.NaoDefinido || string.IsNullOrWhiteSpace(videoId))
                return;

            if (_dbAccesEnabled)
            {
                bool albumInDb = await AlbumInDb(videoId);

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
        }
        public async Task PlayMusic(PlaylistItem playlistItem)
        {
            await Task.Run(async () =>
            {
                MusicSelectedName = string.Empty;

                if (!_playerIsFullLoaded)
                    return;

                List<PlaylistItem> lstItem = Album.Playlist
                                                  .Where(plist => plist.IsPlaying)
                                                  .ToList();

                if (lstItem.Count > 1)
                {
                    lstItem.ForEach(plist =>
                    {
                        if (plist.Id != playlistItem.Id)
                            plist.IsPlaying = false;
                    });
                }

                if (_playlistItemLastSelected != null)
                    _playlistItemLastSelected.IsPlaying = false;

                if (playlistItem.IsPlaying)
                {
                    _audioPlayer.Pause();
                    playlistItem.IsPlaying = false;
                }
                else
                {
                    MusicStreamProgress = 0;
                    MusicSelectedName = playlistItem.NomeMusica;
                    _playlistItemLastSelected = playlistItem;

                    _bottomPlayerViewModel.PlayBottomPlayer(playlistItem);
                }

                if (!_isAlbumHistorySaved)
                {
                    await _albumPlayedHistoryViewModel.SaveLocalHistory(_album, _album.ParseType, _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage);
                    _isAlbumHistorySaved = true;
                }
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
        private async Task GetAlbumFromApi(AlbumParseType tpParse, string videoId)
        {
            await Task.Run(async () => //Required for Android
            {
                Task<StreamManifest> taskUrl = _ytClient.Videos.Streams.GetManifestAsync(videoId).AsTask();
                Task<ApiAlbumModel> taskAlbumModel = _tocaTudoApi.AlbumEndpoint(tpParse, videoId);
                Task<List<UserAlbumPlayedHistory>> taskAlbumPlayer = _pclStorageDb.GetJson<List<UserAlbumPlayedHistory>>(USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY).AsTask();

                AudioOnlyStreamInfo streamInfo = null;

                await Task.Delay(200);//Required for multiples selections
                await Task.WhenAll(taskUrl, taskAlbumModel, taskAlbumPlayer).ContinueWith(tsk =>
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
                    if (taskUrl.Exception.Message.IndexOf("410") >= 0)
                    {
                        RaiseAppErrorEvent(0, AppResource.AlbumIsNotPlayable);
                    }

                    return;
                }

                if (streamInfo == null)
                    return;

                if (taskAlbumModel.Result == null)
                {
                    RaiseAppErrorEvent(0, "Não foi é possível tocar este álbum");
                    return;
                }

                byte[] imgAlbum = Convert.FromBase64String(taskAlbumModel.Result.ImgAlbum);

                _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(imgAlbum);

                _albumModel.Album = taskAlbumModel.Result.Album;
                _albumModel.ByteImgAlbum = imgAlbum;
                _albumModel.ParseType = tpParse;
                _albumModel.GenerateImageMusic(imgAlbum);
                _albumModel.VideoId = videoId;
                _albumModel.Playlist.Clear();

                for (int index = 0; index < taskAlbumModel.Result.Playlist.Count(); index++)
                {
                    ApiPlaylist apiPlaylist = taskAlbumModel.Result.Playlist[index];
                    _albumModel.Playlist.Add(new PlaylistItem(apiPlaylist, (short)(index + 1)));
                }

                AlbumModelServicePlayer albumServicePlayer = new AlbumModelServicePlayer(_albumModel.VideoId, _albumModel.Album, _albumModel.ByteImgAlbum, _albumModel.Playlist.ToArray());
                _audioPlayer.Source(streamInfo.Url, albumServicePlayer);

                ShowDownloadingInfo = true;
            });
        }
        private async Task GetAlbumFromDb(string videoId)
        {
            if (!_dbAccesEnabled)
                return;

            _download.IsDownloadEventEnabled = false;
            _download.PercentDesc = AppResource.AlbumPlayingOfflineLabel;

            Task<(AlbumModel, byte[])> taskAlbumMusic = _dbLogic.GetAlbumByVideoIdAsync(videoId);
            Task<List<UserAlbumPlayedHistory>> taskAlbumPlayedHist = _pclStorageDb.GetJson<List<UserAlbumPlayedHistory>>(USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY).AsTask();

            await Task.WhenAll(taskAlbumMusic, taskAlbumPlayedHist);

            if (taskAlbumMusic.Result.Item1 == null || taskAlbumMusic.Result.Item2 == null)
                return;

            UserAlbum userAlbum = await _pclStorageDb.GetJson<UserAlbum>(taskAlbumMusic.Result.Item1.UAlbumlId).AsTask();

            if (userAlbum == null)
                return;

            ShowHideDownloadMusicOptions = true;
            ShowPlayingOfflineInfo = true;

            _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(userAlbum.ImgAlbum);

            _albumModel.Album = userAlbum.Album;
            _albumModel.VideoId = userAlbum.VideoId;
            _albumModel.ByteImgAlbum = userAlbum.ImgAlbum;
            _albumModel.GenerateImageMusic(userAlbum.ImgAlbum);
            _albumModel.Playlist.Clear();

            foreach (UserAlbumPlaylist item in userAlbum.Playlist)
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
        private void DownloadAlbumStarted()
        {
            if (_download.IsDownloading)
                UpdDownloadMusicImg();
            //_download.IsDownloadEventEnabled = true;
        }
        private async void DownloadAlbumComplete((bool, byte[]) compressedMusic, object album)
        {
            AlbumModel albumModel = (AlbumModel)album;
            UserAlbum userAlbum = new UserAlbum(albumModel);

            string userAlbumId = UserAlbum.GenerateLocalKey();

            try
            {
                await _dbLogic.InsertOrUpdateAsync(userAlbumId, albumModel, compressedMusic);
                await _pclStorageDb.SaveFile(userAlbumId, userAlbum);
            }
            catch
            {
                await _dbLogic.DeleteAlbum(albumModel.VideoId)
                              .ContinueWith((result) => { });
                await _pclStorageDb.RemoveFile(userAlbumId)
                                   .ContinueWith((result) => { });
            }

            _isDownloadComplete = true;

            UpdDownloadMusicImg();

            CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
            CrossMTAdmob.Current.ShowInterstitial();
        }
        private async Task<bool> AlbumInDb(string videoId)
        {
            if (!_dbAccesEnabled)
                return false;

            return await _dbLogic.ExistsAlbumAsync(videoId);
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
        private void AudioPlayer_PlayerReady(ICommonMusicModel musicModel)
        {
            if (!PlayerLoaded)
                _showInterstitial(() => _audioPlayer.Play());

            if (Album.Playlist.Count == 0)
            {
                Album.Album = _albumModel.Album;
                Album.ByteImgAlbum = _albumModel.ByteImgAlbum;
                Album.ImgAlbum = _albumModel.ImgAlbum;
                Album.ParseType = _albumModel.ParseType;
                Album.VideoId = _albumModel.VideoId;

                for (int index = 0; index < _albumModel.Playlist.Count(); index++)
                {
                    Album.Playlist.Add(_albumModel.Playlist[index]);
                }
            }

            PlayerLoaded = true;
            IsSearching = false;
            IsActionsEnabled = true;
        }
        private void AudioPlayer_PlayerPlaylistChanged(PlaylistItemServicePlayer obj)
        {
            if (!string.Equals(obj.AlbumId, Album.VideoId))
                throw new InvalidOperationException("Id de álbum inválido");

            bool alreadyPlaying = Album.Playlist.Where(item => item.IsPlaying && item.Id == obj.Id)
                                                .ToList()
                                                .Count() > 0;
            if (alreadyPlaying)
                return;

            for (int index = 0; index < Album.Playlist.Count; index++)
            {
                PlaylistItem item = Album.Playlist[index];
                if (item.Id == obj.Id)
                {
                    item.IsPlaying = true;

                    MusicStreamProgress = 0;
                    MusicSelectedName = item.NomeMusica;

                    _bottomPlayerViewModel.PlayBottomPlayer(item);
                }
                else
                    item.IsPlaying = false;
            }
        }

        #endregion
    }
}
