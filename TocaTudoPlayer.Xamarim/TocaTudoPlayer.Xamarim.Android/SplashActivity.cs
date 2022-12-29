using Android.App;
using Android.Content;
using Android.OS;

namespace TocaTudo
{
    [Activity(Theme = "@style/Theme.Splash", Label = "@string/app_name", Icon = "@drawable/icon", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(typeof(MainActivity));
            Finish();
        }
    }
}