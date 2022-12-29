using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Pages;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    public partial class MainPage : TabbedPage
    {
#if DEBUG
        private const int DEFAULT_MUNUTES_TIME_SLEEPING = 2;
#else
        private const int DEFAULT_MUNUTES_TIME_SLEEPING = 5;
#endif
        private readonly IDatabaseConn _database;
        private readonly ITocaTudoApi _tocaTudoApi;
        private readonly IAudio _audioPlayer;
        private readonly CommonMusicPlayerViewModel _commonMusicPlayer;
        private readonly MusicPageViewModel _musicPageViewModel;
        private readonly SavedMusicPageViewModel _savedMusicPageViewModel;
        private readonly AlbumPlayerViewModel _albumPlayerViewModel;
        private static WeakEventManager _timeSleepingEvent;
        private static TimeSpan _timeSleeping;
        private bool _formIsVisible;
        private bool _formLoaded;
        public MainPage()
        {
            InitializeComponent();

            _formLoaded = false;
            _timeSleepingEvent = new WeakEventManager();

            _audioPlayer = DependencyService.Get<IAudio>();
            _database = App.Services.GetRequiredService<IDatabaseConn>();
            _tocaTudoApi = App.Services.GetRequiredService<ITocaTudoApi>();
            _commonMusicPlayer = App.Services.GetRequiredService<CommonMusicPlayerViewModel>();
            _musicPageViewModel = App.Services.GetRequiredService<MusicPageViewModel>();
            _savedMusicPageViewModel = App.Services.GetRequiredService<SavedMusicPageViewModel>();
            _albumPlayerViewModel = App.Services.GetRequiredService<AlbumPlayerViewModel>();

            App.IsResumeEvent += App_IsResume;
            App.IsSleepingEvent += App_IsSleeping;

            CrossMTAdmob.Current.OnInterstitialClosed -= AudioPlayer_OnInterstitialClosed;
            CrossMTAdmob.Current.OnInterstitialClosed += AudioPlayer_OnInterstitialClosed;


            if (DeviceDisplay.MainDisplayInfo.Density >= 3)
            {
                tbpMain.Children.Add(new NavigationPage(new Album()) { Title = AppHelper.ToTitleCase(AppResource.MusicAlbumButton) });
                tbpMain.Children.Add(new NavigationPage(new Music()) { Title = AppHelper.ToTitleCase(AppResource.MusicMusicButton) });
                tbpMain.Children.Add(new NavigationPage(new Saved()) { Title = AppHelper.ToTitleCase(AppResource.MusicSavedButton) });
            }
            else
            {
                tbpMain.Children.Add(new NavigationPage(new Album()) { Title = AppHelper.ToTitleCase(AppResource.MusicAlbumButton), IconImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.FileImageO, 20, Color.White) });
                tbpMain.Children.Add(new NavigationPage(new Music()) { Title = AppHelper.ToTitleCase(AppResource.MusicMusicButton), IconImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.Music, 20, Color.White) });
                tbpMain.Children.Add(new NavigationPage(new Saved()) { Title = AppHelper.ToTitleCase(AppResource.MusicSavedButton), IconImageSource = AppHelper.FaviconImageSource(TocaTudoPlayer.Xamarim.Icon.ArrowDown, 20, Color.White) });
            }

            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            }

            CommonMusicPlayerManager.Init(App.Services.GetRequiredService<AlbumPlayerViewModel>(), App.Services.GetRequiredService<MusicPageViewModel>(), App.Services.GetRequiredService<SavedMusicPageViewModel>());
        }
        public static event EventHandler TimeSleepingEvent
        {
            add => _timeSleepingEvent.AddEventHandler(value);
            remove => _timeSleepingEvent.RemoveEventHandler(value);
        }
        protected async override void OnAppearing()
        {
            _formIsVisible = true;
            if (!_formLoaded)
            {
                DependencyService.Get<IAudio>().Start();

                await CheckSavedTab();
                LoadAppConfig();

                _formLoaded = true;
            }

            if (_timeSleeping.TotalSeconds > 0 && (DateTime.Now.TimeOfDay - _timeSleeping).TotalMinutes >= DEFAULT_MUNUTES_TIME_SLEEPING)
            {
                _timeSleeping = new TimeSpan();
                _timeSleepingEvent.RaiseEvent(this, null, nameof(TimeSleepingEvent));
            }

            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            _formIsVisible = false;

            if (_timeSleeping.TotalSeconds == 0)
                _timeSleeping = DateTime.Now.TimeOfDay;

            base.OnDisappearing();
        }
        protected void LoadAppConfig()
        {
            Task.Run(async () =>
            {
                App.AppConfig = await _tocaTudoApi.AppConfigEndpoint();

                if (App.AppConfig == null)
                    App.AppConfig = new AppConfig();
#if DEBUG
                App.AppConfig.MusicMerchanMinutesIntervalToShow = 5;
                App.AppConfig.AlbumMerchanMinutesIntervalToShow = 5;
#endif
                if (!string.Equals(VersionTracking.CurrentVersion, App.AppConfig.AppVersion) || !string.Equals(VersionTracking.CurrentBuild, App.AppConfig.AppBuildVersion))
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert(AppResource.PlayStoreNewVersionAlert, AppResource.PlayStoreNewVersion, "ok");

                        bool isOppened = await Launcher.TryOpenAsync("market://details?id=com.moraesvi.tocatudo");
                    });
                }
            }).ConfigureAwait(false);
        }

        #region Private Methods
        private async void App_IsResume(object sender, EventArgs e)
        {
            await LoadPage();
        }
        private void App_IsSleeping(object sender, bool sleeping)
        {
            _formIsVisible = !sleeping;
        }
        private void AudioPlayer_OnInterstitialClosed(object sender, EventArgs e)
        {
            AppHelper.HasInterstitialToShow = false;
            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            if (_musicPageViewModel.ReadyMusic != null)
            {
                _musicPageViewModel.MusicPlayerViewModel.PlayMusic();
            }
            else if (_savedMusicPageViewModel.ReadyMusic != null)
            {
                _savedMusicPageViewModel.MusicPlayerViewModel.PlayMusic();
            }
            else if (_albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow != null && _albumPlayerViewModel.BottomPlayerViewModel.AlbumHasMusicSelected)
            {
                _albumPlayerViewModel.MusicPlayerViewModel.PlayMusic();
            }
        }
        private async Task LoadPage()
        {
            if (AppHelper.HasInterstitialToShow)
            {
                Action intertistialIsNotLoaded = () =>
                {
                    _commonMusicPlayer.PlayMusic();
                };

                if (_musicPageViewModel.ReadyMusic != null)
                {
                    await Music.LoadAndShowInterstitial(intertistialIsNotLoaded);
                }
                else if (_savedMusicPageViewModel.ReadyMusic != null)
                {
                    await SavedMusic.LoadAndShowInterstitial(intertistialIsNotLoaded);
                }
                else if (_albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow != null)
                {
                    await AlbumPlayer.LoadAndShowInterstitial(intertistialIsNotLoaded);
                }

                AppHelper.MusicPlayerInterstitialIsLoadded = CrossMTAdmob.Current.IsInterstitialLoaded();
            }
            else
            {
                CheckMusicPlayingNow();
            }
        }
        private void CheckMusicPlayingNow()
        {
            if (AppHelper.HasInterstitialToShow)
                return;

            if (_commonMusicPlayer.MusicPlayingNow != null)
            {
                if (_commonMusicPlayer.MusicPlayingNow.IsActiveMusic && !_commonMusicPlayer.HasMusicPlaying)
                {
                    _commonMusicPlayer.PlayMusic();
                }
            }
            if (_albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow != null)
            {
                if (_albumPlayerViewModel.BottomPlayerViewModel.AlbumHasMusicSelected)
                {
                    if (_albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow != null && _albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow.IsActiveMusic)
                    {
                        _albumPlayerViewModel.MusicPlayerViewModel.PlayMusic();
                    }
                }
            }
        }
        private async Task CheckSavedTab()
        {
            //await CheckAndRequestLocalStoragePermission(_database);
            //await SaveLocalSqlFileDatabase(_database);
            //bool saveDatabaseIsAllowed = await SaveLocalSqlFileDatabase(_database);

            //if (!saveDatabaseIsAllowed)
            //{
            //tbpMain.Children.RemoveAt(2);
            //}
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

            return statusRead == PermissionStatus.Granted && statusWrite == PermissionStatus.Granted;
        }
        private async void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledException)
        {
            try
            {
                AppException ex = new AppException();
                Exception exception = (Exception)unhandledException.ExceptionObject ?? null;

                ex.LocalDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                ex.Msg = exception.Message;
                ex.InnerMsg = exception.InnerException?.Message;
                ex.StackTrace = exception.StackTrace;

                App.EventTracker.SendEvent("Exception", new Dictionary<string, string>()
                {
                    { "Msg", exception.Message },
                    { "InnerMsg", exception.InnerException?.Message },
                    { "StackTrace", exception.StackTrace },
                });

                await _tocaTudoApi.AppExceptionEndpoint(ex);
                Application.Current.Quit();
            }
            catch
            {
                Application.Current.Quit();
            }
        }
        #endregion
    }
}
