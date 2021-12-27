using System;
using System.Collections.Generic;
using System.Linq;
using TocaTudoPlayer.Xamarim.Interface;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim.ViewModel.CustomView
{
    public class MusicBottomPlayerViewModel : MusicBottomPlayerViewModelBase, IMusicBottomPlayerViewModel
    {
        private readonly IAudio _audioPlayer;
        private readonly IDbLogic _albumDbLogic;

        private ICommonMusicModel _musicPlayingNow;
        private SearchMusicModel _lastMusicPlayed;
        private List<SearchMusicModel> _searchMusicCollection;

        private bool _startProgressBarPlayer;

        private event Action<float> _musicPlayerLoadedEvent;
        private event Action<float> _musicStreamProgessEvent;
        private event Action<SearchMusicModel> _nextMusicEvent;
        public MusicBottomPlayerViewModel(ITocaTudoApi tocaTudoApi, IDbLogic albumDbLogic)
            : base(tocaTudoApi)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _albumDbLogic = albumDbLogic;
            _startProgressBarPlayer = false;
            _searchMusicCollection = new List<SearchMusicModel>();

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;

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
        public event Action<SearchMusicModel> NextMusicEvent
        {
            add
            {
                _nextMusicEvent += value;
            }
            remove
            {
                _nextMusicEvent -= value;
            }
        }
        public SearchMusicModel LastMusicPlayed 
        {
            get { return _lastMusicPlayed; }
            set { _lastMusicPlayed = value; }
        }
        public void StartBottomPlayer()
        {
            _startProgressBarPlayer = true;

            MillisecondsDelayMusicClock = 0;
            BottomPlayerControlIsVisible = true;
            MusicStatusBottomModel.BottomPlayerIsVisible = false;

            MusicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromSeconds(0);

            StartClock(PlayerType.Music);
        }
        public void StartBottomPlayer(SearchMusicModel[] searchMusicCollection)
        {
            _searchMusicCollection = searchMusicCollection.ToList();

            StartBottomPlayer();
        }
        public override void StopBottomPlayer()
        {
            if (_musicPlayingNow != null && !BottomPlayerControlIsVisible)
                _musicPlayingNow.IsActiveMusic = false;

            //base.StopBottomPlayer();
        }
        private void AudioPlayer_PlayerReady(ICommonMusicModel music)
        {
            if (music != null)
                if (music.IsActiveMusic)
                {
                    if (music.IsSavedOnLocalDb)
                        MusicStatusBottomModel.LoadMusicImageInfo(music.MusicModel.MusicImage);

                    float maxDuration = _audioPlayer.Max();

                    PlayerIsActive = true;
                    KeepUpdatingMusicClock = true;

                    MusicStatusBottomModel.MusicName = music.MusicName;
                    MusicStatusBottomModel.BottomPlayerIsVisible = true;

                    MusicStatusBottomModel.MusicTotalTimeDesc = GetAudioMusicTotalTimeFromMilisseconds(_audioPlayer.Max());

                    if (_startProgressBarPlayer)
                    {
                        BottomPlayerControlIsVisible = true;

                        _musicPlayerLoadedEvent(maxDuration);
                        _startProgressBarPlayer = false;
                        _musicPlayingNow = music;
                    }
                }
        }
        private void PlayerUpdate()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (PlayerIsActive)
                {
                    RaiseMusicIsPlayingEvent(_audioPlayer.IsPlaying);

                    if (_audioPlayer.IsPlaying && KeepUpdatingMusicClock)
                    {
                        long ticPosition = _audioPlayer.CurrentPosition();

                        if (ticPosition > 0)
                        {
                            decimal maxDuration = _audioPlayer.Max();
                            decimal musicPart = ticPosition;

                            MusicStatusBottomModel.MusicStreamProgress = (float)Math.Round(musicPart / maxDuration, 2);

                            if (MusicStatusBottomModel.MusicStreamProgress >= 1)
                            {
                                if (_musicPlayingNow?.IsActiveMusic ?? false)
                                {
                                    int index = _searchMusicCollection.ToList()
                                                                      .FindIndex(music => music.Id == _musicPlayingNow.Id);
                                    NextMusicToPlay(index);
                                }
                            }

                            _musicStreamProgessEvent((float)musicPart);
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
        private void NextMusicToPlay(int index)
        {
            if (index == -1 || index >= (_searchMusicCollection.Count - 1))
                return;
            
            if (_musicPlayingNow.IsActiveMusic)
            {
                SearchMusicModel musicPlaying = _searchMusicCollection[index];
                SearchMusicModel nextMusic = _searchMusicCollection[index + 1];

                musicPlaying.ReloadMusicPlayingIcon();
                musicPlaying.UpdMusicSelectedColor(false);
                musicPlaying.UpdMusicFontColor(false);

                _lastMusicPlayed = nextMusic;
                _musicPlayingNow.IsActiveMusic = false;

                _nextMusicEvent(nextMusic);
            }
        }
    }
}
