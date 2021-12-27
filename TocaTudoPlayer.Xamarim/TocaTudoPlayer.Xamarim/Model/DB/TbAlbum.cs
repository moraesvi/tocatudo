using SQLite;

namespace TocaTudoPlayer.Xamarim
{
    [Table("tb_album")]
    public class TbAlbum : BaseDB
    {
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("u_album_id")]
        [NotNull]
        public string UAlbumlId { get; set; }

        [Column("video_id")]
        [MaxLength(10)]
        [NotNull]
        public string VideoId { get; set; }

        [Column("album_name")]
        [MaxLength(500)]
        [NotNull]
        public string AlbumName { get; set; }

        [Column("is_album_music_compressed")]
        public bool IsAlbumMusicCompressed { get; set; }

        [Column("album_music_path")]
        [NotNull]
        public string AlbumMusicPath { get; set; }
    }
}
