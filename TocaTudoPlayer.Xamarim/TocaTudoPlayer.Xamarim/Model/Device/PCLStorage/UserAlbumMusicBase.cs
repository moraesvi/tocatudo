using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TocaTudoPlayer.Xamarim
{
    public abstract class UserAlbumMusicBase
    {
        protected string GetFileNameLocalPath(string videoId)
        {
            if (string.IsNullOrEmpty(videoId))
                return string.Empty;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{videoId}.txt");
            return path;
        }
    }
}
