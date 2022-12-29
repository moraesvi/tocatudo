using dotMorten.Xamarin.Forms;
using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Xamarin.Essentials.Permissions;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Music : ContentPage
    {
        private MusicPageViewModel _vm;
        private SavedMusicPageViewModel _savedVm;
        private SearchMusicModel _searchMusicLastSelected;
        private MusicAlbumPopup _musicAlbumPopup;
        private MusicAlbumConfigPopup _musicAlbumConfigPopup;
        private Action _audioPlayerPlay;
        private SelectModel _albumPopupDelete;
        private bool _formLoaded;
        private bool _adLoadded;
        private static bool _formIsVisible;
        private bool _textChangedIsRunning;
        private int _searchTimeDelay;
        private float _playedHistoryCollectionSize;
        public Music()
        {
            InitializeComponent();

            Title = $"{AppResource.AppName} - {AppHelper.ToTitleCase(AppResource.MusicMusicButton)}";

            _searchTimeDelay = 500;
            _vm = App.Services.GetRequiredService<MusicPageViewModel>();
            _savedVm = App.Services.GetRequiredService<SavedMusicPageViewModel>();

            BindingContext = _vm;

            _vm.ShowInterstitial += ViewModel_ShowInterstitial;
            _vm.ActionShowInterstitial += ViewModel_ActionShowInterstitial;
            _vm.AppErrorEvent += ViewModel_AppErrorEvent;

            _vm.MusicPlayerViewModel.MusicPlayedHistoricIsSaved += MusicPlayerViewModel_MusicPlayedHistoricIsSaved;
            _vm.MusicPlayerViewModel.AppErrorEvent += ViewModel_AppErrorEvent;

            MainPage.TimeSleepingEvent += MainPage_TimeSleepingEvent;

            MusicBottomPlayerViewModel bottomPlayer = App.Services.GetRequiredService<MusicBottomPlayerViewModel>();

            ucPlayerControl.BindingContext = bottomPlayer;
            ucPlayerControl.ViewModel = bottomPlayer;

            ucPlayerControl.ViewModel.MusicShowInterstitial += ViewModel_ShowInterstitial;

            CrossMTAdmob.Current.LoadInterstitial(App.AppConfigAdMob.AdsMusicIntersticial);

            frmPlayedHistory.TranslateTo(220, 0);
            lblPlayedHistorySelected.FadeTo(0, 0);
            lblPlayedAlbumModeHistorySelected.FadeTo(0, 0);

            myAds.AdsId = App.AppConfigAdMob.AdsMusicBanner;
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
        public static async Task LoadAndShowInterstitial(Action intertistialNotLoaded) => await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsMusicIntersticial, intertistialNotLoaded, () => { });
        private async void MusicAlbumPopup_DownloadButtonClicked(object sender, MusicModelBase param)
        {
            await _vm.StartDownloadMusicCommand.ExecuteAsync(param);
        }
        private void MusicAlbumPopup_LoadPopup(object sender, MusicModelBase param)
        {
            _vm.CommonMusicPageViewModel.InitFormMusicUtils(param.MusicAlbumPopupModel);
        }
        private async void ViewCellPlayMusic_Tapped(object sender, EventArgs e)
        {
            Grid grid = (Grid)sender;
            Grid gridContent = (Grid)grid.Children[3];

            Task task = Task.Run(async () =>
            {
                await Task.Delay(250);

                Label musicName = (Label)gridContent.Children[0];
                Label musicTime = (Label)gridContent.Children[1];
                Label musicNameAlbumMode = (Label)gridContent.Children[2];

                musicName.SizeUpSizeDownAnimation("MusicNameAnimation");
                musicTime.SizeUpSizeDownAnimation("MusicTimeAnimation");
                musicNameAlbumMode.SizeUpSizeDownAnimation("MusicNameAlbumModeAnimation");
            });

            await frmPlayedHistory.TranslateTo(160, 0);
            await Task.WhenAll(task, _vm.SelectMusicCommand.ExecuteAsync((SearchMusicModel)((TappedEventArgs)e).Parameter));
        }
        private async void ViewCellPlusMusicPlaylist_Clicked(object sender, EventArgs e)
        {
            MusicModelBase musicModel = (MusicModelBase)((Button)sender).CommandParameter;

            _musicAlbumPopup = new MusicAlbumPopup();
            _musicAlbumPopup.BindingContext = new MusicAlbumDialogModel()
            {
                MusicModel = musicModel,
                AlbumMusicSavedCollection = _vm.CommonMusicPageViewModel.AlbumMusicSavedSelectFilteredCollection
            };

            _musicAlbumPopup.LoadPopup += MusicAlbumPopup_LoadPopup;
            _musicAlbumPopup.AlertActionAlbumPopup += MusicAlbumPopup_AlertActionAlbumPopup;
            _musicAlbumPopup.DeleteActionAlbumPopup += MusicAlbumPopup_DeleteActionAlbumPopup;
            _musicAlbumPopup.NewAlbumNameClicked += MusicAlbumPopup_NewAlbumNameClicked;
            _musicAlbumPopup.UpdateAlbumNameClicked += MusicAlbumPopup_UpdateAlbumNameClicked;
            _musicAlbumPopup.DownloadButtonClicked += MusicAlbumPopup_DownloadButtonClicked;
            _musicAlbumPopup.SetupPopupDeleteMusicInvoked += MusicAlbumPopup_SetupPopupDeleteMusicInvoked;
            _musicAlbumPopup.SetupPopupDeleteMusicFromAlbumInvoked += MusicAlbumPopup_SetupPopupDeleteMusicFromAlbumInvoked;

            await Navigation.ShowPopupAsync(_musicAlbumPopup);
        }
        private async void GridMusicHistoryForm_Clicked(object sender, EventArgs e)
        {
            ViewCellMusicPlaylistLastMusicSelected(_vm);
            await _vm.MusicHistoryFormCommand.ExecuteAsync((UserMusicPlayedHistory)((Button)sender).CommandParameter);
        }
        private async void SelectHistoryRecentlyPlayed_Clicked(object sender, EventArgs e)
        {
            await Task.Delay(100);
            await Task.WhenAll(frmPlayedHistory.TranslateTo(0, 0), lblPlayedHistorySelected.FadeTo(1), lblPlayedAlbumModeHistorySelected.FadeTo(1));
        }
        private async void BtnSearch_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchName.Text))
            {
                _vm.MusicSearchedName = txtSearchName.Text;
                //pkAlbumMusicSavedSelect.SelectedItem = null;

                await ((Button)sender).FadeTo(0);
                await actInd.FadeTo(1, 0);

                await ((MusicPageViewModel)BindingContext).SearchMusicCommand.ExecuteAsync();

                await actInd.FadeTo(0);
                await ((Button)sender).FadeTo(1);
            }
        }
        private void ImgbDownloadMusic_Clicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            SearchMusicModel playlistItem = (SearchMusicModel)btn.CommandParameter;

            ((MusicPageViewModel)BindingContext).DownloadMusicCommand.Execute(playlistItem);
        }
        private async void MusicAlbumConfigPopup_AlertDeleteAlbumPopup(object sender, (int AlbumId, string Text) param)
        {
            bool ok = await DisplayAlert(AppResource.AppName, param.Text, "ok", AppResource.CancelLabel);
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
                        await _vm.DeleteAlbum(param.AlbumId);
                    }
                };

                processingPopup.Dismissed += (sender, e) =>
                {
                    _musicAlbumConfigPopup.Dismiss(_musicAlbumConfigPopup);
                };

                await Navigation.ShowPopupAsync(processingPopup);
            }
        }
        private async void MusicAlbumPopup_SetupPopupDeleteMusicInvoked(object sender, (string Message, ICommonMusicModel Model) tupple)
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
                        await _vm.CommonMusicPageViewModel.DeleteDownloadedMusic(tupple.Model);
                    }
                };

                processingPopup.Dismissed += (sender, e) =>
                {
                    _musicAlbumPopup.Dismiss(_musicAlbumPopup);
                };

                await Navigation.ShowPopupAsync(processingPopup);
            }
        }
        private async void MusicAlbumPopup_SetupPopupDeleteMusicFromAlbumInvoked(object sender, (string Message, ICommonMusicModel Model) tupple)
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
                    _musicAlbumPopup.Dismiss(_musicAlbumPopup);
                };

                await Navigation.ShowPopupAsync(processingPopup);
            }
        }
        private async void AlbumMusicSavedSelect_Clicked(object sender, EventArgs e)
        {
            await _vm.MusicAlbumSavedSelectedCommand.ExecuteAsync(_vm.AlbumMusicSavedSelected);
        }
        private void ViewModel_AppErrorEvent(object sender, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                _vm.IsSearching = false;
                await DisplayAlert("Ops!", msg, "ok");
            });
        }
        private void ViewModel_ActionShowInterstitial(object sender, Action audioPlayerPlay)
        {
            if (_vm.MusicPlayerViewModel.KindMusicPlayingNow != MusicSearchType.Undefined)
                if (!(_vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusic || _vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicHistory || _vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicAlbumHistory))
                    return;

            if (!App.IsSleeping)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsMusicIntersticial, () =>
                    {
                        _vm.MusicPlayerViewModel.PlayMusic();
                    }, () =>
                    {
                        AppHelper.MusicPlayerInterstitialIsLoadded = true;
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
        private void ViewModel_ShowInterstitial(object sender, EventArgs e)
        {
            if (!(_vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusic || _vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicHistory || _vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicAlbumHistory))
                return;

            if (!App.IsSleeping)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await CustomCrossMTAdmob.LoadAndShowInterstitial(App.AppConfigAdMob.AdsMusicIntersticial, () =>
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
        private async void MusicPlayerViewModel_MusicPlayedHistoricIsSaved(object sender, ICommonMusicModel music)
        {
            await Task.Run(() =>
            {
                if (music.IsActiveMusic)
                {
                    cvMusicPlayedHistory.ScrollTo(0, -1, ScrollToPosition.Start);
                    _searchMusicLastSelected = _vm.MusicPlaylist.Where(mp => string.Equals(mp.VideoId, music.VideoId))
                                                                .FirstOrDefault();
                }
            });
        }
        private void ViewCellMusicPlaylistLastMusicSelected(MusicPageViewModel bindingContext)
        {
            if (_searchMusicLastSelected != null && _searchMusicLastSelected.IconMusicStatusVisible)
            {
                _searchMusicLastSelected.IsBufferingMusic = false;
                _searchMusicLastSelected.IsSelected = false;
                _searchMusicLastSelected.IsLoadded = true;
                _searchMusicLastSelected.IconMusicStatusVisible = false;
                _searchMusicLastSelected.IconMusicDownloadVisible = false;
                _searchMusicLastSelected.IsDownloadMusicVisible = false;

                bindingContext.MusicPlayerViewModel.PlayPauseMusic(_searchMusicLastSelected);
            }
        }
        private async void AlbumMusicConfigButton_Clicked(object sender, EventArgs e)
        {
            _musicAlbumConfigPopup = new MusicAlbumConfigPopup() { BindingContext = _vm.CommonMusicPageViewModel };
            _musicAlbumConfigPopup.AlertDeleteAlbumPopup += MusicAlbumConfigPopup_AlertDeleteAlbumPopup;

            await Navigation.ShowPopupAsync(_musicAlbumConfigPopup);
        }
        private async void MusicAlbumPopup_AlertActionAlbumPopup(object sender, string e)
        {
            await DisplayAlert(AppResource.AppName, e, "ok");
        }
        private async void MusicAlbumPopup_DeleteActionAlbumPopup(object sender, MusicModelBase param)
        {
            bool deletarOk = await DisplayAlert(AppResource.AppName, AppResource.PopupAlertDeleteAlbum, "ok", AppResource.CancelLabel);

            if (deletarOk)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    _albumPopupDelete = param.MusicAlbumPopupModel.AlbumMusicSavedSelected;
                    await _vm.CommonMusicPageViewModel.DeleteMusicFromAlbumPlaylist(param);

                    if (param.SearchType == MusicSearchType.SearchMusicAlbumHistory && param.MusicAlbumPopupModel.SavedAlbumModeIsVisible)
                    {
                        //_vm.RemoveMusicPlaylistItem(param);
                    }

                    await _vm.MusicAlbumSavedSelectedCommand.ExecuteAsync(_albumPopupDelete);
                    _musicAlbumPopup.Dismiss(null);
                });
            }
        }
        private void TxtSearchNameClear_Clicked(object sender, EventArgs e)
        {
            txtSearchName.Text = string.Empty;
            txtSearchName.IsSuggestionListOpen = false;

            txtSearchName.Focus();
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
                _vm.MusicSearchedName = sender.Text;

                _textChangedIsRunning = true;

                await Task.Delay(_searchTimeDelay);

                if (sender.Text.Length >= 1)
                {
                    List<string> lstFilters = sender.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();

                    if (lstFilters.Count() > 1 && _vm.MusicPlayedHistoryViewModel.UserSearchHistory != null)
                    {
                        txtSearchName.ItemsSource = _vm.MusicPlayedHistoryViewModel.FilterUserSearchHistory(lstFilters);
                    }
                    else
                    {
                        string[] t = _vm.MusicPlayedHistoryViewModel.FilterUserSearchHistory(sender.Text);
                        txtSearchName.ItemsSource = t;
                    }
                }

                _textChangedIsRunning = false;
            }
        }
        private void TxtSearchName_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            _vm.MusicSearchedName = e.QueryText;
        }
        private async void MusicAlbumPopup_NewAlbumNameClicked(object sender, (string AlbumName, MusicModelBase MusicModel) param)
        {
            bool musicAlbumExists = await _vm.CommonMusicPageViewModel.ExistsMusicAlbumPlaylist(param.AlbumName, param.MusicModel);
            if (!musicAlbumExists)
            {
                LoadingControlPopup processingPopup = new LoadingControlPopup()
                {
                    StackLayoutBackgroundColor = Color.WhiteSmoke,
                    ActivityIndicatorColor = Color.FromHex("#ec7211"),
                    LabelColor = Color.FromHex("#ec7211"),
                    LabelText = AppResource.InsertingLagel,
                    CloseWhen = async () =>
                    {
                        await _vm.InsertOrUpdateMusicAlbumPlaylistSelected(param.AlbumName, param.MusicModel);
                    }
                };

                processingPopup.Dismissed += (sender, e) =>
                {
                    _musicAlbumPopup.Dismiss(_musicAlbumPopup);
                };

                await Navigation.ShowPopupAsync(processingPopup);
            }
        }
        private async void MusicAlbumPopup_UpdateAlbumNameClicked(object sender, MusicModelBase param)
        {
            LoadingControlPopup processingPopup = new LoadingControlPopup()
            {
                StackLayoutBackgroundColor = Color.WhiteSmoke,
                ActivityIndicatorColor = Color.FromHex("#ec7211"),
                LabelColor = Color.FromHex("#ec7211"),
                LabelText = AppResource.UpdatingLagel,
                CloseWhen = async () =>
                {
                    await _vm.UpdateMusicAlbumPlaylistSelected(param.MusicAlbumPopupModel.AlbumMusicSavedSelected.Id, param);
                }
            };

            processingPopup.Dismissed += (sender, e) =>
            {
                _musicAlbumPopup.Dismiss(_musicAlbumPopup);
            };

            await Navigation.ShowPopupAsync(processingPopup);
        }
        private void MainPage_TimeSleepingEvent(object sender, EventArgs e)
        {
            myAds.AdsId = App.AppConfigAdMob.AdsMusicBanner;
        }
        private void PlayMusicPlayingNow()
        {
            //if ((_vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusic || _vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicHistory || _vm.MusicPlayerViewModel.KindMusicPlayingNow == MusicSearchType.SearchMusicAlbumHistory) && _vm.MusicPlayerViewModel.MusicPlayingNow != null && !AppHelper.HasInterstitialToShow)
            //{
            //    if (_vm.MusicPlayerViewModel.MusicPlayingNow.IsActiveMusic && !_vm.MusicPlayerViewModel.HasMusicPlaying)
            //    {
            //        _vm.MusicPlayerViewModel.PlayPauseMusic();
            //    }
            //}
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;
            btnSearch.Focus();

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.MusicPlayedHistoryViewModel.LoadUserSearchHistory(), _vm.MusicPlayedHistoryViewModel.LoadPlayedHistory(), _vm.LoadMusicAlbumPlaylistSelected());

                _playedHistoryCollectionSize = _vm.MusicPlayedHistoryViewModel.PlayedHistoryCollectionSize;
                _formLoaded = true;

                App.EventTracker.SendScreenView(Title, nameof(Music));
            }

            if (_formIsVisible)
            {
                PlayMusicPlayingNow();
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
    }
}