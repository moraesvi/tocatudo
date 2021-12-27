using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit;
using System;
using Xamarin.Forms.CustomControls.Entries;
using TocaTudoPlayer.Xamarim.Interface;
using System.Text.RegularExpressions;
using Xamarin.CommunityToolkit.Extensions;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicAlbumPopup : Xamarin.CommunityToolkit.UI.Views.Popup
    {
        private IMusicPageViewModel _viewModel;
        private Action<SearchMusicModel> _deleteActionAlbumPopup;
        public MusicAlbumPopup(IMusicPageViewModel viewModel)
        {

            _viewModel = viewModel;

            InitializeComponent();

            Opened += MusicAlbumPopup_Opened;
            Dismissed += MusicAlbumPopup_Dismissed;
        }
        public MusicAlbumPopup(IMusicSavedPageViewModel viewModel)
        {
            //_viewModel = viewModel;
            InitializeComponent();
        }
        private void MusicAlbumPopup_Opened(object sender, Xamarin.CommunityToolkit.UI.Views.PopupOpenedEventArgs e)
        {
            Load();
        }
        private void MusicAlbumPopup_Dismissed(object sender, Xamarin.CommunityToolkit.UI.Views.PopupDismissedEventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((MusicAlbumPopup)sender).BindingContext;
            musicModel.UpdateMusicDetailsFormAlbumIcon(false);
        }
        public event Action<SearchMusicModel> DeleteActionAlbumPopup
        {
            add => _deleteActionAlbumPopup += value;
            remove => _deleteActionAlbumPopup -= value;
        }
        public IMusicPageViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
            }
        }
        private void Load()
        {
            SearchMusicModel musicModel = BindingContext as SearchMusicModel;

            _viewModel.CommonMusicPageViewModel.InitFormMusicUtils(musicModel);
            musicModel.UpdateMusicDetailsFormAlbumIcon(true);

            if (musicModel.AlbumModeIsVisible)
            {
                musicPopup.Size = new Size(350, 113);
            }
            else
            {
                musicPopup.Size = new Size(350, 70);
            }
        }
        private void ViewCellPlusMusicPlaylistAddAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnEditAlbum = (Button)sender;
            SearchMusicModel musicModel = (SearchMusicModel)btnEditAlbum.CommandParameter;

            Button btnDownloadAlbum = (Button)((StackLayout)btnEditAlbum.Parent).Children[1];

            if (!musicModel.MusicAlbumEditFormIsVisible)
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                btnDownloadAlbum.FontAttributes = FontAttributes.None;
                btnDownloadAlbum.Opacity = 0.7;

                musicModel.MusicAlbumEditFormIsVisible = true;
                musicPopup.Size = new Size(350, 113);
            }
            else
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.BorderColor = Color.Default;
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                musicModel.MusicAlbumEditFormIsVisible = false;
                musicPopup.Size = new Size(350, 70);
            }

            musicModel.UpdateMusicUtilsAddAlbumIconAndForm();
        }
        private async void ViewCellPlusMusicPlaylistNewAlbumName_Clicked(object sender, EventArgs e)
        {
            ImageButton imgAddAlbum = (ImageButton)sender;
            Entry labelAddAlbum = (Entry)((StackLayout)imgAddAlbum.Parent).Children[0];
            SearchMusicModel sMusicModel = (SearchMusicModel)imgAddAlbum.CommandParameter;

            string albumName = Regex.Replace(string.IsNullOrEmpty(txtAlbumName.Text) ? txtAlbumNameNovo.Text : txtAlbumName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            bool musicAlbumExists = await _viewModel.CommonMusicPageViewModel.ExistsMusicAlbumPlaylist(albumName, sMusicModel);

            if (!musicAlbumExists)
            {
                await _viewModel.CommonMusicPageViewModel.InsertMusicAlbumPlaylistSelected(albumName, sMusicModel);
                Dismiss(this);
            }
        }
        private void ViewCellPlusMusicPlaylistOpenCloseAddAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnOpenCloseAlbum = (Button)sender;
            SearchMusicModel sMusicModel = (SearchMusicModel)btnOpenCloseAlbum.CommandParameter;

            if (btnOpenCloseAlbum.FontAttributes == FontAttributes.None)
            {
                btnOpenCloseAlbum.FontAttributes = FontAttributes.Bold;
                btnOpenCloseAlbum.ImageSource = new FontImageSource() { Glyph = TocaTudoPlayer.Xamarim.Icon.MinusCircle, Size = 18, Color = Color.Red, FontFamily = "FontAwesomeBold" };

                sMusicModel.MusicDetailsSelectAlbumIsVisible = false;
                sMusicModel.MusicDetailsAddAlbumIsVisible = true;
            }
            else
            {
                btnOpenCloseAlbum.FontAttributes = FontAttributes.None;
                btnOpenCloseAlbum.ImageSource = new FontImageSource() { Glyph = TocaTudoPlayer.Xamarim.Icon.PlusCircle, Size = 18, Color = Color.Green, FontFamily = "FontAwesomeBold" };

                sMusicModel.MusicDetailsSelectAlbumIsVisible = true;
                sMusicModel.MusicDetailsAddAlbumIsVisible = false;
            }
        }
        private void ViewCellPlusMusicPlaylistEditAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnEditAlbum = (Button)sender;
            SearchMusicModel musicModel = (SearchMusicModel)btnEditAlbum.CommandParameter;

            Button btnDownloadAlbum = (Button)((StackLayout)btnEditAlbum.Parent).Children[1];

            if (!musicModel.MusicAlbumEditFormIsVisible)
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                btnDownloadAlbum.FontAttributes = FontAttributes.None;
                btnDownloadAlbum.Opacity = 0.7;

                musicModel.MusicDetailsFormDownloadIsVisible = false;
                musicModel.MusicAlbumEditFormIsVisible = true;
                musicModel.CollectionMusicOptionSize = 140;

                musicPopup.Size = new Size(350, 150);
            }
            else
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.BorderColor = Color.Default;
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                musicModel.MusicAlbumEditFormIsVisible = false;
                musicModel.CollectionMusicOptionSize = 90;

                musicPopup.Size = new Size(350, 113);
            }
        }
        private void ViewCellPlusMusicPlaylistDownloadMusicForm_Clicked(object sender, EventArgs e)
        {
            Button btnDownloadAlbum = (Button)sender;
            SearchMusicModel musicModel = (SearchMusicModel)btnDownloadAlbum.CommandParameter;

            Button btnEditAlbum = (Button)((StackLayout)btnDownloadAlbum.Parent).Children[0];

            musicModel.MusicAlbumEditFormIsVisible = false;

            if (!musicModel.MusicDetailsFormDownloadIsVisible)
            {
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                btnEditAlbum.FontAttributes = FontAttributes.None;
                btnEditAlbum.Opacity = 0.7;

                musicModel.MusicDetailsFormDownloadIsVisible = true;
                //musicModel.CollectionMusicOptionSize = 138;

                if (musicModel.AlbumModeDetailsIsVisible)
                {
                    musicPopup.Size = new Size(350, 150);
                    Grid.SetRow(grdDownload, 3);
                }
                else
                {
                    musicPopup.Size = new Size(350, 113);
                    Grid.SetRow(grdDownload, 2);
                }
            }
            else
            {
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.BorderColor = Color.Default;
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                musicModel.MusicDetailsFormDownloadIsVisible = false;

                if (musicModel.AlbumModeDetailsIsVisible)
                {
                    musicPopup.Size = new Size(350, 110);
                }
                else
                {
                    musicPopup.Size = new Size(350, 70);
                }

                Grid.SetRow(grdDownload, 2);
            }

            musicModel.ReloadMusicUtilsAddAlbumIcon();
        }
        private async void ViewCellPlusMusicPlaylistUpdateAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;
            if (musicModel.AlbumMusicSavedSelected != null)
            {
                await _viewModel.CommonMusicPageViewModel.UpdateMusicAlbumPlaylistSelected(musicModel.AlbumMusicSavedSelected.Id, musicModel.AlbumMusicSavedSelected.Value, musicModel);
                Dismiss(this);
            }
        }
        private void ViewCellPlusMusicPlaylistSelectAlbumBack_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((ImageButton)sender).CommandParameter;

            musicModel.MusicDetailsSelectAlbumIsVisible = true;
            musicModel.MusicDetailsAddAlbumIsVisible = false;
        }
        private void ViewCellPlusMusicPlaylistAddAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;
            musicModel.MusicDetailsSelectAlbumIsVisible = false;
            musicModel.MusicDetailsAddAlbumIsVisible = true;
        }
        private void ViewCellPlusMusicPlaylistDeleteAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            _deleteActionAlbumPopup(musicModel);
        }
        private async void ViewCellPlusMusicPlaylistDownloadMusic_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            await _viewModel.StartDownloadMusicCommand.ExecuteAsync(musicModel);
        }
    }
}