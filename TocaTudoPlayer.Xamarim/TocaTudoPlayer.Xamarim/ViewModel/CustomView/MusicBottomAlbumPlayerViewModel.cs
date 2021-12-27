using MarcTron.Plugin;
using System;
using System.Linq;
using TocaTudoPlayer.Xamarim.Interface;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim.ViewModel.CustomView
{
    public class MusicBottomAlbumPlayerViewModel : MusicBottomPlayerViewModelBase, IMusicBottomAlbumPlayerViewModel
    {
        private readonly IAudio _audioPlayer;
        private string _videoId;
        private int _musicTimeSeeked;
        private int _nextMusicTicStarts;

        private PlaylistItem _musicPlayingNow;
        private PlaylistItem[] _playlist;

        public event Action<float> _musicPlayerLoadedEvent;
        public event Action<float> _musicStreamProgessEvent;

        public MusicBottomAlbumPlayerViewModel(ITocaTudoApi tocaTudoApi)
            : base(tocaTudoApi)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            PlayerUpdate();
        }

        public event Action<float> MusicPlayerLoadedEvent
        {
            add
            {
                _musicPlayerLoadedEvent += value;
            }
            remove
            {
                _musicPlayerLoadedEvent -= value;
            }
        }
        public event Action<float> MusicStreamProgessEvent
        {
            add
            {
                _musicStreamProgessEvent += value;
            }
            remove
            {
                _musicStreamProgessEvent -= value;
            }
        }
        public void SetAlbumPlaylist(string album, string videoId, PlaylistItem[] playlist)
        {
            _videoId = videoId;
            _playlist = playlist;
            _nextMusicTicStarts = 0;
            _musicTimeSeeked = 0;

            MusicStatusBottomModel.AlbumName = album;
            MusicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromSeconds(0);

            StartClock(PlayerType.Album);
        }
        public void PlayBottomPlayer(PlaylistItem music)
        {
            KeepUpdatingMusicClock = false;
            _musicTimeSeeked = 0;

            _audioPlayer.Seek(GetAudioMusicTime(music.TempoSegundos), new PlaylistItemServicePlayer(_videoId, music));

            PlayerIsActive = true;
            music.IsPlaying = true;

            MillisecondsDelayMusicClock = 0;
            BottomPlayerControlIsVisible = true;

            MusicStatusBottomModel.MusicName = music.NomeMusica;
            MusicStatusBottomModel.BottomPlayerIsVisible = true;

            MusicStatusBottomModel.MusicTotalTimeDesc = GetAudioMusicTotalTimeFromSeconds(music.TempoSegundosFim);

            _musicPlayerLoadedEvent(GetAudioMusicTime(music.TempoSegundosFim));

            _musicPlayingNow = music;
        }
        public override void StopBottomPlayer()
        {
            base.StopBottomPlayer();
        }
        public override void MusicSeekTo(int time)
        {
            int musicTime = GetAudioMusicTime(_musicPlayingNow.TempoSegundosInicio) + time;
            base.MusicSeekTo(musicTime);

            _musicTimeSeeked = musicTime;
            MillisecondsDelayMusicClock = GetMusicSeconds(time);
        }

        #region Private Methods
        private void PlayerUpdate()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                RaiseMusicIsPlayingEvent(_audioPlayer.IsPlaying);

                if (_audioPlayer.IsPlaying && KeepUpdatingMusicClock)
                {
                    PlaylistItem musicPlaying = _musicPlayingNow;
                    PlaylistItem nextMusic = GetNextMusicFromPlaylist(_playlist);

                    if (musicPlaying != null && nextMusic != null)
                    {
                        float ticPosition = _audioPlayer.CurrentPosition();
                        ticPosition = (_musicTimeSeeked > ticPosition) ? (_musicTimeSeeked - ticPosition) + ticPosition : ticPosition;

                        if (ticPosition > 0)
                        {
                            float sizeOfMusicPlaying = GetAudioMusicTime(musicPlaying.TempoSegundos);
                            float sizeNextMusic = GetAudioMusicTime(musicPlaying.TempoSegundosFim);

                            MusicStatusBottomModel.MusicStreamProgress = (ticPosition - sizeNextMusic) / (sizeNextMusic + sizeOfMusicPlaying);

                            _musicStreamProgessEvent(ticPosition - sizeOfMusicPlaying);
                            _nextMusicTicStarts = GetAudioMusicTime(nextMusic.TempoSegundos);

                            if (ticPosition >= _nextMusicTicStarts)
                            {
                                if (!nextMusic?.IsPlaying ?? false)
                                {
                                    musicPlaying.IsPlaying = false;

                                    ClearAllIconPlaying();
                                    PlayBottomPlayer(nextMusic);
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
        private PlaylistItem GetNextMusicFromPlaylist(PlaylistItem[] playlist)
        {
            for (int index = 0; index < playlist.Count(); index++)
            {
                PlaylistItem item = playlist[index];

                if (playlist.Count() - 1 == index)
                    return playlist[index];

                if (item.IsPlaying)
                    return playlist[index + 1];
            }

            return null;
        }
        private void ClearAllIconPlaying()
        {
            foreach (PlaylistItem plist in _playlist)
            {
                plist.IsPlaying = false;
            }
        }
        #endregion
    }
}
