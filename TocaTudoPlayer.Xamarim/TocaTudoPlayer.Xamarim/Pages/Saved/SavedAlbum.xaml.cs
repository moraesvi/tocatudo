using dotMorten.Xamarin.Forms;
using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SavedAlbum : ContentPage
    {
        private Grid _lastGridAlbumSelected;
        private SearchMusicModel _searchAlbumLastSelected;
        private readonly AlbumSavedPageViewModel _vm;
        private bool _formLoaded;
        private bool _formIsVisible;
        private bool _showInterstitialOnAppearing;
        public SavedAlbum()
        {
            _vm = App.Services.GetRequiredService<AlbumSavedPageViewModel>();

            BindingContext = _vm;

            InitializeComponent();
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
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            //albumPlaylistHead.Text = Regex.Replace(txtSearchName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            //((AlbumPageViewModel)BindingContext).SearchAlbumCommand.Execute(null);
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
            //txtSearchName.Text = string.Empty;
        }
        private async Task AlbumSelectedPlay(SearchMusicModel playlistItem)
        {
            bool navigateTo = await AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                _vm.CommonPageViewModel.SelectedAlbum = new AlbumPlayer();

                await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedAlbum);
            }
        }
        private async Task AlbumSelectedPlay(HistoryAlbumModel playlistItem)
        {
            bool navigateTo = await AlbumSelectedPage(playlistItem.VideoId);

            if (navigateTo)
            {
                _vm.CommonPageViewModel.SelectedAlbum = new AlbumPlayer();
                await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedAlbum);
            }
        }
        private async Task<bool> AlbumSelectedPage(string videoId)
        {
            if (_vm.CommonPageViewModel.SelectedAlbum != null && string.Equals(_vm.CommonPageViewModel.SelectedAlbum.PlaylistItem.VideoId, videoId))
            {
                await Navigation.PushModalAsync(_vm.CommonPageViewModel.SelectedAlbum);
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
                    //TODO: Insert saved album ad
                    //CrossMTAdmob.Current.LoadInterstitial(App.AppConfigAdMob.AdsSavedMusicIntersticial);
                    CrossMTAdmob.Current.ShowInterstitial();
                }

                _showInterstitialOnAppearing = false;
            }

            base.OnAppearing();

            await Task.Delay(10000).ContinueWith((tsk) =>
            {
                if (tsk.IsCompleted)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ((Grid)stlBottom.Children[0]).RowDefinitions[0].Height = GridLength.Auto;
                    });
                }
            });
        }
        protected override void OnDisappearing()
        {
            //_vm.MusicPlayer.StopBottomPlayer();
            _formIsVisible = false;

            base.OnDisappearing();
        }
    }
}