using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class TbAppException : IEqualityComparer<TbAppException>
    {
        public static DateTime _dtNow;
        public TbAppException()
        {
            _dtNow = DateTime.UtcNow;
        }
        public TbAppException(AppException appEx)
        {
            _dtNow = DateTime.UtcNow;

            LocalDate = appEx.LocalDate;
            Msg = appEx.Msg;
            InnerMsg = appEx.InnerMsg;
            StackTrace = appEx.StackTrace;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "localDate")]
        public string LocalDate { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Msg { get; set; }

        [JsonProperty(PropertyName = "innerMsg")]
        public string InnerMsg { get; set; }

        [JsonProperty(PropertyName = "stackTrace")]
        public string StackTrace { get; set; }

        [JsonProperty(PropertyName = "dtAdd")]
        public DateTime DtAdd
        {
            get { return _dtNow; }
        }

        public bool Equals(TbAppException x, TbAppException y) => string.Equals(x.Id, y.Id);

        public int GetHashCode(TbAppException obj) => obj.GetHashCode();
    }
}
