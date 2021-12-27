using MarcTron.Plugin;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using YoutubeParse.ExplodeV2;

namespace TocaTudoPlayer.Xamarim
{
    public class HistoryMusicModel : MusicBaseViewModel
    {
        private byte[] _byteImgMusic;
        private bool _musicIsEnabled;
        private bool _downloadMusicButtonFormIsVisible;
        private int _formDownloadSize;
        private Action _downloadComplete;
        public HistoryMusicModel(ICommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool isSavedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient)
        {
            base.IsSavedOnLocalDb = isSavedOnLocalDb;

            _musicIsEnabled = true;
            _formDownloadSize = 0;
            _downloadMusicButtonFormIsVisible = isSavedOnLocalDb ? false : true;

            Download.DownloadComplete += HistoryMusicModel_DownloadCompleteEvent;
        }
        public byte[] ByteImgMusic
        {
            get { return _byteImgMusic; }
            set
            {
                _byteImgMusic = value;
            }
        }
        public bool MusicIsEnabled
        {
            get { return _musicIsEnabled; }
            set
            {
                _musicIsEnabled = value;
                OnPropertyChanged(nameof(MusicIsEnabled));
            }
        }
        public int FormDownloadSize
        {
            get { return _formDownloadSize; }
            set
            {
                _formDownloadSize = value;
                OnPropertyChanged(nameof(FormDownloadSize));
            }
        }
        public bool DownloadMusicButtonFormIsVisible
        {
            get { return _downloadMusicButtonFormIsVisible; }
            set
            {
                _downloadMusicButtonFormIsVisible = value;
                OnPropertyChanged(nameof(DownloadMusicButtonFormIsVisible));
            }
        }
        public override Task StartDownloadMusic()
        {
            MusicIsEnabled = false;
            return base.StartDownloadMusic(ByteImgMusic);
        }
        private void HistoryMusicModel_DownloadCompleteEvent((bool, byte[]) compressedMusic, object model)
        {
            MusicIsEnabled = true;
            DownloadMusicButtonFormIsVisible = false;
        }
    }
}
