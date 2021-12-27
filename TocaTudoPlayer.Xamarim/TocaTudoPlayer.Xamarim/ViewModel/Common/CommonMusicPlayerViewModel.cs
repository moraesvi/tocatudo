using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;
using YoutubeParse.ExplodeV2.Videos.Streams;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonMusicPlayerViewModel : BaseViewModel, ICommonMusicPlayerViewModel
    {
        private readonly IAudio _audioPlayer;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly YoutubeClient _ytClient;
        private readonly IMusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private readonly IMusicBottomPlayerViewModel _bottomPlayerViewModel;
        private HttpDownload _download;
        private static MusicPlayerConfig _musicPlayerConfig;
        private static MusicPlayerConfig _nextMusicPlayerConfig;
        private Action<ICommonMusicModel> _musicPlayedHistoricIsSaved;
        private event Action _playerLosedAudioFocus;
        public CommonMusicPlayerViewModel(IPCLStorageDb pclStorageDb, IPCLUserMusicLogic pclUserMusicLogic, IMusicPlayedHistoryViewModel musicPlayedHistoryViewModel, IMusicBottomPlayerViewModel bottomPlayerViewModel, YoutubeClient ytClient)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;
            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;
            _bottomPlayerViewModel = bottomPlayerViewModel;
            _ytClient = ytClient;

            _download = new HttpDownload();

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerInvalidUri += AudioPlayer_PlayerInvalidUri;
            _bottomPlayerViewModel.NextMusicEvent += BottomPlayerViewModel_NextMusicEvent;
            _audioPlayer.PlayerLosedAudioFocus += AudioPlayer_PlayerLosedAudioFocus;
        }

        public event Action<ICommonMusicModel> MusicPlayedHistoricIsSaved
        {
            add => _musicPlayedHistoricIsSaved += value;
            remove => _musicPlayedHistoricIsSaved -= value;
        }
        public event Action PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus += value;
            remove => _playerLosedAudioFocus -= value;
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
        public async Task PlayMusic(ICommonMusicModel music, CancellationToken cancellationToken)
        {
            AppHelper.MusicPlayerInterstitialWasShowed = false;
            await _pclUserMusicLogic.LoadDb();

            music.ShowMerchandisingAlert = _musicPlayerConfig.CheckIfMusicPlayedCountAchieveTotal(autoRebuild: true);

            if (music.IsSavedOnLocalDb)
            {
                await _pclUserMusicLogic.LoadDb();
                await PlayPauseMusicFromDb(music);
            }
            else
            {
                bool musicSavedInDb = _pclUserMusicLogic.ExistsOnLocalDb(music.VideoId);

                if (musicSavedInDb)
                {
                    await PlayPauseMusicFromDb(music);
                }
                else
                {
                    await Task.Run(async () => //Required for Android
                    {
                        _bottomPlayerViewModel.StartBottomPlayer();

                        music.IsLoadded = false;
                        music.IsActiveMusic = true;
                        music.IsBufferingMusic = true;

                        Task<StreamManifest> tskMusic = _ytClient.Videos.Streams.GetManifestAsync(music.VideoId, cancellationToken).AsTask();
                        Task tskMusicImageInfo = _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(music.VideoId, cancellationToken);

                        AudioOnlyStreamInfo streamInfo = null;

                        await Task.Delay(300);//Required for multiples selections
                        await Task.WhenAll(tskMusic, tskMusicImageInfo).ContinueWith(tsk =>
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
                                catch (Exception)
                                {

                                }
                            }
                        });

                        if (tskMusic.IsFaulted || tskMusicImageInfo.IsFaulted)
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

                        MusicModelServicePlayer musicModel = new MusicModelServicePlayer()
                        {
                            Id = music.Id,
                            Number = 0,
                            Music = music.MusicName,
                            Image = _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage
                        };

                        _audioPlayer.Source(streamInfo.Url, music, musicModel);
                    }).ConfigureAwait(false);
                }
            }

            _pclUserMusicLogic.UnLoadDb();
        }
        public async Task PlayMusic(ICommonMusicModel music, SearchMusicModel[] nextMusics, MusicPlayerConfig config, CancellationToken cancellationToken)
        {
            AppHelper.MusicPlayerInterstitialWasShowed = false;
            _nextMusicPlayerConfig = config;

            await _pclUserMusicLogic.LoadDb();

            if (music.IsSavedOnLocalDb)
            {
                await PlayPauseMusicFromDb(music, nextMusics);
            }
            else
            {
                bool musicSavedInDb = _pclUserMusicLogic.ExistsOnLocalDb(music.VideoId);

                if (musicSavedInDb)
                {
                    await PlayPauseMusicFromDb(music, nextMusics);
                }
                else
                {
                    await Task.Run(async () => //Required for Android
                    {
                        _bottomPlayerViewModel.StartBottomPlayer(nextMusics);

                        music.IsLoadded = false;
                        music.IsActiveMusic = true;
                        music.IsBufferingMusic = true;

                        Task<StreamManifest> tskMusic = _ytClient.Videos.Streams.GetManifestAsync(music.VideoId, cancellationToken).AsTask();
                        Task tskMusicImageInfo = _bottomPlayerViewModel.MusicStatusBottomModel.LoadMusicImageInfo(music.VideoId, cancellationToken);

                        AudioOnlyStreamInfo streamInfo = null;

                        await Task.Delay(300);//Required for multiples selections
                        await Task.WhenAll(tskMusic, tskMusicImageInfo).ContinueWith(tsk =>
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

                        if (tskMusic.IsFaulted || tskMusicImageInfo.IsFaulted)
                        {
                            PlayMusicFaultedResult(music, tskMusic);
                            return;
                        }

                        if (streamInfo == null)
                            return;

                        MusicModelServicePlayer musicModel = new MusicModelServicePlayer()
                        {
                            Id = music.Id,
                            Number = 0,
                            Music = music.MusicName,
                            Image = _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage
                        };

                        _audioPlayer.Source(streamInfo.Url, music, musicModel);
                    }).ConfigureAwait(false);
                }
            }

            _pclUserMusicLogic.UnLoadDb();
        }
        public async Task PlaySavedMusic(ICommonMusicModel music)
        {
            AppHelper.MusicPlayerInterstitialWasShowed = false;

            music.ShowMerchandisingAlert = _musicPlayerConfig.CheckIfMusicPlayedCountAchieveTotal(autoRebuild: true);

            if (music.IsSavedOnLocalDb)
            {
                await _pclUserMusicLogic.LoadDb();
                await PlayPauseMusicFromDb(music);
            }
        }
        public void PlayPauseMusic(ICommonMusicModel music)
        {
            if (_audioPlayer.IsPlaying)
            {
                _audioPlayer.Pause();

                music.IsActiveMusic = false;
                music.IsPlaying = false;
            }
            else
            {
                _audioPlayer.Play();

                music.IsActiveMusic = true;
                music.IsPlaying = true;
            }
        }
        public void Stop(ICommonMusicModel music)
        {
            _audioPlayer.Pause();

            music.IsActiveMusic = false;
            music.IsPlaying = false;

            HideBottomPlayer();
            StopBottomPlayer();
        }
        public void PlayPauseMusic()
        {
            if (_audioPlayer.IsPlaying)
            {
                _audioPlayer.Pause();
            }
            else
            {
                _audioPlayer.Play();
            }
        }
        public async Task StartDownloadMusic(ICommonMusicModel music)
        {
            await music.StartDownloadMusic(_bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage);
        }
        public void ActiveBottomPlayer()
        {
            _bottomPlayerViewModel.ActiveBottomPlayer(this);
        }
        public void HideBottomPlayer()
        {
            _bottomPlayerViewModel.BottomPlayerControlIsVisible = false;
        }
        public void StopBottomPlayer()
        {
            _bottomPlayerViewModel.StopBottomPlayer();
        }

        #region Private Methods
        private async Task PlayPauseMusicFromDb(ICommonMusicModel musicPlayer)
        {
            if (musicPlayer.IsPlaying)
            {
                _audioPlayer.Pause();
                musicPlayer.IsPlaying = false;
                musicPlayer.IsBufferingMusic = false;

                return;
            }

            _bottomPlayerViewModel.StartBottomPlayer();

            await PlayPauseFromDb(musicPlayer);
        }
        private async Task PlayPauseMusicFromDb(ICommonMusicModel musicPlayer, SearchMusicModel[] nextMusics)
        {
            if (musicPlayer.IsPlaying)
            {
                _audioPlayer.Pause();
                musicPlayer.IsPlaying = false;
                musicPlayer.IsBufferingMusic = false;

                return;
            }

            _bottomPlayerViewModel.StartBottomPlayer(nextMusics);

            await PlayPauseFromDb(musicPlayer);
        }
        private async Task PlayPauseFromDb(ICommonMusicModel musicPlayer)
        {
            await Task.Run(async () =>
            {
                (UserMusic, byte[]) music = await _pclUserMusicLogic.GetMusicById(musicPlayer.VideoId);

                musicPlayer.IsLoadded = false;
                musicPlayer.IsActiveMusic = true;
                musicPlayer.IsBufferingMusic = true;
                musicPlayer.MusicModel = new MusicModel(music.Item1, music.Item2);

                MusicModelServicePlayer musicModel = new MusicModelServicePlayer()
                {
                    Id = musicPlayer.Id,
                    Number = 0,
                    Music = musicPlayer.MusicName,
                    Image = musicPlayer.MusicModel.MusicImage
                };

                _audioPlayer.Source(music.Item2, musicPlayer, musicModel);
            }).ConfigureAwait(false);
        }
        private async Task OnlyPlayMusic(ICommonMusicModel music)
        {
            StreamManifest ytManifest = null;

            music.IsLoadded = false;
            music.IsActiveMusic = true;
            music.IsBufferingMusic = true;

            AppHelper.MusicPlayerInterstitialWasShowed = false;

            await _ytClient.Videos.Streams.GetManifestAsync(music.VideoId)
                                          .AsTask()
                                          .ContinueWith((tsk) =>
                                          {
                                              if (tsk.IsFaulted)
                                              {
                                                  PlayMusicFaultedResult(music, tsk);
                                              }

                                              ytManifest = tsk.Result;
                                          })
                                          .ConfigureAwait(false);

            AudioOnlyStreamInfo streamInfo = ytManifest.GetAudioOnlyStreams()
                                                       .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                       .FirstOrDefault();

            _audioPlayer.Source(streamInfo.Url, music);
        }
        private void PlayMusicFaultedResult(ICommonMusicModel music, Task<StreamManifest> tsk)
        {
            music.IsLoadded = true;
            music.IsActiveMusic = false;
            music.IsBufferingMusic = false;

            _bottomPlayerViewModel.BottomPlayerControlIsVisible = false;
            _bottomPlayerViewModel.StopBottomPlayer();

            if (tsk.Exception.Message.IndexOf("410") >= 0)
            {
                RaiseAppErrorEvent(0, AppResource.MusicIsNotPlayable);
            }
        }
        private async void AudioPlayer_PlayerReady(ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsActiveMusic)
                {
                    if (music.IsLoadded)
                        return;

                    if (music.SearchType != MusicSearchType.SearchSavedMusic)
                    {
                        if (!music.IsHistoryPlayedSavedOnLocalDb)
                        {
                            await _musicPlayedHistoryViewModel.SaveLocalHistory(music, _bottomPlayerViewModel.MusicStatusBottomModel.ByteMusicImage);
                            
                            if (music.SearchType == MusicSearchType.SearchMusicAlbumHistory)
                            {
                                if (_musicPlayedHistoricIsSaved != null)
                                    _musicPlayedHistoricIsSaved(music);
                            }
                        }
                    }
                }
            }
        }
        private async void AudioPlayer_PlayerInvalidUri(ICommonMusicModel obj)
        {
            await OnlyPlayMusic(obj);
        }
        private async void BottomPlayerViewModel_NextMusicEvent(SearchMusicModel obj)
        {
            obj.IsSelected = true;
            obj.ShowMerchandisingAlert = _nextMusicPlayerConfig?.CheckIfMusicPlayedCountAchieveTotal(autoRebuild: true) ?? false;

            await PlayMusic(obj, CancellationToken.None);
        }
        private void AudioPlayer_PlayerLosedAudioFocus()
        {
            if (_playerLosedAudioFocus != null)
                _playerLosedAudioFocus();
        }

        #endregion
    }
}
