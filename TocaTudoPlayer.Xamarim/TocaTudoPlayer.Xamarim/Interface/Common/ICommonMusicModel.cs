using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface ICommonMusicModel : INotifyPropertyChanged, ICommonMusicServiceModel
    {
        short Id { get; set; }
        bool IsLoadded { get; set; }
        bool ShowMerchandisingAlert { get; set; }
        bool IsPlaying { get; set; }
        bool IsSelected { get; set; }
        bool IconMusicDownloadVisible { get; set; }
        bool IsBufferingMusic { get; set; }
        bool IsSavedOnLocalDb { get; set; }
        bool IsHistoryPlayedSavedOnLocalDb { get; set; }
        bool IsAnimated { get; set; }
        string MusicName { get; set; }
        string VideoId { get; set; }
        string MusicTime { get; set; }
        long MusicTimeTotalSeconds { get; set; }
        string MusicImageUrl { get; set; }
        byte[] ByteMusicImage { get; set; }
        ImageSource ImgMusic { get; set; }
        int[] AlbumTypeParse { get; set; }
        MusicSearchType SearchType { get; set; }
        MusicModel MusicModel { get; set; }
        MusicAlbumDialogDataModel MusicAlbumPopupModel { get; set; }
        void SetDownloadingMode(bool force = true);
        void ReloadMusicPlayingIcon();
        void UpdateMusicPlayingIcon();
        Task<DownloadQueueStatus> StartDownloadMusic(MusicPlayedHistoryViewModel musicPlayedHistoryViewModel);
        Task<DownloadQueueStatus> StartDownloadMusic(MusicPlayedHistoryViewModel musicPlayedHistoryViewModel, Action downloadComplete);
        Task<DownloadQueueStatus> StartDownloadMusic(byte[] imgMusic, MusicPlayedHistoryViewModel musicPlayedHistoryViewModel);
    }
}
