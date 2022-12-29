using NetEscapades.EnumGenerators;

namespace TocaTudoPlayer.Xamarim
{
    [EnumExtensions]
    public enum AlbumParseType
    {
        NaoDefinido = -1,
        Pagina = 1,
        Comentario = 2,
        InDatabase = 3
    }
    public enum DownloadQueueStatus
    {
        MusicQueued,
        AchievedMaxQueue,
        MusicNotFound,
        ErrorHasOccurred,
    }
    public enum MusicSearchType
    {
        Undefined,
        SearchAlbum,
        SearchMusic,
        SearchSavedMusic,
        SearchSavedAlbum,
        SearchMusicHistory,
        SearchMusicAlbumHistory,
    }
    public enum PlayerType
    {
        Album,
        Music,
    }
}
