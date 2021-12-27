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
    public partial class SavedMusic : ContentPage
    {
        private readonly IUnityContainer _unityContainer;
        private IMusicSavedPageViewModel _vm;
        private SearchMusicModel _searchMusicLastSelected;
        private CancellationTokenSource _searchMusicLastSelectedToken;
        private MusicAlbumPopup _musicAlbumPopup;
        private Action _audioPlayerPlay;
        private Grid _lastGridMusicSelected;
        private bool _formLoaded;
        private bool _formIsVisible;
        private bool _showInterstitialOnAppearing;
        private int _playedHistoryCollectionSize;
        public SavedMusic(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _vm = unityContainer.Resolve<IMusicSavedPageViewModel>();

            BindingContext = _vm;

            _musicAlbumPopup = new MusicAlbumPopup(_vm);

            //_vm.PlayerReady += ViewModel_PlayerReady;
            _vm.ActionShowInterstitial += ViewModel_ShowInterstitial;
            _musicAlbumPopup.DeleteActionAlbumPopup += MusicAlbumPopup_DeleteActionAlbumPopup;
            //_vm.MusicPlayerViewModel.MusicPlayedHistoricIsSaved += MusicPlayerViewModel_MusicPlayedHistoricIsSaved;
            _vm.AppErrorEvent += ViewModel_AppErrorEvent;

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

            //if (_vm.MusicPlayerViewModel.LastMusicPlayed != null)
            //    if (_vm.MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic)
            //    {
            //        _vm.MusicPlayerViewModel.LastMusicPlayed.ReloadMusicPlayingIcon();
            //        _vm.MusicPlayerViewModel.LastMusicPlayed.UpdMusicSelectedColor();
            //        _vm.MusicPlayerViewModel.LastMusicPlayed.UpdMusicFontColor();
            //        _vm.MusicPlayerViewModel.LastMusicPlayed.IsActiveMusic = false;
            //        _searchMusicLastSelected = null;
            //        _lastGridMusicSelected = null;
            //    }

            if (_searchMusicLastSelected != null)
            {
                if (_searchMusicLastSelected.Download.IsDownloading || _searchMusicLastSelected.IsDownloadMusicVisible)
                    return;
            }

            if (_lastGridMusicSelected != null && _searchMusicLastSelected != null)
            {
                //ViewCellMusicPlaylistLastMusicSelected(_vm);
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

            //searchMusic.UpdMusicSelectedColor();
            //searchMusic.UpdMusicFontColor();

            _lastGridMusicSelected = grid;
            _searchMusicLastSelected = searchMusic;
            _searchMusicLastSelectedToken = cancellationToken;

            //_vm.MusicPlayedHistoryViewModel.PlayedHistoryPlayerFormSize = 0;

            //if (_vm.LastMusicHistorySelected != null)
            //{
            //    _vm.LastMusicHistorySelected.UpdMusicSelectedColor();
            //    _vm.LastMusicHistorySelected = null;
            //    _vm.MusicPlayedHistoryViewModel.RecentlyPlayedSelected = null;
            //}

            //await _vm.PlayMusic(searchMusic, cancellationToken.Token);
        }
        private async void ViewCellPlusMusicPlaylist_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            _musicAlbumPopup.BindingContext = musicModel;

            await Navigation.ShowPopupAsync(_musicAlbumPopup);
        }
        private async void GridMusicHistoryForm_Clicked(object sender, EventArgs e)
        {
            //ViewCellMusicPlaylistLastMusicSelected(_vm);
            //await _vm.MusicHistoryFormCommand.ExecuteAsync((UserMusicPlayedHistory)((Button)sender).CommandParameter);
        }
        private async void AlbumBackButton_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedMusic);
        }
        private void CollectionViewMusicPlaylist_Changed(object sender, SelectionChangedEventArgs e)
        {

        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            pkAlbumMusicSavedSelect.SelectedItem = null;
            ((IMusicPageViewModel)BindingContext).SearchMusicCommand.Execute(null);
        }
        private void ImgbDownloadMusicVisible_Clicked(object sender, EventArgs e)
        {

        }
        private void ImgbDownloadMusic_Clicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            SearchMusicModel playlistItem = (SearchMusicModel)btn.CommandParameter;

            ((IMusicPageViewModel)BindingContext).DownloadMusicCommand.Execute(playlistItem);
        }
        private void AlbumMusicSavedSelect_Clicked(object sender, EventArgs e)
        {
            SelectModel musicSelected = (SelectModel)((Picker)sender).SelectedItem;
            //_vm.MusicHistoryAlbumSelectedCommand.Execute(musicSelected);
        }
        private void ViewModel_AppErrorEvent(int level, string msg)
        {
        }
        private void ViewModel_ShowInterstitial(Action audioPlayerPlay)
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
        private void ViewModel_PlayerReady()
        {
            
        }
        private void ViewCellMusicPlaylistLastMusicSelected(IMusicPageViewModel bindingContext)
        {
            if (_searchMusicLastSelected != null && _searchMusicLastSelected.IconMusicStatusVisible)
            {
                _searchMusicLastSelected.IsBufferingMusic = false;
                _searchMusicLastSelected.IsLoadded = true;
                _searchMusicLastSelected.IsSelected = true;
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
                //await _vm.DeleteMusicAlbumPlaylistSelected(musicModel);
                _musicAlbumPopup.Dismiss(null);
            }
        }
        private void TxtSearchNameClear_Clicked(object sender, EventArgs e)
        {
            txtSearchName.Text = string.Empty;
        }
        private void TxtSearchName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {

        }
        private void TxtSearchName_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;

            if (!_formLoaded)
            {
                await Task.WhenAll(_vm.MusicPlaylistSearchFromDb(), _vm.CommonMusicPageViewModel.LoadMusicAlbumPlaylistSelected());

                _formLoaded = true;
            }

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


            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            //_vm.MusicPlayerViewModel.StopBottomPlayer();
            _formIsVisible = false;

            base.OnDisappearing();
        }
        private void CvMusicPlayedHistory_BindingContextChanged(object sender, EventArgs e)
        {

        }
    }
}