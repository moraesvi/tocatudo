using Newtonsoft.Json;
using System;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class BaseCosmos
    {
        public static DateTime _dtNow;
        public BaseCosmos()
        {
            _dtNow = DateTime.UtcNow;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "isActive")]
        public bool IsActive { get; set; } = true;

        [JsonProperty(PropertyName = "dtAdd")]
        public DateTime DtAdd
        {
            get { return _dtNow; }
        }

        [JsonProperty(PropertyName = "dtUpd")]
        public DateTime? DtUpd { get; set; }
    }
}
