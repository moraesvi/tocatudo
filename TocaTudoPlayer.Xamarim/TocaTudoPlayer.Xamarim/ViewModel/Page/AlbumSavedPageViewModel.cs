using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumSavedPageViewModel : MusicAlbumPageBaseViewModel, IAlbumSavedPageViewModel
    {
        private readonly IAlbumSavedPlayedHistoryViewModel _albumSavedPlayedHistoryViewModel;
        private readonly ICommonPageViewModel _commonPageViewModel;
        private ObservableCollection<SearchMusicModel> _savedAlbumPlaylist;
        public AlbumSavedPageViewModel(IDbLogic albumDbLogic, IPCLUserMusicLogic pclUserMusicLogic, IAlbumSavedPlayedHistoryViewModel albumSavedPlayedHistoryViewModel, ICommonPageViewModel commonPageViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
            : base(albumDbLogic, pclUserMusicLogic, tocaTudoApi, ytClient)
        {
            _albumSavedPlayedHistoryViewModel = albumSavedPlayedHistoryViewModel;
            _commonPageViewModel = commonPageViewModel;
            _savedAlbumPlaylist = new ObservableCollection<SearchMusicModel>();
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
        public IAlbumSavedPlayedHistoryViewModel AlbumSavedPlayedHistoryViewModel
        {
            get { return _albumSavedPlayedHistoryViewModel; }
        }
        public ICommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public async Task AlbumPlaylistSearchFromDb()
        {
            Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcMusicPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.GetAlbums();
            };

            await SerializeMusicModelFromDb(SavedAlbumPlaylist, funcMusicPlaylist, MusicSearchType.SearchSavedAlbum, Icon.ArrowDown);
        }
        public void ClearSavedAlbumPlaylistLoaded()
        {
            SavedAlbumPlaylist.Clear();
        }
    }
}
