using System;
using System.Collections.Generic;
using System.Text;

namespace TocaTudoPlayer.Xamarim
{
    public class ApiResultCommon<TResult>
    {
        public bool Ok { get; set; }
        public string MsgResul { get; set; }
        public TResult Result { get; set; }
        public int Data { get; set; }
    }
}
