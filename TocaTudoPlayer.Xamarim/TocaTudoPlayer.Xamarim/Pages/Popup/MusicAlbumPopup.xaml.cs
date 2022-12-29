using System;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicAlbumPopup : Popup
    {
        private WeakEventManager<MusicModelBase> _loadPopup;
        private WeakEventManager<MusicModelBase> _downloadButtonClicked;
        private WeakEventManager<(string AlbumName, MusicModelBase MusicModel)> _newAlbumNameClicked;
        private WeakEventManager<MusicModelBase> _updateAlbumNameClicked;
        private WeakEventManager<string> _alertActionAlbumPopup;
        private WeakEventManager<MusicModelBase> _deleteActionAlbumPopup;
        private WeakEventManager<(string Message, ICommonMusicModel Model)> _setupPopupDeleteMusicInvoked;
        private WeakEventManager<(string Message, ICommonMusicModel Model)> _setupPopupDeleteMusicFromAlbumInvoked;
        private MusicAlbumSetupPopup _musicAlbumSetupPopup;
        private bool _albumFormIsOpened;
        private int _albumSavedMode;
        public MusicAlbumPopup()
        {
            InitializeComponent();

            _loadPopup = new WeakEventManager<MusicModelBase>();
            _downloadButtonClicked = new WeakEventManager<MusicModelBase>();
            _newAlbumNameClicked = new WeakEventManager<(string, MusicModelBase)>();
            _updateAlbumNameClicked = new WeakEventManager<MusicModelBase>();
            _alertActionAlbumPopup = new WeakEventManager<string>();
            _deleteActionAlbumPopup = new WeakEventManager<MusicModelBase>();
            _setupPopupDeleteMusicInvoked = new WeakEventManager<(string Message, ICommonMusicModel Model)>();
            _setupPopupDeleteMusicFromAlbumInvoked = new WeakEventManager<(string Message, ICommonMusicModel Model)>();

            Opened += MusicAlbumPopup_Opened;
            Dismissed += MusicAlbumPopup_Dismissed;
        }
        public event EventHandler<MusicModelBase> LoadPopup
        {
            add => _loadPopup.AddEventHandler(value);
            remove => _loadPopup.RemoveEventHandler(value);
        }
        public event EventHandler<MusicModelBase> DownloadButtonClicked
        {
            add => _downloadButtonClicked.AddEventHandler(value);
            remove => _downloadButtonClicked.RemoveEventHandler(value);
        }
        public event EventHandler<(string AlbumName, MusicModelBase MusicModel)> NewAlbumNameClicked
        {
            add => _newAlbumNameClicked.AddEventHandler(value);
            remove => _newAlbumNameClicked.RemoveEventHandler(value);
        }
        public event EventHandler<MusicModelBase> UpdateAlbumNameClicked
        {
            add => _updateAlbumNameClicked.AddEventHandler(value);
            remove => _updateAlbumNameClicked.RemoveEventHandler(value);
        }
        public event EventHandler<string> AlertActionAlbumPopup
        {
            add => _alertActionAlbumPopup.AddEventHandler(value);
            remove => _alertActionAlbumPopup.RemoveEventHandler(value);
        }
        public event EventHandler<MusicModelBase> DeleteActionAlbumPopup
        {
            add => _deleteActionAlbumPopup.AddEventHandler(value);
            remove => _deleteActionAlbumPopup.RemoveEventHandler(value);
        }
        public event EventHandler<(string Mesage, ICommonMusicModel Model)> SetupPopupDeleteMusicInvoked
        {
            add => _setupPopupDeleteMusicInvoked.AddEventHandler(value);
            remove => _setupPopupDeleteMusicInvoked.RemoveEventHandler(value);
        }
        public event EventHandler<(string Mesage, ICommonMusicModel Model)> SetupPopupDeleteMusicFromAlbumInvoked
        {
            add => _setupPopupDeleteMusicFromAlbumInvoked.AddEventHandler(value);
            remove => _setupPopupDeleteMusicFromAlbumInvoked.RemoveEventHandler(value);
        }
        private void MusicAlbumPopup_Opened(object sender, PopupOpenedEventArgs e)
        {
            App.EventTracker.SendScreenView("MusicAlbumPopup", nameof(MusicAlbumPopup));
            Load();
        }
        private void MusicAlbumPopup_Dismissed(object sender, PopupDismissedEventArgs e)
        {
            MusicModelBase musicModel = ((MusicAlbumDialogModel)((MusicAlbumPopup)sender).BindingContext).MusicModel;

            musicModel.MusicAlbumPopupModel.Reload();
            musicModel.MusicAlbumPopupModel.MusicAlbumModel.ShowFormAlbumMusicDetailsCommand.Execute(null);

            _musicAlbumSetupPopup?.Dismiss(_musicAlbumSetupPopup);
        }
        private void ViewCellPlusMusicPlaylistAddAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnEditAlbum = (Button)sender;
            MusicAlbumDialogDataModel musicModel = (MusicAlbumDialogDataModel)btnEditAlbum.CommandParameter;

            Button btnDownloadAlbum = (Button)((StackLayout)btnEditAlbum.Parent).Children[1];

            const int sizeWindowClosed = 70;    
            const int sizeWindowOpened = 113;

            if (musicPopup.Size.Height <= sizeWindowClosed)
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                btnDownloadAlbum.FontAttributes = FontAttributes.None;
                btnDownloadAlbum.Opacity = 0.7;

                musicPopup.Size = new Size(350, sizeWindowOpened);

                musicModel.UpdateAddAlbumIconAndForm();
            }
            else
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.BorderColor = Color.Default;
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                musicPopup.Size = new Size(350, sizeWindowClosed);

                musicModel.UpdateAddAlbumIconAndForm(reload: true);
            }
        }
        private void ViewCellPlusMusicPlaylistNewAlbumName_Clicked(object sender, EventArgs e)
        {
            ImageButton imgAddAlbum = (ImageButton)sender;
            Entry labelAddAlbum = (Entry)((StackLayout)imgAddAlbum.Parent).Children[0];
            MusicModelBase musicModel = (MusicModelBase)imgAddAlbum.CommandParameter;

            string albumName = string.IsNullOrEmpty(txtAlbumName.Text) ? string.IsNullOrEmpty(txtAlbumNameNovo.Text) ? string.Empty
                                                                                                                     : txtAlbumNameNovo.Text
                                                                       : txtAlbumName.Text;

            if (string.IsNullOrWhiteSpace(albumName))
            {
                _alertActionAlbumPopup.RaiseEvent(this, AppResource.PopupAlertAlbumNameInsert, nameof(AlertActionAlbumPopup));
                return;
            }

            _newAlbumNameClicked.RaiseEvent(this, (albumName.ToUpperFirst(), musicModel), nameof(NewAlbumNameClicked));
        }
        private void ViewCellPlusMusicPlaylistOpenCloseAddAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnOpenCloseAlbum = (Button)sender;
            MusicAlbumDialogDataModel sMusicModel = (MusicAlbumDialogDataModel)btnOpenCloseAlbum.CommandParameter;

            if (btnOpenCloseAlbum.FontAttributes == FontAttributes.None)
            {
                btnOpenCloseAlbum.FontAttributes = FontAttributes.Bold;
                btnOpenCloseAlbum.ImageSource = new FontImageSource() { Glyph = Icon.MinusCircle, Size = 18, FontFamily = "FontAwesomeBold" };

                sMusicModel.SelectAlbumIsVisible = false;
                sMusicModel.AddAlbumIsVisible = true;
            }
            else
            {
                btnOpenCloseAlbum.FontAttributes = FontAttributes.None;
                btnOpenCloseAlbum.ImageSource = new FontImageSource() { Glyph = Icon.PlusCircle, Size = 18, FontFamily = "FontAwesomeBold" };

                sMusicModel.SelectAlbumIsVisible = true;
                sMusicModel.AddAlbumIsVisible = false;
            }
        }
        private void ViewCellPlusMusicPlaylistEditAlbum_Clicked(object sender, EventArgs e)
        {
            Button btnEditAlbum = (Button)sender;
            MusicAlbumDialogDataModel popupModel = (MusicAlbumDialogDataModel)btnEditAlbum.CommandParameter;

            Button btnDownloadAlbum = (Button)((StackLayout)btnEditAlbum.Parent).Children[1];

            if (!popupModel.EditFormIsVisible)
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.Opacity = 1;

                btnDownloadAlbum.FontAttributes = FontAttributes.None;
                btnDownloadAlbum.Opacity = 0.7;

                popupModel.FormDownloadIsVisible = false;
                popupModel.EditFormIsVisible = true;

                musicPopup.Size = new Size(350, 150);
            }
            else
            {
                btnEditAlbum.FontAttributes = FontAttributes.Bold;
                btnEditAlbum.BorderColor = Color.Default;
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                popupModel.EditFormIsVisible = false;

                musicPopup.Size = new Size(350, 113);
            }
        }
        private void ViewCellPlusMusicPlaylistDownloadMusicForm_Clicked(object sender, EventArgs e)
        {
            Button btnDownloadAlbum = (Button)sender;

            MusicAlbumDialogDataModel popupModel = (MusicAlbumDialogDataModel)btnDownloadAlbum.CommandParameter;
            Button btnEditAlbum = (Button)((StackLayout)btnDownloadAlbum.Parent).Children[0];

            if (!popupModel.FormDownloadIsVisible)
            {
                btnDownloadAlbum.FontAttributes = FontAttributes.Bold;
                btnDownloadAlbum.Opacity = 1;

                btnEditAlbum.FontAttributes = FontAttributes.None;
                btnEditAlbum.Opacity = 0.7;

                popupModel.FormDownloadIsVisible = true;
                popupModel.EditFormIsVisible = false;

                if (popupModel.AlbumModeIsVisible || popupModel.SavedAlbumModeIsVisible)
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

                popupModel.FormDownloadIsVisible = false;

                if (popupModel.AlbumModeIsVisible || popupModel.SavedAlbumModeIsVisible)
                {
                    musicPopup.Size = new Size(350, 113);
                    Grid.SetRow(grdDownload, 2);
                }
                else
                {
                    musicPopup.Size = new Size(350, 70);
                }
            }

            popupModel.AddAlbumIsVisible = false;
            popupModel.ReloadAddAlbumIcon();
        }
        private void ViewCellPlusMusicPlaylistUpdateAlbumName_Clicked(object sender, EventArgs e)
        {
            MusicModelBase musicModel = (MusicModelBase)((Button)sender).CommandParameter;
            if (musicModel.MusicAlbumPopupModel.AlbumMusicSavedSelected != null)
            {
                _updateAlbumNameClicked.RaiseEvent(this, musicModel, nameof(UpdateAlbumNameClicked));
            }
        }
        private void ViewCellPlusMusicPlaylistSelectAlbumBack_Clicked(object sender, EventArgs e)
        {
            MusicAlbumDialogDataModel popupModel = (MusicAlbumDialogDataModel)((ImageButton)sender).CommandParameter;
            popupModel.SelectAlbumIsVisible = true;
            popupModel.AddAlbumIsVisible = false;
        }
        private void ViewCellPlusMusicPlaylistAddAlbumName_Clicked(object sender, EventArgs e)
        {
            MusicAlbumDialogDataModel popupModel = (MusicAlbumDialogDataModel)((Button)sender).CommandParameter;
            popupModel.SelectAlbumIsVisible = false;
            popupModel.AddAlbumIsVisible = true;
        }
        private void ViewCellPlusMusicPlaylistDeleteAlbumName_Clicked(object sender, EventArgs e)
        {
            MusicModelBase musicModel = (MusicModelBase)((Button)sender).CommandParameter;
            _deleteActionAlbumPopup.RaiseEvent(this, musicModel, nameof(DeleteActionAlbumPopup));
        }
        private void ViewCellPlusMusicPlaylistDownloadMusic_Clicked(object sender, EventArgs e)
        {
            MusicModelBase musicModel = (MusicModelBase)((Button)sender).CommandParameter;
            _downloadButtonClicked.RaiseEvent(this, musicModel, nameof(DownloadButtonClicked));
        }
        private async void ViewCellPlusMusicSetupPopup_Clicked(object sender, EventArgs e)
        {
            ICommonMusicModel musicModel = (ICommonMusicModel)((Button)sender).CommandParameter;

            _musicAlbumSetupPopup = new MusicAlbumSetupPopup() { BindingContext = musicModel };
            _musicAlbumSetupPopup.DeleteMusicInvoked += MusicAlbumSetupPopup_DeleteMusicInvoked;
            _musicAlbumSetupPopup.DeleteMusicFromAlbumInvoked += MusicAlbumSetupPopup_DeleteMusicFromAlbumInvoked;

            await Navigation.ShowPopupAsync(_musicAlbumSetupPopup);
        }
        private void MusicAlbumSetupPopup_DeleteMusicInvoked(object sender, (string Message, ICommonMusicModel Model) tupple)
        {
            _setupPopupDeleteMusicInvoked.RaiseEvent(this, tupple, nameof(SetupPopupDeleteMusicInvoked));
        }
        private void MusicAlbumSetupPopup_DeleteMusicFromAlbumInvoked(object sender, (string Message, ICommonMusicModel Model) tupple)
        {
            _setupPopupDeleteMusicFromAlbumInvoked.RaiseEvent(this, tupple, nameof(SetupPopupDeleteMusicFromAlbumInvoked));
        }
        private void Load()
        {
            MusicModelBase musicModel = (BindingContext as MusicAlbumDialogModel).MusicModel;

            txtAlbumNameNovo.Text = string.Empty;
            txtAlbumName.Text = string.Empty;

            //_albumSavedMode = musicModel.IsSavedOnLocalDb ? 40 : 0;
            _albumSavedMode = 0;
            _albumFormIsOpened = false;

            musicModel.MusicAlbumPopupModel.MusicAlbumModel.ShowFormAlbumMusicDetailsCommand.Execute(null);

            if (musicModel.MusicAlbumPopupModel.AlbumModeIsVisible || musicModel.MusicAlbumPopupModel.SavedAlbumModeIsVisible)
            {
                musicPopup.Size = new Size(350, 113 + _albumSavedMode);
            }
            else
            {
                musicPopup.Size = new Size(350, 70 + _albumSavedMode);
            }

            _loadPopup.RaiseEvent(this, musicModel, nameof(LoadPopup));
        }
    }
}