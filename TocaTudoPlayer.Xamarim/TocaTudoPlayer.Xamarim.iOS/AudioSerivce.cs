using AudioToolbox;
using AVFoundation;
using CoreMedia;
using Foundation;
using System;
using System.IO;
using TocaTudoPlayer.Xamarim.iOS;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioSerivce))]
namespace TocaTudoPlayer.Xamarim.iOS
{
    public class AudioSerivce : IAudio
    {
        private AVPlayer _player;
        private ICommonMusicModel _music;
        private WeakEventManager _playerInitializing;
        private WeakEventManager<ICommonMusicModel> _playerReady;
        private WeakEventManager<ICommonMusicModel> _playerReadyBuffering;
        private WeakEventManager _playerSeekComplete;
        private WeakEventManager<PlaylistItemServicePlayer> _playerPlaylistChanged;
        private WeakEventManager<ICommonMusicModel> _playerInvalidUri;
        private WeakEventManager<string> _playerAlbumInvalidUri;
        private WeakEventManager<ICommonMusicModel> _playerMusicInvalidUri;
        private WeakEventManager _playerLosedAudioFocus;

        public bool IsPlaying
        {
            get { return _player == null ? false : _player.TimeControlStatus == AVPlayerTimeControlStatus.Playing; }
        }

        public bool EventsBinded => throw new NotImplementedException();
        public event EventHandler PlayerInitializing
        {
            add => _playerInitializing.AddEventHandler(value);
            remove => _playerInitializing.RemoveEventHandler(value);
        }
        public event EventHandler<ICommonMusicModel> PlayerReady
        {
            add => _playerReady.AddEventHandler(value);
            remove => _playerReady.RemoveEventHandler(value);
        }
        public event EventHandler PlayerSeekComplete
        {
            add => _playerSeekComplete.AddEventHandler(value);
            remove => _playerSeekComplete.RemoveEventHandler(value);
        }
        public event EventHandler<ICommonMusicModel> PlayerReadyBuffering
        {
            add => _playerReadyBuffering.AddEventHandler(value);
            remove => _playerReadyBuffering.RemoveEventHandler(value);
        }
        public event EventHandler<PlaylistItemServicePlayer> PlayerPlaylistChanged
        {
            add => _playerPlaylistChanged.AddEventHandler(value);
            remove => _playerPlaylistChanged.RemoveEventHandler(value);
        }
        public event EventHandler<string> PlayerAlbumInvalidUri
        {
            add => _playerAlbumInvalidUri.AddEventHandler(value);
            remove => _playerAlbumInvalidUri.RemoveEventHandler(value);
        }
        public event EventHandler<ICommonMusicModel> PlayerMusicInvalidUri
        {
            add => _playerMusicInvalidUri.AddEventHandler(value);
            remove => _playerMusicInvalidUri.RemoveEventHandler(value);
        }
        public event EventHandler PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus.AddEventHandler(value);
            remove => _playerLosedAudioFocus.RemoveEventHandler(value);
        }
        public void Source(string url, string videoId)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);

            if (_player == null)
            {
                _player = AVPlayer.FromUrl(NSUrl.FromString(url));
                _player.AddObserver("status", NSKeyValueObservingOptions.New, (NSObservedChange obj) =>
                {
                    if (_player.Status == AVPlayerStatus.ReadyToPlay)
                    {
                        _playerReady.HandleEvent(this, null, nameof(PlayerReady));
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
                        _playerReady.HandleEvent(this, _music, nameof(PlayerReady));
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
                        _playerReady.HandleEvent(this, null, nameof(PlayerReady));
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
                        _playerReady.HandleEvent(this, music, nameof(PlayerReady));
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

        public void Source(string url, string videoId, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, string videoId, MusicModelServicePlayer musicServicePlayer)
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

        public void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, string videoId, MusicStatusBottomModel musicStatusBottom, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, string videoId, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, AlbumModelServicePlayer albumServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Source(byte[] music, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom)
        {
            throw new NotImplementedException();
        }

        public void Source(byte[] music, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer)
        {
            throw new NotImplementedException();
        }

        public void Next(PlaylistItemServicePlayer playlistItem)
        {
            throw new NotImplementedException();
        }
    }
}