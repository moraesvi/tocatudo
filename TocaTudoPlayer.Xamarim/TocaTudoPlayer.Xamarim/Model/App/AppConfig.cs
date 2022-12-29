using System;

namespace TocaTudoPlayer.Xamarim
{
    public class AppConfig
    {
        private const string USER_APP_CONFIG_LOCAL_KEY = "u_appconfig";
        private const int MUSIC_MERCHAN_MINUTES_INTERVAL_DEFAULT = 20;
        public string Id { get; set; }
        public string AppBuild { get; set; }
        public string AppBuildVersion { get; set; }
        public string AppVersion { get; set; }
        public int MusicMerchanMinutesIntervalToShow { get; set; } = MUSIC_MERCHAN_MINUTES_INTERVAL_DEFAULT;
        public int AlbumMerchanMinutesIntervalToShow { get; set; } = MUSIC_MERCHAN_MINUTES_INTERVAL_DEFAULT;
        public int TotalMusicsWillPlayBeforeMerchan { get; set; }
        public static string UserAppConfigLocalKey => USER_APP_CONFIG_LOCAL_KEY;
        public DateTimeOffset? DtUpd { get; set; }  
    }
}
