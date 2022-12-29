using System;

namespace TocaTudoPlayer.Xamarim
{
    public interface IAudio
    {
        event EventHandler PlayerInitializing;
        event EventHandler<ICommonMusicModel> PlayerReady;
        event EventHandler<ICommonMusicModel> PlayerReadyBuffering;
        event EventHandler PlayerSeekComplete;
        event EventHandler<ItemServicePlayer> PlayerAlbumPlaylistChanged;
        event EventHandler<ItemServicePlayer> PlayerAlbumMusicPlaylistChanged;
        event EventHandler<string> PlayerAlbumInvalidUri;
        event EventHandler<ICommonMusicModel> PlayerMusicInvalidUri;
        event EventHandler PlayerLosedAudioFocus;
        event EventHandler<string> PlayerException;
        bool EventsBinded { get; }
        bool IsPlaying { get; }
        void Start();
        void Source(AlbumMusicModelServicePlayer musicServicePlayer);
        void Source(string url, string videoId);
        void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom);
        void Source(string url, string videoId, MusicStatusBottomModel musicStatusBottom, AlbumModelServicePlayer albumServicePlayer);
        void Source(string url, string videoId, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer);
        void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer);
        void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, AlbumModelServicePlayer albumServicePlayer);
        void Source(byte[] music);
        void Source(byte[] music, AlbumModelServicePlayer albumServicePlayer);
        void Source(byte[] music, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom);
        void Source(byte[] music, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer);
        void Seek(long milisegundos);
        void Seek(long milisegundos, ItemServicePlayer playlistItem);
        void Seek(long milisegundos, MusicModelServicePlayer musicModel);
        void Next(ItemServicePlayer playlistItem);
        void ClearAlbumSource();
        void HideStatusBarPlayerControls();
        void Pause();
        bool Play();
        bool Stop();
        long Max();
        long CurrentPosition();
    }
}
