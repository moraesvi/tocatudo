using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlbumPlayer : ContentPage
    {
        private readonly AlbumPlayerViewModel _vm;
        private bool _formLoaded;
        private static bool _formIsVisible;
        public AlbumPlayer()
        {
            InitializeComponent();

            Title = $"{AppResource.AppName} - {AppHelper.ToTitleCase(AppResource.MusicAlbumButton)}";

            _vm = App.Services.GetRequiredService<AlbumPlayerViewModel>();

            MusicBottomAlbumPlayerViewModel bottomPlayer = App.Services.GetRequiredService<MusicBottomAlbumPlayerViewModel>();

            ucPlayerControl.BindingContext = bottomPlayer;
            ucPlayerControl.ViewModel = bottomPlayer;

            BindingContext = _vm;

            grdImgAlbum.TranslateTo(220, 0);
            grdImgAlbum.FadeTo(0, 0);
            lblAlbumName.FadeTo(0, 0);

            InitCallbackEvents();

            _vm.Album.Playlist?.Clear();

            CrossMTAdmob.Current.LoadInterstitial(App.AppConfigAdMob.AdsAlbumIntersticial);

            myAds.AdsId = App.AppConfigAdMob.AdsAlbumPlayerBanner;
            myAds.AdsLoaded += (sender, obj) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ((Grid)stlBottom.Children[0]).RowDefinitions[0].Height = GridLength.Auto;
                });
            };
        }
        public static bool FormIsVisible => _formIsVisible;
        public ApiSearchMusicModel PlaylistItem { get; set; }
        public static async Task ShowInterstitial(Action intertistialNotLoaded) => await CustomCrossMTAdmob.ShowInterstitial(intertistialNotLoaded, () => { });
        public static async Task LoadAndShowInterstitial(Action intertistialNotLoaded) => await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsAlbumIntersticial, intertistialNotLoaded, () => { });
        public async Task Load()
        {
            await grdImgAlbum.TranslateTo(70, 0);
            await grdImgAlbum.FadeTo(0, 0);
            await lblAlbumName.FadeTo(0, 0);

            Device.BeginInvokeOnMainThread(() =>
            {
                ((Grid)stlBottom.Children[0]).RowDefinitions[0].Height = 60;
            });
        }
        protected override async void OnAppearing()
        {
            _formIsVisible = true;

            if (_formLoaded)
            {
                await Task.Delay(50);
                await Task.WhenAll(
                    grdImgAlbum.FadeTo(1, 130),
                    grdImgAlbum.TranslateTo(0, 0)
                                 .ContinueWith(async (task) =>
                {
                    if (task.IsCompleted)
                    {
                        await Task.Delay(100);
                        await lblAlbumName.FadeTo(1);
                    }
                }));
            }

            if (!_formLoaded || !_vm.PlayerLoaded || _vm.Album.Playlist?.Count == 0)
            {
                bool localStorageEnabled = await CheckAndRequestLocalStoragePermission();

                _vm.Album.Playlist?.Clear();
                _vm.ShowHideDownloadMusicOptions = false;
                _vm.BottomPlayerViewModel.BottomPlayerControlIsVisible = false;
                _vm.DbAccessEnabled(localStorageEnabled);

                AlbumParseTypeExtensions.TryParse(PlaylistItem.TipoParse[0].ToString(), out AlbumParseType album);

                await _vm.GetAlbum(album, PlaylistItem.VideoId)
                         .OnError(Title, () => ShowErrorAlertPopModal(AppResource.AppDefaultError));

                App.EventTracker.SendScreenView(Title, nameof(AlbumPlayer));
            }

            if (_vm.IsInternetAvaiableGridSize > 0)
            {
                _vm.CheckInternetConnection();
            }

            if (_vm.CommonPageViewModel.SelectedAlbum == null)
            {
                if (Navigation.NavigationStack.Count > 0)
                {
                    await Task.Delay(500);
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await App.Services.GetRequiredService<MusicPageViewModel>().UnloadViewModel();
                App.Services.GetRequiredService<SavedMusicPageViewModel>().UnloadViewModel();
            }

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _formIsVisible = false;
            base.OnDisappearing();
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            TappedEventArgs tapped = (TappedEventArgs)e;
            Grid gridContent = (Grid)sender;

            PlaylistItem item = (PlaylistItem)tapped.Parameter;
            if (!item.IsPlaying)
            {
                Label musicName = (Label)gridContent.Children[1];
                Label musicTime = (Label)gridContent.Children[2];

                musicName.SizeUpSizeDownAnimation("MusicNameAnimation");
                musicTime.SizeUpSizeDownAnimation("MusicTimeAnimation");
            }

            _vm.PlayCommand.Execute(tapped.Parameter);
        }
        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            ImageButton tapped = (ImageButton)sender;
            _vm.DownloadMusicOptionsCommand.Execute(tapped.CommandParameter);
        }
        private void FrmDownloadMusic_Tapped(object sender, EventArgs e)
        {
            TappedEventArgs tapped = (TappedEventArgs)e;
            AlbumPlayerViewModel context = (AlbumPlayerViewModel)BindingContext;

            context.StartDownloadMusicCommand.Execute(tapped.Parameter);
        }
        private async Task<bool> CheckAndRequestLocalStoragePermission()
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            bool enabled = (statusRead == PermissionStatus.Granted) && (statusWrite == PermissionStatus.Granted);

            return enabled;
        }
        private void MusicItemPlayingNow()
        {
            //if (_vm.BottomPlayerViewModel.AlbumHasMusicSelected && !AppHelper.HasInterstitialToShow)
            //{
            //    if (_vm.BottomPlayerViewModel.PlaylistItemNow != null && _playMusicOnAppering)
            //    {
            //        _vm.MusicPlayerViewModel.PlayMusic();
            //    }
            //}
        }
        private void MusicItemPlayingNow(object sender, EventArgs e)
        {
            MusicItemPlayingNow();
        }
        private async void BbiShowDownload_Clicked(object sender, EventArgs e)
        {
            if (!_vm.ShowHideDownloadMusicOptions)
            {
                await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsAlbumPlayerIntersticial, () => { }, () => { });
            }

            _vm.DownloadMusicOptionsCommand.Execute(null);
        }
        private void MusicPlayerViewModel_MusicPlayedHistoricIsSaved(object sender, ICommonMusicModel music)
        {

        }
        private void MusicPlayerViewModel_PlayerLosedAudioFocus(object sender, EventArgs e)
        {
        }
        private void ViewModel_AppErrorEvent(object sender, string msg)
        {
            ShowErrorAlertPopModal(msg);
        }
        private void InitCallbackEvents()
        {
            _vm.PlayerReady -= MusicPlayerViewModel_PlayerReady;
            _vm.PlayerReady += MusicPlayerViewModel_PlayerReady;
            _vm.AppErrorEvent -= ViewModel_AppErrorEvent;
            _vm.AppErrorEvent += ViewModel_AppErrorEvent;
            //_vm.ShowInterstitial -= ViewModel_ShowInterstitial;
            //_vm.ShowInterstitial += ViewModel_ShowInterstitial;
            _vm.MusicPlayerViewModel.MusicPlayedHistoricIsSaved -= MusicPlayerViewModel_MusicPlayedHistoricIsSaved;
            _vm.MusicPlayerViewModel.MusicPlayedHistoricIsSaved += MusicPlayerViewModel_MusicPlayedHistoricIsSaved;
            _vm.MusicPlayerViewModel.PlayerLosedAudioFocus -= MusicPlayerViewModel_PlayerLosedAudioFocus;
            _vm.MusicPlayerViewModel.PlayerLosedAudioFocus += MusicPlayerViewModel_PlayerLosedAudioFocus;

            //ucPlayerControl.ViewModel.MusicShowInterstitial -= ViewModel_ShowInterstitial;
            //ucPlayerControl.ViewModel.MusicShowInterstitial += ViewModel_ShowInterstitial;

            CrossMTAdmob.Current.OnInterstitialClosed -= MusicItemPlayingNow;
            CrossMTAdmob.Current.OnInterstitialClosed += MusicItemPlayingNow;
        }
        private async void MusicPlayerViewModel_PlayerReady(object sender, EventArgs e)
        {
            await Task.Delay(1500);
            await Task.WhenAll(
                grdImgAlbum.FadeTo(1, 130),
                grdImgAlbum.TranslateTo(0, 0)
                           .ContinueWith(async (task) =>
                           {
                               if (task.IsCompleted)
                               {
                                   await Task.Delay(100);
                                   await lblAlbumName.FadeTo(1);
                               }
                           }));

            _formLoaded = true;
        }
        private void ShowErrorAlertPopModal(string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                _vm.IsSearching = false;

                await DisplayAlert("Ops!", msg, "ok");
                await Navigation.PopAsync();
            });
        }
    }
}