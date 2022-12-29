using Microsoft.Extensions.DependencyInjection;
using System;
using TocaTudoPlayer.Xamarim.Model.DB.Cosmos;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicAlbumConfigPopup : Popup
    {
        private WeakEventManager<(int AlbumId, string Text)> _alertDeleteAlbumPopup;
        private CommonMusicPageViewModel _vm;
        public MusicAlbumConfigPopup()
        {
            InitializeComponent();

            _alertDeleteAlbumPopup = new WeakEventManager<(int AlbumId, string Text)>();
            _vm = App.Services.GetRequiredService<CommonMusicPageViewModel>();
        }
        public event EventHandler<(int AlbumId, string Text)> AlertDeleteAlbumPopup
        {
            add => _alertDeleteAlbumPopup.AddEventHandler(value);
            remove => _alertDeleteAlbumPopup.RemoveEventHandler(value);
        }
        private async void DeleteAlbumButton_Clicked(object sender, EventArgs e)
        {
            SelectModel albumSelected = pkAlbumSelect.SelectedItem as SelectModel;
            if (albumSelected != null)
            {
                _alertDeleteAlbumPopup.RaiseEvent(this, (albumSelected.Id, AppResource.PopupDeleteAlbumAlert.Replace("##", albumSelected.Value).Replace("#", (await _vm.MusicAlbumTotalPlaylist(albumSelected.Id)).ToString())), nameof(AlertDeleteAlbumPopup));
            }
        }
    }
}