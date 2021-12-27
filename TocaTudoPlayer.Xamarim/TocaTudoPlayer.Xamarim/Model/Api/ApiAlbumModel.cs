namespace TocaTudoPlayer.Xamarim
{
    public class ApiAlbumModel
    {
        public string Album { get; set; }
        public string ImgAlbum { get; set; }
        public ApiPlaylist[] Playlist { get; set; } 
    }
    public class ApiPlaylist
    {
        public short Id { get; set; }
        public string NomeMusica { get; set; }
        public int TempoSegundos { get; set; }
        public int TempoSegundosInicio { get; set; }
        public int TempoSegundosFim { get; set; }
        public string TempoDesc { get; set; }      
    }
}
