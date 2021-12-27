using SQLite;
using System;

namespace TocaTudoPlayer.Xamarim
{
    public abstract class BaseDB
    {
        [Column("is_active")]
        [NotNull]
        public bool IsActive { get; set; } = true;
        
        [Column("dt_add")]
        [NotNull]
        public DateTimeOffset DtAdd { get; set; }
        
        [Column("dt_upd")]
        public DateTimeOffset DtUpd { get; set; }
    }
}
