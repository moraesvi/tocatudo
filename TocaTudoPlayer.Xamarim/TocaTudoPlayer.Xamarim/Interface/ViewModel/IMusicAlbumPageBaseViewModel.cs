using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicAlbumPageBaseViewModel : IBaseViewModel
    {
        event EventHandler PlayerReady;
        bool IsReady { get; set; }
        bool IsSearching { get; set; }
        bool PlaylistIsVisible { get; set; }
        Task SerializeMusicModelFromDb(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcDbSearch, MusicSearchType searchType, string icon);
    }
}
