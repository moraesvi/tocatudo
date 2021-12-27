using dotMorten.Xamarin.Forms;
using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Music : ContentPage
    {
        private readonly IUnityContainer _unityContainer;
        private IMusicPageViewModel _vm;
        private SearchMusicModel _searchMusicLastSelected;
        private CancellationTokenSource _searchMusicLastSelectedToken;
        private MusicAlbumPopup _musicAlbumPopup;
        private Action _audioPlayerPlay;
        private Grid _lastGridMusicSelected;
        private bool _formLoaded;
        private bool _formIsVisible;
        private bool _showInterstitialOnAppearing;
        private bool _playMusicOnAperring;
        private int _playedHistoryCollectionSize;
        public Music(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _vm = unityContainer.Resolve<IMusicPageViewModel>();

            BindingContext = _vm;

            _musicAlbumPopup = new MusicAlbumPopup(_vm);

            _vm.ShowInterstitial += ViewModel_ShowInterstitial;
            _vm.ActionShowInterstitial += ViewModel_ActionShowInterstitial;
            _musicAlbumPopup.DeleteActionAlbumPopup += MusicAlbumPopup_DeleteActionAlbumPopup;
            _musicAlbumPopup.Dismissed += MusicAlbumPopup_Dismissed;
            _vm.MusicPlayerViewModel.MusicPlayedHistoricIsSaved += MusicPlayerViewModel_MusicPlayedHistoricIsSaved;
            _vm.MusicPlayerViewModel.PlayerLosedAudioFocus += MusicPlayerViewModel_PlayerLosedAudioFocus;
            _vm.MusicPlayerViewModel.AppErrorEvent += ViewModel_AppErrorEvent;

            _formLoaded = false;
            _formIsVisible = false;
            _showInterstitialOnAppearing = false;
            _playMusicOnAperring = false;

            InitializeComponent();

            ucPlayerControl.BindingContext = unityContainer.Resolve<IMusicBottomPlayerViewModel>();
            ucPlayerControl.ViewModel = unityContainer.Resolve<IMusicBottomPlayerViewModel>();
        }
        private void SearchMusictActionButton_Clicked(object sender, EventArgs e)
        {

        }
        private async void ViewCellMusicPlaylist_Tapped(object sender, EventArgs e)
        {
            SearchMusicModel searchMusic = (SearchMusicModel)((TappedEventArgs)e).Parameter;

            if (_vm.MusicPlayerViewModel.LastMusicPlayed != null)
                if (_vm.MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic)
                {
                    _vm.MusicPlayerViewModel.LastMusicPlayed.ReloadMusicPlayingIcon();
                    _vm.MusicPlayerViewModel.LastMusicPlayed.IsSelected = false;
                    _vm.MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic = false;
                    _searchMusicLastSelected = null;
                    _lastGridMusicSelected = null;
                }

            if (_lastGridMusicSelected != null && _searchMusicLastSelected != null)
            {
                ViewCellMusicPlaylistLastMusicSelected(_vm);
            }

            if (_searchMusicLastSelectedToken != null)
                _searchMusicLastSelectedToken.Cancel();

            Grid grid = (Grid)sender;

            if (grid.Id == _lastGridMusicSelected?.Id)
            {
                _lastGridMusicSelected = null;
                return;
            }

            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            searchMusic.IconMusicStatusEnabled = true;
            searchMusic.IconMusicStatusVisible = true;

            searchMusic.IsSelected = true;

            _lastGridMusicSelected = grid;
            _searchMusicLastSelected = searchMusic;
            _searchMusicLastSelectedToken = cancellationToken;

            _vm.MusicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 0;

            if (_vm.LastMusicHistorySelected != null)
            {
                _vm.LastMusicHistorySelected.UpdMusicSelectedColor();
                _vm.LastMusicHistorySelected = null;
                _vm.MusicPlayedHistoryViewModel.HistoryMusicPlayingNow = null;
            }

            await _vm.PlayMusic(searchMusic, cancellationToken.Token);
        }
        private async void ViewCellPlusMusicPlaylist_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            _musicAlbumPopup.BindingContext = musicModel;

            await Navigation.ShowPopupAsync(_musicAlbumPopup);
        }
        private async void GridMusicHistoryForm_Clicked(object sender, EventArgs e)
        {
            ViewCellMusicPlaylistLastMusicSelected(_vm);
            await _vm.MusicHistoryFormCommand.ExecuteAsync((UserMusicPlayedHistory)((Button)sender).CommandParameter);
        }
        private async void AlbumBackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            pkAlbumMusicSavedSelect.SelectedItem = null;
            ((IMusicPageViewModel)BindingContext).SearchMusicCommand.Execute(null);
        }
        private void ImgbDownloadMusic_Clicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            SearchMusicModel playlistItem = (SearchMusicModel)btn.CommandParameter;

            ((IMusicPageViewModel)BindingContext).DownloadMusicCommand.Execute(playlistItem);
        }
        private async void AlbumMusicSavedSelect_Clicked(object sender, EventArgs e)
        {
            SelectModel musicSelected = (SelectModel)((Picker)sender).SelectedItem;
            await _vm.MusicHistoryAlbumSelectedCommand.ExecuteAsync(musicSelected);
        }
        private void ViewModel_AppErrorEvent(int level, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Ops!", msg, "ok");
            });
        }
        private void ViewModel_ActionShowInterstitial(Action audioPlayerPlay)
        {
            if (_formIsVisible)
            {
                if (CrossMTAdmob.Current.IsInterstitialLoaded() && !AppHelper.MusicPlayerInterstitialWasShowed)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                        CrossMTAdmob.Current.ShowInterstitial();
                    });

                    AppHelper.MusicPlayerInterstitialWasShowed = true;
                }
                else
                    audioPlayerPlay();
            }
            else
            {
                _audioPlayerPlay = audioPlayerPlay;
                _showInterstitialOnAppearing = true;
            }
        }
        private void ViewModel_ShowInterstitial()
        {
            AppHelper.MusicPlayerInterstitialWasShowed = false;

            if (_formIsVisible)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                    CrossMTAdmob.Current.ShowInterstitial();
                });
            }
            else
            {
                _showInterstitialOnAppearing = true;
            }
        }
        private void MusicPlayerViewModel_MusicPlayedHistoricIsSaved(ICommonMusicModel music)
        {
            if (music.IsActiveMusic)
            {
                cvMusicPlayedHistory.ScrollTo(0, -1, ScrollToPosition.Start);
                _searchMusicLastSelected = _vm.MusicPlaylist.Where(mp => string.Equals(mp.VideoId, music.VideoId))
                                                            .FirstOrDefault();
            }
        }
        private void MusicPlayerViewModel_PlayerLosedAudioFocus()
        {
            _playMusicOnAperring = true;
        }
        private void ViewCellMusicPlaylistLastMusicSelected(IMusicPageViewModel bindingContext)
        {
            if (_searchMusicLastSelected != null && _searchMusicLastSelected.IconMusicStatusVisible)
            {
                _searchMusicLastSelected.IsBufferingMusic = false;
                _searchMusicLastSelected.IsSelected = false;
                _searchMusicLastSelected.IsLoadded = true;
                _searchMusicLastSelected.IconMusicStatusVisible = false;
                _searchMusicLastSelected.IconMusicDownloadVisible = false;
                _searchMusicLastSelected.IsDownloadMusicVisible = false;

                bindingContext.MusicPlayerViewModel.PlayPauseMusic((ICommonMusicModel)_searchMusicLastSelected);
            }
        }
        private async void MusicAlbumPopup_DeleteActionAlbumPopup(SearchMusicModel musicModel)
        {
            bool deletarOk = await DisplayAlert("TocaTudo", "Deletar álbum ?", "ok", "cancelar");

            if (deletarOk)
            {
                await _vm.CommonMusicPageViewModel.DeleteMusicAlbumPlaylistSelected(musicModel);
                _musicAlbumPopup.Dismiss(null);
            }
        }
        private void TxtSearchNameClear_Clicked(object sender, EventArgs e)
        {
            txtSearchName.Text = string.Empty;
            txtSearchName.IsSuggestionListOpen = false;
        }
        private void TxtSearchName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            if (sender.Text?.Length > 0)
            {
                txtSearchNameClear.IsVisible = true;
                txtSearchName.IsSuggestionListOpen = true;
            }
            else
                txtSearchNameClear.IsVisible = false;

            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                _vm.MusicSearchedName = sender.Text;

                if (sender.Text.Length >= 2)
                {
                    List<string> lstFilters = sender.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();

                    if (lstFilters.Count() > 1 && _vm.MusicPlayedHistoryViewModel.UserSearchHistory != null)
                    {
                        sender.ItemsSource = _vm.MusicPlayedHistoryViewModel.FilterUserSearchHistory(lstFilters);
                    }
                    else
                    {
                        sender.ItemsSource = _vm.MusicPlayedHistoryViewModel.FilterUserSearchHistory(sender.Text);
                    }
                }
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }
        private void TxtSearchName_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            _vm.MusicSearchedName = e.QueryText;
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.MusicPlayedHistoryViewModel.LoadUserSearchHistory(), _vm.MusicPlayedHistoryViewModel.LoadPlayedHistory(), _vm.CommonMusicPageViewModel.LoadMusicAlbumPlaylistSelected());

                //_vm.CommonMusicPageViewModel.LoadAlbumMusicSavedSelect();

                _playedHistoryCollectionSize = _vm.MusicPlayedHistoryViewModel.PlayedHistoryCollectionSize;
                _formLoaded = true;
            }

            _vm.MusicPlayerViewModel.ActiveBottomPlayer();

            if (_vm.IsInternetAvaiable && _showInterstitialOnAppearing)
            {
                if (CrossMTAdmob.Current.IsInterstitialLoaded())
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                        CrossMTAdmob.Current.ShowInterstitial();
                    });

                    AppHelper.MusicPlayerInterstitialWasShowed = true;
                }

                if (!AppHelper.MusicPlayerInterstitialWasShowed)
                {
                    if (_audioPlayerPlay != null)
                        _audioPlayerPlay();
                }

                _showInterstitialOnAppearing = false;
            }

            if (_playMusicOnAperring)
            {
                _vm.MusicPlayerViewModel.PlayPauseMusic();
                _playMusicOnAperring = false;
            }

            //if (!_vm.IsInternetAvaiable)
            //{
            //stlMusicSearch.IsVisible = false;
            //}
            //if (!_vm.IsInternetAvaiable && btnSalvos.IsEnabled)
            //{
            //    LoadSavedMusicData();
            //}

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _vm.MusicPlayerViewModel.StopBottomPlayer();
            _formIsVisible = false;

            base.OnDisappearing();
        }
        private async void ShowHideMusicPlayedHistory_Tapped(object sender, EventArgs e)
        {
            await ShowHideMusicPlayedHistory();
        }
        private async void ShowHideMusicPlayedHistory_Clicked(object sender, EventArgs e)
        {
            await ShowHideMusicPlayedHistory();
        }
        private void MusicAlbumPopup_Dismissed(object sender, Xamarin.CommunityToolkit.UI.Views.PopupDismissedEventArgs e)
        {

        }
        private async Task ShowHideMusicPlayedHistory()
        {
            if (_vm.MusicPlayedHistoryViewModel.PlayedHistoryCollectionSize == _playedHistoryCollectionSize)
            {
                Task tsk1 = cvMusicPlayedHistory.LayoutTo(new Rectangle(cvMusicPlayedHistory.Bounds.X, cvMusicPlayedHistory.Bounds.Y, cvMusicPlayedHistory.Bounds.Width, 0), 500, Easing.CubicIn);
                Task tsk2 = btnOpenPlayedHistory.RotateTo(360, 500, Easing.SpringOut);

                await Task.WhenAll(tsk1, tsk2);

                btnOpenPlayedHistory.ImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.Plus, 12, Color.White);

                _vm.MusicPlayedHistoryViewModel.PlayedHistoryCollectionSize = 0;

                _vm.StopMusicHistoryIsPlaying();
            }
            else
            {
                Task tsk1 = cvMusicPlayedHistory.LayoutTo(new Rectangle(cvMusicPlayedHistory.Bounds.X, cvMusicPlayedHistory.Bounds.Y, cvMusicPlayedHistory.Bounds.Width, _playedHistoryCollectionSize), 500, Easing.CubicOut);
                Task tsk2 = btnOpenPlayedHistory.RotateTo(180, 500, Easing.SpringOut);

                await Task.WhenAll(tsk1, tsk2);

                btnOpenPlayedHistory.ImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.Minus, 12, Color.White);

                _vm.MusicPlayedHistoryViewModel.PlayedHistoryCollectionSize = _playedHistoryCollectionSize;
            }
        }
        private void CvMusicPlayedHistory_BindingContextChanged(object sender, EventArgs e)
        {

        }
    }
}