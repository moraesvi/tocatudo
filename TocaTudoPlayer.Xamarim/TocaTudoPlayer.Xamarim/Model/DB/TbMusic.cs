using SQLite;

namespace TocaTudoPlayer.Xamarim
{
    [Table("tb_music")]
    public class TbMusic : BaseDB
    {
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("u_music_id")]
        [NotNull]
        public string UMusicId { get; set; }

        [Column("video_id")]
        [MaxLength(10)]
        [NotNull]
        public string VideoId { get; set; }

        [Column("music_name")]
        [NotNull]
        public string MusicName { get; set; }

        [Column("is_music_compressed")]
        public bool IsMusicCompressed { get; set; }

        [Column("music_path")]
        [NotNull]
        public string MusicPath { get; set; }
    }
}
