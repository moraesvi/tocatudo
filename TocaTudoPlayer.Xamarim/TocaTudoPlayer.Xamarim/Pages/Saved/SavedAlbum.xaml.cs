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
    public partial class SavedAlbum : ContentPage
    {
        private Grid _lastGridAlbumSelected;
        private SearchMusicModel _searchAlbumLastSelected;
        private readonly IUnityContainer _unityContainer;
        private readonly IAlbumSavedPageViewModel _vm;
        private Action _audioPlayerPlay;
        private bool _formLoaded;
        private bool _formIsVisible;
        private bool _showInterstitialOnAppearing;
        public SavedAlbum(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _vm = unityContainer.Resolve<IAlbumSavedPageViewModel>();

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
            //await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
        }
        private void SearchMusictActionButton_Clicked(object sender, EventArgs e)
        {

        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            albumPlaylistHead.Text = Regex.Replace(txtSearchName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            ((IAlbumPageViewModel)BindingContext).SearchAlbumCommand.Execute(null);
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

            searchMusic.IsSelected = true;

            _lastGridAlbumSelected = grid;
            _searchAlbumLastSelected = searchMusic;
        }
        private void TxtSearchNameClear_Clicked(object sender, EventArgs e)
        {
            txtSearchName.Text = string.Empty;
        }
        private async Task AlbumSelectedPlay(SearchMusicModel playlistItem)
        {
            bool navigateTo = await AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                //_vm.CommonPageViewModel.SelectedMusic = new NavigationPage(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel(playlistItem)));       

                //await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
            }
        }
        private async Task AlbumSelectedPlay(HistoryAlbumModel playlistItem)
        {
            bool navigateTo = await AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                //_vm.CommonPageViewModel.SelectedMusic = new NavigationPage(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel(playlistItem)));

                //await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
            }
        }
        private async Task<bool> AlbumSelectedPage(string videoId) 
        {
            //_vm.MusicPlayer.HideBottomPlayer();
            //_vm.MusicPlayer.StopBottomPlayer();

            //if (_vm.CommonPageViewModel.SelectedMusic != null && string.Equals(((AlbumPlayer)_vm.CommonPageViewModel.SelectedMusic.CurrentPage).CurrentVideoId, videoId))
            {
                //await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
                return false;
            }

            return true;
        }       
        protected async override void OnAppearing()
        {
            _formIsVisible = true;

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.AlbumPlaylistSearchFromDb());

                _formLoaded = true;
            }

            //_vm.MusicPlayer.ActiveBottomPlayer();

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
            //_vm.MusicPlayer.StopBottomPlayer();
            _formIsVisible = false;

            base.OnDisappearing();
        }
    }
}