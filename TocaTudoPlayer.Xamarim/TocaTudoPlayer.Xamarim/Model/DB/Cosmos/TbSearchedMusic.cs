using Newtonsoft.Json;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class TbSearchedMusic : BaseCosmos, IEqualityComparer<TbSearchedMusic>
    {
        public TbSearchedMusic() { }
        public TbSearchedMusic(string searchedTerm, ApiSearchMusicModel[] musics)
        {
            SearchedTerm = searchedTerm;

            foreach (ApiSearchMusicModel item in musics)
            {
                Musics.Add(new Music()
                {
                    Index = item.Id,
                    VideoId = item.VideoId,
                    MusicName = item.NomeAlbum,
                });
            }
        }

        [JsonProperty(PropertyName = "searchedTerm")]
        public string SearchedTerm { get; set; }

        [JsonProperty(PropertyName = "musics")]
        public List<Music> Musics { get; set; } = new List<Music>();

        public bool Equals(TbSearchedMusic x, TbSearchedMusic y) => x.Id == y.Id;

        public int GetHashCode(TbSearchedMusic obj) => obj.GetHashCode();
    }
}
