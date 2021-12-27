using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TocaTudoPlayer.Xamarim.Interface;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;
using YoutubeParse.ExplodeV2.Videos.Streams;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchPlaylistViewModel : BaseViewModel, ISearchPlaylistViewModel
    {
        private readonly IDbLogic _albumDbLogic;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly ICommonMusicPlayerViewModel _musicPlayer;
        private readonly IMusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private readonly IAlbumPlayedHistoryViewModel _albumPlayedHistoryViewModel;
        private readonly YoutubeClient _ytClient;
        private bool _isSearching;
        private bool _playerIsFullLoaded;
        private bool _menuActionsEnabled;
        private bool _searchAlbumVisible;
        private bool _searchSavedForm;
        private bool _albumPlaylistIsVisible;
        private bool _playlistIsVisible;
        private bool _musicPlaylistIsVisible;
        private bool _musicDetailsChangeAlbumIsVisible;
        private bool _musicDetailsAddAlbumIsVisible;
        private int _albumPlayedHistoryCollectionSize;
        private int _musicPlayedHistoryCollectionSize;
        private HttpDownload _download;
        private ICommonMusicModel _music;
        private UserAlbumPlayedHistory _lastAlbumHistorySelected;
        private UserMusicPlayedHistory _lastMusicHistorySelected;
        private List<UserMusicAlbumSelect> _musicAlbumPlaylistSelected;
        private ObservableCollection<SelectModel> _albumMusicSavedSelectCollection;
        private ObservableCollection<SearchMusicModel> _albumPlaylist;
        private ObservableCollection<SearchMusicModel> _musicPlaylist;
        private ObservableCollection<SearchMusicModel> _savedAlbumPlaylist;
        private ObservableCollection<SearchMusicModel> _savedMusicPlaylist;
        private ObservableCollection<SearchMusicModel> _playlist;

        private event Action _playerReady;
        private event Action<Action> _showInterstitial;

        private const string USER_MUSIC_ALBUM_SELECT_KEY = "mas_select.json";
        public SearchPlaylistViewModel(IDbLogic albumDbLogic, IPCLStorageDb pclStorageDb, ITocaTudoApi tocaTudoApi, ICommonMusicPlayerViewModel musicPlayer, IMusicPlayedHistoryViewModel musicPlayedHistoryViewModel, IAlbumPlayedHistoryViewModel albumPlayedHistoryViewModel, YoutubeClient ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _audioPlayer = DependencyService.Get<IAudio>();
            _musicPlayer = musicPlayer;
            _albumPlayedHistoryViewModel = albumPlayedHistoryViewModel;
            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;
            _isSearching = false;
            _menuActionsEnabled = true;
            _searchAlbumVisible = true;
            _searchSavedForm = false;
            _playerIsFullLoaded = false;
            _albumPlaylistIsVisible = false;
            _playlistIsVisible = false;
            _musicPlaylistIsVisible = false;
            _musicDetailsChangeAlbumIsVisible = false;
            _musicDetailsAddAlbumIsVisible = false;
            _albumPlayedHistoryCollectionSize = 0;
            _musicPlayedHistoryCollectionSize = 0;
            _ytClient = ytClient;
            _albumDbLogic = albumDbLogic;
            _pclStorageDb = pclStorageDb;
            _albumPlaylist = new ObservableCollection<SearchMusicModel>();
            _playlist = new ObservableCollection<SearchMusicModel>();
            _savedMusicPlaylist = new ObservableCollection<SearchMusicModel>();
            _savedAlbumPlaylist = new ObservableCollection<SearchMusicModel>();
            _albumMusicSavedSelectCollection = new ObservableCollection<SelectModel>();
            _musicPlaylist = new ObservableCollection<SearchMusicModel>();
            //SearchAlbumCommand = new SearchAlbumPlaylistCommand(this);
            //SearchMusicCommand = new SearchMusicPlaylistCommand(this);
            MusicSavedCommand = new MusicSavedActionCommand(this);
            DownloadMusicVisibleCommand = new SearchDownloadMusicVisibleCommand(this);
            //DownloadMusicCommand = new SearchDownloadMusicCommand(this);

            _audioPlayer.Start();

            _download = new HttpDownload();

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering += AudioPlayer_PlayerReadyBuffering;

            CrossMTAdmob.Current.OnInterstitialClosed += (sender, e) =>
            {
                if (_music?.IsActiveMusic ?? false)
                    _audioPlayer.Play();
            };
        }
        public event Action PlayerReady
        {
            add => _playerReady += value;
            remove => _playerReady -= value;
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
        public string AlbumSearchedName { get; set; }
        public string MusicSearchedName { get; set; }
        public bool MenuActionsEnabled
        {
            get { return _menuActionsEnabled; }
            set
            {
                _menuActionsEnabled = value;
                OnPropertyChanged(nameof(MenuActionsEnabled));
            }
        }
        public bool SearchAlbumVisible
        {
            get { return _searchAlbumVisible; }
            set
            {
                _searchAlbumVisible = value;
                OnPropertyChanged(nameof(SearchAlbumVisible));
            }
        }
        public bool AlbumPlaylistIsVisible
        {
            get { return _albumPlaylistIsVisible; }
            set
            {
                _albumPlaylistIsVisible = value;

                if (_albumPlaylistIsVisible)
                    PlaylistIsVisible = false;

                OnPropertyChanged(nameof(AlbumPlaylistIsVisible));
            }
        }
        public bool PlaylistIsVisible
        {
            get { return _playlistIsVisible; }
            set
            {
                _playlistIsVisible = value;

                if (_playlistIsVisible)
                    AlbumPlaylistIsVisible = false;

                OnPropertyChanged(nameof(PlaylistIsVisible));
            }
        }
        public bool MusicPlaylistIsVisible
        {
            get { return _musicPlaylistIsVisible; }
            set
            {
                _musicPlaylistIsVisible = value;
                OnPropertyChanged(nameof(MusicPlaylistIsVisible));
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
        public bool SearchSavedForm
        {
            get { return _searchSavedForm; }
            set
            {
                _searchSavedForm = value;
                OnPropertyChanged(nameof(SearchSavedForm));
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
        public bool MusicDetailsChangeAlbumIsVisible
        {
            get { return _musicDetailsChangeAlbumIsVisible; }
            set
            {
                _musicDetailsChangeAlbumIsVisible = value;
                OnPropertyChanged(nameof(MusicDetailsChangeAlbumIsVisible));
            }
        }
        public bool MusicDetailsAddAlbumIsVisible
        {
            get { return _musicDetailsAddAlbumIsVisible; }
            set
            {
                _musicDetailsAddAlbumIsVisible = value;
                OnPropertyChanged(nameof(MusicDetailsAddAlbumIsVisible));
            }
        }
        public int AlbumPlayedHistoryCollectionSize
        {
            get { return _albumPlayedHistoryCollectionSize; }
            set
            {
                _albumPlayedHistoryCollectionSize = value;
                OnPropertyChanged(nameof(AlbumPlayedHistoryCollectionSize));
            }
        }
        public int MusicPlayedHistoryCollectionSize
        {
            get { return _musicPlayedHistoryCollectionSize; }
            set
            {
                _musicPlayedHistoryCollectionSize = value;
                OnPropertyChanged(nameof(MusicPlayedHistoryCollectionSize));
            }
        }
        public ICommonMusicPlayerViewModel MusicPlayer
        {
            get { return _musicPlayer; }
        }
        public IAlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel
        {
            get { return _albumPlayedHistoryViewModel; }
        }
        public IMusicPlayedHistoryViewModel MusicPlayedHistoryViewModel
        {
            get { return _musicPlayedHistoryViewModel; }
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
        public ICommand SearchAlbumCommand { get; set; }
        public ICommand SearchMusicCommand { get; set; }
        public ICommand MusicSavedCommand { get; set; }
        public ICommand DownloadMusicVisibleCommand { get; set; }
        public ICommand DownloadMusicCommand { get; set; }
        public Command<UserAlbumPlayedHistory> AlbumHistoryFormCommand => AlbumHistoryFormEventCommand();
        public AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormCommand => MusicHistoryFormEventCommand();
        public Command<HistoryMusicModel> MusicHistoryPlayCommand => MusicHistoryPlayEventCommand();
        public ObservableCollection<SearchMusicModel> AlbumPlaylist
        {
            get { return _albumPlaylist; }
            set
            {
                _albumPlaylist = value;
                OnPropertyChanged(nameof(AlbumPlaylist));
            }
        }
        public ObservableCollection<SearchMusicModel> Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                OnPropertyChanged(nameof(Playlist));
            }
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
        public ObservableCollection<SearchMusicModel> SavedAlbumPlaylist
        {
            get { return _savedAlbumPlaylist; }
            set
            {
                _savedAlbumPlaylist = value;
                OnPropertyChanged(nameof(SavedAlbumPlaylist));
            }
        }
        public ObservableCollection<SearchMusicModel> SavedMusicPlaylist
        {
            get { return _savedMusicPlaylist; }
            set
            {
                _savedMusicPlaylist = value;
                OnPropertyChanged(nameof(SavedMusicPlaylist));
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
        //public async Task<string[]> SearchTerm(string term)
        //{
        //    return await _tocaTudoApi.SearchEndpoint(term);
        //}
        //public void MusicIconType(SearchMusicModel searchMusic)
        //{
        //    switch (searchMusic.SearchType)
        //    {
        //        case MusicSearchType.SearchAlbum:
        //        case MusicSearchType.SearchSavedAlbum:
        //            searchMusic.IconMusicStatus = AppHelper.FaviconImageSource(Icon.ArrowRight, 35, Color.White);
        //            break;
        //        case MusicSearchType.SearchMusic:
        //        case MusicSearchType.SearchSavedMusic:
        //            searchMusic.IconMusicStatus = AppHelper.FaviconImageSource(Icon.PlayCircle, 35, Color.Black);
        //            break;
        //    }
        //}
        public async Task AlbumPlaylistSearch()
        {
            AlbumPlaylistIsVisible = true;

            Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcSearchPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.SearchPlaylistEndpoint(AlbumSearchedName);
            };

            await SerializeMusicModel(AlbumPlaylist, funcSearchPlaylist, MusicSearchType.SearchAlbum, Icon.FileImageO);
        }
        public async Task MusicPlaylistSearch()
        {
            MusicPlaylistIsVisible = true;

            Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcMusicPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.SearchMusicEndpoint(MusicSearchedName);
            };

            await SerializeMusicModel(MusicPlaylist, funcMusicPlaylist, MusicSearchType.SearchMusic, Icon.Music);
        }
        public async Task AlbumPlaylistSearchFromDb()
        {
            MusicPlaylistIsVisible = false;
            PlaylistIsVisible = false;

            Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcMusicPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.GetAlbums();
            };

            await SerializeMusicModelFromDb(SavedAlbumPlaylist, funcMusicPlaylist, MusicSearchType.SearchSavedAlbum, Icon.ArrowDown);

            PlaylistIsVisible = true;
            MusicPlaylistIsVisible = false;
        }
        public async Task MusicPlaylistSearchFromDb()
        {
            MusicPlaylistIsVisible = false;
            PlaylistIsVisible = false;

            Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcMusicPlaylist = async (param1) =>
            {
                return await param1.GetMusics();
            };

            await SerializeMusicModelFromDb(SavedMusicPlaylist, funcMusicPlaylist, MusicSearchType.SearchSavedMusic, Icon.ArrowDown);

            PlaylistIsVisible = false;
            MusicPlaylistIsVisible = true;
        }
        public async void DownloadMusicVisible(SearchMusicModel searchMusic)
        {
            if (searchMusic == null || (searchMusic.SearchType != MusicSearchType.SearchMusic))
                return;

            bool musicDbSaved = await _albumDbLogic.ExistsMusicAsync(searchMusic.VideoId);
            if (musicDbSaved)
                return;

            searchMusic.IsDownloadMusicVisible = !searchMusic.IsDownloadMusicVisible;
        }
        public async Task InsertMusicAlbumPlaylistSelected(string albumName, SearchMusicModel music)
        {
            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            if (musicAlbumPlaylist == null)
            {
                musicAlbumPlaylist = new List<UserMusicAlbumSelect>();
                UserMusicAlbumSelect musicAlbumInsert = new UserMusicAlbumSelect()
                {
                    Id = 1,
                    AlbumName = albumName,
                    TimestampIn = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

                musicAlbumInsert.MusicsModel.Add(new UserMusicSelect() { VideoId = music.VideoId });
                musicAlbumPlaylist.Add(musicAlbumInsert);

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }
            else
            {
                UserMusicAlbumSelect musicAlbumInsert = musicAlbumPlaylist.Where(ma => string.Equals(ma.AlbumName, albumName, StringComparison.OrdinalIgnoreCase))
                                                                          .FirstOrDefault();

                if (musicAlbumInsert != null)
                {
                    if (musicAlbumInsert.MusicsModel.Exists(ma => string.Equals(ma.VideoId, music.VideoId)))
                        return;

                    musicAlbumInsert.Id = (short)(musicAlbumPlaylist.Count + 1);

                    musicAlbumInsert.MusicsModel.Add(new UserMusicSelect() { VideoId = music.VideoId });
                    musicAlbumPlaylist.Add(musicAlbumInsert);
                }
                else
                {
                    musicAlbumInsert = new UserMusicAlbumSelect()
                    {
                        Id = (short)(musicAlbumPlaylist.Count + 1),
                        AlbumName = AppHelper.FirstLetterToUpper(albumName),
                        TimestampIn = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };

                    musicAlbumInsert.MusicsModel.Add(new UserMusicSelect() { VideoId = music.VideoId });
                    musicAlbumPlaylist.Add(musicAlbumInsert);
                }

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }

            SearchMusicModel musicModel = _musicPlaylist.Where(mp => string.Equals(mp.VideoId, music.VideoId))
                                                        .FirstOrDefault();

            if (musicModel != null)
            {
                musicModel.SetAlbumMode();
                await LoadMusicAlbumPlaylistSelected();
            }
        }
        public async Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel musicModel)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0 || musicModel == null)
                return;

            SelectModel selectModel = null;

            _musicAlbumPlaylistSelected.ForEach(ma =>
            {
                var mModel = ma.MusicsModel.Where(m => string.Equals(m.VideoId, musicModel.VideoId))
                                           .FirstOrDefault();

                if (mModel != null)
                {
                    ma.MusicsModel.Remove(mModel);
                }
                if (ma.Id == albumId)
                {
                    ma.MusicsModel.Add(new UserMusicSelect() { VideoId = musicModel.VideoId });
                    selectModel = new SelectModel(ma.Id, ma.AlbumName);
                }
            });

            await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, _musicAlbumPlaylistSelected);

            musicModel.SelectMusicAlbum(selectModel);
            musicModel.SetAlbumMode();
        }
        public async Task DeleteMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel musicModel)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0 || musicModel == null)
                return;

            _musicAlbumPlaylistSelected.ForEach(ma =>
            {
                var mModel = ma.MusicsModel.Where(m => string.Equals(m.VideoId, musicModel.VideoId))
                                           .FirstOrDefault();

                if (mModel != null)
                {
                    ma.MusicsModel.Remove(mModel);
                }
            });

            await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, _musicAlbumPlaylistSelected);

            musicModel.SelectMusicAlbum(null);
            musicModel.SetNormalMode();
        }
        public async Task<bool> ExistsMusicAlbumPlaylist(string albumName, SearchMusicModel music)
        {
            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            if (musicAlbumPlaylist == null)
                return false;

            UserMusicAlbumSelect musicAlbumInsert = musicAlbumPlaylist.Where(ma => string.Equals(ma.AlbumName, albumName, StringComparison.OrdinalIgnoreCase))
                                                                      .FirstOrDefault();
            if (musicAlbumInsert == null)
                return false;

            return musicAlbumInsert.MusicsModel.Exists(ma => string.Equals(ma.VideoId, music.VideoId));
        }
        public async Task LoadMusicAlbumPlaylistSelected()
        {
            _musicAlbumPlaylistSelected = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);
        }
        public void LoadAlbumMusicSavedSelect()
        {
            if (_musicAlbumPlaylistSelected == null)
                return;

            AlbumMusicSavedSelectCollection.Clear();

            foreach (UserMusicAlbumSelect musicAlbum in _musicAlbumPlaylistSelected)
            {
                AlbumMusicSavedSelectCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));
            }
        }
        public void ClearPlaylistLoaded()
        {
            Playlist.Clear();
        }
        public void ClearSavedAlbumPlaylistLoaded()
        {
            SavedAlbumPlaylist.Clear();
        }
        public void ClearSavedMusicPlaylistLoaded()
        {
            SavedMusicPlaylist.Clear();
        }

        #region Private Methods
        private async Task SerializeMusicModel(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcApiSearch, MusicSearchType searchType, string icon)
        {
            IsSearching = true;
            MenuActionsEnabled = false;
            Task tskUserLocalHist = null;
            UserMusicAlbumSelect musicAlbumSelect = null;

            bool existsAnyAlbum = false;

            if (searchType == MusicSearchType.SearchAlbum)
            {
                if (string.IsNullOrWhiteSpace(AlbumSearchedName))
                    return;

                tskUserLocalHist = _albumPlayedHistoryViewModel.SaveLocalSearchHistory(AlbumSearchedName);
            }
            else if (searchType == MusicSearchType.SearchMusic)
            {
                if (string.IsNullOrWhiteSpace(MusicSearchedName))
                    return;

                tskUserLocalHist = _musicPlayedHistoryViewModel.SaveLocalSearchHistory(MusicSearchedName);
            }

            searchMusicCollection.Clear();

            Task<ApiSearchMusicModel[]> tskApiSearch = funcApiSearch(_tocaTudoApi);

            await Task.WhenAll(tskUserLocalHist, tskApiSearch);

            foreach (ApiSearchMusicModel playlistItem in tskApiSearch.Result)
            {
                if (_musicAlbumPlaylistSelected != null)
                {
                    existsAnyAlbum = _musicAlbumPlaylistSelected.Count > 0;
                    musicAlbumSelect = _musicAlbumPlaylistSelected.Where(ma =>
                    {
                        bool hasAlbum = ma.MusicsModel.Exists(mm => string.Equals(mm.VideoId, playlistItem.VideoId));
                        playlistItem.HasAlbum = hasAlbum;

                        return hasAlbum;
                    }).FirstOrDefault();
                }

                playlistItem.Icon = icon;
                searchMusicCollection.Add(new SearchMusicModel(playlistItem, null, _tocaTudoApi, _ytClient, searchType, false));
            }

            IsSearching = false;
            MenuActionsEnabled = true;
        }
        private async Task SerializeMusicModelFromDb(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcDbSearch, MusicSearchType searchType, string icon)
        {
            MenuActionsEnabled = false;
            IsSearching = true;

            searchMusicCollection.Clear();

            Task<ApiSearchMusicModel[]> tskDbSearch = funcDbSearch(_albumDbLogic);

            await Task.WhenAll(tskDbSearch);

            foreach (ApiSearchMusicModel playlistItem in tskDbSearch.Result)
            {
                playlistItem.Icon = icon;
                searchMusicCollection.Add(new SearchMusicModel(playlistItem, null, _tocaTudoApi, _ytClient, searchType, true));
            }

            IsSearching = false;
            MenuActionsEnabled = true;
        }
        private Command<UserAlbumPlayedHistory> AlbumHistoryFormEventCommand()
        {
            return new Command<UserAlbumPlayedHistory>(
                execute: (userAlbumHistory) =>
                {
                    if (_lastAlbumHistorySelected != null)
                        _lastAlbumHistorySelected.UpdAlbumSelectedColor();

                    _albumPlayedHistoryViewModel.RecentlyPlayedFormIsVisible = true;
                    _albumPlayedHistoryViewModel.RecentlyPlayedSelected = new HistoryAlbumModel()
                    {
                        AlbumName = userAlbumHistory.AlbumName,
                        VideoId = userAlbumHistory.VideoId,
                        ParseType = userAlbumHistory.ParseType
                    };

                    userAlbumHistory.UpdAlbumSelectedColor();

                    _lastAlbumHistorySelected = userAlbumHistory;
                }
            );
        }
        private AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormEventCommand()
        {
            return new AsyncCommand<UserMusicPlayedHistory>(
                execute: async (userMusicHistory) =>
                {
                    if (_lastMusicHistorySelected != null)
                        _lastMusicHistorySelected.UpdMusicSelectedColor();

                    _musicPlayedHistoryViewModel.HistoryMusicPlayingNow = new HistoryMusicModel(null, _tocaTudoApi, _ytClient, true)
                    {
                        VideoId = userMusicHistory.VideoId,
                        IsLoadded = false,
                        IsActiveMusic = true,
                        IsSelected = true,
                        MusicName = userMusicHistory.MusicName,
                    };

                    userMusicHistory.UpdMusicSelectedColor();

                    //await _musicPlayer.PlayMusic(_musicPlayedHistoryViewModel.RecentlyPlayedSelected);

                    _lastMusicHistorySelected = userMusicHistory;
                }
            );
        }
        private Command<HistoryMusicModel> MusicHistoryPlayEventCommand()
        {
            return new Command<HistoryMusicModel>(
                execute: (userMusicHistory) =>
                {
                    _musicPlayer.PlayPauseMusic(userMusicHistory);
                }
            );
        }
        private async void AudioPlayer_PlayerReady(ICommonMusicModel music)
        {
            _playerReady();

            if (music != null)
            {
                if (music.IsActiveMusic)
                {
                    bool musicSavedOnDb = await _albumDbLogic.ExistsMusicAsync(music.VideoId);

                    if (!music.IsPlaying && IsInternetAvaiable)
                        _showInterstitial(() => { _audioPlayer.Play(); });

                    if (!IsInternetAvaiable)
                        _audioPlayer.Play();

                    music.IsPlaying = true;
                    music.IconMusicDownloadVisible = !musicSavedOnDb;

                    PlayerLoaded = true;

                    music.IsBufferingMusic = false;
                    music.IsLoadded = true;

                    if (_musicPlayedHistoryViewModel.HistoryMusicPlayingNow != null)
                        _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsLoadded = true;
                }
            }

            _music = music;
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
        #endregion
    }
}
