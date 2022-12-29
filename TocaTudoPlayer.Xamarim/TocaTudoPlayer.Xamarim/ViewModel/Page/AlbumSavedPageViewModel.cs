using System.Collections.ObjectModel;
using System.Threading.Tasks;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumSavedPageViewModel : MusicAlbumPageBaseViewModel
    {
        private readonly AlbumSavedPlayedHistoryViewModel _albumSavedPlayedHistoryViewModel;
        private readonly CommonPageViewModel _commonPageViewModel;
        private readonly CommonMusicPageViewModel _commonMusicPageViewModel;
        private ObservableCollection<SearchMusicModel> _savedAlbumPlaylist;
        public AlbumSavedPageViewModel(IDbLogic albumDbLogic, IPCLUserAlbumLogic pclUserAlbumLogic, IPCLUserMusicLogic pclUserMusicLogic, AlbumSavedPlayedHistoryViewModel albumSavedPlayedHistoryViewModel, CommonPageViewModel commonPageViewModel, CommonMusicPageViewModel commonMusicPageViewModel, CommonMusicPlayerViewModel musicPlayerViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
            : base(albumDbLogic, pclUserAlbumLogic, pclUserMusicLogic, commonPageViewModel, commonMusicPageViewModel, musicPlayerViewModel, tocaTudoApi, ytClient)
        {
            _albumSavedPlayedHistoryViewModel = albumSavedPlayedHistoryViewModel;
            _commonPageViewModel = commonPageViewModel;
            _commonMusicPageViewModel = commonMusicPageViewModel;
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
        public AlbumSavedPlayedHistoryViewModel AlbumSavedPlayedHistoryViewModel
        {
            get { return _albumSavedPlayedHistoryViewModel; }
        }
        public CommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public async Task AlbumPlaylistSearchFromDb()
        {
            await SerializeAlbumModelFromDb(SavedAlbumPlaylist);
        }
        public void ClearSavedAlbumPlaylistLoaded()
        {
            SavedAlbumPlaylist.Clear();
        }
    }
}
