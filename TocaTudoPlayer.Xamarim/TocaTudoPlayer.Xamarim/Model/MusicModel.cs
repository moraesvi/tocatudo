using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicModel : IVideoModel
    {
        private HttpClient _httpClient;
        private long _musicTimeTotalSeconds;
        public MusicModel()
        {
            _httpClient = new HttpClient();
        }
        public MusicModel(UserMusic userMusic, byte[] music)
        {
            _httpClient = new HttpClient();

            VideoId = userMusic.VideoId;
            UMusicId = userMusic.UMusicId;
            MusicTime = userMusic.MusicTime;
            MusicTimeTotalSeconds = userMusic.MusicTimeTotalSeconds;
            MusicName = userMusic.MusicName;
            MusicImage = userMusic.MusicImage;
            Music = music;
        }
        public string VideoId { get; set; }
        public string UMusicId { get; set; }
        public string MusicName { get; set; }
        public string MusicTime { get; set; }
        public long MusicTimeTotalSeconds
        {
            get { return _musicTimeTotalSeconds; }
            set
            {
                _musicTimeTotalSeconds = value;
            }
        }
        public byte[] MusicImage { get; set; }
        public byte[] Music { get; set; }
        public string GetFileNameLocalPath()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{VideoId}.txt");
            return path;
        }
        public async Task LoadMusicImageInfo(ITocaTudoApi tocaTudoApi, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(VideoId))
                return;

            MusicImage = await tocaTudoApi.PlayerImageWidescreenEndpoint(VideoId);
        }
        public async Task LoadMusicImageInfo(string musicUri, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(musicUri))
                return;

            MusicImage = await _httpClient.GetByteArrayAsync(musicUri);
        }
    }
}
