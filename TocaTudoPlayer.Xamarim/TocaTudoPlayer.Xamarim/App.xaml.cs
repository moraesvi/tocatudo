using Microsoft.Extensions.DependencyInjection;
using Plugin.FirebasePushNotification;
using System;
using System.Globalization;
using TocaTudoPlayer.Xamarim.Logic;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public partial class App : Application
    {
        private const int DEFAULT_TOTAL_MUSICS_PLAYING_BEFORE_MERCHAN = 3;
        private static IServiceProvider _service;
        private static IEventTracker _eventTracker;
        private static WeakEventManager _isStartEvent;
        private static WeakEventManager _isResumeEvent;
        private static WeakEventManager<bool> _isSleepingEvent;
        private static bool _appIsRunning;
        private static bool _isSleeping;
        public App()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;

            InitializeComponent();
            LoadAppConfig();

            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<CommonPageViewModel>();
            serviceCollection.AddScoped<CommonMusicPageViewModel>();
            serviceCollection.AddSingleton<CommonMusicPlayerViewModel>();
            serviceCollection.AddSingleton<CommonFormDownloadViewModel>();
            serviceCollection.AddSingleton<AlbumPlayedHistoryViewModel>();
            serviceCollection.AddSingleton<AlbumSavedPlayedHistoryViewModel>();
            serviceCollection.AddSingleton<MusicSavedPlayedHistoryViewModel>();
            serviceCollection.AddSingleton<MusicPlayedHistoryViewModel>();
            serviceCollection.AddSingleton<AlbumPageViewModel>();
            serviceCollection.AddSingleton<MusicPageViewModel>();
            serviceCollection.AddSingleton<SavedMusicPageViewModel>();
            serviceCollection.AddTransient<AlbumPlayer>();
            serviceCollection.AddSingleton<AlbumPlayerViewModel>();
            serviceCollection.AddSingleton<MusicBottomPlayerViewModel>();
            serviceCollection.AddSingleton<MusicBottomAlbumPlayerViewModel>();
            serviceCollection.AddSingleton<AlbumSavedPageViewModel>();
            serviceCollection.AddSingleton<YoutubeClient>();
            serviceCollection.AddSingleton<IPCLStorageDb, PCLDbLogic>();
            serviceCollection.AddTransient<IPCLUserAlbumDb, PCLUserAlbumDb>();
            serviceCollection.AddTransient<IPCLUserMusicDb, PCLUserMusicDb>();
            serviceCollection.AddSingleton<IPCLAppConfigLogic, PCLAppConfigLogic>();
            serviceCollection.AddTransient<IPCLUserMusicLogic, PCLUserMusicLogic>();
            serviceCollection.AddTransient<IPCLUserAlbumLogic, PCLUserAlbumLogic>();
            serviceCollection.AddSingleton<IDatabaseConn, SQLiteConn>();
            serviceCollection.AddSingleton<ITocaTudoApi, TocaTudoApi>();
            serviceCollection.AddSingleton<IDbLogic, DbLogic>();

            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);

            _service = serviceCollection.BuildServiceProvider();

            _isStartEvent = new WeakEventManager();
            _isResumeEvent = new WeakEventManager();
            _isSleepingEvent = new WeakEventManager<bool>();

            _eventTracker = DependencyService.Get<IEventTracker>();

            DependencyService.Get<IEnvironment>().SetStatusBarColor(Color.FromHex("#41577c"), false);

            //MainPage = new NavigationPage(new AlbumPlayer(_unityContainer, new ApiSearchMusicModel() { Id = 1, NomeAlbum = "Iron Maiden - Nights of the Dead, Legacy of the Beast", TipoParse = new int[] { 1 }, VideoId = "OopYRt_-4Ls" }));
            MainPage = new NavigationPage(ActivatorUtilities.CreateInstance<MainPage>(_service));

            CrossFirebasePushNotification.Current.Subscribe("all");
            CrossFirebasePushNotification.Current.OnTokenRefresh += Current_OnTokenRefresh;
        }
        public static event EventHandler IsStartEvent
        {
            add => _isStartEvent.AddEventHandler(value);
            remove => _isStartEvent.RemoveEventHandler(value);
        }
        public static event EventHandler<bool> IsSleepingEvent
        {
            add => _isSleepingEvent.AddEventHandler(value);
            remove => _isSleepingEvent.RemoveEventHandler(value);
        }
        public static event EventHandler IsResumeEvent
        {
            add => _isResumeEvent.AddEventHandler(value);
            remove => _isResumeEvent.RemoveEventHandler(value);
        }
        public static bool IsRunning => _appIsRunning;
        public static bool IsSleeping => _isSleeping;
        public static IServiceProvider Services => _service;
        public static IEventTracker EventTracker => _eventTracker;
        public static AppConfig AppConfig { get; set; }
        public static AppConfigAdMob AppConfigAdMob { get; private set; }
        protected override void OnStart()
        {
            _isSleeping = false;
            _appIsRunning = true;
            _isStartEvent.RaiseEvent(this, null, nameof(IsStartEvent));
            _isSleepingEvent.RaiseEvent(this, false, nameof(IsSleepingEvent));
            base.OnStart();
        }
        protected override void OnSleep()
        {
            _isSleeping = true;
            _isSleepingEvent.RaiseEvent(this, true, nameof(IsSleepingEvent));

            base.OnSleep();
        }
        protected override void OnResume()
        {
            _isSleeping = false;
            _isSleepingEvent.RaiseEvent(this, false, nameof(IsSleepingEvent));
            _isResumeEvent.RaiseEvent(this, null, nameof(IsResumeEvent));

            base.OnResume();
        }
        public static int GetTotalMusicsWillPlayBeforeMerchan() => AppConfig == null || AppConfig.TotalMusicsWillPlayBeforeMerchan <= 0 ? DEFAULT_TOTAL_MUSICS_PLAYING_BEFORE_MERCHAN : AppConfig.TotalMusicsWillPlayBeforeMerchan;

        #region Private Methods
        private void Current_OnTokenRefresh(object source, FirebasePushNotificationTokenEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Token: {e.Token}");
        }
        private static void LoadAppConfig()
        {
            if (AppConfigAdMob != null)
                return;

            AppConfigAdMob = new AppConfigAdMob()
            {
                AdsActiveProdMode = false,
                AdsAlbumBanner = "ca-app-pub-3382139004617696/7972731197",
                AdsMusicBanner = "ca-app-pub-3382139004617696/5979573779",
                AdsSavedMusicBanner = "ca-app-pub-3382139004617696/4250513571",
                AdsAlbumPlayerBanner = "ca-app-pub-3382139004617696/5360415265",
                AdsAlbumIntersticial = "ca-app-pub-3382139004617696/2108126429",
                AdsMusicIntersticial = "ca-app-pub-3382139004617696/2937431902",
                AdsSavedMusicIntersticial = "ca-app-pub-3382139004617696/6819184208",
                AdsAlbumPlayerIntersticial = "ca-app-pub-3382139004617696/4485267548"
            };


#if DEBUG
            //AppConfigAdMob = new AppConfigAdMob()
            //{
            //    AdsActiveProdMode = false,
            //    AdsMusicBanner = "ca-app-pub-3940256099942544/6300978111",
            //    AdsIntersticialAlbum = "ca-app-pub-3940256099942544/1033173712"
            //};
#endif
        }
        #endregion
    }
}
public class AppApiUri
{
    public const string TOCA_TUDO_URL = "";
    public const string TOCA_TUDO_BUSCA_PLAYLIST_ENDPOINT = "";
    public const string TOCA_TUDO_BUSCAV2_PLAYLIST_ENDPOINT = "";
    public const string TOCA_TUDO_PLAYLIST_ENDPOINT = "";
    public const string TOCA_TUDO_PLAYLISTV2_ENDPOINT = "";
    public static string TocaTudoAppConfigEndpoint() => "";
    public static string TocaTudoAppExceptionEndpoint() => "";
    public static string TocaTudoGetSearchEndpoint(string term) => $"";
    public static string TocaTudoGetPlaylistEndpoint(string term) => $"";
    public static string TocaTudoGetMusicEndpoint(string term) => $"";
    public static string TocaTudoGetAlbumEndpoint(string videoId, int tpMusic) => $"";
    public static string TocaTudoGetStreamUrlEndpoint(string videoId) => $"";
    public static string TocaTudoGetPlayerImageEndpoint(string videoId) => $"";
    public static string TocaTudoGetPlayerImageWidescreenEndpoint(string videoId) => $"";
}

