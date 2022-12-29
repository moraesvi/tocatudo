using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public interface IMusicBottomPlayerViewModel : IBaseViewModel, INotifyPropertyChanged
    {
        event EventHandler<float> MusicPlayerLoadedEvent;
        event EventHandler<float> MusicStreamProgessEvent;
        event EventHandler MusicShowInterstitial;
        event EventHandler<SearchMusicModel> NextMusicEvent;
        event EventHandler ActivePlayer;
        event EventHandler StopPlayer;
        ICommonMusicModel MusicPlayingNow { get; set; }
        MusicSearchType KindMusicPlayingNow { get; set; }
        SearchMusicModel LastMusicPlayed { get; set; }
        bool PlayerIsActive { get; }
        bool BottomPlayerControlIsVisible { get; set; }
        MusicStatusBottomModel MusicStatusBottomModel { get; }
        Command PausePlayCommand { get; }
        Command ProgressBarDragStartedCommand { get; }
        Command<int> ProgressBarDragCompletedCommand { get; }
        void Init(CommonMusicPlayerViewModel commonMusicPlayerViewModel);
        void StartBottomPlayer(MusicSearchType musicSearchType);
        void StartBottomPlayer(MusicSearchType musicSearchType, SearchMusicModel[] searchMusicCollection);
        void StopBottomPlayer(bool force = false);
        void UpdateMusicPartTimeDesc(int time);
        void MusicSeekTo(int time);
        void ActiveBottomPlayer();
    }
}
