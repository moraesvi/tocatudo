using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TocaTudoPlayer.Xamarim
{
    [Table("tb_album_musica")]
    public class TbAlbumMusic : BaseDB
    {
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("api_id")]
        [NotNull]
        public short ApiId { get; set; }

        [Column("name")]
        [MaxLength(200)]
        [NotNull]
        public string Name { get; set; }

        [Column("seconds")]
        [NotNull]
        public int Seconds { get; set; }

        [Column("start_seconds")]
        [NotNull]
        public int StartSeconds { get; set; }

        [Column("end_seconds")]
        [NotNull]
        public int EndSeconds { get; set; }

        [Column("seconds_desc")]
        [MaxLength(10)]
        [NotNull]
        public string SecondsDesc { get; set; }

        [ForeignKey(typeof(TbAlbum))]
        [Column("fk_album")]
        public int FkAlbum { get; set; }
    }
}
