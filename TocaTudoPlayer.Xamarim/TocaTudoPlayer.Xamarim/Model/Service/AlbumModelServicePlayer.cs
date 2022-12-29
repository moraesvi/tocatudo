using System.Linq;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumModelServicePlayer : IModelServicePlayer
    {
        public AlbumModelServicePlayer(string albumId, string albumName, byte[] image, PlaylistItem[] playlist)
        {
            AlbumId = albumId;
            AlbumName = albumName;
            Image = image;
            Playlist = playlist?.Select(item => new PlaylistItemServicePlayer()
            {
                PlaylistItem = item,
                AlbumId = albumId,
                Id = item.Id,
                Number = item.Number,
                Music = item.NomeMusica,
                TotalMilliseconds = item.TempoSegundosFim
            })?.ToArray();
        }
        public string AlbumId { get; }
        public string AlbumName { get; }
        public string ImageUri { get; set; }
        public byte[] Image { get; }
        public PlaylistItemServicePlayer[] Playlist { get; }
    }
    public class PlaylistItemServicePlayer : ItemAlbumServicePlayer, ItemServicePlayer
    {
        public PlaylistItemServicePlayer() { }
        public PlaylistItemServicePlayer(string albumId, PlaylistItem playlistItem)
        {
            PlaylistItem = playlistItem;
            AlbumId = albumId;
            Id = playlistItem.Id;
            Number = playlistItem.Number;
            Music = playlistItem.NomeMusica;
            TotalMilliseconds = playlistItem.TempoSegundosFim;
        }
        public ICommonMusicServiceModel PlaylistItem { get; internal set; }
        public string AlbumId { get; internal set; }
        public short Id { get; internal set; }
        public string VideoId { get; internal set; }
        public short Number { get; internal set; }
        public string Music { get; internal set; }
        public long TotalMilliseconds { get; internal set; }
    }
}
