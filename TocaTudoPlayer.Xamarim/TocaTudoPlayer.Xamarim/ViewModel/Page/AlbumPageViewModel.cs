using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumPageViewModel : MusicAlbumPageBaseViewModel, IAlbumPageViewModel
    {
        private readonly IAlbumPlayedHistoryViewModel _albumPlayedHistoryViewModel;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly ICommonMusicPlayerViewModel _musicPlayer;
        private readonly ICommonPageViewModel _commonPageViewModel;
        private UserAlbumPlayedHistory _lastAlbumHistorySelected;

        private ObservableCollection<SearchMusicModel> _albumPlaylist;

        private bool _albumPlaylistIsVisible;
        public AlbumPageViewModel(IDbLogic albumDbLogic, IPCLUserMusicLogic pclUserMusicLogic, YoutubeClient ytClient, ITocaTudoApi tocaTudoApi, ICommonMusicPlayerViewModel musicPlayer, IAlbumPlayedHistoryViewModel albumPlayedHistoryViewModel, ICommonPageViewModel commonPageViewModel)
            : base(albumDbLogic, pclUserMusicLogic, tocaTudoApi, ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _musicPlayer = musicPlayer;
            _albumPlayedHistoryViewModel = albumPlayedHistoryViewModel;
            _commonPageViewModel = commonPageViewModel;
            _albumPlaylistIsVisible = false;
            _albumPlaylist = new ObservableCollection<SearchMusicModel>();
            SearchAlbumCommand = new SearchAlbumPlaylistCommand(this);
        }
        public bool AlbumPlaylistIsVisible
        {
            get { return _albumPlaylistIsVisible; }
            set
            {
                _albumPlaylistIsVisible = value;

                OnPropertyChanged(nameof(AlbumPlaylistIsVisible));
            }
        }
        public string AlbumSearchedName { get; set; }
        public ICommonMusicPlayerViewModel MusicPlayer
        {
            get { return _musicPlayer; }
        }
        public IAlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel
        {
            get { return _albumPlayedHistoryViewModel; }
        }
        public ICommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public ObservableCollection<SearchMusicModel> AlbumPlaylist
        {
            get { return _albumPlaylist; }
            set
            {
                _albumPlaylist = value;
                OnPropertyChanged(nameof(AlbumPlaylist));
            }
        }
        public ICommand SearchAlbumCommand { get; set; }
        public Command<UserAlbumPlayedHistory> AlbumHistoryFormCommand => AlbumHistoryFormEventCommand();
        public async Task AlbumPlaylistSearch()
        {
            AlbumPlaylistIsVisible = true;

            Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcSearchPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.SearchPlaylistEndpoint(AlbumSearchedName);
            };

            Task tskAlbumLocalHist = _albumPlayedHistoryViewModel.SaveLocalSearchHistory(AlbumSearchedName);

            await SerializeMusicModel(AlbumPlaylist, funcSearchPlaylist, tskAlbumLocalHist, MusicSearchType.SearchAlbum, Icon.FileImageO);
        }

        #region Private Methods
        private async Task SerializeMusicModel(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcApiSearch, Task tskAlbumLocalHist, MusicSearchType searchType, string icon)
        {
            IsSearching = true;

            searchMusicCollection.Clear();

            Task<ApiSearchMusicModel[]> tskApiSearch = funcApiSearch(_tocaTudoApi);

            await Task.WhenAll(tskAlbumLocalHist, tskApiSearch);

            foreach (ApiSearchMusicModel playlistItem in tskApiSearch.Result)
            {
                playlistItem.Icon = icon;
                searchMusicCollection.Add(new SearchMusicModel(playlistItem, _tocaTudoApi, YtClient, searchType, false));
            }

            IsSearching = false;
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
                        AlbumImg = userAlbumHistory.ImgAlbum,
                        VideoId = userAlbumHistory.VideoId,
                        ParseType = userAlbumHistory.ParseType
                    };

                    userAlbumHistory.UpdAlbumSelectedColor();

                    _lastAlbumHistorySelected = userAlbumHistory;
                }
            );
        }
        #endregion
    }
}
