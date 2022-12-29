using MarcTron.Plugin;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using YoutubeExplode;

namespace TocaTudoPlayer.Xamarim
{
    public class HistoryMusicModel : MusicModelBase
    {
        private bool _musicIsEnabled;
        private bool _downloadMusicButtonFormIsVisible;
        private int _formDownloadSize;
        public HistoryMusicModel(CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool hasAlbumSaved, bool isSavedOnLocalDb)
            : base(formDownloadViewModel, tocaTudoApi, ytClient, hasAlbumSaved, isSavedOnLocalDb)
        {
            IsSavedOnLocalDb = isSavedOnLocalDb;
            TextColorMusic = "White";

            MusicAlbumPopupModel = new MusicAlbumDialogDataModel(musicHasAlbumSaved: hasAlbumSaved);

            _musicIsEnabled = true;
            _formDownloadSize = 0;
            _downloadMusicButtonFormIsVisible = !isSavedOnLocalDb;

            Download.DownloadComplete += HistoryMusicModel_DownloadCompleteEvent;
        }
        public HistoryMusicModel(UserMusicAlbumSelect musicAlbumSelected, CommonFormDownloadViewModel formDownloadViewModel, ITocaTudoApi tocaTudoApi, YoutubeClient ytClient, bool hasAlbumSaved, bool isSavedOnLocalDb)
            : base(musicAlbumSelected, formDownloadViewModel, tocaTudoApi, ytClient, hasAlbumSaved, isSavedOnLocalDb)
        {
            IsSavedOnLocalDb = isSavedOnLocalDb;
            TextColorMusic = "White";

            _musicIsEnabled = true;
            _formDownloadSize = 0;
            _downloadMusicButtonFormIsVisible = !isSavedOnLocalDb;

            Download.DownloadComplete += HistoryMusicModel_DownloadCompleteEvent;
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
        public override Task<DownloadQueueStatus> StartDownloadMusic(MusicPlayedHistoryViewModel musicPlayedHistoryViewModel)
        {
            MusicIsEnabled = false;
            return StartDownloadMusic(ByteMusicImage, musicPlayedHistoryViewModel);
        }
        private void HistoryMusicModel_DownloadCompleteEvent(object sender, (bool, byte[], object) tpMusic)
        {
            MusicIsEnabled = true;
            DownloadMusicButtonFormIsVisible = false;
        }
    }
}
