using dotMorten.Xamarin.Forms;
using MarcTron.Plugin;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Interface;
using TocaTudoPlayer.Xamarim.Resources;
using Unity;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Search : ContentPage
    {
        private readonly IDatabaseConn _database;
        private readonly IUnityContainer _unityContainer;
        private Grid _lastGridAlbumSelected;
        private Frame _lastFrameMusicSelected;
        private ISearchPlaylistViewModel _vm;
        private SearchMusicModel _searchAlbumLastSelected;
        private SearchMusicModel _searchMusicLastSelected;
        private Image _pvImageSelected;
        private MusicSearchType _musicSearchType;
        private MusicSearchType _savedMusicDataSearchType;
        private Action _audioPlayerPlay;
        private bool _formLoaded;
        private bool _formIsVisible;
        private bool _showInterstitialOnAppearing;

        public Search(IUnityContainer unityContainer)
        {
            InitializeComponent();

            //_vm = unityContainer.Resolve<ISearchPlaylistViewModel>();
            _database = unityContainer.Resolve<IDatabaseConn>();

            ucPlayerControl.BindingContext = unityContainer.Resolve<IMusicBottomPlayerViewModel>();
            ucPlayerControl.ViewModel = unityContainer.Resolve<IMusicBottomPlayerViewModel>();

            BindingContext = _vm;

            Title = $"{AppResource.AppName} - {AppResource.MusicAlbumButton.ToUpperFirst()}";
            txtSearchName.PlaceholderText = AppResource.MusicAlbumButton;

            _vm.PlayerReady += ViewModel_PlayerReady;
            _vm.ShowInterstitial += ViewModel_ShowInterstitial;
            _vm.AppErrorEvent += ViewModel_AppErrorEvent;

            _musicSearchType = MusicSearchType.SearchAlbum;
            _unityContainer = unityContainer;

            _showInterstitialOnAppearing = false;
            _formLoaded = false;
        }
        private async Task CheckAndRequestLocalStoragePermission(IDatabaseConn database)
        {
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (statusWrite != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (statusRead != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            await SaveLocalSqlFileDatabase(_database);
        }
        private async Task SaveLocalSqlFileDatabase(IDatabaseConn database)
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (statusRead == PermissionStatus.Granted && statusWrite == PermissionStatus.Granted)
            {
                if (!database.DatabaseExists)
                    await database.CreateDatabaseIfNotExists();
            }
            //else
                //btnSalvos.IsEnabled = false;
        }
        private async void AlbumSelection_Button_Clicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            SearchMusicModel playlistItem = (SearchMusicModel)btn.CommandParameter;

            _vm.MusicPlayer.HideBottomPlayer();
            _vm.MusicPlayer.StopBottomPlayer();

            //await Navigation.PushModalAsync(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel(playlistItem), _vm.AlbumPlayedHistory?.ToList()));
        }
        private async void SearchMusictActionButton_Clicked(object sender, EventArgs e)
        {
            if (!_vm.MenuActionsEnabled)
                return;

            _vm.ClearSavedAlbumPlaylistLoaded();
            _vm.ClearSavedMusicPlaylistLoaded();

            View frmContent = frmSelecaoTipo;
            StackLayout frmStacklayout = (StackLayout)((Grid)frmContent).Children[0];

            Button btnClicked = (Button)sender;
            Title = string.Concat($"{AppResource.AppName} - ", Regex.Replace(btnClicked.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper()));

            IList<View> lstChildren = frmStacklayout.Children;

            foreach (View view in lstChildren)
            {
                //PancakeView pcv = (PancakeView)view;
                //Button frmButton = (Button)pcv.Children[0];

                //frmButton.BorderColor = Color.Default;
                //frmButton.BorderWidth = 0;

                //if (string.Equals(frmButton.Id, btnClicked.Id))
                //{
                    //frmButton.BorderColor = Color.DarkOrange;
                    //frmButton.BorderWidth = 2;

                    //continue;
                //}
            }

            switch (btnClicked.StyleId.ToUpper())
            {
                case "BTNALBUM":
                    txtSearchName.PlaceholderText = AppResource.MusicAlbumButton;

                    await lstvMusicPlaylist.FadeTo(0, 200);
                    lstvMusicPlaylist.IsVisible = false;

                    lstvSavedAlbumPlaylist.IsVisible = false;
                    lstvSavedMusicPlaylist.IsVisible = false;
                    grdSearchSavedForm.IsVisible = false;

                    await lstvAlbumPlaylist.FadeTo(1, 200);
                    lstvAlbumPlaylist.IsVisible = true;

                    await stlMusicSearch.FadeTo(1, 200);
                    stlMusicSearch.IsVisible = true;

                    _musicSearchType = MusicSearchType.SearchAlbum;
                    break;
                case "BTNMUSIC":
                    txtSearchName.PlaceholderText = AppResource.MusicMusicButton;

                    await lstvAlbumPlaylist.FadeTo(0, 200);
                    lstvAlbumPlaylist.IsVisible = false;

                    lstvSavedAlbumPlaylist.IsVisible = false;
                    grdSearchSavedForm.IsVisible = false;
                    lstvSavedMusicPlaylist.IsVisible = false;

                    await lstvMusicPlaylist.FadeTo(1, 200);
                    lstvMusicPlaylist.IsVisible = true;

                    await stlMusicSearch.FadeTo(1, 200);
                    stlMusicSearch.IsVisible = true;

                    _musicSearchType = MusicSearchType.SearchMusic;
                    break;
                case "BTNSALVOS":
                    _vm.MenuActionsEnabled = false;
                    LoadSavedMusicData();
                    break;
            }
        }
        private void ViewCellAlbumPlaylist_Tapped(object sender, EventArgs e)
        {
            SearchMusicModel searchMusic = (SearchMusicModel)((TappedEventArgs)e).Parameter;
            SearchPlaylistViewModel bindingContext = (SearchPlaylistViewModel)BindingContext;

            if (_lastGridAlbumSelected != null && _searchAlbumLastSelected != null)
            {
                Grid grd = (Grid)_lastGridAlbumSelected;        

                grd.Background = new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 1),
                    GradientStops = new Xamarin.Forms.GradientStopCollection()
                    {
                        new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#F7F9FC"), Offset = 0.5f  },
                        new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#F7F9FC"), Offset = 1f  }
                    }
                };

                //PancakeView pk = (PancakeView)grd.Children[2];
                //pk.IsVisible = false;

                ////PancakeView pkLabel = (PancakeView)grd.Children[0];

                //Label lblImg = (Label)pkLabel.Children[0];
                //lblImg.TextColor = Color.Default;

                Label lbl = (Label)grd.Children[1];
                lbl.TextColor = Color.Default;

                _searchAlbumLastSelected.IconMusicStatusVisible = false;
            }

            Grid grid = (Grid)sender;

            if (grid.Id == _lastGridAlbumSelected?.Id)
            {
                _lastGridAlbumSelected = null;
                return;
            }

            grid.Background = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 1),
                GradientStops = new Xamarin.Forms.GradientStopCollection()
                {
                    new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#4987E5"), Offset = 0.5f  },
                    new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#7C97E3"), Offset = 1.0f  }
                }
            };

            //PancakeView pancake = (PancakeView)grid.Children[2];
            //pancake.IsVisible = true;

            //PancakeView pancakeLabel = (PancakeView)grid.Children[0];

            //Label labelImg = (Label)pancakeLabel.Children[0];
            //labelImg.TextColor = Color.White;

            Label label = (Label)grid.Children[1];
            label.TextColor = Color.White;

            //bindingContext.MusicIconType(searchMusic);

            searchMusic.IconMusicStatusEnabled = true;
            searchMusic.IconMusicStatusVisible = true;

            _lastGridAlbumSelected = grid;
            _searchAlbumLastSelected = searchMusic;
        }
        private async void ViewCellMusicPlaylist_Tapped(object sender, EventArgs e)
        {
            SearchMusicModel searchMusic = (SearchMusicModel)((TappedEventArgs)e).Parameter;
            SearchPlaylistViewModel bindingContext = (SearchPlaylistViewModel)BindingContext;

            if (_searchMusicLastSelected != null)
                if (_searchMusicLastSelected.Download.IsDownloading || _searchMusicLastSelected.IsDownloadMusicVisible)
                    return;

            if (_lastFrameMusicSelected != null && _searchMusicLastSelected != null)
            {
                Frame frm = (Frame)_lastFrameMusicSelected;

                //Grid grd = (Grid)frm.Content;
                //PancakeView pv = (PancakeView)grd.Children[0];
                //Label lbl = (Label)grd.Children[1];

                _searchMusicLastSelected.IconMusicStatusVisible = false;
                _searchMusicLastSelected.IconMusicDownloadVisible = false;
                _searchMusicLastSelected.IsDownloadMusicVisible = false;
                _searchMusicLastSelected.IsSelected = false;

                bindingContext.MusicPlayer.PlayPauseMusic((ICommonMusicModel)_searchMusicLastSelected);
            }

            Frame frame = (Frame)sender;

            if (frame.Id == _lastFrameMusicSelected?.Id)
            {
                _lastFrameMusicSelected = null;
                return;
            }

            //Grid grid = (Grid)frame.Content;
            //PancakeView pView = (PancakeView)grid.Children[0];
            //Label label = (Label)grid.Children[1];

            //_pvImageSelected = (Image)pView.Content;
            //_pvImageSelected.Source = null;

            searchMusic.IconMusicStatusEnabled = true;
            searchMusic.IconMusicStatusVisible = true;
            searchMusic.IsSelected = true;

            _lastFrameMusicSelected = frame;
            _searchMusicLastSelected = searchMusic;

            //await bindingContext.MusicPlayer.PlayMusic((ICommonMusicModel)searchMusic);
        }
        private async void ViewCellPlusMusicPlaylist_Tapped(object sender, EventArgs e)
        {
            //PancakeView view = (PancakeView)sender;
            //Grid grid = (Grid)view.Parent.Parent.Parent;

            SearchMusicModel musicModel = (SearchMusicModel)((TappedEventArgs)e).Parameter;

            if (!musicModel.IsMusicOptionsVisible)
            {
                musicModel.IsMusicOptionsVisible = true;
                musicModel.CollectionMusicOptionSize = 100;
            }
            else
            {                
                musicModel.IsMusicOptionsVisible = false;
                musicModel.CollectionMusicOptionSize = 0;
            }
        }
        private async void CollectionViewMusicPlaylist_Changed(object sender, SelectionChangedEventArgs e)
        {
            SearchMusicModel searchMusic = e.CurrentSelection[0] as SearchMusicModel;

            if (searchMusic == null)
                return;

            SearchPlaylistViewModel bindingContext = (SearchPlaylistViewModel)BindingContext;

            if (_searchMusicLastSelected != null)
                if (_searchMusicLastSelected.Download.IsDownloading || _searchMusicLastSelected.IsDownloadMusicVisible)
                    return;

            if (_lastFrameMusicSelected != null && _searchMusicLastSelected != null)
            {
                Frame frm = (Frame)_lastFrameMusicSelected;

                frm.Background = new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 1),
                    GradientStops = new Xamarin.Forms.GradientStopCollection()
                    {
                        new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#F7F9FC"), Offset = 0.5f  },
                        new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#F7F9FC"), Offset = 1f  }
                    }
                };

                Grid grd = (Grid)frm.Content;
                Label lbl = (Label)grd.Children[1];

                lbl.TextColor = Color.White;

                _searchMusicLastSelected.IconMusicStatusVisible = false;
                _searchMusicLastSelected.IconMusicDownloadVisible = false;
                _searchMusicLastSelected.IsDownloadMusicVisible = false;

                bindingContext.MusicPlayer.PlayPauseMusic((ICommonMusicModel)_searchMusicLastSelected);
            }

            Frame frame = (Frame)sender;

            if (frame.Id == _lastFrameMusicSelected?.Id)
            {
                _lastFrameMusicSelected = null;
                return;
            }

            frame.Background = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 1),
                GradientStops = new Xamarin.Forms.GradientStopCollection()
                {
                    new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#4987E5"), Offset = 0.5f  },
                    new Xamarin.Forms.GradientStop() {  Color = Color.FromHex("#7C97E3"), Offset = 1.0f  }
                }
            };

            Grid grid = (Grid)frame.Content;

            Label label = (Label)grid.Children[1];

            label.TextColor = Color.Black;

            searchMusic.IconMusicStatusEnabled = true;
            searchMusic.IconMusicStatusVisible = true;

            _lastFrameMusicSelected = frame;
            _searchMusicLastSelected = searchMusic;

            //await bindingContext.MusicPlayer.PlayMusic((ICommonMusicModel)searchMusic);
        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            switch (_musicSearchType)
            {
                case MusicSearchType.SearchAlbum:
                    albumPlaylistHead.Text = Regex.Replace(txtSearchName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                    ((SearchPlaylistViewModel)BindingContext).SearchAlbumCommand.Execute(null);
                    break;
                case MusicSearchType.SearchMusic:
                    musicPlaylistHead.Text = Regex.Replace(txtSearchName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                    ((SearchPlaylistViewModel)BindingContext).SearchMusicCommand.Execute(null);
                    break;
            }
        }
        private void ImgbDownloadMusicVisible_Clicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            SearchMusicModel playlistItem = (SearchMusicModel)btn.CommandParameter;

            if (!playlistItem.IsDownloadMusicVisible)
            {
                CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                CrossMTAdmob.Current.ShowInterstitial();
            }

            ((SearchPlaylistViewModel)BindingContext).DownloadMusicVisibleCommand.Execute(playlistItem);
        }
        private void ImgbDownloadMusic_Clicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            SearchMusicModel playlistItem = (SearchMusicModel)btn.CommandParameter;

            ((SearchPlaylistViewModel)BindingContext).DownloadMusicCommand.Execute(playlistItem);
        }
        private async void GrdSearchSavedForm_Clicked(object sender, EventArgs e)
        {
            btnSearchSavedFormAlbum.BackgroundColor = Color.FromHex("#dedede");
            btnSearchSavedFormAlbum.TextColor = Color.Black;
            btnSearchSavedFormMusic.BackgroundColor = Color.FromHex("#dedede");
            btnSearchSavedFormMusic.TextColor = Color.Black;

            Button btn = (Button)sender;
            btn.BackgroundColor = Color.FromHex("#8e8e8e");
            btn.TextColor = Color.White;

            SearchPlaylistViewModel bindingContext = (SearchPlaylistViewModel)BindingContext;

            _vm.ClearSavedAlbumPlaylistLoaded();
            _vm.ClearSavedMusicPlaylistLoaded();

            switch (btn.StyleId.ToUpper())
            {
                case "BTNSEARCHSAVEDFORMALBUM":
                    txtSearchName.PlaceholderText = AppResource.MusicAlbumButton;

                    await lstvSavedMusicPlaylist.FadeTo(0);
                    lstvSavedMusicPlaylist.IsVisible = false;

                    await lstvSavedAlbumPlaylist.FadeTo(1);
                    lstvSavedAlbumPlaylist.IsVisible = true;

                    await bindingContext.AlbumPlaylistSearchFromDb();

                    _musicSearchType = MusicSearchType.SearchSavedAlbum;
                    _savedMusicDataSearchType = MusicSearchType.SearchSavedAlbum;

                    break;
                case "BTNSEARCHSAVEDFORMMUSIC":
                    txtSearchName.PlaceholderText = AppResource.MusicMusicButton;

                    await lstvSavedAlbumPlaylist.FadeTo(0);
                    lstvSavedAlbumPlaylist.IsVisible = false;

                    await lstvSavedMusicPlaylist.FadeTo(1);
                    lstvSavedMusicPlaylist.IsVisible = true;

                    await bindingContext.MusicPlaylistSearchFromDb();

                    _musicSearchType = MusicSearchType.SearchSavedMusic;
                    _savedMusicDataSearchType = MusicSearchType.SearchSavedMusic;

                    break;
            }
        }
        private void ViewModel_AppErrorEvent(int level, string msg)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Ops!", msg, "ok");
            });
        }
        private void ViewModel_ShowInterstitial(Action audioPlayerPlay)
        {
            if (_formIsVisible)
            {
                if (CrossMTAdmob.Current.IsInterstitialLoaded())
                {
                    CrossMTAdmob.Current.LoadInterstitial(App.AppConfig.AdMob.AdsIntersticialAlbum);
                    CrossMTAdmob.Current.ShowInterstitial();
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
            if (_pvImageSelected != null)
                _pvImageSelected.Source = new FontImageSource() { Glyph = TocaTudoPlayer.Xamarim.Icon.Stop, Size = 35, Color = Color.Black, FontFamily = "FontAwesomeBold" };
        }
        private async void LoadSavedMusicData()
        {
            if (grdSearchSavedForm.IsVisible)
            {
                _vm.MenuActionsEnabled = true;
                return;
            }

            lstvSavedAlbumPlaylist.IsVisible = false;
            lstvSavedMusicPlaylist.IsVisible = false;

            await stlMusicSearch.FadeTo(0, 200);
            stlMusicSearch.IsVisible = false;

            await lstvAlbumPlaylist.FadeTo(0, 200);
            lstvAlbumPlaylist.IsVisible = false;

            await lstvMusicPlaylist.FadeTo(0, 200);
            lstvMusicPlaylist.IsVisible = false;

            if (_savedMusicDataSearchType == MusicSearchType.SearchSavedMusic)
            {
                await lstvSavedMusicPlaylist.FadeTo(1, 200);
                lstvSavedMusicPlaylist.IsVisible = true;

                await _vm.MusicPlaylistSearchFromDb();
            }
            else
            {
                await lstvSavedAlbumPlaylist.FadeTo(1, 200);
                lstvSavedAlbumPlaylist.IsVisible = true;

                await _vm.AlbumPlaylistSearchFromDb();
            }

            await grdSearchSavedForm.FadeTo(1, 200);
            grdSearchSavedForm.IsVisible = true;

            _vm.MenuActionsEnabled = true;
        }
        private void TxtSearchName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            //if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            //{
            //    _vm.MusicSearchedName = sender.Text;

            //    if (sender.Text.Length >= 2)
            //    {
            //        List<string> lstFilters = sender.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            //                                             .ToList();

            //        if (lstFilters.Count() > 1 && _vm.UserSearchHistory != null)
            //        {
            //            switch (_musicSearchType)
            //            {
            //                case MusicSearchType.SearchAlbum:
            //                    sender.ItemsSource = _vm.FilterAlbumUserSearchHistory(lstFilters);
            //                    break;
            //                case MusicSearchType.SearchMusic:
            //                    sender.ItemsSource = _vm.FilterMusicUserSearchHistory(lstFilters);
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            switch (_musicSearchType)
            //            {
            //                case MusicSearchType.SearchAlbum:
            //                    sender.ItemsSource = _vm.FilterAlbumUserSearchHistory(sender.Text);
            //                    break;
            //                case MusicSearchType.SearchMusic:
            //                    sender.ItemsSource = _vm.FilterMusicUserSearchHistory(sender.Text);
            //                    break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        sender.ItemsSource = null;
            //    }
            //}
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
                await CheckAndRequestLocalStoragePermission(_database);
                //await Task.WhenAll(_vm.LoadAlbumUserSearchHistory(), _vm.LoadMusicUserSearchHistory(), _vm.LoadUserMusicPlayedHistory(), _vm.LoadMusicAlbumPlaylistSelected());

                _vm.LoadAlbumMusicSavedSelect();

                _formLoaded = true;
            }

            _vm.MusicPlayer.ActiveBottomPlayer();
            //await _vm.LoadUserAlbumPlayedHistory();

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
                stlMusicSearch.IsVisible = false;
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