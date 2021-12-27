using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TocaTudoPlayer.Xamarim
{
    public class UserMusic : UserAlbumMusicBase
    {
        private const string USER_MUSIC_SAVED_LOCAL_KEY = "u_music_s";
        private static List<UserMusic> _lstUserMusic;
        public UserMusic() { }
        public UserMusic(MusicModel music, bool isMusicCompressed)
        {
            VideoId = music.VideoId;
            MusicName = music.MusicName;
            MusicImage = music.MusicImage;
            IsMusicCompressed = isMusicCompressed;
            DateTimeIn = DateTimeOffset.UtcNow.ToString();

            GenerateLocalKey();
        }
        public string UMusicId { get; set; }
        public string VideoId { get; set; }
        public string MusicName { get; set; }
        public byte[] MusicImage { get; set; }
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
        public async Task<string> GetFileNameLocalPath()
        {
            return await base.GetFileNameLocalPath(VideoId);
        }

        #region Private Methods
        private void GenerateLocalKey()
        {
            UMusicId = $"{USER_MUSIC_SAVED_LOCAL_KEY}_{Guid.NewGuid().ToString()}.json";
        }
        private static async Task<List<UserMusic>> LoadDatabase(IPCLStorageDb pclStorage)
        {
            return await pclStorage.GetJson<List<UserMusic>>(UserMusic.UserMusicSavedLocalKey) ?? new List<UserMusic> { };
        }
        #endregion
    }
}
