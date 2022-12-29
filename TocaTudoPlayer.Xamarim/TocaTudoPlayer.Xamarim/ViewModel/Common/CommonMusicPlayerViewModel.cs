using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonMusicPlayerViewModel : BaseViewModel
    {
        private const int TIME_DELAY_REQUIRED_FOR_MULTIPLES_SELECTIONS = 20;
        private readonly IAudio _audioPlayer;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly YoutubeClient _ytClient;
        private readonly MusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private readonly MusicBottomPlayerViewModel _bottomPlayerViewModel;
        private HttpDownload _download;
        private static MusicPlayerConfig _musicPlayerConfig;
        private static MusicPlayerConfig _nextMusicPlayerConfig;
        private WeakEventManager<ICommonMusicModel> _musicPlayedHistoricIsSaved;
        private WeakEventManager _playerLosedAudioFocus;
        public CommonMusicPlayerViewModel(IPCLStorageDb pclStorageDb, IPCLUserMusicLogic pclUserMusicLogic, MusicPlayedHistoryViewModel musicPlayedHistoryViewModel, MusicBottomPlayerViewModel bottomPlayerViewModel, YoutubeClient ytClient)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;
            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;
            _bottomPlayerViewModel = bottomPlayerViewModel;
            _ytClient = ytClient;
            _playerLosedAudioFocus = new WeakEventManager();
            _musicPlayedHistoricIsSaved = new WeakEventManager<ICommonMusicModel>();

            _download = new HttpDownload();

            _audioPlayer.PlayerReady -= AudioPlayer_PlayerReady;
            _audioPlayer.PlayerAlbumInvalidUri -= AudioPlayer_PlayerAlbumInvalidUri;
            _audioPlayer.PlayerMusicInvalidUri -= AudioPlayer_PlayerMusicInvalidUri;
            _bottomPlayerViewModel.NextMusicEvent -= BottomPlayerViewModel_NextMusicEvent;
            _audioPlayer.PlayerLosedAudioFocus -= AudioPlayer_PlayerLosedAudioFocus;

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerAlbumInvalidUri += AudioPlayer_PlayerAlbumInvalidUri;
            _audioPlayer.PlayerMusicInvalidUri += AudioPlayer_PlayerMusicInvalidUri;
            _bottomPlayerViewModel.NextMusicEvent += BottomPlayerViewModel_NextMusicEvent;
            _audioPlayer.PlayerLosedAudioFocus += AudioPlayer_PlayerLosedAudioFocus;
        }

        public event EventHandler<ICommonMusicModel> MusicPlayedHistoricIsSaved
        {
            add => _musicPlayedHistoricIsSaved.AddEventHandler(value);
            remove => _musicPlayedHistoricIsSaved.RemoveEventHandler(value);
        }
        public event EventHandler PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus.AddEventHandler(value);
            remove => _playerLosedAudioFocus.RemoveEventHandler(value);
        }
        public bool BottomPlayerIsActive => _bottomPlayerViewModel.PlayerIsActive;
        public bool HasMusicPlaying => _audioPlayer.IsPlaying;
        public ICommonMusicModel MusicPlayingNow
        {
            get { return _bottomPlayerViewModel.MusicPlayingNow; }
            set { _bottomPlayerViewModel.MusicPlayingNow = value; }
        }
        public MusicSearchType KindMusicPlayingNow
        {
            get { return _bottomPlayerViewModel.KindMusicPlayingNow; }
            set { _bottomPlayerViewModel.KindMusicPlayingNow = value; }
        }
        public SearchMusicModel LastMusicPlayed
        {
            get { return _bottomPlayerViewModel.LastMusicPlayed; }
            set { _bottomPlayerViewModel.LastMusicPlayed = value; }
        }
        public void SetMusicPlayerConfig(MusicPlayerConfig config)
        {
            _musicPlayerConfig = config;
        }
        public void SetAlbumMusicPlaylist(AlbumMusicModelServicePlayer musicServicePlayer)
        {
            _audioPlayer.Source(musicServicePlayer);
        }
        public void ClearAlbumSource()
        {
            _audioPlayer.ClearAlbumSource();
        }
        public void HideStatusBarPlayerControls()
        {
            _audioPlayer.HideStatusBarPlayerControls();
        }
        public void PlayMusic()
        {
            if (AppHelper.MusicPlayerInterstitialIsLoadded)
                return;

            _audioPlayer.Play();
            if (MusicPlayingNow != null)
            {
                MusicPlayingNow.IsPlaying = true;
                MusicPlayingNow.IsActiveMusic = true;
            }
        }
        public async Task PlayMusic(ICommonMusicModel music, CancellationToken cancellationToken)
        {
            AppHelper.MusicPlayerInterstitialIsLoadded = false;
            await _pclUserMusicLogic.LoadDb();

            if (music.IsSavedOnLocalDb)
            {
                await _pclUserMusicLogic.LoadDb();
                await PlayMusicFromDb(music);
            }
            else
            {
                bool musicSavedInDb = _pclUserMusicLogic.ExistsOnLocalDb(music.VideoId);

                if (musicSavedInDb)
                {
                    await PlayMusicFromDb(music);
                }
                else
                {
                    await Task.Run(async () => //Required for Android
                    {
                        _bottomPlayerViewModel.Init(this);
                        _bottomPlayerViewModel.StartBottomPlayer(music.SearchType);

                        music.IsLoadded = false;
                        music.IsActiveMusic = true;
                        music.IsBufferingMusic = true;

                        Task<StreamManifest> tskMusic = _ytClient.Videos.Streams.GetManifestAsync(music.VideoId, cancellationToken).AsTask();

                        AudioOnlyStreamInfo streamInfo = null;

                        await Task.Delay(TIME_DELAY_REQUIRED_FOR_MULTIPLES_SELECTIONS);//Required for multiples selections
                        await tskMusic.ContinueWith(tsk =>
                        {
                            if (!tskMusic.IsCanceled && !tskMusic.IsFaulted)
                            {
                                try
                                {
                                    streamInfo = tskMusic.Result
                                                         .GetAudioOnlyStreams()
                                                         .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                         .FirstOrDefault();
                                }
                                catch (Exception) { }
                            }
                        });

                        if (tskMusic.IsFaulted)
                        {
                            PlayMusicFaultedResult(music, tskMusic);
                            return;
                        }

                        if (streamInfo == null)
                        {
                            music.IsLoadded = false;
                            music.IsActiveMusic = true;
                            music.IsBufferingMusic = false;

                            return;
                        }

                        ShowMerchandisingAlert(music);

                        await _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(music);

                        MusicModelServicePlayer musicModel = new MusicModelServicePlayer(music)
                        {
                            Id = music.Id,
                            Number = 0,
                            Music = music.MusicName,
                            Image = _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage,
                        };

                        _audioPlayer.Source(streamInfo.Url, music, _bottomPlayerViewModel.MusicStatusBottomModel, musicModel);

                        IsInternetAvaiable = true;

                        App.EventTracker.SendEvent("PlayMusic", new Dictionary<string, string>()
                        {
                            { "MusicName", music.MusicName },
                            { "VideoId", music.VideoId }
                        });
                    });
                }
            }

            _pclUserMusicLogic.UnLoadDb();
        }
        public async Task PlayMusic(ICommonMusicModel music, SearchMusicModel[] nextMusics, MusicPlayerConfig config, CancellationToken cancellationToken)
        {
            AppHelper.MusicPlayerInterstitialIsLoadded = false;
            _nextMusicPlayerConfig = config;

            await _pclUserMusicLogic.LoadDb();

            if (music.IsSavedOnLocalDb)
            {
                await PlayFromDb(music, nextMusics);
            }
            else
            {
                bool musicSavedInDb = _pclUserMusicLogic.ExistsOnLocalDb(music.VideoId);

                if (musicSavedInDb)
                {
                    await PlayFromDb(music, nextMusics);
                }
                else
                {
                    await Task.Run(async () => //Required for Android
                    {
                        _bottomPlayerViewModel.Init(this);
                        _bottomPlayerViewModel.StartBottomPlayer(music.SearchType, nextMusics);

                        music.IsLoadded = false;
                        music.IsActiveMusic = true;
                        music.IsBufferingMusic = true;

                        Task<StreamManifest> tskMusic = _ytClient.Videos.Streams.GetManifestAsync(music.VideoId, cancellationToken).AsTask();

                        AudioOnlyStreamInfo streamInfo = null;

                        await Task.Delay(TIME_DELAY_REQUIRED_FOR_MULTIPLES_SELECTIONS);//Required for multiples selections
                        await Task.WhenAll(tskMusic).ContinueWith(tsk =>
                        {
                            if (!tskMusic.IsCanceled && !tskMusic.IsFaulted)
                            {
                                try
                                {
                                    streamInfo = tskMusic.Result
                                                         .GetAudioOnlyStreams()
                                                         .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                         .FirstOrDefault();
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        });

                        if (tskMusic.IsFaulted)
                        {
                            PlayMusicFaultedResult(music, tskMusic);
                            return;
                        }

                        if (streamInfo == null)
                        {
                            music.IsLoadded = false;
                            music.IsActiveMusic = true;
                            music.IsBufferingMusic = false;

                            return;
                        }

                        ShowMerchandisingAlert(music);

                        await _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(music);

                        MusicModelServicePlayer musicModel = new MusicModelServicePlayer(music)
                        {
                            Id = music.Id,
                            Number = 0,
                            Music = music.MusicName,
                            Image = _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage
                        };

                        _audioPlayer.Source(streamInfo.Url, music, _bottomPlayerViewModel.MusicStatusBottomModel, musicModel);
                        IsInternetAvaiable = true;

                        App.EventTracker.SendEvent("PlayMusic", new Dictionary<string, string>()
                        {
                            { "MusicName", music.MusicName },
                            { "VideoId", music.VideoId }
                        });
                    });
                }
            }

            _pclUserMusicLogic.UnLoadDb();
        }
        public async Task PlaySavedMusic(ICommonMusicModel music)
        {
            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            ShowMerchandisingAlert(music);

            if (music.IsSavedOnLocalDb)
            {
                await _pclUserMusicLogic.LoadDb();
                await PlayMusicFromDb(music);
            }
        }
        public async Task PlaySavedMusic(ICommonMusicModel music, SearchMusicModel[] nextMusics, MusicPlayerConfig config)
        {
            AppHelper.MusicPlayerInterstitialIsLoadded = false;
            _nextMusicPlayerConfig = config;

            ShowMerchandisingAlert(music);

            if (music.IsSavedOnLocalDb)
            {
                await _pclUserMusicLogic.LoadDb();
                await PlayFromDb(music, nextMusics);
            }
        }
        public void PlayPauseMusic(ICommonMusicModel music)
        {
            if (_audioPlayer.IsPlaying)
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());
                music.IsPlaying = false;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Play());
                music.IsPlaying = true;
            }
        }
        public void Pause()
        {
            Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());
            if (MusicPlayingNow != null)
            {
                MusicPlayingNow.IsActiveMusic = false;
                MusicPlayingNow.IsPlaying = false;
            }
        }
        public void Stop()
        {
            Device.BeginInvokeOnMainThread(() => _audioPlayer.Stop());
            if (MusicPlayingNow != null)
            {
                MusicPlayingNow.IsActiveMusic = false;
                MusicPlayingNow.IsPlaying = false;
            }

            HideBottomPlayer();
        }
        public void Stop(ICommonMusicModel music)
        {
            Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());

            music.IsActiveMusic = false;
            music.IsPlaying = false;

            HideBottomPlayer();
        }
        public void PlayPauseMusic(bool musicIsActiveOnPause = true)
        {
            if (_audioPlayer.IsPlaying)
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());

                if (MusicPlayingNow != null)
                {
                    MusicPlayingNow.IsActiveMusic = musicIsActiveOnPause;
                    MusicPlayingNow.IsPlaying = !MusicPlayingNow.IsPlaying;
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Play());

                if (MusicPlayingNow != null)
                {
                    MusicPlayingNow.IsActiveMusic = true;
                    MusicPlayingNow.IsPlaying = !MusicPlayingNow.IsPlaying;
                }
            }
        }
        public void PlayPauseMusic(PlaylistItem item, bool musicIsActiveOnPause = true)
        {
            if (_audioPlayer.IsPlaying)
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());

                item.IsActiveMusic = false;

                if (MusicPlayingNow != null)
                {
                    MusicPlayingNow.IsActiveMusic = musicIsActiveOnPause;
                    MusicPlayingNow.IsPlaying = !MusicPlayingNow.IsPlaying;
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Play());

                item.IsActiveMusic = true;

                if (MusicPlayingNow != null)
                {
                    MusicPlayingNow.IsActiveMusic = true;
                    MusicPlayingNow.IsPlaying = !MusicPlayingNow.IsPlaying;
                }
            }
        }
        public async Task StartDownloadMusic(ICommonMusicModel music)
        {
            await music.StartDownloadMusic(_bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage, _musicPlayedHistoryViewModel);
        }
        public void ActiveBottomPlayer()
        {
            _bottomPlayerViewModel.ActiveBottomPlayer();
        }
        public void StopBottomPlayer(bool force = false)
        {
            _bottomPlayerViewModel.StopBottomPlayer(force);
        }
        public void HideBottomPlayer()
        {
            _bottomPlayerViewModel.BottomPlayerControlIsVisible = false;
        }

        #region Private Methods
        private async Task PlayMusicFromDb(ICommonMusicModel music)
        {
            if (music.IsPlaying)
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());

                music.IsPlaying = false;
                music.IsBufferingMusic = false;

                return;
            }

            _bottomPlayerViewModel.Init(this);
            _bottomPlayerViewModel.StartBottomPlayer(music.SearchType);

            await PlayFromDb(music);
        }
        private async Task PlayFromDb(ICommonMusicModel music, SearchMusicModel[] nextMusics)
        {
            if (music.IsPlaying)
            {
                Device.BeginInvokeOnMainThread(() => _audioPlayer.Pause());

                music.IsPlaying = false;
                music.IsBufferingMusic = false;

                return;
            }

            _bottomPlayerViewModel.Init(this);
            _bottomPlayerViewModel.StartBottomPlayer(music.SearchType, nextMusics);

            await PlayFromDb(music);
        }
        private async Task PlayFromDb(ICommonMusicModel musicItem)
        {
            (UserMusic, byte[]) music = await _pclUserMusicLogic.GetMusicById(musicItem.VideoId);

            if (music.Item1 == null || music.Item2 == null)
            {
                musicItem.IsLoadded = false;
                musicItem.IsActiveMusic = false;

                RaiseAppErrorEvent(AppResource.MusicIsNotPlayable);
                return;
            }

            musicItem.IsLoadded = false;
            musicItem.IsActiveMusic = true;
            musicItem.IsBufferingMusic = true;
            musicItem.MusicModel = new MusicModel(music.Item1, music.Item2);

            MusicModelServicePlayer musicModel = new MusicModelServicePlayer(musicItem)
            {
                Id = musicItem.Id,
                Number = 0,
                Music = musicItem.MusicName,
                Image = musicItem.MusicModel.MusicImage
            };

            _audioPlayer.Source(music.Item2, musicItem, _bottomPlayerViewModel.MusicStatusBottomModel, musicModel);

            App.EventTracker.SendEvent("PlayFromDb", new Dictionary<string, string>()
            {
                { "MusicName", musicItem.MusicName },
                { "VideoId", musicItem.VideoId }
            });
        }
        private async Task OnlyPlayMusic(ICommonMusicModel music)
        {
            StreamManifest ytManifest = null;

            music.IsLoadded = false;
            music.IsActiveMusic = true;
            music.IsBufferingMusic = true;

            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            await _ytClient.Videos.Streams.GetManifestAsync(music.VideoId)
                                          .AsTask()
                                          .ContinueWith((tsk) =>
                                          {
                                              if (tsk.IsFaulted)
                                              {
                                                  PlayMusicFaultedResult(music, tsk);
                                              }

                                              ytManifest = tsk.Result;
                                          });

            AudioOnlyStreamInfo streamInfo = ytManifest.GetAudioOnlyStreams()
                                                       .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                       .FirstOrDefault();

            _audioPlayer.Source(streamInfo.Url, music, _bottomPlayerViewModel.MusicStatusBottomModel);
        }
        private async Task OnlyPlayMusic(string videoId)
        {
            StreamManifest ytManifest = null;

            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            await _ytClient.Videos.Streams.GetManifestAsync(videoId)
                                          .AsTask()
                                          .ContinueWith((tsk) =>
                                          {
                                              if (tsk.IsFaulted)
                                              {
                                                  PlayAlbumFaultedResult(tsk);
                                              }

                                              ytManifest = tsk.Result;
                                          });

            AudioOnlyStreamInfo streamInfo = ytManifest.GetAudioOnlyStreams()
                                                       .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                       .FirstOrDefault();

            _audioPlayer.Source(streamInfo.Url, videoId);
        }
        private void ShowMerchandisingAlert(ICommonMusicModel music)
        {
            music.ShowMerchandisingAlert = _musicPlayerConfig.CheckIfMusicPlayedCountAchieveTotal(autoRebuild: true);
            if (music.ShowMerchandisingAlert)
                HideStatusBarPlayerControls();
        }
        private void PlayMusicFaultedResult(ICommonMusicModel music, Task<StreamManifest> tsk)
        {
            music.IsLoadded = true;
            music.IsActiveMusic = false;
            music.IsBufferingMusic = false;

            _bottomPlayerViewModel.BottomPlayerControlIsVisible = false;
            _bottomPlayerViewModel.StopBottomPlayer();

            if (tsk?.Exception?.Message?.IndexOf("410") >= 0 || tsk?.Exception?.Message?.IndexOf("is not", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                RaiseAppErrorEvent(AppResource.MusicIsNotPlayable);
            }
            else
            {
                RaiseAppErrorEvent(AppResource.MusicUnableToPlay);
            }
        }
        private void PlayAlbumFaultedResult(Task<StreamManifest> tsk)
        {
            if (tsk?.Exception?.Message?.IndexOf("410") >= 0 || tsk?.Exception?.Message?.IndexOf("is not", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                RaiseAppErrorEvent(AppResource.MusicIsNotPlayable);
            }
            else
            {
                RaiseAppErrorEvent(AppResource.MusicUnableToPlay);
            }
        }
        private void AudioPlayer_PlayerReady(object sender, ICommonMusicModel music)
        {
            Task.Run(async () =>
            {
                if (music != null)
                {
                    if (music.IsActiveMusic)
                    {
                        if (music.SearchType != MusicSearchType.SearchMusicAlbumHistory)
                        {
                            if (_musicPlayedHistoricIsSaved != null)
                                _musicPlayedHistoricIsSaved.RaiseEvent(sender, music, nameof(MusicPlayedHistoricIsSaved));
                        }

                        if (music.SearchType != MusicSearchType.SearchSavedMusic)
                        {
                            await _musicPlayedHistoryViewModel.SaveLocalHistory(music, _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage);
                        }
                    }
                }
            }).OnError("SaveLocalHistory", () =>
            {
                RaiseDefaultAppErrorEvent();
            }).ConfigureAwait(false);
        }
        private async void AudioPlayer_PlayerAlbumInvalidUri(object sender, string videoId)
        {
            await OnlyPlayMusic(videoId).OnError(nameof(CommonMusicPlayerViewModel), () => RaiseAppErrorEvent(AppResource.AppDefaultError));

            App.EventTracker.SendEvent("PlayerAlbumInvalidUri", new Dictionary<string, string>()
            {
                { "VideoId", videoId },
            });
        }
        private async void AudioPlayer_PlayerMusicInvalidUri(object sender, ICommonMusicModel obj)
        {
            await OnlyPlayMusic(obj).OnError(nameof(CommonMusicPlayerViewModel), () => RaiseAppErrorEvent(AppResource.AppDefaultError));

            App.EventTracker.SendEvent("PlayerMusicInvalidUri", new Dictionary<string, string>()
            {
                { "VideoId", obj.VideoId },
            });
        }
        private async void BottomPlayerViewModel_NextMusicEvent(object sender, SearchMusicModel obj)
        {
            obj.IsSelected = true;
            ShowMerchandisingAlert(obj);

            await PlayMusic(obj, CancellationToken.None);
        }
        private void AudioPlayer_PlayerLosedAudioFocus(object sender, EventArgs e)
        {
            if (_playerLosedAudioFocus != null)
                _playerLosedAudioFocus.RaiseEvent(this, null, nameof(PlayerLosedAudioFocus));
        }

        #endregion
    }
}
