using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Interface;
using TocaTudoPlayer.Xamarim.Resources;
using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Saved : TabbedPage
    {
        private readonly IUnityContainer _unityContainer;
        private ISearchPlaylistViewModel _vm;
        public Saved(IUnityContainer unityContainer)
        {
            InitializeComponent();

            _unityContainer = unityContainer;

            tbpSaved.Children.Add(new NavigationPage(new SavedAlbum(_unityContainer)) { Title = "Álbum" });
            tbpSaved.Children.Add(new NavigationPage(new SavedMusic(_unityContainer)) { Title = "Music" });
        }
        private async void SearchMusictActionButton_Clicked(object sender, EventArgs e)
        {

        }
        private void ImgbDownloadMusicVisible_Clicked(object sender, EventArgs e)
        {

        }
        private void TxtSearchName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {

        }
        private void TxtSearchName_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {

        }
        private async void ViewCellMusicPlaylist_Tapped(object sender, EventArgs e)
        {

        }
        private void BtnSearch_Clicked(object sender, EventArgs e)
        {

        }
        private void ViewCellAlbumPlaylist_Tapped(object sender, EventArgs e)
        {

        }
        private async void AlbumSelection_Button_Clicked(object sender, EventArgs e)
        {

        }
    }
}