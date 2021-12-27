using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.CustomControls.Entries;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicSelectedActions : Frame
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(propertyName: nameof(ViewModel), returnType: typeof(IMusicPageViewModel), declaringType: typeof(IMusicPageViewModel));
        private Button _btnEditAlbum;
        private Button _btnDownloadAlbum;
        public MusicSelectedActions()
        {
            InitializeComponent();

            BindingContext = this;
        }
        public IMusicPageViewModel ViewModel
        {
            get
            {
                return (IMusicPageViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }
        private void ViewCellPlusMusicPlaylistAddAlbum_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;

            musicModel.UpdateMusicUtilsAddAlbumIconAndForm();
        }
        private async void ViewCellPlusMusicPlaylistNewAlbumName_Clicked(object sender, EventArgs e)
        {
            ImageButton imgAddAlbum = (ImageButton)sender;
            FloatingLabelEntry labelAddAlbum = (FloatingLabelEntry)((StackLayout)imgAddAlbum.Parent).Children[0];
            SearchMusicModel sMusicModel = (SearchMusicModel)imgAddAlbum.CommandParameter;

            string albumName = Regex.Replace(txtAlbumName.Text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            bool musicAlbumExists = await ViewModel.CommonMusicPageViewModel.ExistsMusicAlbumPlaylist(albumName, sMusicModel);

            if (!musicAlbumExists)
            {
                await ViewModel.CommonMusicPageViewModel.InsertMusicAlbumPlaylistSelected(albumName, sMusicModel);
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

                musicModel.MusicAlbumEditFormIsVisible = true;
                musicModel.CollectionMusicOptionSize = 140;
            }
            else
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.BorderColor = Color.Default;
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                musicModel.MusicAlbumEditFormIsVisible = false;
                musicModel.CollectionMusicOptionSize = 90;
            }
        }
        private void ViewCellPlusMusicPlaylistDownloadMusic_Clicked(object sender, EventArgs e)
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
                musicModel.CollectionMusicOptionSize = 138;
            }
            else
            {
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.BorderColor = Color.Default;
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                musicModel.MusicDetailsFormDownloadIsVisible = false;
                musicModel.CollectionMusicOptionSize = 92;
            }
        }
        private void ViewCellPlusMusicPlaylistUpdateAlbumName_Clicked(object sender, EventArgs e)
        {
            SearchMusicModel musicModel = (SearchMusicModel)((Button)sender).CommandParameter;
            //ViewModel.UpdateMusicAlbumPlaylistSelected(musicModel.AlbumMusicSavedSelected.Id, musicModel.AlbumMusicSavedSelected.Value, musicModel);
            if(musicModel.AlbumMusicSavedSelected != null)
                ViewModel.CommonMusicPageViewModel.InsertMusicAlbumPlaylistSelected(musicModel.AlbumMusicSavedSelected.Value, musicModel);
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
            ViewModel.CommonMusicPageViewModel.DeleteMusicAlbumPlaylistSelected(musicModel);
        }
    }
}