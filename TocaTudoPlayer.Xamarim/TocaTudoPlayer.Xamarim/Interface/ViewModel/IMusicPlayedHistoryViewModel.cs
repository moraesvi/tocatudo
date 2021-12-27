using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicPlayedHistoryViewModel
    {
        bool PlayedHistoryIsVisible { get; set; }
        int PlayedHistoryCollectionSize { get; set; }
        int PlayedHistoryPlayerFormSize { get; set; }
        int PlayedHistoryCollectionTotalItens { get; set; }
        HistoryMusicModel HistoryMusicPlayingNow { get; set; }
        UserSearchHistoryModel UserSearchHistory { get; set; }
        HttpDownload Download { get; set; }
        UserMusicPlayedHistory ActiveMusicNow { get; set; }
        ObservableCollection<UserMusicPlayedHistory> PlayedHistory { get; set; }
        Command MusicHistoryDownloadFormCommand { get; }
        void SerializarPlayedHistory(List<UserMusicPlayedHistory> lstUserAHistory, string videoId);
        string[] FilterUserSearchHistory(string term);
        string[] FilterUserSearchHistory(List<string> lstFilters);
        Task LoadUserSearchHistory();
        Task LoadPlayedHistory(UserMusicPlayedHistory userMusicSelected = null);
        Task SaveLocalSearchHistory(string musicName);
        Task SaveLocalHistory(ICommonMusicModel musicPlayer, byte[] byteMusicImage);
    }
}
