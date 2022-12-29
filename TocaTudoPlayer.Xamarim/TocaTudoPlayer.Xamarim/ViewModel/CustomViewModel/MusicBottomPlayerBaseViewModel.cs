using System;
using System.ComponentModel;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicBottomPlayerBaseViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IAudio _audioPlayer;
        private CommonMusicPlayerViewModel _commonMusicPlayerViewModel;
        private MusicStatusBottomModel _musicStatusBottomModel;
        private AlbumModel _albumPlayingNow;
        private PlaylistItem _playlistItemNow;
        private TimeSpan _musicTimeTotal;

        private bool _keepUpdatingMusicClock;
        private bool _bottomPlayerControlIsVisible;
        private bool _playerIsActive;
        private long _totalMerchanMusicTimePlayed;

        private PlayerClockUpdate _playerClockUpdate;
        public Action _clockPlayerType;
        private WeakEventManager<bool> _musicIsPlayingEvent;
        private WeakEventManager _musicShowInterstitial;
        private WeakEventManager _activePlayer;
        private WeakEventManager _stopPlayer;

        public MusicBottomPlayerBaseViewModel()
        {
            _musicIsPlayingEvent = new WeakEventManager<bool>();
            _activePlayer = new WeakEventManager();
            _musicShowInterstitial = new WeakEventManager();
            _stopPlayer = new WeakEventManager();
            _totalMerchanMusicTimePlayed = 0;
        }
        public MusicBottomPlayerBaseViewModel(ITocaTudoApi tocaTudoApi)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _bottomPlayerControlIsVisible = false;
            _totalMerchanMusicTimePlayed = 0;
            _musicIsPlayingEvent = new WeakEventManager<bool>();
            _musicStatusBottomModel = new MusicStatusBottomModel(tocaTudoApi, this);
            _musicShowInterstitial = new WeakEventManager();
            _activePlayer = new WeakEventManager();
            _stopPlayer = new WeakEventManager();

            _audioPlayer.PlayerInitializing += AudioPlayer_PlayerInitializing;
            _audioPlayer.PlayerSeekComplete += AudioPlayer_PlayerSeekComplete;

            _clockPlayerType = () => { };

            _playerIsActive = false;
        }
        public event EventHandler<bool> MusicIsPlayingEvent
        {
            add => _musicIsPlayingEvent.AddEventHandler(value);
            remove => _musicIsPlayingEvent.RemoveEventHandler(value);
        }
        public event EventHandler ActivePlayer
        {
            add => _activePlayer.AddEventHandler(value);
            remove => _activePlayer.RemoveEventHandler(value);
        }
        public event EventHandler MusicShowInterstitial
        {
            add => _musicShowInterstitial.AddEventHandler(value);
            remove => _musicShowInterstitial.RemoveEventHandler(value);
        }
        public event EventHandler StopPlayer
        {
            add => _stopPlayer.AddEventHandler(value);
            remove => _stopPlayer.RemoveEventHandler(value);
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
            protected set
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
        public MusicStatusBottomModel MusicStatusBottomModel
        {
            get { return _musicStatusBottomModel; }
        }
        public virtual Command PausePlayCommand => PausePlayEventCommand();
        public Command ProgressBarDragStartedCommand => ProgressBarDragStartedEventCommand();
        public Command<long> ProgressBarDragCompletedCommand => ProgressBarDragCompletedEventCommand();
        protected void Initialize(CommonMusicPlayerViewModel commonMusicPlayerViewModel)
        {
            _commonMusicPlayerViewModel = commonMusicPlayerViewModel;
            _playerIsActive = false;

            _musicStatusBottomModel.Init();
        }
        public virtual void ActiveBottomPlayer()
        {
            _playerIsActive = true;
            _keepUpdatingMusicClock = true;

            if (_playerClockUpdate != null)//Ensure only one clock's instance
            {
                _playerClockUpdate.Dispose();
                _playerClockUpdate = null;
            }

            _playerClockUpdate = new PlayerClockUpdate(this, _audioPlayer, _musicStatusBottomModel);
            _playerClockUpdate.ClockPlayerUpdate(_clockPlayerType);

            if (_activePlayer != null)
                _activePlayer.RaiseEvent(this, null, nameof(ActivePlayer));
        }
        public virtual void StopBottomPlayer(bool force = false)
        {
            if (!_audioPlayer.IsPlaying || force)
            {
                _playerIsActive = false;
                _keepUpdatingMusicClock = false;

                if (_playerClockUpdate != null)//Ensure only one clock's instance
                {
                    _playerClockUpdate.Dispose();
                    _playerClockUpdate = null;
                }
            }

            if (_stopPlayer != null)
                _stopPlayer.RaiseEvent(this, null, nameof(StopPlayer));
        }
        public void StartClock(PlayerType playerType)
        {
            _totalMerchanMusicTimePlayed = 0;

            switch (playerType)
            {
                case PlayerType.Music:
                    _playlistItemNow = null;
                    _clockPlayerType = () => PlayerMusicType();
                    break;
                case PlayerType.Album:
                    _clockPlayerType = () => PlayerAlbumType();
                    break;
            }

            if (_playerClockUpdate != null)//Ensure only one clock's instance
            {
                _playerClockUpdate.Dispose();
                _playerClockUpdate = null;
            }

            _playerClockUpdate = new PlayerClockUpdate(this, _audioPlayer, _musicStatusBottomModel);
            _playerClockUpdate.ClockPlayerUpdate(_clockPlayerType);
        }
        public void SetPlaylistItemPlaying(AlbumModel albumPlayingNow, PlaylistItem playlistItemNow)
        {
            _albumPlayingNow = albumPlayingNow;
            _playlistItemNow = playlistItemNow;
        }
        public virtual void UpdateMusicPartTimeDesc(long time)
        {
            if (!_keepUpdatingMusicClock)
                _musicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromMilisseconds(time);
        }
        public virtual void MusicSeekTo(long time)
        {
            _audioPlayer.Seek(time);
        }
        public void RaiseMusicIsPlayingEvent(bool isPlaying)
        {
            _musicIsPlayingEvent.RaiseEvent(this, isPlaying, nameof(MusicIsPlayingEvent));
        }
        protected string GetAudioMusicTotalTimeFromSeconds(long maxTime)
        {
            _musicTimeTotal = TimeSpan.FromSeconds(maxTime);
            return _musicTimeTotal.Hours == 0 ? _musicTimeTotal.ToString("mm':'ss") : _musicTimeTotal.ToString("hh':'mm':'ss");
        }
        protected string GetAudioMusicTotalTimeFromMilisseconds(long maxTime)
        {
            _musicTimeTotal = TimeSpan.FromMilliseconds(maxTime);
            return _musicTimeTotal.Hours == 0 ? _musicTimeTotal.ToString("mm':'ss") : _musicTimeTotal.ToString("hh':'mm':'ss");
        }
        protected string GetAudioMusicPartTimeFromSeconds(int ticPosition)
        {
            TimeSpan musicTime = TimeSpan.FromSeconds(ticPosition);
            return _musicTimeTotal != null && _musicTimeTotal.Hours == 0 ? musicTime.ToString("mm':'ss") : musicTime.ToString("hh':'mm':'ss");
        }
        protected string GetAudioMusicPartTimeFromMilisseconds(long ticPosition)
        {
            TimeSpan musicTime = TimeSpan.FromMilliseconds(ticPosition);
            return _musicTimeTotal != null && _musicTimeTotal.Hours == 0 ? musicTime.ToString("mm':'ss") : musicTime.ToString("hh':'mm':'ss");
        }
        //protected int GetAudioMusicTime(long milisegundos)
        //{
        //    decimal minDuration = _albumPlayingNow.MusicTimeTotalSeconds / 60;
        //    decimal minMusicPart = AppHelper.ExoplayerTimeToTocaTudo(milisegundos) / 60;

        //    minDuration = (minDuration == 0) ? 1 : minDuration;
        //    minMusicPart = (minMusicPart == 0) ? 1 : minMusicPart;

        //    float musicPartTime = (float)Math.Round((minMusicPart / minDuration) * 100, 2);

        //    float currentPosition = (_albumPlayingNow.MusicTimeTotalSeconds / 100) * musicPartTime;

        //    return (int)currentPosition;
        //}
        protected int GetAudioMusicTime(int milisegundos)
        {
            decimal minDuration = AppHelper.ExoplayerTimeToTocaTudo(_albumPlayingNow.MusicTimeTotalSeconds) / 60;
            decimal minMusicPart = ((milisegundos * 1000) / 60);

            minDuration = (minDuration == 0) ? 1 : minDuration;
            minMusicPart = (minMusicPart == 0) ? 1 : minMusicPart;

            float musicPartTime = (float)Math.Round(((minMusicPart / minDuration) * 100), 2);

            float currentPosition = (AppHelper.ExoplayerTimeToTocaTudo(_albumPlayingNow.MusicTimeTotalSeconds) / 100) * musicPartTime;

            return (int)currentPosition;
        }
        protected long GetMusicSeconds(long milisegundos)
        {
            return milisegundos / 1000;
        }

        #region Private Methods
        private void PlayerMusicType()
        {
            if (AppHelper.MusicPlayerInterstitialIsLoadded || !_audioPlayer.IsPlaying)
                return;

            long ticPosition = _audioPlayer.CurrentPosition();

            _totalMerchanMusicTimePlayed += 1000;

            TimeSpan mTime = TimeSpan.FromMilliseconds(_totalMerchanMusicTimePlayed);

            bool showMerchan = (App.AppConfig?.MusicMerchanMinutesIntervalToShow - mTime.Minutes) <= 0 && App.AppConfig?.MusicMerchanMinutesIntervalToShow > 0;

            SetMusicStatusBottomModel(ticPosition, showMerchan);
        }
        private void PlayerAlbumType()
        {
            if (AppHelper.MusicPlayerInterstitialIsLoadded || !_audioPlayer.IsPlaying || _playlistItemNow == null)
                return;

            long sizeNextMusic = GetAudioMusicTime(_playlistItemNow.TempoSegundos);
            long ticPosition = _audioPlayer.CurrentPosition() - sizeNextMusic;

            _totalMerchanMusicTimePlayed += 1000;

            TimeSpan mTime = TimeSpan.FromMilliseconds(_totalMerchanMusicTimePlayed);

            bool showMerchan = (App.AppConfig?.AlbumMerchanMinutesIntervalToShow - mTime.Minutes) <= 0 && App.AppConfig?.AlbumMerchanMinutesIntervalToShow > 0;

            SetMusicStatusBottomModel(ticPosition, showMerchan);
        }
        private void SetMusicStatusBottomModel(long ticPosition, bool showMerchan)
        {
            if (showMerchan)
            {
                _totalMerchanMusicTimePlayed = 0;
                AudioPlayerShowInterstitial();
            }

            _musicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromMilisseconds(ticPosition);
        }
        private void AudioPlayerShowInterstitial()
        {
            if (!App.IsSleeping)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await CustomCrossMTAdmob.LoadAndShowInterstitial(AppHelper.GetIntertistialAdsVisibleScreen(), () =>
                    {
                        _commonMusicPlayerViewModel.PlayMusic();
                    }, () =>
                    {
                        _commonMusicPlayerViewModel.Pause();
                    });
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppHelper.HasInterstitialToShow = true;
                    _commonMusicPlayerViewModel.Pause();
                });
            }
        }
        private void AudioPlayer_PlayerInitializing(object sender, EventArgs e)
        {
            _musicIsPlayingEvent.RaiseEvent(sender, false, nameof(MusicIsPlayingEvent));
        }
        private void AudioPlayer_PlayerSeekComplete(object sender, EventArgs e)
        {
            _keepUpdatingMusicClock = true;
            _audioPlayer.Play();
        }
        private Command PausePlayEventCommand()
        {
            return new Command(
                   execute: () =>
                   {
                       if (_playlistItemNow == null)
                           _commonMusicPlayerViewModel.PlayPauseMusic(musicIsActiveOnPause: false);
                       else
                           _commonMusicPlayerViewModel.PlayPauseMusic(_playlistItemNow, musicIsActiveOnPause: false);
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
        private Command<long> ProgressBarDragCompletedEventCommand()
        {
            return new Command<long>(
                   execute: (seekTo) =>
                   {
                       MusicSeekTo(seekTo);
                   });
        }
        #endregion
    }
    public class PlayerClockUpdate : IDisposable
    {
        private readonly MusicBottomPlayerBaseViewModel _baseViewModel;
        private readonly IAudio _audioPlayer;
        private readonly MusicStatusBottomModel _musicStatusBottomModel;
        private bool _stopTimer;
        public PlayerClockUpdate(MusicBottomPlayerBaseViewModel baseViewModel, IAudio audioPlayer, MusicStatusBottomModel musicStatusBottomModel)
        {
            _baseViewModel = baseViewModel;
            _audioPlayer = audioPlayer;
            _musicStatusBottomModel = musicStatusBottomModel;

            _stopTimer = false;
        }
        public void ClockPlayerUpdate(Action clockPlayerType)
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (_stopTimer)
                    return false;

                if (_audioPlayer.IsPlaying && !_musicStatusBottomModel.MusicStreamComplete && _baseViewModel.KeepUpdatingMusicClock)
                {
                    clockPlayerType();
                }

                if (_audioPlayer.IsPlaying && _musicStatusBottomModel.MusicStreamComplete && _baseViewModel.KeepUpdatingMusicClock)
                {
                    _musicStatusBottomModel.MusicPartTimeDesc = _musicStatusBottomModel.MusicTotalTimeDesc;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    ClockPlayerUpdate(clockPlayerType);
                });

                return false;
            });
        }
        public void Dispose()
        {
            _stopTimer = true;
            GC.SuppressFinalize(this);
        }
    }
}
