using dotMorten.Xamarin.Forms;
using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Album : ContentPage
    {
        private Grid _lastGridAlbumSelected;
        private SearchMusicModel _searchAlbumLastSelected;
        private readonly IUnityContainer _unityContainer;
        private readonly IAlbumPageViewModel _vm;
        private Action _audioPlayerPlay;
        private bool _formLoaded;
        private bool _formIsVisible;
        private bool _showInterstitialOnAppearing;
        public Album(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _vm = unityContainer.Resolve<IAlbumPageViewModel>();

            BindingContext = _vm;


            _vm.AppErrorEvent += ViewModel_AppErrorEvent;

            InitializeComponent();

            ucPlayerControl.BindingContext = unityContainer.Resolve<IMusicBottomPlayerViewModel>();
            ucPlayerControl.ViewModel = unityContainer.Resolve<IMusicBottomPlayerViewModel>();
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {

        }
        private void ImageButton_Clicked(object sender, EventArgs e)
        {

        }
        private void FrmDownloadMusic_Tapped(object sender, EventArgs e)
        {

        }
        private async Task<bool> CheckAndRequestLocalStoragePermission()
        {
            return false;
        }
        private void BbiShowDownload_Clicked(object sender, EventArgs e)
        {

        }
        private void ViewModel_AppErrorEvent(int level, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Ops!", msg, "ok");
            });
        }
        private async void AlbumSelectionButton_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel playlistItem = (SearchMusicModel)((Button)sender).CommandParameter;
            await AlbumSelectedPlay(playlistItem);
        }
        private async void AlbumHistorySelectionButton_Clicked(object sender, EventArgs e)
        {
            HistoryAlbumModel playlistItem = (HistoryAlbumModel)((Button)sender).CommandParameter;
            await AlbumSelectedPlay(playlistItem);
        }
        private async void AlbumBackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
        }
        private void SearchMusictActionButton_Clicked(object sender, EventArgs e)
        {

        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchName.Text))
            {
                //albumPlaylistHead.Text = Regex.Replace(txtSearchName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                ((IAlbumPageViewModel)BindingContext).SearchAlbumCommand.Execute(null);
            }
        }
        private void ViewCellAlbumPlaylist_Tapped(object sender, EventArgs e)
        {
            SearchMusicModel searchMusic = (SearchMusicModel)((TappedEventArgs)e).Parameter;

            if (_lastGridAlbumSelected != null && _searchAlbumLastSelected != null)
            {
                _searchAlbumLastSelected.IsSelected = false;
                //_searchAlbumLastSelected.UpdMusicSelectedColor();
                //_searchAlbumLastSelected.UpdMusicFontColor();
            }

            Grid grid = (Grid)sender;

            if (grid.Id == _lastGridAlbumSelected?.Id)
            {
                _lastGridAlbumSelected = null;
                return;
            }

            searchMusic.IsSelected = true;
            //searchMusic.UpdMusicSelectedColor();
            //searchMusic.UpdMusicFontColor();

            _lastGridAlbumSelected = grid;
            _searchAlbumLastSelected = searchMusic;
        }
        private void TxtSearchNameClear_Clicked(object sender, EventArgs e)
        {
            txtSearchName.Text = string.Empty;
            txtSearchName.IsSuggestionListOpen = false;
        }
        private async Task AlbumSelectedPlay(SearchMusicModel playlistItem)
        {
            bool navigateTo = await AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                _vm.CommonPageViewModel.SelectedMusic = new NavigationPage(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel(playlistItem)));       

                await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
            }
        }
        private async Task AlbumSelectedPlay(HistoryAlbumModel playlistItem)
        {
            bool navigateTo = await AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                _vm.CommonPageViewModel.SelectedMusic = new NavigationPage(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel(playlistItem)));

                await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
            }
        }
        private async Task<bool> AlbumSelectedPage(string videoId) 
        {
            _vm.MusicPlayer.HideBottomPlayer();
            _vm.MusicPlayer.StopBottomPlayer();

            if (_vm.CommonPageViewModel.SelectedMusic != null && string.Equals(((AlbumPlayer)_vm.CommonPageViewModel.SelectedMusic.CurrentPage).CurrentVideoId, videoId))
            {
                await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
                return false;
            }

            return true;
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
                _vm.AlbumSearchedName = sender.Text;

                if (sender.Text.Length >= 2)
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
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }
        private void TxtSearchName_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            _vm.AlbumSearchedName = e.QueryText;
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.AlbumPlayedHistoryViewModel.LoadUserSearchHistory(), _vm.AlbumPlayedHistoryViewModel.LoadPlayedHistory());

                _formLoaded = true;
            }

            _vm.MusicPlayer.ActiveBottomPlayer();

            if (_vm.IsInternetAvaiable && _showInterstitialOnAppearing)
            {
                if (CrossMTAdmob.Current.IsInterstitialLoaded())
                {
                    CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                    CrossMTAdmob.Current.ShowInterstitial();
                }
                else
                    _audioPlayerPlay();

                _showInterstitialOnAppearing = false;
            }

            if (!_vm.IsInternetAvaiable)
            {
                //stlMusicSearch.IsVisible = false;
            }
            //if (!_vm.IsInternetAvaiable && btnSalvos.IsEnabled)
            //{
            //    LoadSavedMusicData();
            //}

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _vm.MusicPlayer.StopBottomPlayer();
            _formIsVisible = false;

            base.OnDisappearing();
        }
    }
}