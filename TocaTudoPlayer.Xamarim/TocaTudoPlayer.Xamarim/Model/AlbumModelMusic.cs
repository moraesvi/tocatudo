namespace TocaTudoPlayer.Xamarim
{
    public class AlbumModelMusic
    {
        private AlbumModel _album;
        private byte[] _music;
        public AlbumModelMusic(AlbumModel album, byte[] music) 
        {
            _album = album;
            _music = music;
        }
        public AlbumModel Album => _album;
        public byte[] Music => _music;
    }
}
