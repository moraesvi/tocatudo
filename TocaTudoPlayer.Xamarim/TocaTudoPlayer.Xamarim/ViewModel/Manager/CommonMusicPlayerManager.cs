namespace TocaTudoPlayer.Xamarim
{
    public class CommonMusicPlayerManager 
    {
        private static MusicPageViewModel _musicPageViewModel;
        private static SavedMusicPageViewModel _musicSavedPageViewModel;
        private static AlbumPlayerViewModel _albumPlayerViewModel;       
        public static void Init(AlbumPlayerViewModel albumPlayerViewModel, MusicPageViewModel musicPageViewModel, SavedMusicPageViewModel musicSavedPageViewModel) 
        {
            _albumPlayerViewModel = albumPlayerViewModel;
            _musicPageViewModel = musicPageViewModel;
            _musicSavedPageViewModel = musicSavedPageViewModel;
        }
        public static void SetActiveMusicBottomPlayer()
        {
            if (_musicPageViewModel.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusic || _musicPageViewModel.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicHistory)
            {
                StopSavedMusicAndAlbumPlaying();
            }
            else if(_musicSavedPageViewModel.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchSavedMusic) 
            {
                StopMusicAndAlbumPlaying();
            }
        }
        public static void StopAllMusicBottomPlayers() 
        {
            if (!_musicPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive || !_musicSavedPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive)
                return;

            _musicPageViewModel.MusicPlayerViewModel.StopBottomPlayer(force: true);
            _musicSavedPageViewModel.MusicPlayerViewModel.StopBottomPlayer(force: true);
        }
        public static void StopAllAlbumBottomPlayers()
        {
            if (!_albumPlayerViewModel.BottomPlayerViewModel.PlayerIsActive)
                return;

            _albumPlayerViewModel.BottomPlayerViewModel.StopBottomPlayer(force: true);
        }
        public static void SetActiveAlbumBottomPlayer()
        {
            StopMusicAndSavedMusicPlaying();
        }

        #region Private Methods
        private static void StopSavedMusicAndAlbumPlaying() 
        {
            if ((_musicPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive || _musicSavedPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive) && !_albumPlayerViewModel.BottomPlayerViewModel.PlayerIsActive)
                return;

            _musicSavedPageViewModel.MusicPlayerViewModel.StopBottomPlayer(force: true);
            _albumPlayerViewModel.BottomPlayerViewModel.StopBottomPlayer(force: true);

            _musicPageViewModel.MusicPlayerViewModel.ActiveBottomPlayer();
        }
        private static void StopMusicAndAlbumPlaying()
        {
            if ((_musicSavedPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive || _musicPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive) && !_albumPlayerViewModel.BottomPlayerViewModel.PlayerIsActive)
                return;

            _musicPageViewModel.MusicPlayerViewModel.StopBottomPlayer(force: true);
            _albumPlayerViewModel.BottomPlayerViewModel.StopBottomPlayer(force: true);

            _musicSavedPageViewModel.MusicPlayerViewModel.ActiveBottomPlayer();
        }
        private static void StopMusicAndSavedMusicPlaying()
        {
            if (_albumPlayerViewModel.BottomPlayerViewModel.PlayerIsActive && !_musicPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive && !_musicSavedPageViewModel.MusicPlayerViewModel.BottomPlayerIsActive)
                return;

            _musicPageViewModel.MusicPlayerViewModel.StopBottomPlayer(force: true);
            _musicSavedPageViewModel.MusicPlayerViewModel.StopBottomPlayer(force: true);

            _albumPlayerViewModel.BottomPlayerViewModel.ActiveBottomPlayer();
        }
        #endregion
    }
}
