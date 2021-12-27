using Android.Content;
using Android.Media;
using Android.Util;
using TocaTudoPlayer.Xamarim;
using Xamarin.Forms;

namespace TocaTudo
{
    public class MusicBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != AudioManager.ActionAudioBecomingNoisy)
                return;

            // Signal the service to stop
            Intent stopIntent = new Intent(context, typeof(AudioService));
            stopIntent.SetAction(AudioService.ActionHeadphonesUnplugged);

            Android.App.Application.Context.StartService(stopIntent);
        }
    }
}