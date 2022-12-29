using System;

namespace TocaTudo.Helper
{
    internal class AppDroidHelper
    {
        public static long ExoplayerTimeToTocaTudo(decimal totalSeconds) => (long)Math.Round(totalSeconds / 1000);
    }
}