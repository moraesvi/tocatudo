using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonMusicModel : INotifyPropertyChanged
    {
        short Id { get; set; }
        bool IsLoadded { get; set; }
        bool IsActiveMusic { get; set; }
        bool ShowMerchandisingAlert { get; set; }
        bool IsPlaying { get; set; }
        bool IconMusicDownloadVisible { get; set; }
        bool IsBufferingMusic { get; set; }
        bool IsSavedOnLocalDb { get; set; }
        bool IsHistoryPlayedSavedOnLocalDb { get; set; }
        string MusicName { get; set; }
        string VideoId { get; set; }
        int[] AlbumTypeParse { get; set; }
        MusicSearchType SearchType { get; set; }
        MusicModel MusicModel { get; set; }
        void ReloadMusicPlayingIcon();
        void UpdateMusicPlayingIcon();
        Task StartDownloadMusic();
        Task StartDownloadMusic(byte[] imgMusic);
    }
}
