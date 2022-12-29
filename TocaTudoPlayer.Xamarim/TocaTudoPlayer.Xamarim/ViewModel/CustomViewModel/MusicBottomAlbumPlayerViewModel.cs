using System;
using System.Linq;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicBottomAlbumPlayerViewModel : MusicBottomPlayerBaseViewModel
    {
        private readonly IAudio _audioPlayer;
        private string _videoId;
        private int _musicTimeSeeked;
        private int _nextMusicTicStarts;

        private AlbumModel _albumPlayingNow;
        private PlaylistItem _playlistItemNow;
        private PlaylistItem[] _playlist;
        private PlaylistItem _nextMusic;

        public WeakEventManager<float> _musicPlayerLoadedEvent;
        public WeakEventManager<float> _musicStreamProgessEvent;

        public MusicBottomAlbumPlayerViewModel(ITocaTudoApi tocaTudoApi)
            : base(tocaTudoApi)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _musicPlayerLoadedEvent = new WeakEventManager<float>();
            _musicStreamProgessEvent = new WeakEventManager<float>();
            _playlist = new PlaylistItem[] { };
        }

        public event EventHandler<float> MusicPlayerLoadedEvent
        {
            add => _musicPlayerLoadedEvent.AddEventHandler(value);
            remove => _musicPlayerLoadedEvent.RemoveEventHandler(value);
        }
        public event EventHandler<float> MusicStreamProgessEvent
        {
            add => _musicStreamProgessEvent.AddEventHandler(value);
            remove => _musicStreamProgessEvent.RemoveEventHandler(value);
        }
        public bool AlbumHasMusicSelected => _albumPlayingNow?.Playlist
                                                             ?.Any(pl => pl.IsPlaying) ?? false;
        public override Command PausePlayCommand => PausePlayEventCommand();
        public PlaylistItem PlaylistItemNow
        {
            get { return _playlistItemNow; }
        }
        public void Init()
        {
            _playlistItemNow = null;
        }
        public void Init(CommonMusicPlayerViewModel commonMusicPlayerViewModel)
        {
            _playlistItemNow = null;
            Initialize(commonMusicPlayerViewModel);
        }
        public void SetAlbumPlaylist(string album, string videoId, PlaylistItem[] playlist)
        {
            _videoId = videoId;
            _playlist = playlist;
            _nextMusicTicStarts = 0;
            _musicTimeSeeked = 0;
            _playlistItemNow = null;

            MusicStatusBottomModel.AlbumName = album;
            MusicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromSeconds(0);
        }
        public void PlayBottomPlayer(AlbumModel album, PlaylistItem music)
        {
            KeepUpdatingMusicClock = false;
            _musicTimeSeeked = 0;

            _albumPlayingNow = album;
            _playlistItemNow = music;

            SetPlaylistItemPlaying(album, music);
            NextMusicFromPlaylist();

            _audioPlayer.Seek(GetAudioMusicTime(music.TempoSegundos), new PlaylistItemServicePlayer(_videoId, music));

            PlayerIsActive = true;
            music.IsPlaying = true;

            BottomPlayerControlIsVisible = true;

            MusicStatusBottomModel.MusicName = music.NomeMusica;
            MusicStatusBottomModel.BottomPlayerIsVisible = true;

            MusicStatusBottomModel.MusicTotalTimeDesc = GetAudioMusicTotalTimeFromSeconds(music.TempoSegundosFim);

            _musicPlayerLoadedEvent.RaiseEvent(this, GetAudioMusicTime(music.TempoSegundosFim), nameof(MusicPlayerLoadedEvent));

            PlayerUpdate();
            StartClock(PlayerType.Album);
        }
        public override void MusicSeekTo(long time)
        {
            long musicTime = GetAudioMusicTime(PlaylistItemNow.TempoSegundosInicio) + time;
            base.MusicSeekTo(musicTime);
        }

        #region Private Methods
        private void PlayerUpdate()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (!PlayerIsActive)
                    return false;

                if (_audioPlayer.IsPlaying && KeepUpdatingMusicClock)
                {
                    CommonMusicPlayerManager.StopAllMusicBottomPlayers();
                    CommonMusicPlayerManager.SetActiveAlbumBottomPlayer();

                    if (PlaylistItemNow != null && _nextMusic != null)
                    {
                        float ticPosition = _audioPlayer.CurrentPosition();
                        ticPosition = (_musicTimeSeeked > ticPosition) ? (_musicTimeSeeked - ticPosition) + ticPosition : ticPosition;

                        if (ticPosition > 0)
                        {
                            float sizeOfMusicPlaying = GetAudioMusicTime(PlaylistItemNow.TempoSegundos);
                            float sizeNextMusic = GetAudioMusicTime(PlaylistItemNow.TempoSegundosFim);

                            MusicStatusBottomModel.MusicStreamProgress = (ticPosition - sizeNextMusic) / (sizeNextMusic + sizeOfMusicPlaying);

                            _musicStreamProgessEvent.RaiseEvent(this, ticPosition - sizeOfMusicPlaying, nameof(MusicStreamProgessEvent));
                            _nextMusicTicStarts = GetAudioMusicTime(_nextMusic.TempoSegundos);

                            if (ticPosition >= _nextMusicTicStarts)
                            {
                                if (!_nextMusic?.IsPlaying ?? false)
                                {
                                    ClearAllIconPlaying();
                                    SetNextMusicPlaying(_nextMusic);
                                }
                            }
                        }
                    }
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    PlayerUpdate();
                });

                return false;
            });
        }
        private void NextMusicFromPlaylist()
        {
            int musicPlayingIndex = _albumPlayingNow.Playlist.ToList()
                                                             .FindIndex(pl => pl.Id == _playlistItemNow.Id);

            if (_albumPlayingNow.Playlist.Count() - 1 == musicPlayingIndex)
            {
                if (_albumPlayingNow.Playlist.ElementAtOrDefault(musicPlayingIndex) != null)
                    _nextMusic = _albumPlayingNow.Playlist[musicPlayingIndex];
            }
            else
            {
                if (_albumPlayingNow.Playlist.ElementAtOrDefault(musicPlayingIndex) != null)
                    _nextMusic = _albumPlayingNow.Playlist[musicPlayingIndex + 1];
            }
        }
        private void SetNextMusicPlaying(PlaylistItem music)
        {
            _musicTimeSeeked = 0;

            _audioPlayer.Next(new PlaylistItemServicePlayer(_videoId, music));

            PlaylistItemNow.IsPlaying = false;
            PlayerIsActive = true;

            BottomPlayerControlIsVisible = true;

            MusicStatusBottomModel.MusicName = music.NomeMusica;
            MusicStatusBottomModel.BottomPlayerIsVisible = true;

            MusicStatusBottomModel.MusicTotalTimeDesc = GetAudioMusicTotalTimeFromSeconds(music.TempoSegundosFim);

            _musicPlayerLoadedEvent.RaiseEvent(this, GetAudioMusicTime(music.TempoSegundosFim), nameof(MusicPlayerLoadedEvent));

            music.IsPlaying = true;
            _playlistItemNow = music;

            NextMusicFromPlaylist();
            SetPlaylistItemPlaying(_albumPlayingNow, _playlistItemNow);
        }
        private void ClearAllIconPlaying()
        {
            foreach (PlaylistItem plist in _playlist)
            {
                plist.IsPlaying = false;
            }
        }
        private Command PausePlayEventCommand()
        {
            return new Command(
                   execute: () =>
                   {
                       base.PausePlayCommand.Execute(null);
                   });
        }
        #endregion
    }
}
