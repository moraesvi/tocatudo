using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicSavedPageViewModel : MusicAlbumPageBaseViewModel, IMusicSavedPageViewModel
    {
        private readonly IMusicSavedPlayedHistoryViewModel _musicSavedPlayedHistoryViewModel;
        private readonly ICommonMusicPageViewModel _commonMusicPageViewModel;
        private readonly ICommonPageViewModel _commonPageViewModel;
        private readonly ICommonMusicPlayerViewModel _musicPlayerViewModel;
        private ObservableCollection<SearchMusicModel> _savedMusicPlaylist;
        public MusicSavedPageViewModel(IDbLogic albumDbLogic, PCLUserMusicLogic pclUserMusicLogic, ICommonMusicPageViewModel commonMusicPageViewModel, IMusicSavedPlayedHistoryViewModel musicSavedPlayedHistoryViewModel, ICommonPageViewModel commonPageViewModel, ICommonMusicPlayerViewModel musicPlayerViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient)
            : base(albumDbLogic, pclUserMusicLogic, tocaTudoApi, ytClient)
        {
            _commonMusicPageViewModel = commonMusicPageViewModel;
            _musicSavedPlayedHistoryViewModel = musicSavedPlayedHistoryViewModel;
            _commonPageViewModel = commonPageViewModel;
            _musicPlayerViewModel = musicPlayerViewModel;
            _savedMusicPlaylist = new ObservableCollection<SearchMusicModel>();

            IAudio audioPlayer = DependencyService.Get<IAudio>();
            AppHelper.MusicPlayerInterstitialWasShowed = false;

            audioPlayer.PlayerReady += AudioPlayer_PlayerReady;

            MusicPlayerConfig playerConfig = new MusicPlayerConfig()
            {
                TotalMusicsWillPlay = 2
            };

            _musicPlayerViewModel.SetMusicPlayerConfig(playerConfig);
        }
        public ObservableCollection<SearchMusicModel> SavedMusicPlaylist
        {
            get { return _savedMusicPlaylist; }
            set
            {
                _savedMusicPlaylist = value;
                OnPropertyChanged(nameof(SavedMusicPlaylist));
            }
        }
        public ICommonMusicPageViewModel CommonMusicPageViewModel
        {
            get { return _commonMusicPageViewModel; }
        }
        public ICommonPageViewModel CommonPageViewModel
        {
            get { return _commonPageViewModel; }
        }
        public IMusicSavedPlayedHistoryViewModel MusicSavedPlayedHistoryViewModel
        {
            get { return _musicSavedPlayedHistoryViewModel; }
        }
        public AsyncCommand<SearchMusicModel> SelectMusicCommand => SelectMusicEventCommand();
        public async Task MusicPlaylistSearchFromDb()
        {
            await SerializeMusicModelFromDb(SavedMusicPlaylist);
        }
        public void ClearSavedMusicPlaylistLoaded()
        {
            SavedMusicPlaylist.Clear();
        }

        #region Private Methods
        public AsyncCommand<SearchMusicModel> SelectMusicEventCommand()
        {
            return new AsyncCommand<SearchMusicModel>(
                execute: async (musicModel) =>
                {
                    _savedMusicPlaylist.ToList().ForEach(music =>
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
                    else
                    {
                        musicModel.IsActiveMusic = true;
                        musicModel.IsSelected = true;

                        await _musicPlayerViewModel.PlaySavedMusic(musicModel);
                    }
                }
            );
        }
        private void AudioPlayer_PlayerReady(ICommonMusicModel music)
        {
            if (music != null)
            {
                if (music.IsActiveMusic)
                {
                    if (music.SearchType != MusicSearchType.SearchSavedMusic)
                        return;

                    if (music.IsLoadded)
                        return;

                    if (music.ShowMerchandisingAlert)
                    {
                        if (!AppHelper.MusicPlayerInterstitialWasShowed && IsInternetAvaiable)
                            RaiseActionShowInterstitial(() => { _musicPlayerViewModel.PlayPauseMusic(); });
                    }

                    if (!music.ShowMerchandisingAlert || !IsInternetAvaiable)
                        _musicPlayerViewModel.PlayPauseMusic();

                    music.IsPlaying = true;
                    music.IconMusicDownloadVisible = !music.IsSavedOnLocalDb;

                    music.IsBufferingMusic = false;
                    music.IsLoadded = true;
                }
            }

            base.MusicPlaying = music;
        }
        #endregion    
    }
}
