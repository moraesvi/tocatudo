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
        private long _musicTimeTotalSeconds;
        public short Id { get; set; }
        public string MusicName { get; set; }
        public string MusicTime { get; set; }
        public long MusicTimeTotalSeconds
        {
            get { return _musicTimeTotalSeconds; }
            set
            {
                _musicTimeTotalSeconds = value;
            }
        }
        public byte[] MusicImage { get; set; }
        public string VideoId { get; set; }
    }
}
