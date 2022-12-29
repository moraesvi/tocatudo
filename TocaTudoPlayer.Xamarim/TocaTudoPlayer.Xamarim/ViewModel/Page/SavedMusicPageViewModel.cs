using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class SavedMusicPageViewModel : MusicAlbumPageBaseViewModel
    {
        private const string USER_MUSIC_ALBUM_SELECT_KEY = "mas_select.json";

        private readonly CommonMusicPageViewModel _commonMusicPageViewModel;
        private readonly CommonPageViewModel _commonPageViewModel;
        private readonly CommonMusicPlayerViewModel _musicPlayerViewModel;
        private readonly IAudio _audioPlayer;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private List<UserMusicAlbumSelect> _musicAlbumPlaylistSelected;
        private ObservableCollection<SelectModel> _albumMusicSavedSelectCollection;
        private UserMusicAlbumSelect _userMusicAlbumSelected;
        private bool _savedMusicAlbumIsVisible;
        private bool _allMusicsButtonIsVisible;
        public SavedMusicPageViewModel(IDbLogic albumDbLogic, IPCLUserAlbumLogic pclUserAlbumLogic, IPCLUserMusicLogic pclUserMusicLogic, IPCLStorageDb pclStorageDb, CommonMusicPageViewModel commonMusicPageViewModel, MusicSavedPlayedHistoryViewModel musicSavedPlayedHistoryViewModel, CommonPageViewModel commonPageViewModel, CommonMusicPlayerViewModel musicPlayerViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
            : base(albumDbLogic, pclUserAlbumLogic, pclUserMusicLogic, commonPageViewModel, commonMusicPageViewModel, musicPlayerViewModel, tocaTudoApi, ytClient)
        {
            _audioPlayer = DependencyService.Get<IAudio>();
            _commonMusicPageViewModel = commonMusicPageViewModel;
            _commonPageViewModel = commonPageViewModel;
            _musicPlayerViewModel = musicPlayerViewModel;
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;
            _albumMusicSavedSelectCollection = new ObservableCollection<SelectModel>();

            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            _savedMusicAlbumIsVisible = false;
            _allMusicsButtonIsVisible = false;

            _audioPlayer.PlayerReady += AudioPlayer_PlayerReady;

            _audioPlayer.PlayerException -= AudioPlayer_PlayerException;
            _audioPlayer.PlayerException += AudioPlayer_PlayerException;

            MusicPlayerConfig playerConfig = new MusicPlayerConfig()
            {
                TotalMusicsWillPlayBeforeMerchan = 2
            };

            _musicPlayerViewModel.SetMusicPlayerConfig(playerConfig);

            CrossMTAdmob.Current.OnInterstitialClosed -= AudioPlayer_OnInterstitialClosed;
            CrossMTAdmob.Current.OnInterstitialClosed += AudioPlayer_OnInterstitialClosed;
        }
        public ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection
        {
            get { return _albumMusicSavedSelectCollection; }
            set
            {
                _albumMusicSavedSelectCollection = value;
                OnPropertyChanged(nameof(AlbumMusicSavedSelectCollection));
            }
        }
        public List<UserMusicAlbumSelect> MusicAlbumPlaylistSelected => _musicAlbumPlaylistSelected;
        public CommonMusicPageViewModel CommonMusicPageViewModel => _commonMusicPageViewModel;
        public CommonPageViewModel CommonPageViewModel => _commonPageViewModel;
        public CommonMusicPlayerViewModel MusicPlayerViewModel => _musicPlayerViewModel;
        public bool SavedMusicAlbumIsVisible
        {
            get { return _savedMusicAlbumIsVisible; }
            set
            {
                _savedMusicAlbumIsVisible = value;
                OnPropertyChanged(nameof(SavedMusicAlbumIsVisible));
            }
        }
        public bool AllMusicsButtonIsVisible
        {
            get { return _allMusicsButtonIsVisible; }
            set
            {
                _allMusicsButtonIsVisible = value;
                OnPropertyChanged(nameof(AllMusicsButtonIsVisible));
            }
        }
        public AsyncCommand<SearchMusicModel> SelectMusicCommand => SelectMusicEventCommand();
        public AsyncCommand<SelectModel> MusicAlbumSavedSelectedCommand => MusicAlbumSavedSelectedEventCommand();
        public AsyncCommand AllMusicMusicCommand => AllMusicMusicEventCommand();
        public async Task MusicPlaylistSearchFromDb()
        {
            await SerializeMusicModelFromDb(MusicPlaylist);
            await LoadMusicAlbumPlaylistSelected();

            LoadAlbumMusicSavedSelect();
        }
        public async Task LoadMusicAlbumPlaylistSelected()
        {
            _musicAlbumPlaylistSelected = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY) ?? new List<UserMusicAlbumSelect>();

            int countEqualsAlbumId = _musicAlbumPlaylistSelected.GroupBy(ma => ma.Id)
                                                                .Where(ma => ma.Count() > 1)
                                                                .Count();
            if (countEqualsAlbumId >= 1) //FIXING - In production the album id has been equals
            {
                _musicAlbumPlaylistSelected = _musicAlbumPlaylistSelected.Select((ma, index) =>
                                                                         {
                                                                             ma.Id = (short)(index + 1);
                                                                             return ma;
                                                                         }).ToList();
            }

            await _pclUserMusicLogic.LoadDb();

            string[] userMusicsSaved = (await _pclUserMusicLogic.GetMusics())
                                                                ?.Select(music => music.VideoId)
                                                                ?.ToArray() ?? new string[] { };

            _musicAlbumPlaylistSelected = _musicAlbumPlaylistSelected?.Where(music => music?.MusicsModel?.Exists(m => userMusicsSaved.Contains(m?.VideoId)) ?? false)
                                                                     ?.ToList() ?? new List<UserMusicAlbumSelect>() { };

            await Task.Run(async () =>
            {
                UserMusicAlbumSelect[] albumMusicsUpdate = _musicAlbumPlaylistSelected?.Where(music => music?.MusicsModel?.Exists(m => userMusicsSaved.Contains(m?.VideoId) && (string.IsNullOrEmpty(m?.MusicTime) || m?.MusicImage == null)) ?? false)
                                                                                      ?.ToArray() ?? new UserMusicAlbumSelect[] { };
                if (albumMusicsUpdate.Count() > 0)
                {
                    foreach (UserMusicAlbumSelect albumUpdate in albumMusicsUpdate)
                    {
                        if (string.IsNullOrEmpty(albumUpdate.AlbumName))
                        {
                            _musicAlbumPlaylistSelected.ForEach(music =>
                            {
                                music.MusicsModel.RemoveAll(m => userMusicsSaved.Contains(m.VideoId));
                            });

                            continue;
                        }

                        foreach (UserMusicSelect musicUpdate in albumUpdate.MusicsModel)
                        {
                            var music = await _pclUserMusicLogic.GetMusicById(musicUpdate.VideoId);
                            if (music.Item1 == null)
                                continue;

                            await _commonMusicPageViewModel.InsertOrUpdateMusicAlbumPlaylistSelected(albumUpdate.AlbumName, music.Item1, _musicAlbumPlaylistSelected);
                        }
                    }

                    _musicAlbumPlaylistSelected = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY) ?? new List<UserMusicAlbumSelect>();
                }
            });

            if (userMusicsSaved.Count() == 0 || _musicAlbumPlaylistSelected.Count() == 0)
            {
                SavedMusicAlbumIsVisible = false;
                AllMusicsButtonIsVisible = false;
                return;
            }

            _musicAlbumPlaylistSelected = _musicAlbumPlaylistSelected?.Where(music => music?.MusicsModel?.Exists(m => userMusicsSaved.Contains(m?.VideoId)) ?? false)
                                                                     ?.ToList() ?? new List<UserMusicAlbumSelect>() { };

            _musicAlbumPlaylistSelected.ForEach(music =>
                                       {
                                           music.MusicsModel.RemoveAll(m => !userMusicsSaved.Contains(m?.VideoId));
                                       });

            LoadAlbumMusicSavedSelect();

            _pclUserMusicLogic.UnLoadDb();
        }
        public async Task DeleteMusicFromAlbumPlaylist(ICommonMusicModel musicModel)
        {
            SearchMusicModel musicModelRemove = MusicPlaylist.Where(mp => string.Equals(mp.VideoId, musicModel.VideoId))
                                                             .FirstOrDefault();
            if (musicModelRemove != null)
            {
                SearchMusicModel music = MusicPlaylist.Where(m => string.Equals(m.VideoId, musicModel.VideoId))
                                                      .FirstOrDefault();

                await CommonMusicPageViewModel.DeleteMusicFromAlbumPlaylist(musicModel);

                await App.Services.GetRequiredService<MusicPageViewModel>().CheckMusicAlbumPlaylistSelected();
                await App.Services.GetRequiredService<MusicPageViewModel>().SerializeMusicModel();
            }
        }
        public async Task DeleteDownloadedMusic(ICommonMusicModel musicModel)
        {
            await CommonMusicPageViewModel.DeleteDownloadedMusic(musicModel);
            SearchMusicModel searchMusicModel = MusicPlaylist.Where(mu => string.Equals(mu.VideoId, musicModel.VideoId))
                                                             .FirstOrDefault();
            if (searchMusicModel != null)
                MusicPlaylist.Remove(searchMusicModel);

            await App.Services.GetRequiredService<MusicPageViewModel>().CheckMusicAlbumPlaylistSelected();
            await App.Services.GetRequiredService<MusicPageViewModel>().SerializeMusicModel(musicModel);

            if (MusicPlaylist.Count == 0)
                await MusicPlaylistSearchFromDb();
        }
        public void LoadViewModel()
        {
            _audioPlayer.PlayerAlbumMusicPlaylistChanged -= AudioPlayer_PlayerAlbumMusicPlaylistChanged;
            _audioPlayer.PlayerAlbumMusicPlaylistChanged += AudioPlayer_PlayerAlbumMusicPlaylistChanged;
        }
        public void UnloadViewModel()
        {
            MusicPlayerViewModel.StopBottomPlayer(force: true);
            MusicPlayerViewModel.HideBottomPlayer();

            _audioPlayer.PlayerAlbumMusicPlaylistChanged -= AudioPlayer_PlayerAlbumMusicPlaylistChanged;

            if (MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchSavedMusic)
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

        #region Private Methods
        private void LoadAlbumMusicSavedSelect()
        {
            if (_musicAlbumPlaylistSelected == null)
                return;

            _userMusicAlbumSelected = null;

            Device.BeginInvokeOnMainThread(() =>
            {
                AlbumMusicSavedSelectCollection.Clear();

                foreach (UserMusicAlbumSelect musicAlbum in _musicAlbumPlaylistSelected)
                {
                    if (musicAlbum.MusicsModel.Count > 0)
                    {
                        AlbumMusicSavedSelectCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));
                    }
                }

                SavedMusicAlbumIsVisible = AlbumMusicSavedSelectCollection.Count > 0;
                AllMusicsButtonIsVisible = false;
            });
        }
        private AsyncCommand<SearchMusicModel> SelectMusicEventCommand()
        {
            return new AsyncCommand<SearchMusicModel>(
                execute: async (musicModel) =>
                {
                    MusicPlaylist.ToList().ForEach(music =>
                    {
                        if (!string.Equals(music.VideoId, musicModel.VideoId))
                        {
                            music.IsActiveMusic = false;
                            music.IsSelected = false;
                        }
                    });

                    if (musicModel.IsPlaying)
                    {
                        musicModel.IsActiveMusic = false;
                        musicModel.IsSelected = false;

                        _musicPlayerViewModel.Stop(musicModel);
                    }

                    musicModel.IsActiveMusic = true;
                    musicModel.IsSelected = true;

                    if (CommonPageViewModel.SelectedAlbum != null)
                    {
                        ((AlbumPlayerViewModel)CommonPageViewModel.SelectedAlbum.BindingContext).MusicPlayerViewModel.Stop();
                        ((AlbumPlayerViewModel)CommonPageViewModel.SelectedAlbum.BindingContext).BottomPlayerViewModel.StopBottomPlayer(force: true);

                        CommonPageViewModel.AlbumPlayingGridSize = 0;
                        CommonPageViewModel.SelectedAlbum = null;
                    }

                    LoadViewModel();

                    if (_userMusicAlbumSelected != null)
                        LoadAlbumMusicPlaylist(_userMusicAlbumSelected.AlbumName, _userMusicAlbumSelected.AlbumName);
                    else
                        LoadAlbumMusicPlaylist("tocatudo", "TocaTudo");

                    await PlayMusic(musicModel)
                    .OnError(nameof(SavedMusicPageViewModel), () =>
                    {
                        IsReady = true;
                        IsSearching = false;
                        RaiseAppErrorEvent(AppResource.AppDefaultError);
                    });

                    App.EventTracker.SendEvent("SavedMusicPlayMusic", new Dictionary<string, string>()
                    {
                        { "MusicName", musicModel.MusicName },
                        { "VideoId", musicModel.VideoId },
                    });
                }
            );
        }
        private void AudioPlayer_PlayerReady(object sender, ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsActiveMusic)
                {
                    if (music.SearchType != MusicSearchType.SearchSavedMusic)
                        return;

                    ReadyMusic = music;

                    if (music.ShowMerchandisingAlert)
                    {
                        if (!AppHelper.MusicPlayerInterstitialIsLoadded && IsInternetAvaiable)
                            RaiseActionShowInterstitial(() => _musicPlayerViewModel.HideStatusBarPlayerControls(), 
                                () => _musicPlayerViewModel.PlayPauseMusic());
                    }

                    if (!music.ShowMerchandisingAlert || !IsInternetAvaiable)
                        _musicPlayerViewModel.PlayMusic();

                    music.IsPlaying = true;
                    music.IconMusicDownloadVisible = !music.IsSavedOnLocalDb;

                    music.IsBufferingMusic = false;
                    music.IsLoadded = true;
                }
            }
        }
        private void SerializeMusicModel(UserMusicAlbumSelect userMusicAlbum)
        {
            IsSearching = true;

            MusicPlaylist.Clear();

            foreach (UserMusicSelect userMusic in userMusicAlbum.MusicsModel.ToArray())
            {
                if (string.Equals(userMusic.VideoId, ReadyMusic?.VideoId))
                    MusicPlaylist.Add((SearchMusicModel)ReadyMusic);
                else 
                    MusicPlaylist.Add(new SearchMusicModel(userMusicAlbum, userMusic, Icon.ArrowDown, MusicSearchType.SearchSavedMusic, true));
            }

            IsSearching = false;
        }
        private AsyncCommand<SelectModel> MusicAlbumSavedSelectedEventCommand()
        {
            return new AsyncCommand<SelectModel>(
                execute: async (selected) =>
                {
                    await Task.Run(async () =>
                    {
                        if (selected == null)
                            return;

                        IsSearching = true;

                        _userMusicAlbumSelected = MusicAlbumPlaylistSelected.Where(mu => mu.Id == selected.Id)
                                                                            .FirstOrDefault();

                        if (_userMusicAlbumSelected != null)
                        {
                            AllMusicsButtonIsVisible = true;
                            SerializeMusicModel(_userMusicAlbumSelected);
                        }
                        else
                            IsSearching = false;

                    });
                }
            );
        }
        private AsyncCommand AllMusicMusicEventCommand()
        {
            return new AsyncCommand(
                execute: async () =>
                {
                    await MusicPlaylistSearchFromDb();
                    LoadAlbumMusicSavedSelect();
                }
            );
        }
        private async Task PlayMusic(ICommonMusicModel musicModel)
        {
            if (musicModel == null)
                return;

            _pclUserMusicLogic.UnLoadDb();
            await App.Services.GetRequiredService<MusicPageViewModel>()
                              .UnloadViewModel()
                              .ConfigureAwait(false);

            int indice = MusicPlaylist.ToList()
                                      .FindIndex(music => string.Equals(music.VideoId, musicModel.VideoId));

            SearchMusicModel[] musicModelCollection = MusicPlaylist.Skip(indice)
                                                                   .ToArray();

            MusicPlayerConfig playerConfig = new MusicPlayerConfig()
            {
                TotalMusicsWillPlayBeforeMerchan = App.GetTotalMusicsWillPlayBeforeMerchan()
            };

            ReadyMusic = musicModel;

            await _musicPlayerViewModel.PlaySavedMusic(musicModel, musicModelCollection, playerConfig)
                                       .OnError(nameof(SavedMusicPageViewModel), () =>
                                       {
                                           musicModel.IsLoadded = false;
                                           musicModel.IsActiveMusic = false;

                                           RaiseDefaultAppErrorEvent();
                                       });
        }
        private async void AudioPlayer_PlayerException(object sender, string e)
        {
            if (MusicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchSavedMusic)
                return;

            IsReady = true;
            IsSearching = false;

            RaiseDefaultAppErrorEvent();

            await Task.Delay(1000);
            UnloadViewModel();
        }
        #endregion    
    }
}
