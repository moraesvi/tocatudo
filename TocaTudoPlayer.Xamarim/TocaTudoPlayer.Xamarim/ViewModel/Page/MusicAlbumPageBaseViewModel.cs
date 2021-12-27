using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public abstract class MusicAlbumPageBaseViewModel : BaseViewModel, IMusicAlbumPageBaseViewModel
    {
        private readonly IDbLogic _dbLogic;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly ICommonFormDownloadViewModel _formDownloadViewModel;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly YoutubeClient _ytClient;

        private bool _isSearching;

        protected event Action _playerReady;
        protected event Action _showInterstitial;
        protected event Action<Action> _actionShowInterstitial;
        public MusicAlbumPageBaseViewModel(IDbLogic dbLogic, IPCLUserMusicLogic pclUserMusicLogic, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
        {
            _dbLogic = dbLogic;
            _pclUserMusicLogic = pclUserMusicLogic;
            _tocaTudoApi = tocaTudoApi;
            _ytClient = ytClient;
            _isSearching = false;

            IAudio audioPlayer = DependencyService.Get<IAudio>();

            CrossMTAdmob.Current.OnInterstitialClosed += (sender, e) =>
            {
                if (MusicPlaying?.IsActiveMusic ?? false)
                    audioPlayer.Play();
            };
        }
        public MusicAlbumPageBaseViewModel(IDbLogic dbLogic, IPCLUserMusicLogic pclUserMusicLogic, ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
        {
            _dbLogic = dbLogic;
            _pclUserMusicLogic = pclUserMusicLogic;
            _formDownloadViewModel = formDownloadViewModel;
            _tocaTudoApi = tocaTudoApi;
            _ytClient = ytClient;
            _isSearching = false;

            IAudio audioPlayer = DependencyService.Get<IAudio>();

            CrossMTAdmob.Current.OnInterstitialClosed += (sender, e) =>
            {
                if (MusicPlaying?.IsActiveMusic ?? false)
                    audioPlayer.Play();
            };
        }
        public event Action PlayerReady
        {
            add => _playerReady += value;
            remove => _playerReady -= value;
        }
        public event Action ShowInterstitial
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
        public event Action<Action> ActionShowInterstitial
        {
            add
            {
                _actionShowInterstitial += value;
            }
            remove
            {
                _actionShowInterstitial -= value;
            }
        }
        protected ICommonMusicModel MusicPlaying { get; set; }
        protected IDbLogic DbLogic 
        {
            get { return _dbLogic; }
        }
        protected YoutubeClient YtClient
        {
            get { return _ytClient; }
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
            IsSearching = true;

            searchMusicCollection.Clear();

            await _pclUserMusicLogic.LoadDb();

            UserMusic[] userMusics = _pclUserMusicLogic.GetMusics();

            foreach (UserMusic playlistItem in userMusics)
            {
                searchMusicCollection.Add(new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, true));
            }

            IsSearching = false;
        }
        public async Task SerializeMusicModelFromDb(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcDbSearch, MusicSearchType searchType, string icon)
        {
            IsSearching = true;

            searchMusicCollection.Clear();

            Task<ApiSearchMusicModel[]> tskDbSearch = funcDbSearch(_dbLogic);

            await Task.WhenAll(tskDbSearch);

            foreach (ApiSearchMusicModel playlistItem in tskDbSearch.Result)
            {
                playlistItem.Icon = icon;
                searchMusicCollection.Add(new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, searchType, true));
            }

            IsSearching = false;
        }
        protected void RaiseplayerReady()
        {
            _playerReady();
        }
        protected void RaiseShowInterstitial()
        {
            _showInterstitial();
        }
        protected void RaiseActionShowInterstitial(Action action)
        {
            _actionShowInterstitial(action);
        }
    }
}
