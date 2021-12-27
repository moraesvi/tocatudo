using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using System;
using TocaTudoPlayer.Xamarim;
using TocaTudoPlayer.Xamarim.Logic;

namespace TocaTudo
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private bool _isConfigurationChange = false;
        private AudioServiceBinder _binder;
        private AudioServiceConnection _audioServiceConnection;
        private ICosmosDbLogic _cosmosDbLogic;

        private event Action _binderConnected;
        public AudioServiceConnection AudioServiceConnection => _audioServiceConnection;
        public AudioServiceBinder Binder
        {
            get { return _binder; }
            set
            {
                _binder = value;
                _binderConnected();
            }
        }
        public event Action BinderConnected
        {
            add => _binderConnected += value;
            remove => _binderConnected -= value;
        }
        public MusicModelServicePlayer MusicModelServicePlayerParameter { get; set; }
        public AlbumModelServicePlayer AlbumModelServicePlayerParameter { get; set; }
        public PlaylistItemServicePlayer PlaylistItemServicePlayerParameter { get; set; }
        public byte[] AudioServiceMusicParameter { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            MobileAds.Initialize(ApplicationContext);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);

            _cosmosDbLogic = new CosmosDbLogic(App.InitializeCosmosClientInstance());

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            LoadApplication(new App());
        }
        protected override void OnStart()
        {
            base.OnStart();

            Intent intent = new Intent(this, typeof(AudioService));
            _audioServiceConnection = new AudioServiceConnection(this);

            BindService(intent, _audioServiceConnection, Bind.None);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!_isConfigurationChange)
            {
                UnbindService(_audioServiceConnection);
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        private async void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledException)
        {
            AppException ex = new AppException();
            Exception exception = (Exception)unhandledException.ExceptionObject;

            ex.LocalDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            ex.Msg = exception.Message;
            ex.InnerMsg = exception.InnerException?.Message;
            ex.StackTrace = exception.StackTrace;

            await _cosmosDbLogic.InsertAppException(ex);
        }
    } 
}