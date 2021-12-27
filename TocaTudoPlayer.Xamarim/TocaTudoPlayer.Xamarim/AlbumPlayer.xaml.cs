using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Interface;
using Unity;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlbumPlayer : ContentPage
    {
        private readonly IUnityContainer _unityContainer;
        private readonly IAlbumPlayerViewModel _plViewModel;
        private readonly ApiSearchMusicModel _playlist;
        private Action _audioPlayerPlay;
        private bool _formLoaded;
        private static bool _formIsVisible;
        private bool _showInterstitialOnAppearing;
        private bool _playMusicOnAperring;
        private string _currentVideoId;
        public AlbumPlayer(IUnityContainer unityContainer)
        {
            _formLoaded = false;

            InitializeComponent();

            _plViewModel = unityContainer.Resolve<IAlbumPlayerViewModel>();

            _formIsVisible = false;
            _showInterstitialOnAppearing = false;
            _playMusicOnAperring = false;

            _unityContainer = unityContainer;

            ucPlayerControl.BindingContext = unityContainer.Resolve<IMusicBottomAlbumPlayerViewModel>();
            ucPlayerControl.ViewModel = unityContainer.Resolve<IMusicBottomAlbumPlayerViewModel>();

            _plViewModel.PlayerLoaded = false;

            BindingContext = _plViewModel;

            InitViewModelSingleton();
        }
        public AlbumPlayer(IUnityContainer unityContainer, ApiSearchMusicModel playlist)
        {
            _formLoaded = false;

            InitializeComponent();

            _playlist = playlist;
            _plViewModel = unityContainer.Resolve<IAlbumPlayerViewModel>();
            _formIsVisible = false;
            _showInterstitialOnAppearing = false;
            _playMusicOnAperring = false;

            _unityContainer = unityContainer;

            ucPlayerControl.BindingContext = unityContainer.Resolve<IMusicBottomAlbumPlayerViewModel>();
            ucPlayerControl.ViewModel = unityContainer.Resolve<IMusicBottomAlbumPlayerViewModel>();

            _plViewModel.PlayerLoaded = false;
            _currentVideoId = playlist.VideoId;

            BindingContext = _plViewModel;

            InitViewModelSingleton();
        }
        public string CurrentVideoId => _currentVideoId;
        protected override async void OnAppearing()
        {
            _formIsVisible = true;

            if (!_formLoaded || !_plViewModel.PlayerLoaded || _plViewModel.Album.Playlist?.Count == 0)
            {
                bool localStorageEnabled = await CheckAndRequestLocalStoragePermission();

                _plViewModel.Album.Playlist?.Clear();
                _plViewModel.ShowHideDownloadMusicOptions = false;
                _plViewModel.BottomPlayerViewModel.BottomPlayerControlIsVisible = false;
                _plViewModel.DbAccessEnabled(localStorageEnabled);

                await _plViewModel.GetAlbum((AlbumParseType)Enum.Parse(typeof(AlbumParseType), _playlist.TipoParse[0].ToString()), _playlist.VideoId);
                tbiShowDownload.IsEnabled = _plViewModel.Download.IsDownloadEventEnabled;

                _formLoaded = true;
            }

            _plViewModel.BottomPlayerViewModel.ActiveBottomPlayer(_unityContainer.Resolve<ICommonMusicPlayerViewModel>());

            if (_formIsVisible && _showInterstitialOnAppearing)
            {
                CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                CrossMTAdmob.Current.ShowInterstitial();

                _plViewModel.PlayerLoaded = true;
                _plViewModel.IsSearching = false;
                _plViewModel.IsActionsEnabled = true;

                _showInterstitialOnAppearing = false;

                AppHelper.MusicPlayerInterstitialWasShowed = true;
            }

            if(_playMusicOnAperring) 
            {
                _plViewModel.MusicPlayerViewModel.PlayPauseMusic();
                _playMusicOnAperring = false;
            }

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _formIsVisible = false;

            _plViewModel.BottomPlayerViewModel.StopBottomPlayer();

            base.OnDisappearing();
        }
        private void ViewModel_ShowInterstitial(Action audioPlayerPlay)
        {
            if (_formIsVisible)
            {
                CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                CrossMTAdmob.Current.ShowInterstitial();

                _plViewModel.PlayerLoaded = true;
                _plViewModel.IsSearching = false;
                _plViewModel.IsActionsEnabled = true;

                AppHelper.MusicPlayerInterstitialWasShowed = true;
            }
            else
            {
                _audioPlayerPlay = audioPlayerPlay;
                _showInterstitialOnAppearing = true;
            }
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            TappedEventArgs tapped = (TappedEventArgs)e;
            _plViewModel.PlayCommand.Execute(tapped.Parameter);
        }
        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            ImageButton tapped = (ImageButton)sender;
            _plViewModel.DownloadMusicOptionsCommand.Execute(tapped.CommandParameter);
        }
        private void FrmDownloadMusic_Tapped(object sender, EventArgs e)
        {
            TappedEventArgs tapped = (TappedEventArgs)e;
            AlbumPlayerViewModel context = ((AlbumPlayerViewModel)BindingContext);

            context.StartDownloadMusicCommand.Execute(tapped.Parameter);
            tbiShowDownload.IsEnabled = context.Download.IsDownloadEventEnabled;
        }
        private async Task<bool> CheckAndRequestLocalStoragePermission()
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            bool enabled = (statusRead == PermissionStatus.Granted) && (statusWrite == PermissionStatus.Granted);
            tbiShowDownload.IsEnabled = enabled;

            return enabled;
        }        
        private void BbiShowDownload_Clicked(object sender, EventArgs e)
        {
            if (!_plViewModel.ShowHideDownloadMusicOptions)
            {
                CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                CrossMTAdmob.Current.ShowInterstitial();
            }

            _plViewModel.DownloadMusicOptionsCommand.Execute(null);
        }
        private void MusicPlayerViewModel_MusicPlayedHistoricIsSaved(ICommonMusicModel music)
        {
            if (music.IsActiveMusic)
                cvAlbumPlayedHistory.ScrollTo(0, -1, ScrollToPosition.Start);
        }
        private void MusicPlayerViewModel_PlayerLosedAudioFocus()
        {
            _playMusicOnAperring = true;
        }
        private void ViewModel_AppErrorEvent(int level, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Ops!", msg, "ok");
                await Navigation.PopModalAsync();
            });
        }
        private void InitViewModelSingleton() 
        {
            if (!_plViewModel.ViewModelLoadded)
            {
                _plViewModel.AppErrorEvent += ViewModel_AppErrorEvent;
                _plViewModel.ShowInterstitial += ViewModel_ShowInterstitial;
                _plViewModel.MusicPlayerViewModel.MusicPlayedHistoricIsSaved += MusicPlayerViewModel_MusicPlayedHistoricIsSaved;
                _plViewModel.MusicPlayerViewModel.PlayerLosedAudioFocus += MusicPlayerViewModel_PlayerLosedAudioFocus;

                _plViewModel.ViewModelLoadded = true;
            }
        }
    }
}