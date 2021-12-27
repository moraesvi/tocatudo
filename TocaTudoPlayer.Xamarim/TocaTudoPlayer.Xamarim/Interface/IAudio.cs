using System;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAudio
    {
        event Action PlayerInitializing;
        event Action<ICommonMusicModel> PlayerReady;
        event Action<ICommonMusicModel> PlayerReadyBuffering;
        event Action PlayerSeekComplete;
        event Action<PlaylistItemServicePlayer> PlayerPlaylistChanged;
        event Action<ICommonMusicModel> PlayerInvalidUri;
        event Action PlayerLosedAudioFocus;
        bool EventsBinded { get; }
        bool IsPlaying { get; }
        void Start();
        void Source(string url);
        void Source(string url, ICommonMusicModel musicPlayer);
        void Source(string url, AlbumModelServicePlayer albumServicePlayer);
        void Source(string url, MusicModelServicePlayer musicServicePlayer);
        void Source(string url, ICommonMusicModel musicPlayer, MusicModelServicePlayer musicServicePlayer);
        void Source(string url, ICommonMusicModel musicPlayer, AlbumModelServicePlayer albumServicePlayer);
        void Source(byte[] music);
        void Source(byte[] music, AlbumModelServicePlayer albumServicePlayer);
        void Source(byte[] music, ICommonMusicModel musicPlayer);
        void Source(byte[] music, ICommonMusicModel musicPlayer, MusicModelServicePlayer musicServicePlayer);
        void Seek(int milisegundos);
        void Seek(int milisegundos, PlaylistItemServicePlayer playlistItem);
        void Seek(int milisegundos, MusicModelServicePlayer musicModel);
        void Pause();
        bool Play();
        bool Stop();
        long Max();
        long CurrentPosition();
    }
}
