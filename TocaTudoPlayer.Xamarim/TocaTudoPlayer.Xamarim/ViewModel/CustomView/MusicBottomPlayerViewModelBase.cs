using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim.ViewModel.CustomView
{
    public abstract class MusicBottomPlayerViewModelBase : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IAudio _audioPlayer;
        private readonly ITocaTudoApi _tocaTudoApi;
        private ICommonMusicPlayerViewModel _commonMusicPlayerViewModel;
        private MusicStatusBottomModel _musicStatusBottomModel;

        private bool _keepUpdatingMusicClock;
        private bool _bottomPlayerControlIsVisible;
        private bool _playerIsActive;
        private int _millisecondsDelayMusicClock;

        public Action _clockPlayerType;
        public event Action<bool> _musicIsPlayingEvent;
        private event Action _activePlayer;
        private event Action _stopPlayer;

        public MusicBottomPlayerViewModelBase(ITocaTudoApi tocaTudoApi)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _bottomPlayerControlIsVisible = false;
            _musicStatusBottomModel = new MusicStatusBottomModel(tocaTudoApi, this);

            _audioPlayer.PlayerInitializing += AudioPlayer_PlayerInitializing;
            _audioPlayer.PlayerSeekComplete += AudioPlayer_PlayerSeekComplete;

            _clockPlayerType = () => { };

            _playerIsActive = false;

            ClockPlayerUpdate();
        }
        public event Action<bool> MusicIsPlayingEvent
        {
            add
            {
                _musicIsPlayingEvent += value;
            }
            remove
            {
                _musicIsPlayingEvent -= value;
            }
        }
        public event Action ActivePlayer
        {
            add
            {
                _activePlayer += value;
            }
            remove
            {
                _activePlayer -= value;
            }
        }
        public event Action StopPlayer
        {
            add
            {
                _stopPlayer += value;
            }
            remove
            {
                _stopPlayer -= value;
            }
        }
        public bool KeepUpdatingMusicClock
        {
            get { return _keepUpdatingMusicClock; }
            set
            {
                _keepUpdatingMusicClock = value;
            }
        }
        public bool PlayerIsActive
        {
            get { return _playerIsActive; }
            set
            {
                _playerIsActive = value;
            }
        }
        public bool BottomPlayerControlIsVisible
        {
            get { return _bottomPlayerControlIsVisible; }
            set
            {
                _bottomPlayerControlIsVisible = value;
                OnPropertyChanged(nameof(BottomPlayerControlIsVisible));
            }
        }
        public int MillisecondsDelayMusicClock
        {
            get { return _millisecondsDelayMusicClock; }
            set
            {
                _millisecondsDelayMusicClock = value;
            }
        }
        public MusicStatusBottomModel MusicStatusBottomModel
        {
            get { return _musicStatusBottomModel; }
        }
        public Command PausePlayCommand => PausePlayEventCommand();
        public Command ProgressBarDragStartedCommand => ProgressBarDragStartedEventCommand();
        public Command<int> ProgressBarDragCompletedCommand => ProgressBarDragCompletedEventCommand();
        public virtual void ActiveBottomPlayer(ICommonMusicPlayerViewModel commonMusicPlayerViewModel)
        {
            _commonMusicPlayerViewModel = commonMusicPlayerViewModel;

            _playerIsActive = true;
            _musicStatusBottomModel.Init();

            if (_activePlayer != null)
                _activePlayer();
        }
        public virtual void StopBottomPlayer()
        {
            _playerIsActive = false;
            //_millisecondsDelayMusicClock = 0;

            //_clockPlayerType();

            if (_stopPlayer != null)
                _stopPlayer();
        }
        public void StartClock(PlayerType playerType)
        {
            _millisecondsDelayMusicClock = 0;

            switch (playerType)
            {
                case PlayerType.Music:
                    _clockPlayerType = () => PlayerMusicType();
                    break;
                case PlayerType.Album:
                    _clockPlayerType = () => PlayerAlbumType();
                    break;
            }
        }
        public virtual void UpdateMusicPartTimeDesc(int time)
        {
            if (!_keepUpdatingMusicClock)
                _musicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromMilisseconds(time);
        }
        public virtual void MusicSeekTo(int time)
        {
            _millisecondsDelayMusicClock = time;
            _audioPlayer.Seek(_millisecondsDelayMusicClock);
        }
        public void RaiseMusicIsPlayingEvent(bool isPlaying)
        {
            _musicIsPlayingEvent(isPlaying);
        }
        protected string GetAudioMusicTotalTimeFromSeconds(long maxTime)
        {
            TimeSpan musicTime = TimeSpan.FromSeconds(maxTime);

            return musicTime.Hours == 0 ? $"{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}"
                                        : musicTime.Minutes == 0 ? $"{musicTime.Seconds.ToString("D2")}"
                                        : $"{musicTime.Hours.ToString("D2")}:{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}";
        }
        protected string GetAudioMusicTotalTimeFromMilisseconds(long maxTime)
        {
            TimeSpan musicTime = TimeSpan.FromMilliseconds(maxTime);

            return musicTime.Hours == 0 ? $"{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}"
                                        : musicTime.Minutes == 0 ? $"{musicTime.Seconds.ToString("D2")}"
                                        : $"{musicTime.Hours.ToString("D2")}:{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}";
        }
        protected string GetAudioMusicPartTimeFromSeconds(int ticPosition)
        {
            TimeSpan musicTime = TimeSpan.FromSeconds(ticPosition);

            return musicTime.Hours == 0 ? $"{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}"
                                        : musicTime.Minutes == 0 ? $"{musicTime.Seconds.ToString("D2")}"
                                        : $"{musicTime.Hours.ToString("D2")}:{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}";
        }
        protected string GetAudioMusicPartTimeFromMilisseconds(int ticPosition)
        {
            TimeSpan musicTime = TimeSpan.FromMilliseconds(ticPosition);

            return musicTime.Hours == 0 ? $"{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}"
                                        : musicTime.Minutes == 0 ? $"{musicTime.Seconds.ToString("D2")}"
                                        : $"{musicTime.Hours.ToString("D2")}:{musicTime.Minutes.ToString("D2")}:{musicTime.Seconds.ToString("D2")}";
        }
        protected int GetAudioMusicTime(int milisegundos)
        {
            decimal minDuration = _audioPlayer.Max() / 60;
            decimal minMusicPart = ((milisegundos * 1000) / 60);

            minDuration = (minDuration == 0) ? 1 : minDuration;
            minMusicPart = (minMusicPart == 0) ? 1 : minMusicPart;

            float musicPartTime = (float)Math.Round(((minMusicPart / minDuration) * 100), 2);

            float currentPosition = (_audioPlayer.Max() / 100) * musicPartTime;

            return (int)currentPosition;
        }
        protected int GetMusicSeconds(int milisegundos)
        {
            return milisegundos / 1000;
        }

        #region Private Methods
        private void ClockPlayerUpdate()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (_audioPlayer.IsPlaying && _keepUpdatingMusicClock && !_musicStatusBottomModel.MusicStreamComplete)
                {
                    _clockPlayerType();
                }
                else if (_musicStatusBottomModel.MusicStreamComplete)
                {
                    _musicStatusBottomModel.MusicPartTimeDesc = _musicStatusBottomModel.MusicTotalTimeDesc;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    ClockPlayerUpdate();
                });

                return false;
            });
        }
        public void PlayerMusicType()
        {
            _millisecondsDelayMusicClock += 1000;
            _musicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromMilisseconds(_millisecondsDelayMusicClock);
        }
        public void PlayerAlbumType()
        {
            _millisecondsDelayMusicClock += 1;
            _musicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromSeconds(_millisecondsDelayMusicClock);
        }
        private void AudioPlayer_PlayerInitializing()
        {
            _musicIsPlayingEvent(false);
        }
        private void AudioPlayer_PlayerSeekComplete()
        {
            _keepUpdatingMusicClock = true;
            _audioPlayer.Play();
        }
        private Command PausePlayEventCommand()
        {
            return new Command(
                   execute: () =>
                   {
                       _commonMusicPlayerViewModel.PlayPauseMusic();
                   });
        }
        private Command ProgressBarDragStartedEventCommand()
        {
            return new Command(
                   execute: () =>
                   {
                       _keepUpdatingMusicClock = false;
                   });
        }
        private Command<int> ProgressBarDragCompletedEventCommand()
        {
            return new Command<int>(
                   execute: (seekTo) =>
                   {
                       MusicSeekTo(seekTo);
                   });
        }
        #endregion
    }
}
