using Android.OS;
using Firebase.Analytics;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TocaTudo.CustomControl;
using TocaTudoPlayer.Xamarim;
using Xamarin.Forms;

[assembly: Dependency(typeof(EventTrackerDroid))]
namespace TocaTudo.CustomControl
{
    public class EventTrackerDroid : IEventTracker
    {
        public void SendScreenView(string screenName, string value)
        {
            var firebaseAnalytics = FirebaseAnalytics.GetInstance(Android.App.Application.Context.ApplicationContext);

            var bundle = new Bundle();
            bundle.PutString(screenName, value);

            firebaseAnalytics.LogEvent(FirebaseAnalytics.Event.ScreenView, bundle);
        }
        public void SendEvent(string eventId)
        {
            SendEvent(eventId, null);
        }
        public void SendEvent(string eventId, string paramName, string value)
        {
            SendEvent(eventId, new Dictionary<string, string>
            {
                {paramName, value}
            });
        }
        public void SendEvent(string eventId, IDictionary<string, string> parameters)
        {
            var firebaseAnalytics = FirebaseAnalytics.GetInstance(Android.App.Application.Context.ApplicationContext);

            if (parameters == null)
            {
                firebaseAnalytics.LogEvent(eventId, null);
                return;
            }

            var bundle = new Bundle();
            foreach (var param in parameters)
            {
                bundle.PutString(param.Key, param.Value);
            }

            firebaseAnalytics.LogEvent(eventId, bundle);
        }
    }
}