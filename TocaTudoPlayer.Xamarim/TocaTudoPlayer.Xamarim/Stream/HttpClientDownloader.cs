using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class HttpClientDownloader : IDisposable
    {
        private HttpClient _httpClient;
        private const int BUFFER_SIZE = 64 * 1024;

        private const float MAX_FILE_SIZE_COMPRESS = 200;

        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);
        public event ProgressChangedHandler ProgressChanged;

        public delegate void DownloadStartedHandler();
        public event DownloadStartedHandler DownloadStarted;

        public delegate void DownloadCompleteHandler((bool, byte[]) compressedMusic);
        public event DownloadCompleteHandler DownloadComplete;
        public async Task Download(string downloadUrl)
        {
            _httpClient = new HttpClient(DependencyService.Get<IHTTPClientHandlerCreationService>().GetInsecureHandler())
            {
                Timeout = TimeSpan.FromDays(1),
            };

            using (HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                await DownloadFileFromHttpResponseMessage(response);
            }
        }
        public async Task<(bool, byte[])> GZipCompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException();

            float fileSize = (float)Math.Round((float)(inputData.Length / 1048576.00), 2);

            if (fileSize >= MAX_FILE_SIZE_COMPRESS)
                return (false, inputData);

            using (MemoryStream ms = new MemoryStream(inputData))
            {
                using (MemoryStream compressIntoMs = new MemoryStream())
                {
                    using (BufferedStream gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionMode.Compress), BUFFER_SIZE))
                    {
                        await gzs.WriteAsync(ms.ToArray(), 0, ms.ToArray().Length);
                    }

                    return (true, compressIntoMs.ToArray());
                }
            }
        }
        public static async Task<byte[]> GZipDescompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException();

            float fileSize = (float)Math.Round((float)(inputData.Length / 1048576.00), 2);

            if (fileSize >= MAX_FILE_SIZE_COMPRESS)
                return inputData;

            using (MemoryStream compressedMs = new MemoryStream(inputData))
            {
                using (MemoryStream decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), BUFFER_SIZE))
                    {
                        await gzs.CopyToAsync(decompressedMs);
                    }

                    return decompressedMs.ToArray();
                }
            }
        }
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        #region Metodos Privados
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
            bool downloadStarted = false;
            double percDownload = 0;
            long totalBytesRead = 0;
            long readCount = 0L;
            byte[] buffer = new byte[1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    if (read == 0)
                    {
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    totalBytesRead += (int)read;
                    readCount += 1;
                    percDownload = Math.Round((double)(totalBytesRead / totalDownloadSize.Value) * 100, 2);

                    if (!downloadStarted)
                    {
                        TriggerDownloadStarted(true);
                        downloadStarted = true;
                    }

                    if (readCount % 100 == 0)
                    {
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                    }

                    await ms.WriteAsync(buffer, 0, read);
                }

                if (percDownload >= 100)
                {
                    (bool, byte[]) compressedMusic = await GZipCompress(ms.ToArray());
                    TriggerDownloadComplete(percDownload, compressedMusic);
                }
            }
        }
        private void TriggerDownloadStarted(bool iniciado)
        {
            DownloadStarted();
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
        private void TriggerDownloadComplete(double porcentagem, (bool, byte[]) compressedMusic)
        {
            if (porcentagem == 100)
            {
                DownloadComplete(compressedMusic);
            }
        }
        #endregion
    }
}
