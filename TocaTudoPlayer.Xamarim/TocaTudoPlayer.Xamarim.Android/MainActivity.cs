using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.View;
using Plugin.CurrentActivity;
using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TocaTudo.Services;
using TocaTudoPlayer.Xamarim;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(TocaTudo.Environment))]
namespace TocaTudo
{
    [Activity(Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private AudioServiceBinder _binder;
        private readonly WeakEventManager _binderConnected;
        private AudioServiceConnection _audioServiceConnection;
        public MainActivity()
        {
            _binderConnected = new WeakEventManager();
            IsConfigurationChanged = false;
        }
        public AudioServiceBinder Binder
        {
            get { return _binder; }
            set
            {
                _binder = value;
                _binderConnected.RaiseEvent(this, null, nameof(BinderConnected));
            }
        }
        public event EventHandler BinderConnected
        {
            add => _binderConnected.AddEventHandler(value);
            remove => _binderConnected.RemoveEventHandler(value);
        }
        public bool IsConfigurationChanged { get; set; }
        public static AlbumModelServicePlayer AlbumModelServicePlayerParameter { get; set; }
        public static MusicModelServicePlayer MusicModelServicePlayerParameter { get; set; }
        public static AlbumMusicModelServicePlayer AlbumMusicModelServicePlayerParameter { get; set; }
        public static ItemServicePlayer PlaylistItemServicePlayerParameter { get; set; }
        public byte[] AudioServiceMusicParameter { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FirebasePushNotificationManager.ShowAlert = false;
            FirebasePushNotificationManager.ShouldShowWhen = false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";
                FirebasePushNotificationManager.DefaultNotificationChannelName = "General";
            }

            //If debug you should reset the token each time.
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, true);
#else
            FirebasePushNotificationManager.Initialize(this, false);
#endif
            FirebasePushNotificationManager.IconResource = Resource.Drawable.icon;
            FirebasePushNotificationManager.ProcessIntent(this, Intent);

            Window.SetFlags(Android.Views.WindowManagerFlags.Secure, Android.Views.WindowManagerFlags.Secure);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            MobileAds.Initialize(ApplicationContext);

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
            FirebasePushNotificationManager.ShowAlert = true;
            FirebasePushNotificationManager.ShouldShowWhen = true;

            base.OnDestroy();
            UnbindService(_audioServiceConnection);
        }
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            FirebasePushNotificationManager.ProcessIntent(this, intent);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
    public class Environment : IEnvironment
    {
        public async void SetStatusBarColor(System.Drawing.Color color, bool darkStatusBarTint)
        {
            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
                return;

            var activity = Platform.CurrentActivity;
            var window = activity.Window;

            //this may not be necessary(but may be fore older than M)
            window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);


            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                await Task.Delay(50);
                WindowCompat.GetInsetsController(window, window.DecorView).AppearanceLightStatusBars = darkStatusBarTint;
            }

            window.SetStatusBarColor(color.ToPlatformColor());
        }
    }
}