using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class Music : IEqualityComparer<Music>
    {
        [JsonProperty(PropertyName = "index")]
        public Int16 Index { get; set; }

        [JsonProperty(PropertyName = "videoId")]
        public string VideoId { get; set; }

        [JsonProperty(PropertyName = "musicName")]
        public string MusicName { get; set; }

        public bool Equals(Music x, Music y) => string.Equals(x.VideoId, y.VideoId);

        public int GetHashCode(Music obj) => obj.GetHashCode();
    }
}
