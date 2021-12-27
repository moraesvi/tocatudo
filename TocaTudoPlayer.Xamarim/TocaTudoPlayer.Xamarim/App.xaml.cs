using MarcTron.Plugin;
using Microsoft.Azure.Cosmos;
using System;
using System.Globalization;
using System.IO;
using TocaTudoPlayer.Xamarim.Interface;
using TocaTudoPlayer.Xamarim.Logic;
using TocaTudoPlayer.Xamarim.ViewModel.CustomView;
using Unity;
using Xamarin.Essentials;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public partial class App : Application
    {
        public static readonly string _dbLocalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tocaTudo.db3");
        private static AppConfig _appConfig;
        private readonly IUnityContainer _unityContainer;
        public App()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;

            InitializeComponent();
            LoadAppConfig();

            _unityContainer = new UnityContainer();

            _unityContainer.RegisterSingleton<ICommonPageViewModel, CommonPageViewModel>();
            _unityContainer.RegisterSingleton<ICommonMusicPageViewModel, CommonMusicPageViewModel>();
            _unityContainer.RegisterType<ICommonMusicPlayerViewModel, CommonMusicPlayerViewModel>();
            _unityContainer.RegisterSingleton<ICommonFormDownloadViewModel, CommonFormDownloadViewModel>();
            _unityContainer.RegisterSingleton<IAlbumPlayedHistoryViewModel, AlbumPlayedHistoryViewModel>();
            _unityContainer.RegisterSingleton<IAlbumSavedPlayedHistoryViewModel, AlbumSavedPlayedHistoryViewModel>();
            _unityContainer.RegisterSingleton<IMusicSavedPlayedHistoryViewModel, MusicSavedPlayedHistoryViewModel>();
            _unityContainer.RegisterSingleton<IMusicPlayedHistoryViewModel, MusicPlayedHistoryViewModel>();
            _unityContainer.RegisterSingleton<IAlbumPageViewModel, AlbumPageViewModel>();
            _unityContainer.RegisterSingleton<IMusicPageViewModel, MusicPageViewModel>();
            _unityContainer.RegisterSingleton<IAlbumPlayerViewModel, AlbumPlayerViewModel>();
            _unityContainer.RegisterSingleton<IMusicBottomPlayerViewModel, MusicBottomPlayerViewModel>();
            _unityContainer.RegisterSingleton<IMusicBottomAlbumPlayerViewModel, MusicBottomAlbumPlayerViewModel>();
            _unityContainer.RegisterSingleton<IAlbumSavedPageViewModel, AlbumSavedPageViewModel>();
            _unityContainer.RegisterSingleton<IMusicSavedPageViewModel, MusicSavedPageViewModel>();
            _unityContainer.RegisterSingleton<YoutubeClient, YoutubeClient>();
            _unityContainer.RegisterSingleton<IPCLStorageDb, PCLDbLogic>();
            _unityContainer.RegisterSingleton<IPCLUserMusicDb, PCLUserMusicDb>();
            _unityContainer.RegisterType<IPCLUserMusicLogic, PCLUserMusicLogic>();
            _unityContainer.RegisterSingleton<IDatabaseConn, SQLiteConn>();
            _unityContainer.RegisterSingleton<ITocaTudoApi, TocaTudoApi>();
            _unityContainer.RegisterSingleton<IDbLogic, DbLogic>();

            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);

            //MainPage = new NavigationPage(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel() { Id = 1, NomeAlbum = "Iron Maiden - Nights of the Dead, Legacy of the Beast", TipoParse = new int[] { 1 }, VideoId = "OopYRt_-4Ls" }));
            MainPage = new MainPage(_unityContainer);
        }
        public static AppConfig AppConfig => _appConfig;
        protected override void OnStart()
        {
            base.OnStart();
        }
        protected override void OnSleep()
        {
            base.OnSleep();
        }
        protected override void OnResume()
        {
            base.OnResume();
        }
        public static IAzureCosmosConn InitializeCosmosClientInstance()
        {
            string databaseName = "tocatudodb";
            string account = "";//Cloud content
            string key = "";//Cloud content

            CosmosClient client = new CosmosClient(account, key);
            IAzureCosmosConn cosmosDbService = new AzureCosmosConn(client, databaseName);

            return cosmosDbService;
        }

        #region Private Methods
        private static void LoadAppConfig()
        {
            if (_appConfig != null)
                return;

            _appConfig = new AppConfig()
            {
                AdMob = new AppConfigAdMob()
                {
                    AdsActiveProdMode = false,
                    AdsMusicBanner = "ca-app-pub-3382139004617696/5979573779",
                    AdsIntersticialAlbum = "ca-app-pub-3382139004617696/2108126429"
                }
            };

#if DEBUG
            _appConfig = new AppConfig()
            {
                AdMob = new AppConfigAdMob()
                {
                    AdsActiveProdMode = false,
                    AdsMusicBanner = "ca-app-pub-3940256099942544/6300978111",
                    AdsIntersticialAlbum = "ca-app-pub-3940256099942544/1033173712"
                }
            };
#endif

            CrossMTAdmob.Current.AdsId = _appConfig.AdMob.AdsMusicBanner;
            CrossMTAdmob.Current.LoadInterstitial(_appConfig.AdMob.AdsIntersticialAlbum);
        }
        #endregion
    }
}
public class AppApiUri
{
    public const string TOCA_TUDO_URL = "";//Cloud content
    public const string TOCA_TUDO_BUSCA_PLAYLIST_ENDPOINT = "api/busca/";
    public const string TOCA_TUDO_BUSCAV2_PLAYLIST_ENDPOINT = "api/buscaV2/";
    public const string TOCA_TUDO_PLAYLIST_ENDPOINT = "api/pagina/playlist/";
    public const string TOCA_TUDO_PLAYLISTV2_ENDPOINT = "api/paginaV2/playlist/";
    public static string TocaTudoGetSearchEndpoint(string term)
    {
        return string.Concat("api/busca/", term.Trim(), "/autocomplete");
    }
    public static string TocaTudoGetMusicEndpoint(string term)
    {
        return string.Concat("api/busca/", term.Trim(), "/musica");
    }
    public static string TocaTudoGetStreamUrlEndpoint(string videoId)
    {
        return string.Concat("api/stream/", videoId, "/url");
    }
    public static string TocaTudoGetPlayerImageEndpoint(string videoId)
    {
        return string.Concat("api/pagina/playerImg/", videoId);
    }
}

