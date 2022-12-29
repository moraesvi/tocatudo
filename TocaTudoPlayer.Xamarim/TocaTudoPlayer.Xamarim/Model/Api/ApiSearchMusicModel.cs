using System;

namespace TocaTudoPlayer.Xamarim
{
    public class ApiSearchMusicModel
    {
        public ApiSearchMusicModel()
        {
        
        }
        public ApiSearchMusicModel(SearchMusicModel musicModel)
        {
            Id = musicModel.Id;
            NomeAlbum = musicModel.MusicName;
            Icon = musicModel.TypeIcon;
            VideoId = musicModel.VideoId;
            TipoParse = musicModel.TipoParse;
            MusicTime = musicModel.MusicTime;
            MusicTimeTotalSeconds = musicModel.MusicTimeTotalSeconds;
            MusicImageUrl = musicModel.MusicImageUrl;
        }
        public ApiSearchMusicModel(HistoryAlbumModel musicModel)
        {
            VideoId = musicModel.VideoId;
            TipoParse = new int[] { musicModel.ParseType };
        }
        public short Id { get; set; }
        public string NomeAlbum { get; set; }
        public bool HasAlbum { get; set; } = false;
        public string Icon { get; set; }
        public string VideoId { get; set; }
        public string MusicTime { get; set; }
        public long MusicTimeTotalSeconds { get; set; }
        public string MusicImageUrl { get; set; }
        public bool MusicDataStoraged { get; set; }
        public int[] TipoParse { get; set; }
    }
}
