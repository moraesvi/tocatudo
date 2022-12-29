using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim
{
    public interface IEventTracker
    {
        void SendScreenView(string screenName, string value);
        void SendEvent(string eventId);
        void SendEvent(string eventId, string paramName, string value);
        void SendEvent(string eventId, IDictionary<string, string> parameters);
    }
}
