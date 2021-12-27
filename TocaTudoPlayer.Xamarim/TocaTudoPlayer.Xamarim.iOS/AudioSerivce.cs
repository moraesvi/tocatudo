using AudioToolbox;
using AVFoundation;
using CoreMedia;
using Foundation;
using System;
using System.IO;
using TocaTudoPlayer.Xamarim.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioSerivce))]
namespace TocaTudoPlayer.Xamarim.iOS
{
    public class AudioSerivce : IAudio
    {
        private AVPlayer _player;
        private ICommonMusicModel _music;
        private Action _playerInitializing;
        private Action<ICommonMusicModel> _playerReady;
        private Action<ICommonMusicModel> _playerReadyBuffering;
        private Action _playerSeekComplete;
        private Action<PlaylistItemServicePlayer> _playerPlaylistChanged;
        private Action<ICommonMusicModel> _playerInvalidUri;

        public event Action PlayerLosedAudioFocus;

        public bool IsPlaying
        {
            get { return _player == null ? false : _player.TimeControlStatus == AVPlayerTimeControlStatus.Playing; }
        }

        public bool EventsBinded => throw new NotImplementedException();

        public event Action PlayerInitializing
        {
            add
            {
                _playerInitializing += value;
            }
            remove
            {
                _playerInitializing -= value;
            }
        }
        public event Action<ICommonMusicModel> PlayerReady
        {
            add
            {
                _playerReady += value;
            }
            remove
            {
                _playerReady -= value;
            }
        }
        public event Action PlayerSeekComplete
        {
            add
            {
                _playerSeekComplete += value;
            }
            remove
            {
                _playerSeekComplete -= value;
            }
        }
        public event Action<ICommonMusicModel> PlayerReadyBuffering
        {
            add
            {
                _playerReadyBuffering += value;
            }
            remove
            {
                _playerReadyBuffering -= value;
            }
        }
        public event Action<PlaylistItemServicePlayer> PlayerPlaylistChanged
        {
            add => _playerPlaylistChanged += value;
            remove => _playerPlaylistChanged -= value;
        }
        public event Action<ICommonMusicModel> PlayerInvalidUri
        {
            add => _playerInvalidUri += value;
            remove => _playerInvalidUri -= value;
        }
        public void Source(string url)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);

            if (_player == null)
            {
                _player = AVPlayer.FromUrl(NSUrl.FromString(url));
                _player.AddObserver("status", NSKeyValueObservingOptions.New, (NSObservedChange obj) =>
                {
                    if (_player.Status == AVPlayerStatus.ReadyToPlay)
                    {
                        _playerReady(null);
                    }
                });
            }

            _player.Play();
        }
        public void Source(string url, ICommonMusicModel music)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);

            _music = music;

            if (_player == null)
            {
                _player = AVPlayer.FromUrl(NSUrl.FromString(url));
                _player.AddObserver("status", NSKeyValueObservingOptions.New, (NSObservedChange obj) =>
                {
                    if (_player.Status == AVPlayerStatus.ReadyToPlay)
                    {
                        _playerReady(music);
                    }
                });
            }

            _player.Play();
        }
        public void Source(byte[] music)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);
            NSData nsdata = NSData.FromArray(music);

            string fileName = string.Format("Myfile{0}.mp4", "saket");
            string urlPath = Path.Combine(Path.GetTempPath(), fileName);

            NSUrl audioFilePath = NSUrl.FromFilename(urlPath);
            NSError err;
            nsdata.Save(audioFilePath, false, out err);

            if (_player == null)
            {
                _player = new AVPlayer(audioFilePath);
                _player.AddObserver("status", NSKeyValueObservingOptions.New, (NSObservedChange obj) =>
                {
                    if (_player.Status == AVPlayerStatus.ReadyToPlay)
                    {
                        _playerReady(null);
                    }
                });
            }

            _player.Play();
        }
        public void Source(byte[] byteMusic, ICommonMusicModel music)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);
            NSData nsdata = NSData.FromArray(byteMusic);

            _music = music;

            string fileName = string.Format("Myfile{0}.mp4", "saket");
            string urlPath = Path.Combine(Path.GetTempPath(), fileName);

            NSUrl audioFilePath = NSUrl.FromFilename(urlPath);
            NSError err;
            nsdata.Save(audioFilePath, false, out err);

            if (_player == null)
            {
                _player = new AVPlayer(audioFilePath);
                _player.AddObserver("status", NSKeyValueObservingOptions.New, (NSObservedChange obj) =>
                {
                    if (_player.Status == AVPlayerStatus.ReadyToPlay)
                    {
                        _playerReady(music);
                    }
                });
            }

            _player.Play();
        }
        public bool Play()
        {
            _player.Play();
            return true;
        }
        public void Pause()
        {
            _player.Pause();
        }
        public void Seek(int milisegundos)
        {
            _player.SeekAsync(CMTime.FromSeconds(milisegundos, 0));
        }
        public long Max()
        {
            return (long)_player.CurrentItem.Asset.Duration.Seconds;
        }
        public long CurrentPosition()
        {
            return (long)_player.CurrentTime.Seconds;
        }     
        public bool Stop()
        {
            _player.Rate = 0;
            return true;
        }
        private void PlayerAttributes()
        {
            //AudioAttributes attr = new AudioAttributes.Builder().SetFlags(AudioFlags.LowLatency).SetLegacyStreamType(Stream.Music).SetContentType(AudioContentType.Music).SetUsage(AudioUsageKind.Media).Build();
            //_player.SetAudioAttributes(attr);

            //if (HeadphonesPluggedIn())
            //    PlayAudioOnEarPhone();
            //else
            //    PlayAudioOnSpeaker();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Source(string url, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, SearchMusicModel searchMusic, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(byte[] music, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Seek(int milisegundos, PlaylistItemServicePlayer playlistItem)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, SearchMusicModel searchMusic, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(byte[] music, SearchMusicModel searchMusic, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Seek(int milisegundos, MusicModelServicePlayer musicModel)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, ICommonMusicModel musicPlayer, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, ICommonMusicModel musicPlayer, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(byte[] music, ICommonMusicModel musicPlayer, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }
    }
}