using System;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicAlbumSetupPopup : Popup
    {
        private WeakEventManager<(string Message, ICommonMusicModel Model)> _deleteMusicInvoked;
        private WeakEventManager<(string Message, ICommonMusicModel Model)> _deleteMusicFromAlbumInvoked;
        public MusicAlbumSetupPopup()
        {
            InitializeComponent();

            _deleteMusicInvoked = new WeakEventManager<(string Message, ICommonMusicModel Model)>();
            _deleteMusicFromAlbumInvoked = new WeakEventManager<(string Message, ICommonMusicModel Model)>();

            Opened += MusicAlbumConfigPopup_Opened;
        }
        public event EventHandler<(string Message, ICommonMusicModel Model)> DeleteMusicInvoked 
        {
            add => _deleteMusicInvoked.AddEventHandler(value);
            remove => _deleteMusicInvoked.RemoveEventHandler(value);
        }
        public event EventHandler<(string Message, ICommonMusicModel Model)> DeleteMusicFromAlbumInvoked
        {
            add => _deleteMusicFromAlbumInvoked.AddEventHandler(value);
            remove => _deleteMusicFromAlbumInvoked.RemoveEventHandler(value);
        }
        private void MusicAlbumConfigPopup_Opened(object sender, PopupOpenedEventArgs e)
        {
            ICommonMusicModel musicModel = (ICommonMusicModel)BindingContext;

            int formHeight = 110;
            
            if (!musicModel.IsSavedOnLocalDb)
            {
                grdSetupPopup.RowDefinitions[1].Height = 0;
                formHeight -= 40;
            }
            if (!musicModel.MusicAlbumPopupModel.AlbumModeIsVisible && !musicModel.MusicAlbumPopupModel.SavedAlbumModeIsVisible)
            {
                grdSetupPopup.RowDefinitions[2].Height = 0;
                formHeight -= 40;
            }

            Size = new Size(350, formHeight);
        }
        private void ButtonDeleteMusic_Clicked(object sender, EventArgs e)//Is not possible call alert box from here
        {
            ICommonMusicModel musicModel = (ICommonMusicModel)((Button)sender).CommandParameter;
            _deleteMusicInvoked.RaiseEvent(this, (AppResource.PopupDeleteMusicAlert.Replace("#", musicModel.MusicName), musicModel), nameof(DeleteMusicInvoked));
        }
        private void ButtonDeleteMusicFromAlbum_Clicked(object sender, EventArgs e)//Is not possible call alert box from here
        {
            ICommonMusicModel musicModel = (ICommonMusicModel)((Button)sender).CommandParameter;
            _deleteMusicFromAlbumInvoked.RaiseEvent(this, (AppResource.PopupDeleteMusicFromAlbum.Replace("#", musicModel.MusicAlbumPopupModel?.AlbumMusicSavedSelected?.Value), musicModel), nameof(DeleteMusicFromAlbumInvoked));
        }
    }
}