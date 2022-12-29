using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumPageViewModel : MusicAlbumPageBaseViewModel
    {
        private readonly AlbumPlayedHistoryViewModel _albumPlayedHistoryViewModel;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly CommonMusicPlayerViewModel _musicPlayer;
        private readonly CommonPageViewModel _commonPageViewModel;
        private UserAlbumPlayedHistory _lastAlbumHistorySelected;
        private ObservableCollection<SearchMusicModel> _albumPlaylist;

        private bool _albumPlaylistIsVisible;
        public AlbumPageViewModel(IDbLogic albumDbLogic, IPCLUserAlbumLogic pclUserAlbumLogic, IPCLUserMusicLogic pclUserMusicLogic, YoutubeClient ytClient, ITocaTudoApi tocaTudoApi, CommonMusicPlayerViewModel musicPlayer, AlbumPlayedHistoryViewModel albumPlayedHistoryViewModel, CommonPageViewModel commonPageViewModel, CommonMusicPageViewModel commonMusicPageViewModel)
            : base(albumDbLogic, pclUserAlbumLogic, pclUserMusicLogic, commonPageViewModel, commonMusicPageViewModel, musicPlayer, tocaTudoApi, ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _musicPlayer = musicPlayer;
            _albumPlayedHistoryViewModel = albumPlayedHistoryViewModel;
            _commonPageViewModel = commonPageViewModel;
            _albumPlaylistIsVisible = false;
            _albumPlaylist = new ObservableCollection<SearchMusicModel>();
            SearchAlbumCommand = new SearchAlbumPlaylistCommand(this);
        }
        public string AlbumSearchedName { get; set; }
        public CommonMusicPlayerViewModel MusicPlayer
        {
            get { return _musicPlayer; }
        }
        public AlbumPlayedHistoryViewModel AlbumPlayedHistoryViewModel
        {
            get { return _albumPlayedHistoryViewModel; }
        }
        public CommonPageViewModel CommonPageViewModel
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
        public IAsyncCommand SearchAlbumCommand { get; set; }
        public Command<UserAlbumPlayedHistory> AlbumHistoryFormCommand => AlbumHistoryFormEventCommand();
        public async Task AlbumPlaylistSearch()
        {
            Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcSearchPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.SearchPlaylistEndpoint(AlbumSearchedName);
            };

            Task tskAlbumLocalHist = _albumPlayedHistoryViewModel.SaveLocalSearchHistory(AlbumSearchedName);

            await SerializeMusicModel(AlbumPlaylist, funcSearchPlaylist, tskAlbumLocalHist, MusicSearchType.SearchAlbum, Icon.FileImageO)
                  .OnError(nameof(AlbumPageViewModel), () =>
                  {
                      IsReady = true;
                      IsSearching = false;
                      RaiseAppErrorEvent(AppResource.AppDefaultError);
                  });

            App.EventTracker.SendEvent("AlbumPlaylistSearch", new Dictionary<string, string>()
            {
                { "AlbumSearched", AlbumSearchedName },
            });
        }

        #region Private Methods
        private async Task SerializeMusicModel(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcApiSearch, Task tskAlbumLocalHist, MusicSearchType searchType, string icon)
        {
            IsReady = false;
            IsSearching = true;
            PlaylistIsVisible = false;

            searchMusicCollection.Clear();

            Task<ApiSearchMusicModel[]> tskApiSearch = funcApiSearch(_tocaTudoApi);
                              
            await Task.WhenAll(tskAlbumLocalHist, tskApiSearch);

            (string VideoId, string MusicImageUrl, bool MusicImageUrlStoraged)[] apiSearchUri = new (string, string, bool)[] { };

            await Task.Run(() =>
            {
                apiSearchUri = tskApiSearch.Result.Select(search => (search.VideoId, search.MusicImageUrl, search.MusicDataStoraged))
                                              .ToArray();

                foreach (ApiSearchMusicModel playlistItem in tskApiSearch.Result)
                {
                    SearchMusicModel music = new SearchMusicModel(playlistItem, _tocaTudoApi, YtClient, searchType, false);
                    music.MusicImageUrl = playlistItem.MusicDataStoraged ? music.MusicImageUrl : null;
                    playlistItem.Icon = icon;

                    PlaylistIsVisible = true;
                    searchMusicCollection.Add(music);
                }

                IsReady = true;
                IsSearching = false;
                IsInternetAvaiable = true;
            });

            await Task.Delay(1500).ContinueWith(tsk =>
            {
                if (tsk.IsCompleted)
                {
                    foreach (SearchMusicModel musicModel in searchMusicCollection)
                    {
                        foreach (var musicImgModel in apiSearchUri)
                        {
                            if (string.Equals(musicModel.VideoId, musicImgModel.VideoId) && !musicImgModel.MusicImageUrlStoraged)
                            {
                                musicModel.MusicImageUrl = musicImgModel.MusicImageUrl;
                            }
                        }
                    }
                }
            }).ConfigureAwait(false);
        }
        private Command<UserAlbumPlayedHistory> AlbumHistoryFormEventCommand()
        {
            return new Command<UserAlbumPlayedHistory>(
                execute: (userAlbumHistory) =>
                {
                    if (_lastAlbumHistorySelected != null)
                        _lastAlbumHistorySelected.UpdAlbumSelectedColor();

                    _albumPlayedHistoryViewModel.RecentlyPlayedFormIsVisible = true;
                    _albumPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 40;
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
