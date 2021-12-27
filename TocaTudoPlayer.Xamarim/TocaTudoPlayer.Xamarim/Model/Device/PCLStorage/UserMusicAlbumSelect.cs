using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim
{
    public class UserMusicAlbumSelect
    {
        public short Id { get; set; }
        public string AlbumName { get; set; }
        public List<UserMusicSelect> MusicsModel { get; set; } = new List<UserMusicSelect>() { };
        public long TimestampIn { get; set; }
    }
    public class UserMusicSelect 
    {
        public short Id { get; set; }
        public string MusicName { get; set; }
        public string VideoId { get; set; }
    }
}
