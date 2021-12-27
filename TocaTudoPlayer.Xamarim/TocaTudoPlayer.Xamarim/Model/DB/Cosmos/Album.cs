using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class Album : IEqualityComparer<Album>
    {
        [JsonProperty(PropertyName = "index")]
        public Int16 Index { get; set; }

        [JsonProperty(PropertyName = "musicName")]
        public string MusicName { get; set; }

        [JsonProperty(PropertyName = "videoId")]
        public string VideoId { get; set; }

        [JsonProperty(PropertyName = "tipoParse")]
        public int TipoParse { get; set; }

        public bool Equals(Album x, Album y) => string.Equals(x.VideoId, y.VideoId);

        public int GetHashCode(Album obj) => obj.GetHashCode();
    }
}
