using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicPageViewModel : MusicAlbumPageBaseViewModel
    {
        private readonly IDbLogic _albumDbLogic;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly CommonMusicPageViewModel _commonMusicPageViewModel;
        private readonly CommonMusicPlayerViewModel _musicPlayerViewModel;
        private readonly MusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private readonly CommonPageViewModel _commonPageViewModel;
        private readonly CommonFormDownloadViewModel _formDownloadViewModel;
        private UserMusicPlayedHistory _lastMusicHistorySelected;
        private SearchMusicModel _musicLastSelected;
        private CancellationTokenSource _lastMusicHistorySelectedToken;
        private SelectModel _albumSelected;
        private string _musicCollectionFrameColorPrimary;
        private string _musicCollectionFrameColorSecondary;
        private int _albumMusicSavedFormSize;
        private bool _albumMusicSavedIsVisible;
        private bool _historyAllMusicsButtonIsVisible;
        public MusicPageViewModel(IDbLogic albumDbLogic, IPCLStorageDb pclStorageDb, IPCLUserAlbumLogic pclUserAlbumLogic, IPCLUserMusicLogic pclUserMusicLogic, ITocaTudoApi tocaTudoApi, CommonMusicPageViewModel commonMusicPageViewModel, CommonMusicPlayerViewModel musicPlayerViewModel, CommonPageViewModel commonPageViewModel, CommonFormDownloadViewModel formDownloadViewModel, MusicPlayedHistoryViewModel musicPlayedHistoryViewModel, YoutubeClient ytClient)
            : base(albumDbLogic, pclUserAlbumLogic, pclUserMusicLogic, commonPageViewModel, commonMusicPageViewModel, musicPlayerViewModel, tocaTudoApi, ytClient)
        {
            _tocaTudoApi = tocaTudoApi;
            _audioPlayer = DependencyService.Get<IAudio>();
            _commonMusicPageViewModel = commonMusicPageViewModel;
            _commonPageViewModel = commonPageViewModel;
            _musicPlayerViewModel = musicPlayerViewModel;
            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;
            _formDownloadViewModel = formDownloadViewModel;
            _albumDbLogic = albumDbLogic;
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;

            _albumMusicSavedFormSize = 0;
            _musicCollectionFrameColorPrimary = "#D7DFF6";
            _musicCollectionFrameColorSecondary = "#F5F7FA";

            SearchMusicCommand = new SearchMusicPlaylistCommand(this);
            DownloadMusicCommand = new SearchDownloadMusicCommand(this);

            _audioPlayer.PlayerReady -= AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering -= AudioPlayer_PlayerReadyBuffering;
            _audioPlayer.PlayerException -= AudioPlayer_PlayerException;

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;
            _audioPlayer.PlayerReadyBuffering += AudioPlayer_PlayerReadyBuffering;
            _audioPlayer.PlayerException += AudioPlayer_PlayerException;

            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            MusicPlayerConfig playerConfig = new MusicPlayerConfig()
            {
                TotalMusicsWillPlayBeforeMerchan = 2
            };

            _musicPlayerViewModel.SetMusicPlayerConfig(playerConfig);

            CrossMTAdmob.Current.OnInterstitialClosed -= AudioPlayer_OnInterstitialClosed;
            CrossMTAdmob.Current.OnInterstitialClosed += AudioPlayer_OnInterstitialClosed;
        }
        public string MusicSearchedName { get; set; }
        public SelectModel AlbumMusicSavedSelected { get; set; }
        public int AlbumMusicSavedFormSize
        {
            get { return _albumMusicSavedFormSize; }
            set
            {
                _albumMusicSavedFormSize = value;
                OnPropertyChanged(nameof(AlbumMusicSavedFormSize));
            }
        }
        public bool AlbumMusicSavedIsVisible
        {
            get { return _albumMusicSavedIsVisible; }
            set
            {
                _albumMusicSavedIsVisible = value;
                AlbumMusicSavedFormSize = _albumMusicSavedIsVisible ? 40 : 0;
            }
        }
        public string MusicCollectionFrameColorPrimary
        {
            get { return _musicCollectionFrameColorPrimary; }
            set
            {
                _musicCollectionFrameColorPrimary = value;
                OnPropertyChanged(nameof(MusicCollectionFrameColorPrimary));
            }
        }
        public string MusicCollectionFrameColorSecondary
        {
            get { return _musicCollectionFrameColorSecondary; }
            set
            {
                _musicCollectionFrameColorSecondary = value;
                OnPropertyChanged(nameof(MusicCollectionFrameColorSecondary));
            }
        }
        public CommonMusicPageViewModel CommonMusicPageViewModel
        {
            get { return _commonMusicPageViewModel; }
        }
        public CommonMusicPlayerViewModel MusicPlayerViewModel
        {
            get { return _musicPlayerViewModel; }
        }
        public CommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public MusicPlayedHistoryViewModel MusicPlayedHistoryViewModel
        {
            get { return _musicPlayedHistoryViewModel; }
        }
        public UserMusicPlayedHistory LastMusicHistorySelected
        {
            get { return _lastMusicHistorySelected; }
            set
            {
                _lastMusicHistorySelected = value;
            }
        }
        public IAsyncCommand SearchMusicCommand { get; set; }
        public ICommand DownloadMusicCommand { get; set; }
        public AsyncCommand<SearchMusicModel> SelectMusicCommand => SelectMusicEventCommand();
        public AsyncCommand<MusicModelBase> StartDownloadMusicCommand => StartDownloadMusicEventCommand();
        public AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormCommand => MusicHistoryFormEventCommand();
        public AsyncCommand<SelectModel> MusicAlbumSavedSelectedCommand => MusicAlbumSavedSelectedEventCommand();
        public Command<MusicModelBase> MusicPlayCommand => MusicPlayEventCommand();
        public async Task PlayMusic(ICommonMusicModel musicModel, CancellationToken cancellationToken)
        {
            if (musicModel == null)
                return;

            _pclUserMusicLogic.UnLoadDb();
            App.Services.GetRequiredService<SavedMusicPageViewModel>().UnloadViewModel();

            if (musicModel.SearchType == MusicSearchType.SearchMusicAlbumHistory)
            {
                int indice = MusicPlaylist.ToList()
                                          .FindIndex(music => string.Equals(music.VideoId, musicModel.VideoId));

                SearchMusicModel[] musicModelCollection = MusicPlaylist.Skip(indice)
                                                                       .ToArray();

                MusicPlayerConfig playerConfig = new MusicPlayerConfig()
                {
                    TotalMusicsWillPlayBeforeMerchan = App.GetTotalMusicsWillPlayBeforeMerchan()
                };

                await _musicPlayerViewModel.PlayMusic(musicModel, musicModelCollection, playerConfig, cancellationToken);
            }
            else
            {
                await _musicPlayerViewModel.PlayMusic(musicModel, cancellationToken);
            }
        }
        public async Task MusicPlaylistSearch()
        {
            Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcMusicPlaylist = async (tocaTudoApi) =>
            {
                return await tocaTudoApi.SearchMusicEndpoint(MusicSearchedName);
            };

            await LoadMusicAlbumPlaylistSelected();
            Task tskUserLocalHist = _musicPlayedHistoryViewModel.SaveLocalSearchHistory(MusicSearchedName);

            await SerializeMusicModel(funcMusicPlaylist, tskUserLocalHist, MusicSearchType.SearchMusic, Icon.Music)
                  .OnError(nameof(MusicPageViewModel), () =>
                  {
                      IsReady = true;
                      IsSearching = false;
                      RaiseDefaultAppErrorEvent();
                  })
                  .ConfigureAwait(false);

            App.EventTracker.SendEvent("MusicPlaylistSearch", new Dictionary<string, string>()
            {
                { "MusicSearched", MusicSearchedName },
            });
        }
        public async Task LoadMusicAlbumPlaylistSelected()
        {
            await CommonMusicPageViewModel.LoadMusicAlbumPlaylistSelected();
            AlbumMusicSavedIsVisible = CommonMusicPageViewModel.AlbumMusicSavedSelectCollection.Count > 0;
        }
        public async Task CheckMusicAlbumPlaylistSelected()
        {
            AlbumMusicSavedIsVisible = (await CommonMusicPageViewModel.GetMusicAlbumPlaylistSelected()).Count() > 0;
        }
        public async Task InsertOrUpdateMusicAlbumPlaylistSelected(string albumName, ICommonMusicModel music)
        {
            await CommonMusicPageViewModel.InsertOrUpdateMusicAlbumPlaylistSelected(albumName, music);

            await CheckMusicAlbumPlaylistSelected();
            await SerializeMusicModel();
            await App.Services.GetRequiredService<SavedMusicPageViewModel>().MusicPlaylistSearchFromDb();
        }
        public async Task UpdateMusicAlbumPlaylistSelected(int albumId, ICommonMusicModel musicModel)
        {
            await CommonMusicPageViewModel.UpdateMusicAlbumPlaylistSelected(albumId, musicModel);

            if (musicModel.SearchType == MusicSearchType.SearchMusicAlbumHistory)
            {
                if (albumId == AlbumMusicSavedSelected.Id)
                    return;

                UserMusicAlbumSelect userMusicAlbum = CommonMusicPageViewModel.MusicAlbumPlaylistSelected?.Where(mu => mu.Id == musicModel.MusicAlbumPopupModel?.AlbumMusicSavedSelected?.Id)
                                                                                                         ?.FirstOrDefault();
                UserMusicSelect userMusicToRemove = userMusicAlbum.MusicsModel.Where(um => string.Equals(um?.VideoId, musicModel.VideoId))
                                                                              .FirstOrDefault();
                if (userMusicToRemove != null)
                {
                    SearchMusicModel musicModelToRemove = MusicPlaylist.Where(mp => string.Equals(mp.VideoId, userMusicToRemove.VideoId))
                                                                       .FirstOrDefault();

                    MusicPlaylist.Remove(musicModelToRemove);
                }
            }
            else
            {
                await CheckMusicAlbumPlaylistSelected();
                await SerializeMusicModel();
            }

            await App.Services.GetRequiredService<SavedMusicPageViewModel>().MusicPlaylistSearchFromDb();
        }
        public async Task DeleteAlbum(int albumId)
        {
            UserMusicAlbumSelect[] lstMusicAlbumPlaylistSelected = await CommonMusicPageViewModel.GetMusicAlbumPlaylistSelected();
            AlbumMusicSavedIsVisible = lstMusicAlbumPlaylistSelected.Count() > 0;

            List<UserMusicSelect> userMusics = CommonMusicPageViewModel.MusicAlbumPlaylistSelected
                                                                       ?.Where(ma => ma.Id == albumId)
                                                                       ?.FirstOrDefault()
                                                                       ?.MusicsModel ?? new List<UserMusicSelect>() { };

            bool albumDeleted = await CommonMusicPageViewModel.DeleteAlbum(albumId);

            if (albumDeleted)
            {
                foreach (UserMusicSelect um in userMusics.ToArray())
                {
                    SearchMusicModel musicModel = MusicPlaylist.Where(m => string.Equals(m.VideoId, um?.VideoId))
                                                               .FirstOrDefault();
                    if (musicModel != null)
                    {
                        if (musicModel.SearchType == MusicSearchType.SearchMusicAlbumHistory)
                            MusicPlaylist.Remove(musicModel);
                        else
                            musicModel.MusicAlbumPopupModel.SetNormalMode();
                    }
                }
            }

            await CheckMusicAlbumPlaylistSelected();
            await SerializeMusicModel();
        }
        public async Task DeleteMusicFromAlbumPlaylist(ICommonMusicModel musicModel)
        {
            SearchMusicModel musicModelRemove = MusicPlaylist.Where(mp => string.Equals(mp.VideoId, musicModel.VideoId))
                                                             .FirstOrDefault();
            if (musicModelRemove != null)
            {
                SearchMusicModel music = MusicPlaylist.Where(m => string.Equals(m.VideoId, musicModel.VideoId))
                                                      .FirstOrDefault();

                if (musicModel.SearchType == MusicSearchType.SearchMusicAlbumHistory)
                    MusicPlaylist.Remove(music);
                else
                    musicModel.MusicAlbumPopupModel.SetNormalMode();

                await CommonMusicPageViewModel.DeleteMusicFromAlbumPlaylist(musicModel);

                await CheckMusicAlbumPlaylistSelected();
                await SerializeMusicModel();
            }
        }
        public void LoadViewModel()
        {
            _audioPlayer.PlayerAlbumMusicPlaylistChanged -= AudioPlayer_PlayerAlbumMusicPlaylistChanged;
            _audioPlayer.PlayerAlbumMusicPlaylistChanged += AudioPlayer_PlayerAlbumMusicPlaylistChanged;
        }
        public async Task UnloadViewModel()
        {
            if (_musicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize > 0)
            {
                _musicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 0;
                await MusicPlayedHistoryViewModel.LoadPlayedHistory();
            }

            MusicPlayerViewModel.StopBottomPlayer(force: true);
            MusicPlayerViewModel.HideBottomPlayer();

            _audioPlayer.PlayerAlbumMusicPlaylistChanged -= AudioPlayer_PlayerAlbumMusicPlaylistChanged;

            if (MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicAlbumHistory)
                MusicPlayerViewModel.ClearAlbumSource();

            ReadyMusic = null;

            if (MusicPlaylist?.Count == 0)
                return;

            MusicPlaylist.ToList()
                         .ForEach(music =>
                         {
                             music.IsActiveMusic = false;
                             music.IsSelected = false;
                             music.IsPlaying = false;
                         });
        }
        public async Task SerializeMusicModel(ICommonMusicModel musicModel = null)
        {
            IsReady = false;
            IsSearching = true;

            UserMusicAlbumSelect musicAlbumSelect = null;

            if (MusicPlayerViewModel.MusicPlayingNow == null)
            {
                MusicPlayerViewModel.StopBottomPlayer(force: true);
                MusicPlayerViewModel.HideBottomPlayer();
            }

            await CommonMusicPageViewModel.LoadMusicAlbumPlaylistSelected();

            await Task.Run(() =>
            {
                foreach (SearchMusicModel playlistItem in MusicPlaylist.ToArray())
                {
                    if (CommonMusicPageViewModel.MusicAlbumPlaylistSelected != null)
                    {
                        musicAlbumSelect = CommonMusicPageViewModel.MusicAlbumPlaylistSelected.Where(ma =>
                        {
                            return ma.MusicsModel.Exists(mm => string.Equals(mm?.VideoId, playlistItem.VideoId));
                        }).FirstOrDefault();

                        if (musicAlbumSelect != null)
                        {
                            if (playlistItem.MusicAlbumPopupModel.AlbumMusicSavedSelected == null)
                                playlistItem.MusicAlbumPopupModel.SetAlbumMode(musicAlbumSelect);
                            else if (playlistItem.MusicAlbumPopupModel.AlbumMusicSavedSelected != null && playlistItem.MusicAlbumPopupModel.AlbumMusicSavedSelected.Id != musicAlbumSelect.Id)
                                playlistItem.MusicAlbumPopupModel.SetAlbumMode(musicAlbumSelect);
                        }
                        else
                        {
                            playlistItem.MusicAlbumPopupModel.SetNormalMode();
                        }
                    }


                    if (musicModel != null)
                    {
                        if (string.Equals(musicModel.VideoId, playlistItem.VideoId))
                            playlistItem.IsSavedOnLocalDb = musicModel.IsSavedOnLocalDb;
                    }
                }

                IsReady = true;
                IsSearching = false;

                MusicCollectionFrameColorPrimary = "#FFFFFF";
                MusicCollectionFrameColorSecondary = "#FFFFFF";
            });
        }

        #region Private Methods
        private async Task SerializeMusicModel(Func<ITocaTudoApi, Task<ApiSearchMusicModel[]>> funcApiSearch, Task tskUserLocalHist, MusicSearchType searchType, string icon)
        {
            IsReady = false;
            IsSearching = true;
            PlaylistIsVisible = false;

            (string VideoId, string MusicImageUrl, bool MusicImageUrlStoraged)[] apiSearchUri = new (string, string, bool)[] { };
            UserMusicAlbumSelect musicAlbumSelect = null;

            if (MusicPlayerViewModel.MusicPlayingNow == null)
            {
                MusicPlayerViewModel.StopBottomPlayer(force: true);
                MusicPlayerViewModel.HideBottomPlayer();
            }

            if (string.IsNullOrWhiteSpace(MusicSearchedName))
            {
                IsReady = true;
                IsSearching = false;
                return;
            }

            await Task.Run(async () =>
            {
                MusicPlaylist.Clear();
                MusicPlayerViewModel.ClearAlbumSource();

                _albumSelected = null;

                await _pclUserMusicLogic.LoadDb();

                UserMusic[] userMusicsSaved = await _pclUserMusicLogic.GetMusics();
                Task<ApiSearchMusicModel[]> tskApiSearch = funcApiSearch(_tocaTudoApi);

                await Task.WhenAll(tskUserLocalHist, tskApiSearch);

                apiSearchUri = tskApiSearch.Result.Select(search => (search.VideoId, search.MusicImageUrl, search.MusicDataStoraged))
                                                  .ToArray();

                foreach (ApiSearchMusicModel playlistItem in tskApiSearch.Result.ToArray())
                {
                    if (CommonMusicPageViewModel.MusicAlbumPlaylistSelected != null)
                    {
                        musicAlbumSelect = CommonMusicPageViewModel.MusicAlbumPlaylistSelected.Where(ma =>
                        {
                            playlistItem.HasAlbum = ma.MusicsModel.Exists(mm => string.Equals(mm?.VideoId, playlistItem.VideoId));
                            return playlistItem.HasAlbum;
                        }).FirstOrDefault();
                    }

                    bool savedOnLocalDb = userMusicsSaved.Where(um => string.Equals(um.VideoId, playlistItem.VideoId))
                                                         .Count() > 0;

                    playlistItem.Icon = icon;

                    if (string.Equals(playlistItem.VideoId, ReadyMusic?.VideoId) && ReadyMusic?.SearchType != MusicSearchType.SearchMusicHistory)
                    {
                        MusicPlaylist.Add((SearchMusicModel)ReadyMusic);
                    }
                    if (musicAlbumSelect != null)
                    {
                        SearchMusicModel music = new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, musicAlbumSelect, searchType, savedOnLocalDb);
                        music.MusicImageUrl = playlistItem.MusicDataStoraged ? music.MusicImageUrl : null;
                        music.Download.DownloadComplete += Download_DownloadComplete;

                        PlaylistIsVisible = true;

                        MusicPlaylist.Add(music);
                    }
                    else
                    {
                        SearchMusicModel music = new SearchMusicModel(playlistItem, _formDownloadViewModel, _tocaTudoApi, YtClient, searchType, savedOnLocalDb);
                        music.MusicImageUrl = playlistItem.MusicDataStoraged ? music.MusicImageUrl : null;
                        music.Download.DownloadComplete += Download_DownloadComplete;

                        PlaylistIsVisible = true;

                        MusicPlaylist.Add(music);
                    }
                }

                IsReady = true;
                IsSearching = false;

                MusicCollectionFrameColorPrimary = "#FFFFFF";
                MusicCollectionFrameColorSecondary = "#FFFFFF";
            });

            await Task.Delay(1500).ContinueWith(tsk =>
            {
                if (tsk.IsCompleted)
                {
                    foreach (SearchMusicModel musicModel in MusicPlaylist.ToArray())
                    {
                        foreach (var musicImgModel in apiSearchUri)
                        {
                            if (string.Equals(musicModel.VideoId, musicImgModel.VideoId) && !musicImgModel.MusicImageUrlStoraged)
                            {
                                musicModel.MusicImageUrl = musicImgModel.MusicImageUrl;
                            }
                        }
                    }
                }
            }).ConfigureAwait(false);
        }
        private async Task SerializeMusicModel(SelectModel albumSelected, UserMusicSelect[] userMusics, MusicSearchType searchType, string icon)
        {
            IsReady = false;
            IsSearching = true;

            LoadViewModel();

            await Task.Run(async () =>
            {
                MusicPlaylist.Clear();

                await _pclUserMusicLogic.LoadDb();

                foreach (UserMusicSelect userMusic in userMusics.ToArray())
                {
                    if (userMusic == null)
                        continue;

                    if (string.Equals(userMusic.VideoId, ReadyMusic?.VideoId) && ReadyMusic?.SearchType != MusicSearchType.SearchMusicHistory)
                    {
                        MusicPlaylist.Add((SearchMusicModel)ReadyMusic);
                    }
                    else
                    {
                        bool existsOnLocalDb = _pclUserMusicLogic.ExistsOnLocalDb(userMusic.VideoId);
                        MusicPlaylist.Add(new SearchMusicModel(albumSelected, userMusic, icon, _formDownloadViewModel, _tocaTudoApi, YtClient, searchType, existsOnLocalDb));
                    }
                }

                if (MusicPlaylist.Count > 0)
                    LoadAlbumMusicPlaylist(albumSelected.Value, albumSelected.Value);

                PlaylistIsVisible = true;
                IsReady = true;
                IsSearching = false;
            });
        }
        private AsyncCommand<SearchMusicModel> SelectMusicEventCommand()
        {
            return new AsyncCommand<SearchMusicModel>(
                execute: async (musicModel) =>
                {
                    if (MusicPlayerViewModel.LastMusicPlayed != null)
                    {
                        MusicPlayerViewModel.LastMusicPlayed.ReloadMusicPlayingIcon();
                        MusicPlayerViewModel.LastMusicPlayed.IsSelected = false;
                        MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic = false;
                    }

                    MusicPlaylist.ToList()
                                  .ForEach(music =>
                    {
                        if (!string.Equals(music.VideoId, musicModel.VideoId))
                        {
                            music.IsActiveMusic = false;
                            music.IsSelected = false;
                            music.IsPlaying = false;
                        }
                    });

                    if (LastMusicHistorySelected != null)
                    {
                        MusicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 0;

                        LastMusicHistorySelected.UpdMusicSelectedColor();
                        LastMusicHistorySelected = null;
                        MusicPlayedHistoryViewModel.HistoryMusicPlayingNow = null;
                    }

                    if (musicModel.IsPlaying)
                    {
                        musicModel.IsActiveMusic = false;
                        musicModel.IsSelected = false;

                        _musicPlayerViewModel.Stop(musicModel);
                        await Task.Delay(10);
                    }

                    CancellationTokenSource cancellationToken = new CancellationTokenSource();

                    musicModel.IsActiveMusic = true;
                    musicModel.IsSelected = true;
                    _musicLastSelected = musicModel;

                    bool selectedAlbum = musicModel.SearchType == MusicSearchType.SearchMusicAlbumHistory;
                    if (selectedAlbum && _albumSelected != null)
                    {
                        LoadAlbumMusicPlaylist(_albumSelected.Value, _albumSelected.Value);
                    }

                    if (_albumSelected == null)
                        MusicPlayerViewModel.ClearAlbumSource();

                    RemoveAlbumPlayerIfPlaying();
                    await PlayMusic(musicModel, cancellationToken.Token)
                    .OnError(nameof(MusicPageViewModel), () =>
                    {
                        IsReady = true;
                        IsSearching = false;
                        RaiseAppErrorEvent(AppResource.AppDefaultError);
                    });
                }
            );
        }
        private AsyncCommand<MusicModelBase> StartDownloadMusicEventCommand()
        {
            return new AsyncCommand<MusicModelBase>(
                execute: async (musicModel) =>
                {
                    musicModel.SetDownload(_pclUserMusicLogic);
                    musicModel.SetDownloadingMode();

                    DownloadQueueStatus downloadQueueStatus = await musicModel.StartDownloadMusic(_musicPlayedHistoryViewModel, downloadComplete: () =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            RaiseActionShowInterstitial(() =>
                            {
                                _musicPlayerViewModel.Pause();
                            });
                        });
                    });

                    switch (downloadQueueStatus)
                    {
                        case DownloadQueueStatus.AchievedMaxQueue:
                            RaiseAppErrorEvent("Achieved queue max items.");
                            break;
                        case DownloadQueueStatus.MusicNotFound:
                            RaiseAppErrorEvent("The music to download was not found");
                            break;
                        case DownloadQueueStatus.ErrorHasOccurred:
                            RaiseDefaultAppErrorEvent();
                            break;
                    }
                }
            );
        }
        private Command<MusicModelBase> MusicPlayEventCommand()
        {
            return new Command<MusicModelBase>(
                execute: (music) =>
                {
                    RemoveAlbumPlayerIfPlaying();
                    _musicPlayerViewModel.PlayPauseMusic(music);
                }
            );
        }
        private AsyncCommand<UserMusicPlayedHistory> MusicHistoryFormEventCommand()
        {
            return new AsyncCommand<UserMusicPlayedHistory>(
                execute: async (userMusicHistory) =>
                {
                    MusicPlaylist.ToList().ForEach(music =>
                    {
                        music.IsActiveMusic = false;
                        music.IsSelected = false;
                    });

                    if (_lastMusicHistorySelected != null)
                    {
                        _lastMusicHistorySelected.UpdMusicSelectedColor();
                        _lastMusicHistorySelectedToken.Cancel();
                    }

                    if (_musicPlayedHistoryViewModel.ActiveMusicNow != null)
                    {
                        _musicPlayedHistoryViewModel.ActiveMusicNow.UpdMusicSelectedColor();
                        _musicPlayedHistoryViewModel.ActiveMusicNow = null;
                    }

                    await _pclUserMusicLogic.LoadDb();
                    App.Services.GetRequiredService<SavedMusicPageViewModel>().UnloadViewModel();

                    bool isSavedOnLocalDb = _pclUserMusicLogic.ExistsOnLocalDb(userMusicHistory.VideoId);
                    bool hasAlbumSaved = CommonMusicPageViewModel.HasAlbumSaved(userMusicHistory.VideoId);
                    UserMusicAlbumSelect userMusicAlbum = CommonMusicPageViewModel.GetAlbumSaved(userMusicHistory.VideoId)
                                                                                  .FirstOrDefault();

                    MusicPlayerViewModel.ClearAlbumSource();

                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                    _musicPlayedHistoryViewModel.HistoryMusicPlayingNow = new HistoryMusicModel(userMusicAlbum, _formDownloadViewModel, _tocaTudoApi, base.YtClient, hasAlbumSaved, isSavedOnLocalDb)
                    {
                        VideoId = userMusicHistory.VideoId,
                        MusicName = userMusicHistory.MusicName,
                        MusicTime = userMusicHistory.MusicTime,
                        MusicTimeTotalSeconds = userMusicHistory.MusicTimeTotalSeconds,
                        SearchType = MusicSearchType.SearchMusicHistory,
                        IsLoadded = false,
                        IsActiveMusic = true,
                        IsSelected = true,
                        ByteMusicImage = userMusicHistory.ByteImgMusic,
                        ImgMusic = ImageSource.FromStream(() => new MemoryStream(userMusicHistory.ByteImgMusic))
                    };

                    _musicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 40;

                    _lastMusicHistorySelected = userMusicHistory;
                    _lastMusicHistorySelectedToken = cancellationToken;

                    userMusicHistory.UpdMusicSelectedColor();
                    _pclUserMusicLogic.UnLoadDb();

                    if (!_musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsSavedOnLocalDb)
                    {
                        _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.Download.DownloadComplete -= Download_DownloadComplete;
                        _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.Download.DownloadComplete += Download_DownloadComplete;
                    }

                    RemoveAlbumPlayerIfPlaying();
                    await _musicPlayerViewModel.PlayMusic(_musicPlayedHistoryViewModel.HistoryMusicPlayingNow, cancellationToken.Token);

                    App.EventTracker.SendEvent("MusicHistoryPlayMusic", new Dictionary<string, string>()
                    {
                        { "MusicHistoryPlayMusic", userMusicHistory.MusicName },
                        { "VideoId", userMusicHistory.VideoId },
                    });
                }
            );
        }
        private AsyncCommand<SelectModel> MusicAlbumSavedSelectedEventCommand()
        {
            return new AsyncCommand<SelectModel>(
                execute: async (selected) =>
                {
                    if (selected == null)
                        return;

                    IsReady = false;
                    IsSearching = true;

                    UserMusicAlbumSelect userMusicAlbum = CommonMusicPageViewModel.MusicAlbumPlaylistSelected.Where(mu => mu.Id == selected.Id)
                                                                                                             .FirstOrDefault();
                    if (userMusicAlbum != null)
                    {
                        _albumSelected = selected;
                        UserMusicAlbumSelect[] musicsAlbum = CommonMusicPageViewModel.MusicAlbumPlaylistSelected.Where(mu => mu.Id != userMusicAlbum.Id)
                                                                                                                .ToArray();
                        await SerializeMusicModel(selected, userMusicAlbum.MusicsModel.ToArray(), MusicSearchType.SearchMusicAlbumHistory, Icon.Music);
                    }
                    else
                    {
                        IsReady = true;
                        IsSearching = false;
                        _albumSelected = null;
                    }
                }
            );
        }
        private void RemoveAlbumPlayerIfPlaying()
        {
            if (CommonPageViewModel.SelectedAlbum != null)
            {
                ((AlbumPlayerViewModel)CommonPageViewModel.SelectedAlbum.BindingContext).MusicPlayerViewModel.Stop();
                ((AlbumPlayerViewModel)CommonPageViewModel.SelectedAlbum.BindingContext).BottomPlayerViewModel.StopBottomPlayer(force: true);
                ((AlbumPlayerViewModel)CommonPageViewModel.SelectedAlbum.BindingContext).BottomPlayerViewModel.Init();

                CommonPageViewModel.AlbumPlayingGridSize = 0;
                CommonPageViewModel.SelectedAlbum = null;

                CommonMusicPlayerManager.StopAllAlbumBottomPlayers();
            }
        }
        private void AudioPlayer_PlayerReady(object sender, ICommonMusicModel music)
        {
            Task.Run(() =>
            {
                if (music != null)
                {
                    if (music.IsActiveMusic)
                    {
                        if (music.SearchType == MusicSearchType.SearchSavedMusic)
                            return;

                        ReadyMusic = music;


                        MusicPlaylist.Where(m => !string.Equals(m.VideoId, music.VideoId))
                                     .ForEach(m =>
                        {
                            m.IsPlaying = false;
                            m.IsSelected = false;
                            m.IsActiveMusic = false;
                            m.IsBufferingMusic = false;
                        });

                        if (music.ShowMerchandisingAlert && IsInternetAvaiable)
                        {
                            if (!AppHelper.MusicPlayerInterstitialIsLoadded)
                                RaiseActionShowInterstitial(() => _musicPlayerViewModel.HideStatusBarPlayerControls(),
                                    () => _audioPlayer.Play());
                        }
                        else
                            _audioPlayer.Play();

                        music.IsPlaying = true;
                        music.IconMusicDownloadVisible = !music.IsSavedOnLocalDb;

                        music.IsBufferingMusic = false;
                        music.IsLoadded = true;

                        if (_musicPlayedHistoryViewModel.HistoryMusicPlayingNow != null)
                            _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsLoadded = true;
                    }
                }
            }).OnError(nameof(MusicPageViewModel), () =>
            {
                RaiseDefaultAppErrorEvent();
            }).ConfigureAwait(false);
        }
        private void AudioPlayer_PlayerReadyBuffering(object sender, ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsBufferingMusic && _audioPlayer.IsPlaying)
                {
                    music.IsBufferingMusic = false;
                }
            }
        }
        private void Download_DownloadComplete(object sender, (bool, byte[], object) tpMusic)
        {
            if (tpMusic.Item2 == null || tpMusic.Item3 == null)
                return;

            if (MusicPlayedHistoryViewModel.HistoryMusicPlayingNow != null)
            {
                MusicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize -= MusicPlayedHistoryViewModel.HistoryMusicPlayingNow.FormDownloadSize;
            }

            RaiseActionShowInterstitial(() =>
            {
                _musicPlayerViewModel.Pause();
            });
        }
        private async void AudioPlayer_PlayerException(object sender, string e)
        {
            if (MusicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchMusic && MusicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchMusicHistory)
                return;

            IsReady = true;
            IsSearching = false;

            RaiseDefaultAppErrorEvent();

            await Task.Delay(1000);
            await UnloadViewModel();
        }
        #endregion
    }
}
