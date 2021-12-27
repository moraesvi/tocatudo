using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TocaTudoPlayer.Xamarim
{
    public abstract class UserAlbumMusicBase
    {
        protected async Task<string> GetFileNameLocalPath(string videoId)
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            bool hasLocalPermissionToStorage = (statusRead == PermissionStatus.Granted && statusWrite == PermissionStatus.Granted);

            if (!hasLocalPermissionToStorage)
                return string.Empty;

            if (string.IsNullOrEmpty(videoId))
                return string.Empty;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{videoId}.txt");
            return path;
        }
    }
}
