using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public abstract class MusicAlbumPageBaseViewModel : BaseViewModel
    {
        private readonly IDbLogic _dbLogic;
        private readonly IPCLUserAlbumLogic _pclUserAlbumLogic;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly CommonPageViewModel _commonPageViewModel;
        private readonly CommonMusicPageViewModel _commonMusicPageViewModel;
        private readonly CommonMusicPlayerViewModel _musicPlayerViewModel;
        private readonly CommonFormDownloadViewModel _formDownloadViewModel;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly YoutubeClient _ytClient;
        private ObservableCollection<SearchMusicModel> _musicPlaylist;

        private bool _isReady;
        private bool _isSearching;
        private bool _playlistIsVisible;

        protected WeakEventManager _playerReady;
        public MusicAlbumPageBaseViewModel(IDbLogic dbLogic, IPCLUserAlbumLogic pclUserAlbumLogic, IPCLUserMusicLogic pclUserMusicLogic, CommonPageViewModel commonPageViewModel, CommonMusicPageViewModel commonMusicPageViewModel, CommonMusicPlayerViewModel musicPlayerViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
        {
            _dbLogic = dbLogic;
            _pclUserAlbumLogic = pclUserAlbumLogic;
            _pclUserMusicLogic = pclUserMusicLogic;
            _commonPageViewModel = commonPageViewModel;
            _commonMusicPageViewModel = commonMusicPageViewModel;
            _musicPlayerViewModel = musicPlayerViewModel;
            _tocaTudoApi = tocaTudoApi;
            _audioPlayer = DependencyService.Get<IAudio>();
            _ytClient = ytClient;
            _isReady = true;

            _musicPlaylist = new ObservableCollection<SearchMusicModel>();
            _audioPlayer.PlayerReady += AudioPlayer_CommonPlayerReady;
        }
        public event EventHandler PlayerReady
        {
            add => _playerReady.AddEventHandler(value);
            remove => _playerReady.RemoveEventHandler(value);
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
        public ICommonMusicModel ReadyMusic { get; set; }
        protected IDbLogic DbLogic
        {
            get { return _dbLogic; }
        }
        protected YoutubeClient YtClient
        {
            get { return _ytClient; }
        }
        public bool PlaylistIsVisible
        {
            get { return _playlistIsVisible; }
            set
            {
                _playlistIsVisible = value;
                OnPropertyChanged(nameof(PlaylistIsVisible));
            }
        }
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                OnPropertyChanged(nameof(IsReady));
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
        public async Task SerializeMusicModelFromDb(ObservableCollection<SearchMusicModel> searchMusicCollection)
        {
            IsReady = false;
            IsSearching = true;
            UserMusicAlbumSelect musicAlbumSelect = null;

            searchMusicCollection.Clear();

            await _pclUserMusicLogic.LoadDb();

            UserMusic[] userMusics = await _pclUserMusicLogic.GetMusics();

            foreach (UserMusic playlistItem in userMusics)
            {
                if (_commonMusicPageViewModel.MusicAlbumPlaylistSelected != null)
                {
                    musicAlbumSelect = _commonMusicPageViewModel.MusicAlbumPlaylistSelected.Where(ma =>
                    {
                        playlistItem.HasAlbum = ma.MusicsModel.Exists(mm => string.Equals(mm?.VideoId, playlistItem.VideoId));
                        return playlistItem.HasAlbum;
                    }).FirstOrDefault();
                }

                if (string.Equals(playlistItem.VideoId, ReadyMusic?.VideoId) && ReadyMusic?.SearchType != MusicSearchType.SearchMusicHistory)
                {
                    MusicPlaylist.Add((SearchMusicModel)ReadyMusic);
                }
                else if (musicAlbumSelect != null)
                {
                    searchMusicCollection.Add(new SearchMusicModel(musicAlbumSelect, playlistItem, Icon.ArrowDown, _formDownloadViewModel, _tocaTudoApi, YtClient, true));
                }
                else
                {
                    searchMusicCollection.Add(new SearchMusicModel(playlistItem, Icon.ArrowDown, _formDownloadViewModel, _tocaTudoApi, YtClient, true));
                }
            }

            IsReady = true;
            IsSearching = false;
        }
        public async Task SerializeAlbumModelFromDb(ObservableCollection<SearchMusicModel> searchMusicCollection)
        {
            IsReady = false;
            IsSearching = true;

            searchMusicCollection.Clear();

            await _pclUserAlbumLogic.LoadDb();

            UserAlbum[] userMusics = _pclUserAlbumLogic.GetAlbums();

            foreach (UserAlbum playlistItem in userMusics)
            {
                searchMusicCollection.Add(new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, true));
            }

            IsReady = true;
            IsSearching = false;
        }
        public void LoadAlbumMusicPlaylist(string albumId, string albumName)
        {
            _musicPlayerViewModel.SetAlbumMusicPlaylist(new AlbumMusicModelServicePlayer(albumId, albumName, MusicPlaylist.ToArray()));
        }
        private void AudioPlayer_CommonPlayerReady(object sender, ICommonMusicModel music)
        {
            Task.Run(() =>
            {
                if (music != null)
                {
                    MusicPlaylist.Where(m => !string.Equals(m.VideoId, music.VideoId))
                                 .ForEach(m =>
                                 {
                                     m.IsPlaying = false;
                                     m.IsSelected = false;
                                     m.IsActiveMusic = false;
                                     m.IsBufferingMusic = false;
                                 });

                    music.IsPlaying = true;
                    music.IsSelected = true;
                    music.IsActiveMusic = true;
                }
            });
        }
        protected async void AudioPlayer_PlayerAlbumMusicPlaylistChanged(object sender, ItemServicePlayer obj)
        {
            if (AppHelper.MusicPlayerInterstitialIsLoadded || AppHelper.HasInterstitialToShow)
                return;

            if (_musicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchSavedMusic && _musicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchMusicAlbumHistory)
                return;

            SearchMusicModel musicModel = MusicPlaylist.Where(item => string.Equals(item.VideoId, obj.VideoId))
                                                       .FirstOrDefault();

            MusicPlaylist.ForEach(m =>
            {
                m.IsPlaying = false;
                m.IsSelected = false;
                m.IsActiveMusic = false;
                m.IsBufferingMusic = false;
            });

            musicModel.IsActiveMusic = true;
            musicModel.IsSelected = true;

            if (_musicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchSavedMusic)
                await _musicPlayerViewModel.PlaySavedMusic(musicModel)
                                           .OnError(nameof(SavedMusicPageViewModel), () =>
                                           {
                                               musicModel.IsLoadded = false;
                                               musicModel.IsActiveMusic = false;

                                               RaiseDefaultAppErrorEvent();
                                           });
            else if (_musicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicAlbumHistory)
                await _musicPlayerViewModel.PlayMusic(musicModel, new CancellationTokenSource().Token)
                                           .OnError(nameof(SavedMusicPageViewModel), () =>
                                           {
                                               musicModel.IsLoadded = false;
                                               musicModel.IsActiveMusic = false;

                                               RaiseDefaultAppErrorEvent();
                                           });
        }
        protected void AudioPlayer_OnInterstitialClosed(object sender, EventArgs e)
        {
            AppHelper.HasInterstitialToShow = false;
            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            if (ReadyMusic != null && _commonPageViewModel.SelectedAlbum == null)
            {
                ReadyMusic.IsActiveMusic = true;
                ReadyMusic.IsPlaying = true;

                _musicPlayerViewModel.PlayMusic();
            }
        }
    }
}
