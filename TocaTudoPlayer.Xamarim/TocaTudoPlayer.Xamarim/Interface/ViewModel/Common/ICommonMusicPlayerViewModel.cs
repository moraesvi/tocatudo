using System;
using System.Threading;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim

{
    public interface ICommonMusicPlayerViewModel : IBaseViewModel
    {
        event Action<ICommonMusicModel> MusicPlayedHistoricIsSaved;
        event Action PlayerLosedAudioFocus;
        SearchMusicModel LastMusicPlayed { get; set; }
        void SetMusicPlayerConfig(MusicPlayerConfig config);
        Task PlayMusic(ICommonMusicModel music, CancellationToken cancellationToken);
        Task PlayMusic(ICommonMusicModel music, SearchMusicModel[] nextMusics, MusicPlayerConfig config, CancellationToken cancellationToken);
        Task PlaySavedMusic(ICommonMusicModel music);
        void PlayPauseMusic();
        void PlayPauseMusic(ICommonMusicModel music);
        void Stop(ICommonMusicModel music);
        Task StartDownloadMusic(ICommonMusicModel music);
        void ActiveBottomPlayer();
        void HideBottomPlayer();
        void StopBottomPlayer();
    }
}
