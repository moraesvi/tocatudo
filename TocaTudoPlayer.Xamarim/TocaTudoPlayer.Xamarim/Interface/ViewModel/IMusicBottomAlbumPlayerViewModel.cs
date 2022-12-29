using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicBottomAlbumPlayerViewModel : IBaseViewModel, INotifyPropertyChanged
    {
        event EventHandler<float> MusicPlayerLoadedEvent;
        event EventHandler<float> MusicStreamProgessEvent;
        event EventHandler MusicShowInterstitial;
        event EventHandler ActivePlayer;
        event EventHandler StopPlayer;
        bool PlayerIsActive { get; }
        bool AlbumPlaying { get; }
        PlaylistItem PlaylistItemNow { get; }
        bool BottomPlayerControlIsVisible { get; set; }
        MusicStatusBottomModel MusicStatusBottomModel { get; }
        Command PausePlayCommand { get; }
        Command ProgressBarDragStartedCommand { get; }
        Command<int> ProgressBarDragCompletedCommand { get; }
        void Init(CommonMusicPlayerViewModel commonMusicPlayerViewModel);
        void SetAlbumPlaylist(string album, string videoId, PlaylistItem[] playlist);
        void PlayBottomPlayer(PlaylistItem music);
        void StopBottomPlayer(bool force = false);
        void UpdateMusicPartTimeDesc(int time);
        void MusicSeekTo(int time);
        void ActiveBottomPlayer();
    }
}
