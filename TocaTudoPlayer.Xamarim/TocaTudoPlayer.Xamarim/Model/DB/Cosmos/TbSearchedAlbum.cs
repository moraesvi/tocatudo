using Newtonsoft.Json;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class TbSearchedAlbum : BaseCosmos, IEqualityComparer<TbSearchedAlbum>
    {
        public TbSearchedAlbum()
        {
            Playlist = new List<Album>();
        }
        public TbSearchedAlbum(string term, ApiSearchMusicModel[] playlist)
        {
            SearchedTerm = term;

            foreach (ApiSearchMusicModel item in playlist)
            {
                Playlist.Add(new Album()
                {
                    VideoId = item.VideoId,
                    MusicName = item.NomeAlbum,
                    TipoParse = item.TipoParse[0]
                });
            }
        }

        [JsonProperty(PropertyName = "searchedTerm")]
        public string SearchedTerm { get; set; }

        [JsonProperty(PropertyName = "playlist")]
        public List<Album> Playlist { get; set; }

        public bool Equals(TbSearchedAlbum x, TbSearchedAlbum y) => x.Id == y.Id;

        public int GetHashCode(TbSearchedAlbum obj) => obj.GetHashCode();
    }
}
