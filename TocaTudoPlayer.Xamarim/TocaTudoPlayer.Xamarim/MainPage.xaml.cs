using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Pages;
using Unity;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
    {
        private readonly IDatabaseConn _database;
        private readonly IUnityContainer _unityContainer;
        private bool _formLoaded;
        public MainPage(IUnityContainer unityContainer)
        {
            InitializeComponent();

            _formLoaded = false;
            _unityContainer = unityContainer;
            _database = unityContainer.Resolve<IDatabaseConn>();
        }
        protected async override void OnAppearing()
        {
            if (!_formLoaded)
            {
                await CheckAndRequestLocalStoragePermission(_database);
                await SaveLocalSqlFileDatabase(_database);
                bool saveDatabaseIsAllowed = await SaveLocalSqlFileDatabase(_database);

                DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;

                if (mainDisplayInfo.Density >= 3)
                {
                    tbpMain.Children.Add(new NavigationPage(new Album(_unityContainer)) { Title = "Álbum" });
                    tbpMain.Children.Add(new NavigationPage(new Music(_unityContainer)) { Title = "Música" });

                    if (saveDatabaseIsAllowed)
                        tbpMain.Children.Add(new NavigationPage(new Saved(_unityContainer)) { Title = "Salvos" });

                    _formLoaded = true;
                }
                else 
                {
                    tbpMain.Children.Add(new NavigationPage(new Album(_unityContainer)) { Title = "Álbum", IconImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.FileImageO, 20, Color.White) });
                    tbpMain.Children.Add(new NavigationPage(new Music(_unityContainer)) { Title = "Música", IconImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.Music, 20, Color.White) });
                    
                    if (saveDatabaseIsAllowed)
                        tbpMain.Children.Add(new NavigationPage(new Saved(_unityContainer)) { Title = "Salvos", IconImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.ArrowDown, 20, Color.White) });

                    _formLoaded = true;
                }
            }
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
        private async Task<bool> SaveLocalSqlFileDatabase(IDatabaseConn database)
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (statusRead == PermissionStatus.Granted && statusWrite == PermissionStatus.Granted)
            {
                await database.CreateDatabaseIfNotExists();

                return true;
            }

            return false;
        }
    }
}
