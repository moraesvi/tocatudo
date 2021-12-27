using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicModel : IVideoModel
    {
        public MusicModel()
        {
        }
        public MusicModel(UserMusic userMusic, byte[] music)
        {
            VideoId = userMusic.VideoId;
            UMusicId = userMusic.UMusicId;
            MusicName = userMusic.MusicName;
            MusicImage = userMusic.MusicImage;
            Music = music;
        }
        public string VideoId { get; set; }
        public string UMusicId { get; set; }
        public string MusicName { get; set; }
        public byte[] MusicImage { get; set; }
        public byte[] Music { get; set; }
        public async Task<string> GetFileNameLocalPath()
        {
            PermissionStatus statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            PermissionStatus statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            bool hasLocalPermissionToStorage = statusRead == PermissionStatus.Granted && statusWrite == PermissionStatus.Granted;

            if (!hasLocalPermissionToStorage)
                return string.Empty;

            if (string.IsNullOrEmpty(VideoId))
                return string.Empty;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{VideoId}.txt");
            return path;
        }
        public async Task LoadMusicImageInfo(ITocaTudoApi tocaTudoApi, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(VideoId))
                return;

            MusicImage = await tocaTudoApi.PlayerImageEndpoint(VideoId);
        }
    }
}
