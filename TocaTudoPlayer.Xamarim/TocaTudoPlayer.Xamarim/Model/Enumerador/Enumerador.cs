namespace TocaTudoPlayer.Xamarim
{
    public enum AlbumParseType
    {
        NaoDefinido = -1,
        Pagina = 1,
        Comentario = 2,
        InDatabase = 3
    }
    public enum MusicSearchType
    {
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
