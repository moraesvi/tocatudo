using Newtonsoft.Json;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{

    public class TbSelectedAlbum : BaseCosmos, IEqualityComparer<TbSelectedAlbum>
    {
        public TbSelectedAlbum() { }
        public TbSelectedAlbum(ApiAlbumModel albumModel)
        {
            AlbumName = albumModel.Album;
            ImgAlbum = albumModel.ImgAlbum;

            foreach (ApiPlaylist item in albumModel.Playlist)
            {
                Album.Add(new AlbumMusic(item));
            }
        }

        [JsonProperty(PropertyName = "videoId")]
        public string VideoId { get; set; }

        [JsonProperty(PropertyName = "albumName")]
        public string AlbumName { get; set; }

        [JsonProperty(PropertyName = "imgAlbum")]
        public string ImgAlbum { get; set; }

        [JsonProperty(PropertyName = "album")]
        public List<AlbumMusic> Album { get; set; } = new List<AlbumMusic>();

        public bool Equals(TbSelectedAlbum x, TbSelectedAlbum y) => x.Id == y.Id;

        public int GetHashCode(TbSelectedAlbum obj) => obj.GetHashCode();
    }
    public class AlbumMusic
    {
        public AlbumMusic(ApiPlaylist item)
        {
            Id = item.Id;
            MusicName = item.NomeMusica;
            TimeSeconds = item.TempoSegundos;
            StartTimeSeconds = item.TempoSegundosInicio;
            EndTimeSeconds = item.TempoSegundosFim;
            TimeDesc = item.TempoDesc;
        }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "musicName")]
        public string MusicName { get; set; }

        [JsonProperty(PropertyName = "timeSeconds")]
        public int TimeSeconds { get; set; }

        [JsonProperty(PropertyName = "startTimeSeconds")]
        public int StartTimeSeconds { get; set; }

        [JsonProperty(PropertyName = "endTimeSeconds")]
        public int EndTimeSeconds { get; set; }

        [JsonProperty(PropertyName = "timeDesc")]
        public string TimeDesc { get; set; }

        public bool Equals(AlbumMusic x, AlbumMusic y) => x.Id == y.Id;

        public int GetHashCode(AlbumMusic obj) => obj.GetHashCode();
    }
}
