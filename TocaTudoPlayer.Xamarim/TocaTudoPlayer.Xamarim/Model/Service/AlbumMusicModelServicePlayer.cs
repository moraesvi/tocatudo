using System.Linq;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumMusicModelServicePlayer
    {
        public AlbumMusicModelServicePlayer(string albumId, string albumName, SearchMusicModel[] playlist)
        {
            AlbumId = albumId;
            AlbumName = albumName;
            Playlist = playlist?.Select(item => new MusicModelItemServicePlayer()
            {
                PlaylistItem = item,
                Id = item.Id,
                Number = -1,
                VideoId = item.VideoId,
                Music = item.MusicName,
                TotalMilliseconds = item.MusicTimeTotalSeconds
            })?.ToArray();
        }
        public string AlbumId { get; }
        public string AlbumName { get; }
        public MusicModelItemServicePlayer[] Playlist { get; }
    }
    public class MusicModelItemServicePlayer : ItemServicePlayer
    {
        public ICommonMusicServiceModel PlaylistItem { get; internal set; }
        public short Id { get; internal set; }
        public short Number { get; internal set; }
        public string VideoId { get; internal set; }
        public string Music { get; internal set; }
        public byte[] Image { get; internal set; }
        public long TotalMilliseconds { get; internal set; }
    }
}
