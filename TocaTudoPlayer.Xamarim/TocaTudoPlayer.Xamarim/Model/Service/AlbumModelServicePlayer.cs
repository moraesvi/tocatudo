using System.Linq;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumModelServicePlayer
    {        
        public AlbumModelServicePlayer(string albumId, string albumName, byte[] image, PlaylistItem[] playlist)
        {
            AlbumId = albumId;
            AlbumName = albumName;
            Image = image;
            Playlist = playlist?.Select(item => new PlaylistItemServicePlayer()
            {
                AlbumId = albumId,
                Id = item.Id,
                Number = item.Number,
                Music = item.NomeMusica,
                TotalMilliseconds = item.TempoSegundosFim
            })?.ToArray();
        }
        public string AlbumId { get; }
        public string AlbumName { get; }
        public byte[] Image { get; }
        public PlaylistItemServicePlayer[] Playlist { get; }
    }
    public class PlaylistItemServicePlayer
    {
        public PlaylistItemServicePlayer() { }
        public PlaylistItemServicePlayer(string albumId, PlaylistItem playlistItem)
        {
            AlbumId = albumId;
            Id = playlistItem.Id;
            Number = playlistItem.Number;
            Music = playlistItem.NomeMusica;
            TotalMilliseconds = playlistItem.TempoSegundosFim;
        }
        public string AlbumId { get; internal set; }
        public short Id { get; internal set; }
        public short Number { get; internal set; }
        public string Music { get; internal set; }
        public int TotalMilliseconds { get; internal set; }
    }
}
