namespace TocaTudoPlayer.Xamarim.ViewModel.CustomView
{
    public class MusicSavedPlayedHistoryViewModel : MusicPlayedHistoryViewModel, IMusicSavedPlayedHistoryViewModel
    {
        private const string USER_MUSIC_SAVED_LOCAL_SEARCH_HISTORY_KEY = "ms_history.json";
        private const string USER_LOCAL_MUSIC_SAVED_PLAYED_HISTORY_KEY = "msp_history.json";
        public MusicSavedPlayedHistoryViewModel(IPCLStorageDb pclStorageDb, IPCLUserMusicLogic _pclUserMusicLogic)
            : base(USER_MUSIC_SAVED_LOCAL_SEARCH_HISTORY_KEY, USER_LOCAL_MUSIC_SAVED_PLAYED_HISTORY_KEY, pclStorageDb, _pclUserMusicLogic)
        {
        
        }
    }
}
