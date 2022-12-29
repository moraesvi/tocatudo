using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Interface;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicBottomPlayerViewModel : MusicBottomPlayerBaseViewModel
    {
        private readonly IAudio _audioPlayer;
        private readonly IDbLogic _albumDbLogic;

        private SearchMusicModel _lastMusicPlayed;
        private ICommonMusicModel _musicPlayingNow;
        private MusicSearchType _typeMusicPlayingNow;
        private List<SearchMusicModel> _searchMusicCollection;

        private bool _streamInProgress;
        private bool _startProgressBarPlayer;

        private WeakEventManager<float> _musicPlayerLoadedEvent;
        private WeakEventManager<float> _musicStreamProgessEvent;
        private WeakEventManager<SearchMusicModel> _nextMusicEvent;
        public MusicBottomPlayerViewModel(ITocaTudoApi tocaTudoApi, IDbLogic albumDbLogic)
            : base(tocaTudoApi)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _albumDbLogic = albumDbLogic;

            _streamInProgress = false;
            _startProgressBarPlayer = false;
            
            _musicPlayerLoadedEvent = new WeakEventManager<float>();
            _musicStreamProgessEvent = new WeakEventManager<float>();
            _nextMusicEvent = new WeakEventManager<SearchMusicModel>();
            _searchMusicCollection = new List<SearchMusicModel>();

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
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
        public event EventHandler<SearchMusicModel> NextMusicEvent
        {
            add => _nextMusicEvent.AddEventHandler(value);
            remove => _nextMusicEvent.RemoveEventHandler(value);
        }
        public ICommonMusicModel MusicPlayingNow
        {
            get { return _musicPlayingNow; }
            set { _musicPlayingNow = value; }
        }
        public MusicSearchType KindMusicPlayingNow
        {
            get { return _typeMusicPlayingNow; }
            set { _typeMusicPlayingNow = value; }
        }
        public SearchMusicModel LastMusicPlayed
        {
            get { return _lastMusicPlayed; }
            set { _lastMusicPlayed = value; }
        }
        public void Init(CommonMusicPlayerViewModel commonMusicPlayerViewModel)
        {
            CommonMusicPlayerManager.StopAllAlbumBottomPlayers();
            _musicPlayingNow = null;

            Initialize(commonMusicPlayerViewModel);
        }
        public void StartBottomPlayer(MusicSearchType musicSearchType)
        {
            _musicPlayingNow = null;
            _startProgressBarPlayer = true;

            if (musicSearchType != MusicSearchType.SearchMusicAlbumHistory && musicSearchType != MusicSearchType.SearchSavedMusic)
                _searchMusicCollection.Clear();

            KindMusicPlayingNow = musicSearchType;

            PlayerIsActive = true;
            BottomPlayerControlIsVisible = true;
            MusicStatusBottomModel.BottomPlayerIsVisible = false;

            MusicStatusBottomModel.MusicPartTimeDesc = GetAudioMusicPartTimeFromSeconds(0);

            PlayerUpdate();
            StartClock(PlayerType.Music);
        }
        public void StartBottomPlayer(MusicSearchType musicSearchType, SearchMusicModel[] searchMusicCollection)
        {
            _searchMusicCollection = searchMusicCollection.ToList();

            StartBottomPlayer(musicSearchType);
        }
        public override void StopBottomPlayer(bool force = false)
        {
            if (MusicPlayingNow != null && !BottomPlayerControlIsVisible)
                MusicPlayingNow.IsActiveMusic = false;

            MusicPlayingNow = null;
            LastMusicPlayed = null;
            KindMusicPlayingNow = MusicSearchType.Undefined;

            base.StopBottomPlayer(force);
        }
        private void AudioPlayer_PlayerReady(object sender, ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsActiveMusic)
                {
                    if (music.IsSavedOnLocalDb)
                        MusicStatusBottomModel.LoadMusicImageInfo(music.MusicModel?.MusicImage);

                    float maxDuration = music.MusicTimeTotalSeconds;

                    var t = _audioPlayer.Max();

                    PlayerIsActive = true;
                    KeepUpdatingMusicClock = true;

                    MusicStatusBottomModel.MusicName = music.MusicName;
                    MusicStatusBottomModel.BottomPlayerIsVisible = true;

                    MusicStatusBottomModel.MusicTotalTimeDesc = GetAudioMusicTotalTimeFromSeconds(music.MusicTimeTotalSeconds);

                    if (_startProgressBarPlayer)
                    {
                        BottomPlayerControlIsVisible = true;

                        _musicPlayerLoadedEvent.RaiseEvent(sender, maxDuration, nameof(MusicPlayerLoadedEvent));
                        _startProgressBarPlayer = false;
                        MusicPlayingNow = music;
                    }
                }
            }
        }
        private void PlayerUpdate()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (!PlayerIsActive)
                    return false;

                if (KeepUpdatingMusicClock)
                {
                    if ((_audioPlayer.IsPlaying || _streamInProgress) && _musicPlayingNow != null)
                    {
                        CommonMusicPlayerManager.SetActiveMusicBottomPlayer();

                        long ticPosition = _audioPlayer.CurrentPosition();

                        if (ticPosition > 0)
                        {
                            decimal maxDuration = _musicPlayingNow.MusicTimeTotalSeconds;
                            decimal musicPart = ticPosition;
                            decimal musicPartRound = Math.Round(musicPart / 1000);

                            MusicStatusBottomModel.MusicStreamProgress = (float)Math.Round(musicPartRound / maxDuration, 2);
                            _musicStreamProgessEvent.RaiseEvent(this, (float)musicPartRound, nameof(MusicStreamProgessEvent));

                            if (MusicStatusBottomModel.MusicStreamProgress >= 0.99)
                            {
                                if (MusicPlayingNow?.IsActiveMusic ?? false)
                                {
                                    NextMusicToPlay();
                                    return false;
                                }
                            }

                            _streamInProgress = MusicStatusBottomModel.MusicStreamProgress < 1;
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
        private void NextMusicToPlay()
        {
            int index = _searchMusicCollection.ToList()
                                              .FindIndex(music => string.Equals(music.VideoId, MusicPlayingNow.VideoId));

            if (index == -1 || index >= (_searchMusicCollection.Count - 1))
                return;

            if (MusicPlayingNow.IsActiveMusic)
            {
                SearchMusicModel musicPlaying = _searchMusicCollection[index];
                SearchMusicModel nextMusic = _searchMusicCollection[index + 1];

                musicPlaying.ReloadMusicPlayingIcon();
                musicPlaying.UpdMusicSelectedColor(isPlaying: false);
                musicPlaying.UpdMusicFontColor(isPlaying: false);

                _lastMusicPlayed = nextMusic;

                MusicPlayingNow.ReloadMusicPlayingIcon();
                MusicPlayingNow.IsPlaying = false;
                MusicPlayingNow.IsActiveMusic = false;

                _nextMusicEvent.RaiseEvent(this, nextMusic, nameof(NextMusicEvent));
            }
        }
    }
}
