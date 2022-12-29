using System;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchMusicAlbumStorageLogic : ISearchMusicAlbumStorageLogic
    {
        public Task DeleteMusicAlbumPlaylistSelected(SearchMusicModel music)
        {
            throw new NotImplementedException();
        }
        public Task<bool> ExistsMusicAlbumPlaylist(string albumName, SearchMusicModel music)
        {
            throw new NotImplementedException();
        }
        public Task InsertMusicAlbumPlaylistSelected(string albumName, SearchMusicModel music)
        {
            throw new NotImplementedException();
        }
        public Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel music)
        {
            throw new NotImplementedException();
        }
    }
}
