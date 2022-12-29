using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class UserMusic : UserAlbumMusicBase
    {
        private const string USER_MUSIC_SAVED_LOCAL_KEY = "u_music_s";
        private long _musicTimeTotalSeconds;
        public UserMusic() 
        {
            GenerateLocalKey();
        }
        public UserMusic(MusicModel music, bool isMusicCompressed)
        {
            VideoId = music.VideoId;
            MusicName = music.MusicName;
            MusicImage = music.MusicImage;
            MusicTime = music.MusicTime;
            MusicTimeTotalSeconds = music.MusicTimeTotalSeconds;
            IsMusicCompressed = isMusicCompressed;
            DateTimeIn = DateTimeOffset.UtcNow.ToString();

            GenerateLocalKey();
        }
        public string UMusicId { get; private set; }
        public string VideoId { get; set; }
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
        public bool HasAlbum { get; set; }
        public bool IsMusicCompressed { get; set; }
        public string MusicPath { get; set; }
        public static string UserMusicSavedLocalKey => USER_MUSIC_SAVED_LOCAL_KEY;

        [JsonIgnore]
        public DateTimeOffset DtIn
        {
            get
            {
                return Convert.ToDateTime(DateTimeIn);
            }
        }
        public string DateTimeIn { get; set; }
        public string GetFileNameLocalPath()
        {
            return base.GetFileNameLocalPath(VideoId);
        }

        #region Private Methods
        private void GenerateLocalKey()
        {
            UMusicId = $"{USER_MUSIC_SAVED_LOCAL_KEY}_{Guid.NewGuid().ToString()}.json";
        }
        #endregion
    }
}
