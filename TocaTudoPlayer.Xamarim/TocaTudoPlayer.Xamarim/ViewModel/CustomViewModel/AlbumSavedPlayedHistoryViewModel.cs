namespace TocaTudoPlayer.Xamarim
{
    public class AlbumSavedPlayedHistoryViewModel : AlbumPlayedHistoryViewModel
    {
        private const string USER_ALBUM_SAVED_LOCAL_SEARCH_HISTORY_KEY = "as_history.json";
        private const string USER_LOCAL_ALBUM_SAVED_PLAYED_HISTORY_KEY = "asp_history.json";
        public AlbumSavedPlayedHistoryViewModel(IPCLStorageDb pclStorageDb)
            : base(USER_ALBUM_SAVED_LOCAL_SEARCH_HISTORY_KEY, USER_LOCAL_ALBUM_SAVED_PLAYED_HISTORY_KEY, pclStorageDb)
        {
        }
    }
}
