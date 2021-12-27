using SQLite;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim;
using TocaTudoPlayer.Xamarim.Logic;
using YoutubeParse.ExplodeV2;
using YoutubeParse.ExplodeV2.Videos.Streams;

namespace ConsoleApp1
{
    [Table("tb_music")]
    public class TbMusic
    {
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("music")]
        [NotNull]
        public byte[] Music { get; set; }
        [Column("music_image")]
        [NotNull]
        public byte[] MusicImage { get; set; }
    }
    class Program
    {
        private const string PlayerScriptUrlTemplate = "https://s.ytimg.com/yts/jsbin/html5player-{0}/html5player.js";
        private const string DecodeFunctionPatternTemplate = @"function #NAME#\([^\)]+\){.*?};";
        private const string HelperObjectPatternTemplate = @"var #NAME#={.*?};";

        private static readonly Regex SignatureRegex = new Regex(@"s=(?<Signature>[A-F0-9]+\.[A-F0-9]+)");
        private static readonly Regex PlayerVersionRegex = new Regex(@"html5player-(?<PlayerVersion>[\w\d\-]+)\\\/html5player\.js");
        private static readonly Regex DecodeFunctionNameRegex = new Regex(@"\.sig\|\|(?<FunctionName>[a-zA-Z0-9$]+)\(");
        private static readonly Regex HelperObjectNameRegex = new Regex(@";(?<ObjectName>[A-Za-z0-9]+)\.");

        static double bitrate = 0;
        static void Main(string[] args)
        {
            //ICosmosDbLogic cosmosDbLogic = new CosmosDbLogic(App.InitializeCosmosClientInstance());
            //var teste2 = cosmosDbLogic.GetAppConfig();

            //99S8HtXy590
            string videoId = "99S8HtXy590";

            YoutubeClient _ytClient = new YoutubeClient();
            StreamManifest taskUrl = _ytClient.Videos.Streams.GetManifestAsync(videoId).Result;

            AudioOnlyStreamInfo streamInfo = taskUrl.GetAudioOnlyStreams()
                                                    .Where(audio => !string.Equals(audio.Container.ToString(), "webm", StringComparison.OrdinalIgnoreCase))
                                                    .FirstOrDefault();

            var teste = streamInfo.Url;


            //    string sqLiteDbName = "teste.db3";

            //    using (SQLiteConnection db = new SQLiteConnection(sqLiteDbName))
            //    {
            //        TableQuery<TbMusic> result = db.Table<TbMusic>();

            //        if (result.FirstOrDefault() != null)
            //        {
            //            byte[] music = result.FirstOrDefault().Music;

            //            Image.FromStream(new MemoryStream(result.FirstOrDefault().MusicImage)).Save("teste.jpg", ImageFormat.Jpeg);

            //            using (MemoryStream msCompressed = new MemoryStream(music))
            //            {
            //                using (MemoryStream msDescompressed = new MemoryStream())
            //                {
            //                    using (BufferedStream gzs = new BufferedStream(new GZipStream(msCompressed, CompressionMode.Decompress, true)))
            //                    {
            //                        gzs.CopyTo(msDescompressed);
            //                    }

            //                    using (FileStream fileStream = File.Create("teste.mp4"))
            //                    {
            //                        msDescompressed.Seek(0, SeekOrigin.Begin);
            //                        msDescompressed.CopyTo(fileStream);
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    //bitrate = streamManifest.Streams[0].Bitrate.MegaBitsPerSecond;

            //    using (HttpClientDownloadWithProgress client = new HttpClientDownloadWithProgress(string.Empty, bitrate))
            //    {
            //        //byte[] imgByte = client.DownloadImage("k4d_kfx-8ms").Result;

            //        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
            //        {
            //            Console.WriteLine($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
            //        };
            //        client.DownloadComplete += (compressedMusic) =>
            //        {
            //            using (MemoryStream ms = new MemoryStream(compressedMusic))
            //            {
            //                using (SQLiteConnection db = new SQLiteConnection(sqLiteDbName))
            //                {
            //                    db.CreateTable<TbMusic>();

            //                    TbMusic model = new TbMusic();
            //                    model.Music = ms.ToArray();
            //                    //model.MusicImage = imgByte;

            //                    db.Insert(model);
            //                }
            //            }
            //        };
            //        client.StartDownload().Wait();
            //    }
        }
    }
    public class HttpClientDownloadWithProgress : IDisposable
    {
        private const int BUFFER_SIZE = 64 * 1024;
        private readonly string _downloadUrl;
        private HttpClient _httpClient;

        private double _bitrate = 0;

        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);
        public event ProgressChangedHandler ProgressChanged;

        public delegate void DownloadCompleteHandler(byte[] compressedMusic);
        public event DownloadCompleteHandler DownloadComplete;
        public HttpClientDownloadWithProgress(string downloadUrl, double bitrate)
        {
            _downloadUrl = downloadUrl;
            _bitrate = bitrate;
        }
        public async Task StartDownload()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };

            using (HttpResponseMessage response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                await DownloadFileFromHttpResponseMessage(response);
            }
        }
        public async Task<byte[]> DownloadImage(string videoId)
        {
            string url = string.Format("http://img.youtube.com/vi/{0}/0.jpg", videoId);

            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            contentStream.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
            }
        }
        private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;

            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            {
                await ProcessContentStream(totalBytes, contentStream);
            }
        }
        private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
        {
            int read = 0;
            double percDownload = 0;
            long totalBytesRead = 0;
            long readCount = 0L;
            byte[] buffer = new byte[1024];

            decimal minDuration = ((long)totalDownloadSize / 60);
            decimal minMusicPart = ((154 * 1000) / 60);

            float musicPartTime = (float)Math.Round(((minMusicPart / minDuration) * 100), 2);
            float currentPosition = ((long)totalDownloadSize / 100) * musicPartTime;

            int percentMusic = (int)(minMusicPart / minDuration) * 100;

            int countTotalSeg = 0;

            DateTime t1 = DateTime.Now;
            double tes = 0;

            //_bitrate = 1411.2;

            using (MemoryStream ms = new MemoryStream())
            {
                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    if (read == 0)
                    {
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    totalBytesRead += read;
                    readCount += 1;
                    percDownload = Math.Round((double)(totalBytesRead / totalDownloadSize.Value) * 100, 2);

                    //int minutos = (int)301;
                    //double segundos = Math.Round(((_bitrate % 1) * 100), 2);

                    double tresMinutos = (0.331 * 60);

                    tresMinutos = ((tresMinutos / 8));
                    double kb = ((double)(totalBytesRead / 1000) / 1000);

                    if (kb >= tresMinutos)
                    {
                        percDownload = 100;
                        countTotalSeg++;
                        //await ms.WriteAsync(buffer, 0, read);
                        break;
                    }

                    //if (countTotalSeg == 3000) 
                    //{
                    //    percDownload = 100;
                    //    await ms.WriteAsync(buffer, 0, read);
                    //    break;
                    //}

                    //DateTime t2 = DateTime.Now;
                    //TimeSpan diff = t2 - t1;

                    if (readCount % 100 == 0)
                    {
                        //UInt32 S = BitConverter.ToUInt32(ms.ToArray(), 8); // whole seconds
                        //UInt32 F = BitConverter.ToUInt32(ms.ToArray(), 5); // byte 5-8 (highest byte not needed)
                        //int ms1 = (int)Math.Round(((double)(F & 0xffffff) * 1000) / Math.Pow(2, 24), 2); // milliseconds (cut highest byte, do conversion to milliseconds)

                        //if (ms1 >= ((1000 * 60) * 3))
                        //{
                        //}

                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        //percDownload = 100;
                        //break;
                    }

                    await ms.WriteAsync(buffer, 0, read);
                }

                if (percDownload == 100)
                {
                    TriggerProgressChanged(totalDownloadSize, totalBytesRead);

                    using (FileStream fileStream = File.Create("teste.mp4"))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.CopyTo(fileStream);
                    }

                    //using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(null)))
                    //{
                    //    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    //    {
                    //        waveOut.Init(blockAlignedStream);
                    //        waveOut.Play();

                    //        while (waveOut.PlaybackState == PlaybackState.Playing)
                    //        {
                    //            System.Threading.Thread.Sleep(100);
                    //        }
                    //    }
                    //}

                    byte[] compressedMusic = await GZipCompress(ms.ToArray());

                    //TriggerDownloadComplete(percDownload, compressedMusic);
                }
            }
        }
        public async Task<byte[]> GZipCompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException();

            using (MemoryStream ms = new MemoryStream(inputData))
            {
                using (var compressIntoMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionMode.Compress), BUFFER_SIZE))
                    {
                        await gzs.WriteAsync(ms.ToArray(), 0, ms.ToArray().Length);
                    }

                    return compressIntoMs.ToArray();
                }
            }
        }
        public async Task<byte[]> GZipDescompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException();

            using (var compressedMs = new MemoryStream(inputData))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), BUFFER_SIZE))
                    {
                        await gzs.CopyToAsync(decompressedMs);
                    }

                    return decompressedMs.ToArray();
                }
            }
        }
        private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
        {
            if (ProgressChanged == null)
                return;

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }
        private void TriggerDownloadComplete(double porcentagem, byte[] compressedMusic)
        {
            if (porcentagem == 100)
            {
                DownloadComplete(compressedMusic);
            }
        }
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
