using Android.Content;
using Android.OS;
using System;
using TocaTudoPlayer.Xamarim;
using static TocaTudo.AudioService;
using static TocaTudo.AudioServiceBinder;

namespace TocaTudo
{
    public class AudioServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private event Action _playerInitializing;
        private event Action _playerReady;
        private event Action _playerReadyBuffering;
        private event Action _playerSeekComplete;
        private event Action<PlaylistItemServicePlayer> _playerPlaylistChanged;
        private event Action _playerInvalidUri;
        private event Action _playerLosedAudioFocus;

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
        public event Action PlayerReady
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
        public event Action PlayerReadyBuffering
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
        public event Action<PlaylistItemServicePlayer> PlayerPlaylistChanged
        {
            add => _playerPlaylistChanged += value;
            remove => _playerPlaylistChanged -= value;
        }
        public event Action PlayerInvalidUri
        {
            add => _playerInvalidUri += value;
            remove => _playerInvalidUri -= value;
        }
        public event Action PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus += value;
            remove => _playerLosedAudioFocus -= value;
        }

        private AudioServiceBinder _binder;
        private MainActivity _activity;
        public AudioServiceBinder Binder => _binder;
        public AudioServiceConnection(MainActivity activity)
        {
            _activity = activity;
        }
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _binder = service as AudioServiceBinder;

            if (_activity.Binder == null)
            {
                _activity.Binder = _binder;
                _binder.PlayerInitializing += () =>
                {
                    if (_playerInitializing != null)
                        _playerInitializing();
                };

                _binder.PlayerReady += () =>
                {
                    if (_playerReady != null)
                        _playerReady();
                };

                _binder.PlayerReadyBuffering += () =>
                {
                    if (_playerReadyBuffering != null)
                        _playerReadyBuffering();
                };

                _binder.PlayerSeekComplete += () =>
                {
                    if (_playerSeekComplete != null)
                        _playerSeekComplete();
                };

                _binder.PlayerPlaylistChanged += (obj) =>
               {
                   if (_playerPlaylistChanged != null)
                       _playerPlaylistChanged(obj);
               };

                _binder.PlayerInvalidUri += () =>
                {
                    if (_playerInvalidUri != null)
                        _playerInvalidUri();
                };

                _binder.PlayerLosedAudioFocus += () =>
                {
                    if (_playerLosedAudioFocus != null)
                        _playerLosedAudioFocus();
                };
            }
        }
        public void OnServiceDisconnected(ComponentName name)
        {
            name.Dispose();
        }
    }
}