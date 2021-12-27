using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicBottomPlayerViewModel : INotifyPropertyChanged
    {
        event Action<float> MusicPlayerLoadedEvent;
        event Action<float> MusicStreamProgessEvent;
        event Action<SearchMusicModel> NextMusicEvent;
        event Action ActivePlayer;
        event Action StopPlayer;
        SearchMusicModel LastMusicPlayed { get; set; }
        bool BottomPlayerControlIsVisible { get; set; }
        MusicStatusBottomModel MusicStatusBottomModel { get; }
        Command PausePlayCommand { get; }
        Command ProgressBarDragStartedCommand { get; }
        Command<int> ProgressBarDragCompletedCommand { get; }
        void StartBottomPlayer();
        void StartBottomPlayer(SearchMusicModel[] nextMusics);
        void StopBottomPlayer();
        void UpdateMusicPartTimeDesc(int time);
        void MusicSeekTo(int time);
        void ActiveBottomPlayer(ICommonMusicPlayerViewModel commonMusicPlayerViewModel);
    }
}
