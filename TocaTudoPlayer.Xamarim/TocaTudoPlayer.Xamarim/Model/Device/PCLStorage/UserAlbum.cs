using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TocaTudoPlayer.Xamarim
{
    public class UserAlbum : UserAlbumMusicBase
    {
        private const string USER_ALBUM_SAVED_LOCAL_KEY = "u_album_s";
        public UserAlbum() { }
        public UserAlbum(AlbumModel album)
        {
            Album = album.Album;
            VideoId = album.VideoId;
            ParseType = album.ParseType;
            ImgAlbum = album.ByteImgAlbum;

            if (album.Playlist == null)
                throw new InvalidOperationException(nameof(album.Playlist));

            foreach (PlaylistItem item in album.Playlist)
            {
                Playlist.Add(new UserAlbumPlaylist()
                {
                    Id = item.Id,
                    Number = item.Number,
                    MusicName = item.NomeMusica,
                    TimeSeconds = item.TempoSegundosInicio,
                    SecondsStartTime = item.TempoSegundosInicio,
                    SecondsEndTime = item.TempoSegundosFim,
                    DescTime = item.TempoDesc,
                });
            }
        }
        public string Album { get; set; }
        public string VideoId { get; set; }
        public string UAlbumlId { get; set; }
        public string MusicPath { get; set; }
        public bool IsMusicCompressed { get; set; }
        public AlbumParseType ParseType { get; set; }
        public byte[] ImgAlbum { get; set; }
        public List<UserAlbumPlaylist> Playlist { get; set; } = new List<UserAlbumPlaylist>() { };
        
        [JsonIgnore]
        public DateTimeOffset DtIn
        {
            get
            {
                return Convert.ToDateTime(DateTimeIn);
            }
        }
        public string DateTimeIn { get; set; }

        public static string UserAlbumSavedLocalKey => USER_ALBUM_SAVED_LOCAL_KEY;
        public static string GenerateLocalKey()
        {
            return $"{USER_ALBUM_SAVED_LOCAL_KEY}_{Guid.NewGuid().ToString()}.json";
        }
        public async Task<string> GetFileNameLocalPath()
        {
            return await base.GetFileNameLocalPath(VideoId);
        }
    }
}
