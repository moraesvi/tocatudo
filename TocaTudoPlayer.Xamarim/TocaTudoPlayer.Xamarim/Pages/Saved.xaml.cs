using TocaTudoPlayer.Xamarim.Resources;
//using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Saved : TabbedPage
    {
        public Saved()
        {
            InitializeComponent();

            Title = $"{AppResource.AppName} - {AppHelper.ToTitleCase(AppResource.MusicSavedButton)}";

            tbpSaved.Children.Add(new NavigationPage(new SavedMusic()) { Title = AppHelper.ToTitleCase(AppResource.MusicSavedButton) });
            //tbpSaved.Children.Add(new NavigationPage(new SavedAlbum()) { Title = "Álbum Salvo" });
        }
    }
}