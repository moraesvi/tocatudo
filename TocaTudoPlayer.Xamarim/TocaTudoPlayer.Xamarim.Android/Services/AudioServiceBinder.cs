using Android.OS;
using System;
using TocaTudoPlayer.Xamarim;
using static TocaTudo.AudioService;

namespace TocaTudo
{
    public class AudioServiceBinder : Binder
    {
        private AudioService _service;

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
        public AudioServiceBinder(AudioService service)
        {
            _service = service;

            _service.PlayerInitializing += Service_PlayerInitializing;
            _service.PlayerReady += Service_PlayerReady;
            _service.PlayerReadyBuffering += Service_PlayerReadyBuffering;
            _service.PlayerSeekComplete += Service_PlayerSeekComplete;
            _service.PlayerPlaylistChanged += Service_PlayerPlaylistChanged;
            _service.PlayerInvalidUri += Service_PlayerInvalidUri;
            _service.PlayerLosedAudioFocus += Service_PlayerLosedAudioFocus;
        }
        private void Service_PlayerInitializing()
        {
            if (_playerInitializing != null)
                _playerInitializing();
        }
        private void Service_PlayerReady()
        {
            if (_playerReady != null)
                _playerReady();
        }
        private void Service_PlayerReadyBuffering()
        {
            if (_playerReadyBuffering != null)
                _playerReadyBuffering();
        }
        private void Service_PlayerSeekComplete()
        {
            _playerSeekComplete();
        }
        private void Service_PlayerPlaylistChanged(PlaylistItemServicePlayer obj)
        {
            if (_playerPlaylistChanged != null)
                _playerPlaylistChanged(obj);
        }
        private void Service_PlayerInvalidUri()
        {
            if (_playerInvalidUri != null)
                _playerInvalidUri();
        }
        private void Service_PlayerLosedAudioFocus() 
        {
            if (_playerLosedAudioFocus != null)
                _playerLosedAudioFocus();
        }
        public AudioService GetBackgroundService()
        {
            return _service;
        }
    }
}