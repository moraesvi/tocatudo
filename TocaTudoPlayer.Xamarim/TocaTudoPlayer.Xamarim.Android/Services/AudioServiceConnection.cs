using Android.Content;
using Android.OS;

namespace TocaTudo.Services
{
    internal class AudioServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public AudioServiceConnection(MainActivity activity) 
        {
            Activity = activity;
        }
        public MainActivity Activity { get; private set; }
        public AudioServiceBinder Binder { get; private set; }
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as AudioServiceBinder;
            Activity.Binder = Binder;
        }
        public void OnServiceDisconnected(ComponentName name)
        {
            Binder.Dispose();
        }
    }
}