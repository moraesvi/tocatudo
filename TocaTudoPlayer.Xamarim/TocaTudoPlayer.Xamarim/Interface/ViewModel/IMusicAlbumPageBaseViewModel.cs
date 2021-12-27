using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicAlbumPageBaseViewModel : IBaseViewModel
    {
        event Action PlayerReady;
        event Action ShowInterstitial;
        event Action<Action> ActionShowInterstitial;
        bool IsSearching { get; set; }
        Task SerializeMusicModelFromDb(ObservableCollection<SearchMusicModel> searchMusicCollection, Func<IDbLogic, Task<ApiSearchMusicModel[]>> funcDbSearch, MusicSearchType searchType, string icon);
    }
}
