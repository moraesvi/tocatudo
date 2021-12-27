using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAlbumPlayedHistoryViewModel
    {
        bool RecentlyPlayedFormIsVisible { get; set; }
        bool PlayedHistoryIsVisible { get; set; }
        int PlayedHistoryCollectionSize { get; set; }
        int PlayedHistoryPlayerFormSize { get; set; }
        int PlayedHistoryCollectionTotalItens { get; set; }
        HistoryAlbumModel RecentlyPlayedSelected { get; set; }
        UserSearchHistoryModel UserSearchHistory { get; set; }
        ObservableCollection<UserAlbumPlayedHistory> PlayedHistory { get; set; }
        void SerializarPlayedHistory(List<UserAlbumPlayedHistory> lstUserHistory, string videoId);
        string[] FilterUserSearchHistory(string term);
        string[] FilterUserSearchHistory(List<string> lstFilters);
        Task LoadUserSearchHistory();
        Task LoadPlayedHistory(UserAlbumPlayedHistory userAlbumSelected = null);
        Task SaveLocalSearchHistory(string musicSearchedName);
        Task SaveLocalHistory(AlbumModel album, AlbumParseType parseType, byte[] byteMusicImage);
    }
}
