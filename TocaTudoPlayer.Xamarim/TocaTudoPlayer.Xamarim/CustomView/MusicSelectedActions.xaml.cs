using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicSelectedActions : Frame
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(propertyName: nameof(ViewModel), returnType: typeof(MusicPageViewModel), declaringType: typeof(MusicPageViewModel));
        public MusicSelectedActions()
        {
            InitializeComponent();

            BindingContext = this;
        }
        public MusicPageViewModel ViewModel
        {
            get
            {
                return (MusicPageViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }
        private void ViewCellPlusMusicPlaylistAddAlbum_Clicked(object sender, EventArgs e)
        {
            MusicAlbumDialogDataModel popupModel = (MusicAlbumDialogDataModel)((Button)sender).CommandParameter;
            popupModel.UpdateAddAlbumIconAndForm();
        }
        private async void ViewCellPlusMusicPlaylistNewAlbumName_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAlbumName.Text))
                return;

            ImageButton imgAddAlbum = (ImageButton)sender;
            //FloatingLabelEntry labelAddAlbum = (FloatingLabelEntry)((StackLayout)imgAddAlbum.Parent).Children[0];
            SearchMusicModel sMusicModel = (SearchMusicModel)imgAddAlbum.CommandParameter;

            string albumName = Regex.Replace(txtAlbumName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            bool musicAlbumExists = await ViewModel.CommonMusicPageViewModel.ExistsMusicAlbumPlaylist(albumName, sMusicModel);

            if (!musicAlbumExists)
            {
                await ViewModel.CommonMusicPageViewModel.InsertOrUpdateMusicAlbumPlaylistSelected(albumName, sMusicModel);
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

                sMusicModel.MusicAlbumPopupModel.SelectAlbumIsVisible = false;
                sMusicModel.MusicAlbumPopupModel.AddAlbumIsVisible = true;
            }
            else
            {
                btnOpenCloseAlbum.FontAttributes = FontAttributes.None;
                btnOpenCloseAlbum.ImageSource = new FontImageSource() { Glyph = TocaTudoPlayer.Xamarim.Icon.PlusCircle, Size = 18, Color = Color.Green, FontFamily = "FontAwesomeBold" };

                sMusicModel.MusicAlbumPopupModel.SelectAlbumIsVisible = true;
                sMusicModel.MusicAlbumPopupModel.AddAlbumIsVisible = false;
            }
        }
        private void ViewCellPlusMusicPlaylistEditAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnEditAlbum = (Button)sender;
            SearchMusicModel musicModel = (SearchMusicModel)btnEditAlbum.CommandParameter;

            Button btnDownloadAlbum = (Button)((StackLayout)btnEditAlbum.Parent).Children[1];

            if (!musicModel.MusicAlbumPopupModel.EditFormIsVisible)
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                btnDownloadAlbum.FontAttributes = FontAttributes.None;
                btnDownloadAlbum.Opacity = 0.7;

                musicModel.MusicAlbumPopupModel.EditFormIsVisible = true;
                musicModel.CollectionMusicOptionSize = 140;
            }
            else
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.BorderColor = Color.Default;
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                musicModel.MusicAlbumPopupModel.EditFormIsVisible = false;
                musicModel.CollectionMusicOptionSize = 90;
            }
        }
        private void ViewCellPlusMusicPlaylistDownloadMusic_Clicked(object sender, EventArgs e)
        {
            Button btnDownloadAlbum = (Button)sender;
            SearchMusicModel musicModel = (SearchMusicModel)btnDownloadAlbum.CommandParameter;

            Button btnEditAlbum = (Button)((StackLayout)btnDownloadAlbum.Parent).Children[0];

            musicModel.MusicAlbumPopupModel.EditFormIsVisible = false;

            if (!musicModel.MusicAlbumPopupModel.FormDownloadIsVisible)
            {
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                btnEditAlbum.FontAttributes = FontAttributes.None;
                btnEditAlbum.Opacity = 0.7;

                musicModel.MusicAlbumPopupModel.FormDownloadIsVisible = true;
                musicModel.CollectionMusicOptionSize = 138;
            }
            else
            {
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.BorderColor = Color.Default;
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                musicModel.MusicAlbumPopupModel.FormDownloadIsVisible = false;
                musicModel.CollectionMusicOptionSize = 92;
            }
        }
        private void ViewCellPlusMusicPlaylistUpdateAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            if (musicModel.MusicAlbumPopupModel.AlbumMusicSavedSelected != null)
                ViewModel.CommonMusicPageViewModel.InsertOrUpdateMusicAlbumPlaylistSelected(musicModel.MusicAlbumPopupModel.AlbumMusicSavedSelected.Value, musicModel);
        }
        private void ViewCellPlusMusicPlaylistSelectAlbumBack_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((ImageButton)sender).CommandParameter;

            musicModel.MusicAlbumPopupModel.SelectAlbumIsVisible = true;
            musicModel.MusicAlbumPopupModel.AddAlbumIsVisible = false;
        }
        private void ViewCellPlusMusicPlaylistAddAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;
            musicModel.MusicAlbumPopupModel.SelectAlbumIsVisible = false;
            musicModel.MusicAlbumPopupModel.AddAlbumIsVisible = true;
        }
        private void ViewCellPlusMusicPlaylistDeleteAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;
            ViewModel.CommonMusicPageViewModel.DeleteMusicFromAlbumPlaylist(musicModel);
        }
    }
}