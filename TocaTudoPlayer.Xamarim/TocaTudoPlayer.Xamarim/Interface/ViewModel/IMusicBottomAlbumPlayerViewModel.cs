using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicBottomAlbumPlayerViewModel : INotifyPropertyChanged
    {
        event Action<float> MusicPlayerLoadedEvent;
        event Action<float> MusicStreamProgessEvent;
        event Action ActivePlayer;
        event Action StopPlayer;
        bool BottomPlayerControlIsVisible { get; set; }
        MusicStatusBottomModel MusicStatusBottomModel { get; }
        Command PausePlayCommand { get; }
        Command ProgressBarDragStartedCommand { get; }
        Command<int> ProgressBarDragCompletedCommand { get; }
        void SetAlbumPlaylist(string album, string videoId, PlaylistItem[] playlist);
        void PlayBottomPlayer(PlaylistItem music);
        void StopBottomPlayer();
        void UpdateMusicPartTimeDesc(int time);
        void MusicSeekTo(int time);
        void ActiveBottomPlayer(ICommonMusicPlayerViewModel commonMusicPlayerViewModel);
    }
}
