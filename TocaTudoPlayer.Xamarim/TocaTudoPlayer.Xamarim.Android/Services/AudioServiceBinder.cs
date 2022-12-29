using Android.OS;
using System;
using TocaTudoPlayer.Xamarim;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;
using static TocaTudo.AudioService;

namespace TocaTudo
{
    public class AudioServiceBinder : Binder
    {
        private AudioService _service;

        private readonly WeakEventManager _playerInitializing;
        private readonly WeakEventManager _playerReady;
        private readonly WeakEventManager _playerReadyBuffering;
        private readonly WeakEventManager _playerSeekComplete;
        private readonly WeakEventManager<bool> _playingChanged;
        private readonly WeakEventManager<ItemServicePlayer> _playerPlaylistChanged;
        private readonly WeakEventManager _playerInvalidUri;
        private readonly WeakEventManager _playerLosedAudioFocus;

        public event EventHandler PlayerInitializing
        {
            add => _playerInitializing.AddEventHandler(value);
            remove => _playerInitializing.RemoveEventHandler(value);
        }
        public event EventHandler PlayerReady
        {
            add => _playerReady.AddEventHandler(value);
            remove => _playerReady.RemoveEventHandler(value);
        }
        public event EventHandler PlayerReadyBuffering
        {
            add => _playerReadyBuffering.AddEventHandler(value);
            remove => _playerReadyBuffering.RemoveEventHandler(value);
        }
        public event EventHandler PlayerSeekComplete
        {
            add => _playerSeekComplete.AddEventHandler(value);
            remove => _playerSeekComplete.RemoveEventHandler(value);
        }
        public event EventHandler<bool> PlayingChanged
        {
            add => _playingChanged.AddEventHandler(value);
            remove => _playingChanged.RemoveEventHandler(value);
        }
        public event EventHandler<ItemServicePlayer> PlayerPlaylistChanged
        {
            add => _playerPlaylistChanged.AddEventHandler(value);
            remove => _playerPlaylistChanged.RemoveEventHandler(value);
        }
        public event EventHandler PlayerInvalidUri
        {
            add => _playerInvalidUri.AddEventHandler(value);
            remove => _playerInvalidUri.RemoveEventHandler(value);
        }
        public event EventHandler PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus.AddEventHandler(value);
            remove => _playerLosedAudioFocus.RemoveEventHandler(value);
        }
        public AudioServiceBinder(AudioService service)
        {
            _service = service;

            _playerInitializing = new WeakEventManager();
            _playerReady = new WeakEventManager();
            _playerReadyBuffering = new WeakEventManager();
            _playerSeekComplete = new WeakEventManager();
            _playingChanged = new WeakEventManager<bool>();
            _playerPlaylistChanged = new WeakEventManager<ItemServicePlayer>();
            _playerInvalidUri = new WeakEventManager();
            _playerLosedAudioFocus = new WeakEventManager();

            _service.PlayerInitializing -= Service_PlayerInitializing;
            _service.PlayerReady -= Service_PlayerReady;
            _service.PlayerReadyBuffering -= Service_PlayerReadyBuffering;
            _service.PlayerSeekComplete -= Service_PlayerSeekComplete;
            _service.PlayingChanged -= Service_PlayingChanged;
            _service.PlayerPlaylistChanged -= Service_PlayerPlaylistChanged;
            _service.PlayerInvalidUri -= Service_PlayerInvalidUri;
            _service.PlayerLosedAudioFocus -= Service_PlayerLosedAudioFocus;

            _service.PlayerInitializing += Service_PlayerInitializing;
            _service.PlayerReady += Service_PlayerReady;
            _service.PlayerReadyBuffering += Service_PlayerReadyBuffering;
            _service.PlayerSeekComplete += Service_PlayerSeekComplete;
            _service.PlayingChanged += Service_PlayingChanged;
            _service.PlayerPlaylistChanged += Service_PlayerPlaylistChanged;
            _service.PlayerInvalidUri += Service_PlayerInvalidUri;
            _service.PlayerLosedAudioFocus += Service_PlayerLosedAudioFocus;
        }

        private void Service_PlayerInitializing(object sender, EventArgs e)
        {
            if (_playerInitializing != null)
                _playerInitializing.RaiseEvent(sender, null, nameof(PlayerInitializing));
        }
        private void Service_PlayerReady(object sender, EventArgs e)
        {
            if (_playerReady != null)
                _playerReady.RaiseEvent(sender, null, nameof(PlayerReady));
        }
        private void Service_PlayerReadyBuffering(object sender, EventArgs e)
        {
            if (_playerReadyBuffering != null)
                _playerReadyBuffering.RaiseEvent(sender, null, nameof(PlayerReadyBuffering));
        }
        private void Service_PlayerSeekComplete(object sender, EventArgs e)
        {
            if (_playerSeekComplete != null)
                _playerSeekComplete.RaiseEvent(sender, null, nameof(PlayerSeekComplete));
        }
        private void Service_PlayerPlaylistChanged(object sender, ItemServicePlayer obj)
        {
            if (_playerPlaylistChanged != null)
                _playerPlaylistChanged.RaiseEvent(sender, obj, nameof(PlayerPlaylistChanged));
        }
        private void Service_PlayingChanged(object sender, bool playing)
        {
            if (_playingChanged != null)
                _playingChanged.RaiseEvent(sender, playing, nameof(PlayingChanged));
        }
        private void Service_PlayerInvalidUri(object sender, EventArgs e)
        {
            if (_playerInvalidUri != null)
                _playerInvalidUri.RaiseEvent(sender, null, nameof(PlayerInvalidUri));
        }
        private void Service_PlayerLosedAudioFocus(object sender, EventArgs e)
        {
            if (_playerLosedAudioFocus != null)
                _playerLosedAudioFocus.RaiseEvent(sender, null, nameof(PlayerLosedAudioFocus));
        }
        public AudioService GetBackgroundService()
        {
            return _service;
        }
    }
}