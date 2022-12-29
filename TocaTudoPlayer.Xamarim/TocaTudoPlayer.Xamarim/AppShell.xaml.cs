using MarcTron.Plugin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Pages;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppShell : Shell
	{
		private readonly IDatabaseConn _database;
		private readonly ITocaTudoApi _tocaTudoApi;
		private readonly IAudio _audioPlayer;
		private readonly CommonMusicPlayerViewModel _commonMusicPlayer;
		private readonly MusicPageViewModel _musicPageViewModel;
		private readonly SavedMusicPageViewModel _savedMusicPageViewModel;
		private readonly AlbumPlayerViewModel _albumPlayerViewModel;
		private bool _formIsVisible;
		private bool _formLoaded;
		public AppShell ()
		{
			InitializeComponent ();

			_formLoaded = false;
			_audioPlayer = DependencyService.Get<IAudio>();
			_database = App.Services.GetRequiredService<IDatabaseConn>();
			_tocaTudoApi = App.Services.GetRequiredService<ITocaTudoApi>();
			_commonMusicPlayer = App.Services.GetRequiredService<CommonMusicPlayerViewModel>();
			_musicPageViewModel = App.Services.GetRequiredService<MusicPageViewModel>();
			_savedMusicPageViewModel = App.Services.GetRequiredService<SavedMusicPageViewModel>();
			_albumPlayerViewModel = App.Services.GetRequiredService<AlbumPlayerViewModel>();

            Routing.RegisterRoute(nameof(Album), typeof(Album));
            Routing.RegisterRoute(nameof(Music), typeof(Music));
            Routing.RegisterRoute(nameof(Saved), typeof(Saved));
            Routing.RegisterRoute(nameof(SavedMusic), typeof(SavedMusic));
            Routing.RegisterRoute(nameof(AlbumPlayer), typeof(AlbumPlayer));

            App.IsStartEvent += App_IsStartEvent;
            App.IsResumeEvent += App_IsResume;
			App.IsSleepingEvent += App_IsSleeping;

			CrossMTAdmob.Current.OnInterstitialClosed -= AudioPlayer_OnInterstitialClosed;
			CrossMTAdmob.Current.OnInterstitialClosed += AudioPlayer_OnInterstitialClosed;

            CommonMusicPlayerManager.Init(App.Services.GetRequiredService<AlbumPlayerViewModel>(), App.Services.GetRequiredService<MusicPageViewModel>(), App.Services.GetRequiredService<SavedMusicPageViewModel>());
        }        
        protected void LoadAppConfig()
        {
            Task.Run(async () =>
            {
                App.AppConfig = await _tocaTudoApi.AppConfigEndpoint();

#if DEBUG
                App.AppConfig.MusicMerchanMinutesIntervalToShow = 2;
                App.AppConfig.AlbumMerchanMinutesIntervalToShow = 2;
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
        private async void App_IsStartEvent(object sender, EventArgs e)
        {
            _formIsVisible = true;
            if (!_formLoaded)
            {
                DependencyService.Get<IAudio>().Start();

                await CheckSavedTab();
                LoadAppConfig();

                if (!_formLoaded)
                {
                    await Task.WhenAll(_musicPageViewModel.MusicPlayedHistoryViewModel.LoadUserSearchHistory(), _musicPageViewModel.MusicPlayedHistoryViewModel.LoadPlayedHistory(), _musicPageViewModel.LoadMusicAlbumPlaylistSelected());

                    _formLoaded = true;
                }

                _formLoaded = true;
            }

            base.OnAppearing();
        }
        private void App_IsResume(object sender, EventArgs e)
        {
            LoadPage();
        }
        private void App_IsSleeping(object sender, bool sleeping)
        {
            _formIsVisible = !sleeping;
        }
        private void AudioPlayer_OnInterstitialClosed(object sender, EventArgs e)
        {
            AppHelper.MusicPlayerInterstitialIsLoadded = false;

            if (_musicPageViewModel.ReadyMusic?.IsActiveMusic ?? false)
            {
                _musicPageViewModel.MusicPlayerViewModel.PlayMusic();
            }

            if (_savedMusicPageViewModel.ReadyMusic?.IsActiveMusic ?? false)
            {
                _savedMusicPageViewModel.MusicPlayerViewModel.PlayMusic();
            }

            if (_albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow != null && _albumPlayerViewModel.BottomPlayerViewModel.AlbumHasMusicSelected)
            {
                _albumPlayerViewModel.MusicPlayerViewModel.PlayMusic();
            }
        }
        private async Task LoadPage()
        {
            if (AppHelper.HasInterstitialToShow)
            {
                if (_musicPageViewModel.ReadyMusic != null)
                {
                    await Music.LoadAndShowInterstitial(() =>
                    {
                        _commonMusicPlayer.PlayMusic();
                    });
                }

                if (_savedMusicPageViewModel.ReadyMusic != null)
                {
                    await SavedMusic.LoadAndShowInterstitial(() =>
                    {
                        _commonMusicPlayer.PlayMusic();
                    });
                }

                if (_albumPlayerViewModel.BottomPlayerViewModel.PlaylistItemNow != null)
                {
                    await AlbumPlayer.LoadAndShowInterstitial(() =>
                    {
                        _commonMusicPlayer.PlayMusic();
                    });
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
            if (_commonMusicPlayer.MusicPlayingNow != null)
            {
                if (_commonMusicPlayer.MusicPlayingNow.IsActiveMusic && !_commonMusicPlayer.HasMusicPlaying)
                {
                    _commonMusicPlayer.PlayMusic();
                }
            }
        }
        private async Task CheckSavedTab()
        {
            await CheckAndRequestLocalStoragePermission(_database);
            await SaveLocalSqlFileDatabase(_database);
            bool saveDatabaseIsAllowed = await SaveLocalSqlFileDatabase(_database);

            if (!saveDatabaseIsAllowed)
            {
                //tbpMain.Children.RemoveAt(2);
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
                //await database.CreateDatabaseIfNotExists();

                return true;
            }

            return false;
        }
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledException)
        {
            try
            {
                AppException ex = new AppException();
                Exception exception = (Exception)unhandledException.ExceptionObject ?? null;

                ex.LocalDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                ex.Msg = exception.Message;
                ex.InnerMsg = exception.InnerException?.Message;
                ex.StackTrace = exception.StackTrace;

                _tocaTudoApi.AppExceptionEndpoint(ex).GetAwaiter().GetResult();
            }
            catch
            {

            }
        }
        #endregion
    }
}