using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicPageViewModel : MusicAlbumPageBaseViewModel, IMusicPageViewModel
    {
        private readonly IDbLogic _albumDbLogic;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly ICommonMusicPageViewModel _commonMusicPageViewModel;
        private readonly ICommonMusicPlayerViewModel _musicPlayerViewModel;
        private readonly IMusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private readonly ICommonPageViewModel _commonPageViewModel;
        private readonly ICommonFormDownloadViewModel _formDownloadViewModel;
        private ICommonMusicModel _music;
        private UserMusicPlayedHistory _lastMusicHistorySelected;
        private SearchMusicModel _musicLastSelected;
        private CancellationTokenSource _lastMusicHistorySelectedToken;
        private ObservableCollection<SearchMusicModel> _musicPlaylist;
        private List<UserMusicAlbumSelect> _musicAlbumPlaylistSelected;
        private ObservableCollection<SelectModel> _albumMusicSavedSelectCollection;

        private bool _frameMusicHistorySelectedIsVisible;
        private const string USER_MUSIC_ALBUM_SELECT_KEY = "mas_select.json";
        public MusicPageViewModel(IDbLogic albumDbLogic, IPCLStorageDb pclStorageDb, IPCLUserMusicLogic pclUserMusicLogic, ITocaTudoApi tocaTudoApi, ICommonMusicPageViewModel commonMusicPageViewModel, ICommonMusicPlayerViewModel musicPlayerViewModel, IMusicPlayedHistoryViewModel musicPlayedHistoryViewModel, ICommonPageViewModel commonPageViewModel, ICommonFormDownloadViewModel formDownloadViewModel, YoutubeClient ytClient)
            : base(albumDbLogic, pclUserMusicLogic, tocaTudoApi, ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _audioPlayer = DependencyService.Get<IAudio>();
            _commonMusicPageViewModel = commonMusicPageViewModel;
            _musicPlayerViewModel = musicPlayerViewModel;
            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;
            _commonPageViewModel = commonPageViewModel;
            _formDownloadViewModel = formDownloadViewModel;
            _albumDbLogic = albumDbLogic;
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;
            _musicPlaylist = new ObservableCollection<SearchMusicModel>();
            _musicAlbumPlaylistSelected = new List<UserMusicAlbumSelect>();
            _albumMusicSavedSelectCollection = new ObservableCollection<SelectModel>();

            SearchMusicCommand = new SearchMusicPlaylistCommand(this);
            DownloadMusicCommand = new SearchDownloadMusicCommand(this);

            _audioPlayer.Start();

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering += AudioPlayer_PlayerReadyBuffering;

            AppHelper.MusicPlayerInterstitialWasShowed = false;

            MusicPlayerConfig playerConfig = new MusicPlayerConfig()
            {
                TotalMusicsWillPlay = 2
            };

            _musicPlayerViewModel.SetMusicPlayerConfig(playerConfig);
        }
        public string MusicSearchedName { get; set; }
        public ICommonMusicPageViewModel CommonMusicPageViewModel
        {
            get { return _commonMusicPageViewModel; }
        }
        public ICommonMusicPlayerViewModel MusicPlayerViewModel
        {
            get { return _musicPlayerViewModel; }
        }
        public IMusicPlayedHistoryViewModel MusicPlayedHistoryViewModel
        {
            get { return _musicPlayedHistoryViewModel; }
        }
        public ICommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public ObservableCollection<SearchMusicModel> MusicPlaylist
        {
            get { return _musicPlaylist; }
            set
            {
                _musicPlaylist = value;
                OnPropertyChanged(nameof(MusicPlaylist));
            }
        }
        public ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection
        {
            get { return _albumMusicSavedSelectCollection; }
            set
            {
                _albumMusicSavedSelectCollection = value;
                OnPropertyChanged(nameof(AlbumMusicSavedSelectCollection));
            }
        }
        public UserMusicPlayedHistory LastMusicHistorySelected
        {
            get { return _lastMusicHistorySelected; }
            set
            {
                _lastMusicHistorySelected = value;
            }
        }
        public ICommand SearchMusicCommand { get; set; }
        public ICommand DownloadMusicCommand { get; set; }
        public AsyncCommand<SearchMusicModel> SelectMusicCommand => SelectMusicEventCommand();
        public AsyncCommand<SearchMusicModel> StartDownloadMusicCommand => StartDownloadMusicEventCommand();
        public AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormCommand => MusicHistoryFormEventCommand();
        public AsyncCommand<SelectModel> MusicHistoryAlbumSelectedCommand => MusicHistoryAlbumSelectedEventCommand();
        public AsyncCommand<HistoryMusicModel> MusicHistoryFormDownloadStartCommand => MusicHistoryFormDownloadStartEventCommand();
        public Command<HistoryMusicModel> MusicHistoryPlayCommand => MusicHistoryPlayEventCommand();
        public async Task PlayMusic(ICommonMusicModel musicModel, CancellationToken cancellationToken)
        {
            if (musicModel == null)
                return;

            _pclUserMusicLogic.UnLoadDb();

            if (musicModel.SearchType == MusicSearchType.SearchMusicAlbumHistory)
            {
                int indice = _musicPlaylist.ToList()
                                           .FindIndex(music => music.Id == musicModel.Id);

                SearchMusicModel[] musicModelCollection = _musicPlaylist.Skip(indice)
                                                                        .ToArray();

                MusicPlayerConfig playerConfig = new MusicPlayerConfig()
                {
                    TotalMusicsWillPlay = 2
                };

                await _musicPlayerViewModel.PlayMusic(musicModel, musicModelCollection, playerConfig, cancellationToken);
            }
            else
            {
                await _musicPlayerViewModel.PlayMusic(musicModel, cancellationToken);
            }
        }
        public async Task MusicPlaylistSearch()
        {
            Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcMusicPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.SearchMusicEndpoint(MusicSearchedName);
            };

            Task tskUserLocalHist = _musicPlayedHistoryViewModel.SaveLocalSearchHistory(MusicSearchedName);

            await SerializeMusicModel(funcMusicPlaylist, tskUserLocalHist, MusicSearchType.SearchMusic, Icon.Music);
        }
        public void ClearPlaylistLoaded()
        {
            MusicPlaylist.Clear();
        }
        public void StopMusicHistoryIsPlaying()
        {
            if (_musicPlayedHistoryViewModel.HistoryMusicPlayingNow == null)
                return;

            if (_musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsPlaying)
            {
                _musicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 0;
                _musicPlayerViewModel.Stop(_musicPlayedHistoryViewModel.HistoryMusicPlayingNow);
            }
        }

        #region Private Methods
        private async Task SerializeMusicModel(Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcApiSearch, Task tskUserLocalHist, MusicSearchType searchType, string icon)
        {
            await Task.Run(async () =>
            {
                IsSearching = true;
                UserMusicAlbumSelect musicAlbumSelect = null;

                await _pclUserMusicLogic.LoadDb();

                if (string.IsNullOrWhiteSpace(MusicSearchedName))
                {
                    IsSearching = false;
                    return;
                }

                MusicPlaylist.Clear();

                UserMusic[] userMusicsSaved = _pclUserMusicLogic.GetMusics();

                Task<ApiSearchMusicModel[]> tskApiSearch = funcApiSearch(_tocaTudoApi);

                await Task.WhenAll(tskUserLocalHist, tskApiSearch);

                foreach (ApiSearchMusicModel playlistItem in tskApiSearch.Result)
                {
                    if (_musicAlbumPlaylistSelected != null)
                    {
                        musicAlbumSelect = _musicAlbumPlaylistSelected.Where(ma =>
                        {
                            bool hasAlbum = ma.MusicsModel.Exists(mm => string.Equals(mm.VideoId, playlistItem.VideoId));
                            playlistItem.HasAlbum = hasAlbum;

                            return hasAlbum;
                        }).FirstOrDefault();
                    }

                    bool savedOnLocalDb = userMusicsSaved.Where(um => string.Equals(um.VideoId, playlistItem.VideoId))
                                                         .Count() > 0;

                    playlistItem.Icon = icon;

                    if (musicAlbumSelect != null)
                        MusicPlaylist.Add(new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, musicAlbumSelect, searchType, savedOnLocalDb));
                    else
                        MusicPlaylist.Add(new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, searchType, savedOnLocalDb));
                }

                IsSearching = false;
            });
        }
        private async Task SerializeMusicModel(UserMusicSelect[] userMusics, MusicSearchType searchType, string icon)
        {
            MusicPlaylist.Clear();

            await _pclUserMusicLogic.LoadDb();

            foreach (UserMusicSelect userMusic in userMusics)
            {
                bool existsOnLocalDb = _pclUserMusicLogic.ExistsOnLocalDb(userMusic.VideoId);

                MusicPlaylist.Add(new SearchMusicModel(userMusic, icon, _formDownloadViewModel, _tocaTudoApi, YtClient, searchType, existsOnLocalDb));
            }

            IsSearching = false;
        }
        public AsyncCommand<SearchMusicModel> SelectMusicEventCommand()
        {
            return new AsyncCommand<SearchMusicModel>(
                execute: async (musicModel) =>
                {
                    if (MusicPlayerViewModel.LastMusicPlayed != null)
                        if (MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic)
                        {
                            MusicPlayerViewModel.LastMusicPlayed.ReloadMusicPlayingIcon();
                            MusicPlayerViewModel.LastMusicPlayed.IsSelected = false;
                            MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic = false;
                        }

                    _musicPlaylist.ToList().ForEach(music =>
                    {
                        if (music.Id != musicModel.Id)
                        {
                            music.IsActiveMusic = false;
                            music.IsSelected = false;
                            music.IsPlaying = false;
                        }
                    });

                    if (LastMusicHistorySelected != null)
                    {
                        MusicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 0;

                        LastMusicHistorySelected.UpdMusicSelectedColor();
                        LastMusicHistorySelected = null;
                        MusicPlayedHistoryViewModel.HistoryMusicPlayingNow = null;
                    }

                    if (musicModel.IsPlaying)
                    {
                        musicModel.IsActiveMusic = false;
                        musicModel.IsSelected = false;

                        _musicPlayerViewModel.Stop(musicModel);
                        //_musicLastSelected = null;
                    }
                    else
                    {
                        CancellationTokenSource cancellationToken = new CancellationTokenSource();

                        musicModel.IsActiveMusic = true;
                        musicModel.IsSelected = true;
                        _musicLastSelected = musicModel;

                        await PlayMusic(musicModel, cancellationToken.Token);
                    }
                }
            );
        }
        private AsyncCommand<SearchMusicModel> StartDownloadMusicEventCommand()
        {
            return new AsyncCommand<SearchMusicModel>(
                execute: async (musicModel) =>
                {
                    musicModel.SetDownload(_pclUserMusicLogic);
                    musicModel.SetDownloadingMode();

                    await musicModel.StartDownloadMusic();
                }
            );
        }
        private Command<HistoryMusicModel> MusicHistoryPlayEventCommand()
        {
            return new Command<HistoryMusicModel>(
                execute: (userMusicHistory) =>
                {
                    _musicPlayerViewModel.PlayPauseMusic(userMusicHistory);
                }
            );
        }
        private AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormEventCommand()
        {
            return new AsyncCommand<UserMusicPlayedHistory>(
                execute: async (userMusicHistory) =>
                {
                    _musicPlaylist.ToList().ForEach(music =>
                    {
                        music.IsActiveMusic = false;
                        music.IsSelected = false;
                    });

                    if (_lastMusicHistorySelected != null)
                    {
                        _lastMusicHistorySelected.UpdMusicSelectedColor();
                        _lastMusicHistorySelectedToken.Cancel();
                    }

                    if (_musicPlayedHistoryViewModel.ActiveMusicNow != null)
                    {
                        _musicPlayedHistoryViewModel.ActiveMusicNow.UpdMusicSelectedColor();
                        _musicPlayedHistoryViewModel.ActiveMusicNow = null;
                    }

                    await _pclUserMusicLogic.LoadDb();

                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                    _musicPlayedHistoryViewModel.HistoryMusicPlayingNow = new HistoryMusicModel(_formDownloadViewModel, _tocaTudoApi, base.YtClient, _pclUserMusicLogic.ExistsOnLocalDb(userMusicHistory.VideoId))
                    {
                        VideoId = userMusicHistory.VideoId,
                        SearchType = MusicSearchType.SearchMusicHistory,
                        IsLoadded = false,
                        IsActiveMusic = true,
                        IsSelected = true,
                        MusicName = userMusicHistory.MusicName,
                        ByteImgMusic = userMusicHistory.ByteImgMusic
                    };

                    _musicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 35;

                    _lastMusicHistorySelected = userMusicHistory;
                    _lastMusicHistorySelectedToken = cancellationToken;

                    userMusicHistory.UpdMusicSelectedColor();
                    _pclUserMusicLogic.UnLoadDb();

                    if (!_musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsSavedOnLocalDb)
                        _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.Download.DownloadComplete += HistoryMusicPlayingNow_DownloadComplete;

                    await _musicPlayerViewModel.PlayMusic(_musicPlayedHistoryViewModel.HistoryMusicPlayingNow, cancellationToken.Token);
                }
            );
        }
        private AsyncCommand<SelectModel> MusicHistoryAlbumSelectedEventCommand()
        {
            return new AsyncCommand<SelectModel>(
                execute: async (selected) =>
                {
                    if (selected == null)
                        return;

                    IsSearching = true;

                    UserMusicAlbumSelect userMusicAlbum = CommonMusicPageViewModel.MusicAlbumPlaylistSelected.Where(mu => mu.Id == selected.Id)
                                                                                                             .FirstOrDefault();
                    if (userMusicAlbum != null)
                        await SerializeMusicModel(userMusicAlbum.MusicsModel.ToArray(), MusicSearchType.SearchMusicAlbumHistory, Icon.Music);
                    else
                        IsSearching = false;
                }
            );
        }
        private AsyncCommand<HistoryMusicModel> MusicHistoryFormDownloadStartEventCommand()
        {
            return new AsyncCommand<HistoryMusicModel>(
                execute: async (music) =>
                {
                    music.SetDownload(_pclUserMusicLogic);
                    await music.StartDownloadMusic();
                }
            );
        }
        private void AudioPlayer_PlayerReady(ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsActiveMusic)
                {
                    if (music.SearchType == MusicSearchType.SearchSavedMusic)
                        return;

                    if (music.IsLoadded)
                        return;

                    //bool musicSavedOnDb = await _albumDbLogic.ExistsMusicAsync(music.VideoId);

                    if (music.ShowMerchandisingAlert)
                    {
                        if (!AppHelper.MusicPlayerInterstitialWasShowed && IsInternetAvaiable)
                            RaiseActionShowInterstitial(() => { _audioPlayer.Play(); });
                    }

                    if (!music.ShowMerchandisingAlert || !IsInternetAvaiable)
                        _audioPlayer.Play();

                    music.IsPlaying = true;
                    music.IconMusicDownloadVisible = !music.IsSavedOnLocalDb;

                    music.IsBufferingMusic = false;
                    music.IsLoadded = true;

                    if (_musicPlayedHistoryViewModel.HistoryMusicPlayingNow != null)
                        _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsLoadded = true;
                }
            }

            base.MusicPlaying = music;
        }
        private void AudioPlayer_PlayerReadyBuffering(ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsBufferingMusic && _audioPlayer.IsPlaying)
                {
                    music.IsBufferingMusic = false;
                }
            }
        }
        private void HistoryMusicPlayingNow_DownloadComplete((bool, byte[]) compressedMusic, object model)
        {
            if (MusicPlayedHistoryViewModel.HistoryMusicPlayingNow != null)
                MusicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize -= MusicPlayedHistoryViewModel.HistoryMusicPlayingNow.FormDownloadSize;

            MusicPlayedHistoryViewModel.LoadPlayedHistory();

            RaiseShowInterstitial();
        }
        #endregion
    }
}
