using dotMorten.Xamarin.Forms;
using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Album : ContentPage
    {
        private readonly AlbumPageViewModel _vm;
        private readonly MusicPageViewModel _vmMusic;
        private readonly SavedMusicPageViewModel _vmSavedMusic;
        private readonly AlbumPlayerViewModel _vmAlbumPlayer;
        private Grid _lastGridAlbumSelected;
        private SearchMusicModel _searchAlbumLastSelected;
        private int _searchTimeDelay;
        private bool _formLoaded;
        private static bool _formIsVisible;
        private bool _textChangedIsRunning;
        public Album()
        {
            InitializeComponent();

            Title = $"{AppResource.AppName} - {AppHelper.ToTitleCase(AppResource.MusicAlbumButton)}";

            _searchTimeDelay = 500;
            _vm = App.Services.GetRequiredService<AlbumPageViewModel>();
            _vmMusic = App.Services.GetRequiredService<MusicPageViewModel>();
            _vmSavedMusic = App.Services.GetRequiredService<SavedMusicPageViewModel>();
            _vmAlbumPlayer = App.Services.GetRequiredService<AlbumPlayerViewModel>();

            MainPage.TimeSleepingEvent += MainPage_TimeSleepingEvent;

            BindingContext = _vm;

            App.IsSleepingEvent += MainPage_IsSleeping;
            _vm.AppErrorEvent += ViewModel_AppErrorEvent;

            frmAlbumPlaying.TranslateTo(_vm.CommonPageViewModel.AlbumPlayingGridSize, 0);

            myAds.AdsId = App.AppConfigAdMob.AdsAlbumBanner;
            myAds.AdsLoaded += (sender, obj) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ((Grid)stlBottom.Children[0]).RowDefinitions[0].Height = GridLength.Auto;
                });
            };
        }
        public static bool FormIsVisible => _formIsVisible;
        private void ViewModel_AppErrorEvent(object sender, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                _vm.IsSearching = false;
                await DisplayAlert("Ops!", msg, "ok");
            });
        }
        private void AlbumSelectionButton_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel playlistItem = (SearchMusicModel)((Button)sender).CommandParameter;
            AlbumSelectedPlay(playlistItem);
        }
        private void AlbumHistorySelectionButton_Clicked(object sender, EventArgs e)
        {
            HistoryAlbumModel playlistItem = (HistoryAlbumModel)((Button)sender).CommandParameter;
            AlbumSelectedPlay(playlistItem);
        }
        private async void AlbumBackButton_Clicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 1].GetType() != _vm.CommonPageViewModel.SelectedAlbum.GetType())
            {
                try
                {
                    await Navigation.PushAsync(_vm.CommonPageViewModel.SelectedAlbum);
                }
                catch { }
            }
        }
        private async void BtnSearch_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchName.Text))
            {
                await ((Button)sender).FadeTo(0);
                await actInd.FadeTo(1, 0);

                albumSV.HeightRequest = 0;

                _vm.AlbumSearchedName = txtSearchName.Text;
                await ((AlbumPageViewModel)BindingContext).SearchAlbumCommand.ExecuteAsync();

                await actInd.FadeTo(0);
                await ((Button)sender).FadeTo(1);
            }
        }
        private void ViewCellAlbumPlaylist_Tapped(object sender, EventArgs e)
        {
            SearchMusicModel searchMusic = (SearchMusicModel)((TappedEventArgs)e).Parameter;

            if (_lastGridAlbumSelected != null && _searchAlbumLastSelected != null)
            {
                _searchAlbumLastSelected.IsSelected = false;
            }

            Grid grid = (Grid)sender;

            if (grid.Id == _lastGridAlbumSelected?.Id)
            {
                _lastGridAlbumSelected = null;
                return;
            }

            Grid gridContent = (Grid)grid.Children[1];

            Label musicName = (Label)gridContent.Children[0];
            Label musicTime = (Label)gridContent.Children[1];

            musicName.SizeUpSizeDownAnimation("MusicNameFontSizeAnimation");
            musicTime.SizeUpSizeDownAnimation("MusicTimeFontSizeAnimation");

            searchMusic.IsSelected = true;

            _lastGridAlbumSelected = grid;
            _searchAlbumLastSelected = searchMusic;
        }
        private void TxtSearchNameClear_Clicked(object sender, EventArgs e)
        {
            txtSearchName.Text = string.Empty;
            txtSearchName.IsSuggestionListOpen = false;
        }
        private void AlbumSelectedPlay(SearchMusicModel playlistItem)
        {
            bool navigateTo = AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                _vm.CommonPageViewModel.SelectedAlbum = App.Services.GetRequiredService<AlbumPlayer>();
                _vm.CommonPageViewModel.SelectedAlbum.PlaylistItem = new ApiSearchMusicModel(playlistItem);

                if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 1].GetType() != _vm.CommonPageViewModel.SelectedAlbum.GetType())
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await CollapseAlbumPlayingGrid();
                        try
                        {
                            await Navigation.PushAsync(_vm.CommonPageViewModel.SelectedAlbum, true);
                        }
                        catch { }
                    });
                }
            }
        }
        private void AlbumSelectedPlay(HistoryAlbumModel playlistItem)
        {
            bool navigateTo = AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                _vm.CommonPageViewModel.SelectedAlbum = App.Services.GetRequiredService<AlbumPlayer>();
                _vm.CommonPageViewModel.SelectedAlbum.PlaylistItem = new ApiSearchMusicModel(playlistItem);

                if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 1].GetType() != _vm.CommonPageViewModel.SelectedAlbum.GetType())
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await CollapseAlbumPlayingGrid();
                        try
                        {
                            await Navigation.PushAsync(_vm.CommonPageViewModel.SelectedAlbum, true);
                        }
                        catch { }
                    });
                }
            }
        }
        private bool AlbumSelectedPage(string videoId)
        {          
            if (_vm.CommonPageViewModel.SelectedAlbum != null && string.Equals(_vm.CommonPageViewModel.SelectedAlbum.PlaylistItem.VideoId, videoId))
            {
                if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 1].GetType() != _vm.CommonPageViewModel.SelectedAlbum.GetType())
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await Navigation.PushAsync(_vm.CommonPageViewModel.SelectedAlbum, true);
                        }
                        catch { }
                    });
                    return false;
                }
            }

            return true;
        }
        private async void TxtSearchName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            if (_textChangedIsRunning)
                return;

            if (sender.Text?.Length > 0)
                txtSearchNameClear.IsVisible = true;
            else
                txtSearchNameClear.IsVisible = false;

            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                _vm.AlbumSearchedName = sender.Text;
                _textChangedIsRunning = true;

                await Task.Delay(_searchTimeDelay);

                if (sender.Text.Length >= 1)
                {
                    List<string> lstFilters = sender.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();

                    if (lstFilters.Count() > 1 && _vm.AlbumPlayedHistoryViewModel.UserSearchHistory != null)
                    {
                        sender.ItemsSource = _vm.AlbumPlayedHistoryViewModel.FilterUserSearchHistory(lstFilters);
                    }
                    else
                    {
                        sender.ItemsSource = _vm.AlbumPlayedHistoryViewModel.FilterUserSearchHistory(sender.Text);
                    }
                }

                _textChangedIsRunning = false;
            }
        }
        private void TxtSearchName_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            _vm.AlbumSearchedName = e.QueryText;
        }
        private async Task CollapseAlbumPlayingGrid()
        {
            if (_vm.CommonPageViewModel.AlbumPlayingGridSize > 0)
            {
                await Task.Delay(50);
                await frmAlbumPlaying.TranslateTo(_vm.CommonPageViewModel.AlbumPlayingGridSize, 0);

                if (_vm.CommonPageViewModel.SelectedAlbum != null)
                {
                    ((AlbumPlayerViewModel)_vm.CommonPageViewModel.SelectedAlbum.BindingContext).AlbumFrameSize = 0;
                    ((AlbumPlayerViewModel)_vm.CommonPageViewModel.SelectedAlbum.BindingContext).BottomPlayerViewModel.StopBottomPlayer(true);
                    ((AlbumPlayerViewModel)_vm.CommonPageViewModel.SelectedAlbum.BindingContext).MusicPlayerViewModel.Stop();
                    ((AlbumPlayerViewModel)_vm.CommonPageViewModel.SelectedAlbum.BindingContext).BottomPlayerViewModel.Init();
                }

                _vm.CommonPageViewModel.AlbumPlayingGridSize = 0;
            }
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;
            btnSearch.Focus();

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.AlbumPlayedHistoryViewModel.LoadUserSearchHistory(), _vm.AlbumPlayedHistoryViewModel.LoadPlayedHistory());
                _formLoaded = true;

                App.EventTracker.SendScreenView(Title, nameof(Album));
            }

            if (_vm.CommonPageViewModel.AlbumPlayingGridSize > 0)
            {
                await frmAlbumPlaying.TranslateTo(0, 0);
            }

            if (_vm.IsInternetAvaiableGridSize > 0)
            {
                _vm.CheckInternetConnection();
            }

            if (_vm.CommonPageViewModel.SelectedAlbum != null)
            {
                await _vm.CommonPageViewModel.SelectedAlbum.Load();
            }

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _formIsVisible = false;
            base.OnDisappearing();
        }
        private void MainPage_IsSleeping(object sender, bool sleeping)
        {
            _formIsVisible = !sleeping;
        }
        private void MainPage_TimeSleepingEvent(object sender, EventArgs e)
        {
            myAds.AdsId = App.AppConfigAdMob.AdsMusicBanner;
        }
        private void SelectHistoryRecentlyPlayed_Clicked(object sender, EventArgs e)
        {
            Button btnSelectHistoryRecentlyPlayed = (Button)sender;

            btnSelectHistoryRecentlyPlayed.FadeOutFadeInAnimation("BtnSelectHistoryRecentlyPlayed");
            lblPlayedHistorySelected.SizeUpSizeDownAnimation("LabeFontSizeAnimation");
        }
    }
}