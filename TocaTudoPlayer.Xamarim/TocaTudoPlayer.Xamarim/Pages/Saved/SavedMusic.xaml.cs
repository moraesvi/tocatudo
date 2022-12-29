using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
//using Unity;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SavedMusic : ContentPage
    {
        private SavedMusicPageViewModel _vm;
        private MusicAlbumPopup _musicAlbumPopup;
        private bool _formLoaded;
        private MusicAlbumSetupPopup _musicAlbumSetupPopup;
        private static bool _formIsVisible;
        public SavedMusic()
        {
            InitializeComponent();

            Title = $"{AppResource.AppName} - {AppHelper.ToTitleCase(AppResource.MusicMusicButton)}";

            _vm = App.Services.GetRequiredService<SavedMusicPageViewModel>();

            BindingContext = _vm;

            _musicAlbumPopup = new MusicAlbumPopup();

            _vm.ActionShowInterstitial += ViewModel_ActionShowInterstitial;
            _vm.AppErrorEvent += ViewModel_AppErrorEvent;

            MainPage.TimeSleepingEvent += MainPage_TimeSleepingEvent;

            MusicBottomPlayerViewModel bottomPlayer = App.Services.GetRequiredService<MusicBottomPlayerViewModel>();

            ucPlayerControl.BindingContext = bottomPlayer;
            ucPlayerControl.ViewModel = bottomPlayer;

            ucPlayerControl.ViewModel.MusicShowInterstitial += ViewModel_ShowInterstitial;

            CrossMTAdmob.Current.LoadInterstitial(App.AppConfigAdMob.AdsSavedMusicIntersticial);

            myAds.AdsId = App.AppConfigAdMob.AdsSavedMusicBanner;
            myAds.AdsLoaded += (sender, obj) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ((Grid)stlBottom.Children[0]).RowDefinitions[0].Height = GridLength.Auto;
                });
            };
        }
        public static bool FormIsVisible => _formIsVisible;
        public static async Task ShowInterstitial(Action intertistialNotLoaded) => await CustomCrossMTAdmob.ShowInterstitial(intertistialNotLoaded, () => { });
        public static async Task LoadAndShowInterstitial(Action intertistialNotLoaded) => await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsSavedMusicIntersticial, intertistialNotLoaded, () => { });
        private async void ViewCellPlusMusicPlaylist_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            _musicAlbumPopup.BindingContext = musicModel;

            await Navigation.ShowPopupAsync(_musicAlbumPopup);
        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            ((MusicPageViewModel)BindingContext).SearchMusicCommand.Execute(null);
        }
        private void AlbumMusicSavedSelect_Clicked(object sender, EventArgs e)
        {
            SelectModel musicSelected = (SelectModel)((Picker)sender).SelectedItem;
            _vm.MusicAlbumSavedSelectedCommand.ExecuteAsync(musicSelected);
        }

        private async void ViewCellPlusSavedMusicPlaylist_Clicked(object sender, EventArgs e)
        {
            ICommonMusicModel musicModel = (ICommonMusicModel)((Button)sender).CommandParameter;

            _musicAlbumSetupPopup = new MusicAlbumSetupPopup() { BindingContext = musicModel };
            _musicAlbumSetupPopup.DeleteMusicFromAlbumInvoked += MusicAlbumSetupPopup_DeleteMusicFromAlbumInvoked;
            _musicAlbumSetupPopup.DeleteMusicInvoked += MusicAlbumSetupPopup_DeleteMusicInvoked; ;

            await Navigation.ShowPopupAsync(_musicAlbumSetupPopup);
        }

        private async void MusicAlbumSetupPopup_DeleteMusicFromAlbumInvoked(object sender, (string Message, ICommonMusicModel Model) tupple)
        {
            bool ok = await DisplayAlert(AppResource.AppName, tupple.Message, "ok", AppResource.CancelLabel);
            if (ok)
            {
                LoadingControlPopup processingPopup = new LoadingControlPopup()
                {
                    StackLayoutBackgroundColor = Color.WhiteSmoke,
                    ActivityIndicatorColor = Color.FromHex("#ec7211"),
                    LabelColor = Color.FromHex("#ec7211"),
                    LabelText = AppResource.DeletingLabel,
                    CloseWhen = async () =>
                    {
                        await _vm.DeleteMusicFromAlbumPlaylist(tupple.Model);
                    }
                };

                processingPopup.Dismissed += (sender, e) =>
                {
                    _musicAlbumSetupPopup.Dismiss(_musicAlbumPopup);
                };

                await Navigation.ShowPopupAsync(processingPopup);
            }
        }

        private async void MusicAlbumSetupPopup_DeleteMusicInvoked(object sender, (string Message, ICommonMusicModel Model) tupple)
        {
            bool ok = await DisplayAlert(AppResource.AppName, tupple.Message, "ok", AppResource.CancelLabel);
            if (ok)
            {
                LoadingControlPopup processingPopup = new LoadingControlPopup()
                {
                    StackLayoutBackgroundColor = Color.WhiteSmoke,
                    ActivityIndicatorColor = Color.FromHex("#ec7211"),
                    LabelColor = Color.FromHex("#ec7211"),
                    LabelText = AppResource.DeletingLabel,
                    CloseWhen = async () =>
                    {
                        await _vm.DeleteDownloadedMusic(tupple.Model);
                    }
                };

                processingPopup.Dismissed += (sender, e) =>
                {
                    _musicAlbumSetupPopup.Dismiss(_musicAlbumPopup);
                };

                await Navigation.ShowPopupAsync(processingPopup);
            }
        }

        private void ViewModel_AppErrorEvent(object sender, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Ops!", msg, "ok");
            });
        }
        private void ViewModel_ActionShowInterstitial(object sender, Action audioPlayerPlay)
        {
            if (_vm.MusicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchSavedMusic)
                return;

            if (!App.IsSleeping)
            {
                if (!AppHelper.MusicPlayerInterstitialIsLoadded)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsSavedMusicIntersticial, () =>
                        {
                            _vm.MusicPlayerViewModel.PlayMusic();
                        }, () =>
                        {
                            _vm.MusicPlayerViewModel.Pause();
                        });
                    });
                }
                else
                    audioPlayerPlay();
            }
            else
            {
                AppHelper.HasInterstitialToShow = true;
                _vm.MusicPlayerViewModel.Pause();
            }
        }
        private void ViewModel_ShowInterstitial(object sender, EventArgs e)
        {
            if (_vm.MusicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.SearchSavedMusic)
                return;

            if (!App.IsSleeping)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsSavedMusicIntersticial, () =>
                    {
                        _vm.MusicPlayerViewModel.PlayMusic();
                    }, () =>
                    {
                        _vm.MusicPlayerViewModel.Pause();
                    });
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppHelper.HasInterstitialToShow = true;
                    _vm.MusicPlayerViewModel.Pause();
                });
            }
        }
        private void MainPage_TimeSleepingEvent(object sender, EventArgs e)
        {
            myAds.AdsId = App.AppConfigAdMob.AdsMusicBanner;
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.MusicPlaylistSearchFromDb());

                _formLoaded = true;
                App.EventTracker.SendScreenView($"Saved{Title}", nameof(SavedMusic));
            }

            if (_vm.IsInternetAvaiableGridSize > 0)
            {
                _vm.CheckInternetConnection();
            }

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _formIsVisible = false;
            base.OnDisappearing();
        }
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Grid grid = (Grid)sender;
            Grid gridContent = (Grid)grid.Children[2];

            await Task.Delay(100);

            Label musicName = (Label)gridContent.Children[0];
            Label musicTime = (Label)gridContent.Children[1];

            musicName.SizeUpSizeDownAnimation("MusicNameAnimation");
            musicTime.SizeUpSizeDownAnimation("MusicTimeAnimation");
        }
    }
}