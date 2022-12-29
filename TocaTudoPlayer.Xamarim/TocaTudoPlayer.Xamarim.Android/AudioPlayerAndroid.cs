using Android.Content;
using Plugin.CurrentActivity;
using System;
using TocaTudo;
using TocaTudo.Helper;
using TocaTudoPlayer.Xamarim;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayerAndroid))]
namespace TocaTudo
{
    public class AudioPlayerAndroid : IAudio
    {
        private readonly string _startForegroundServiceDefaultExceptionMsg = $"{AppResource.StartForegroundServiceDefaultExceptionMsg} - {AppResource.TryAgainLabel}";
        private AudioServiceBinder _audioServiceConnection;

        private readonly WeakEventManager _playerInitializing;
        private readonly WeakEventManager<ICommonMusicModel> _playerReady;
        private readonly WeakEventManager<ICommonMusicModel> _playerReadyBuffering;
        private readonly WeakEventManager _playerSeekComplete;
        private readonly WeakEventManager<bool> _playingChanged;
        private readonly WeakEventManager<ItemServicePlayer> _playerAlbumPlaylistChanged;
        private readonly WeakEventManager<ItemServicePlayer> _playerAlbumMusicPlaylistChanged;
        private readonly WeakEventManager<string> _playerAlbumInvalidUri;
        private readonly WeakEventManager<ICommonMusicModel> _playerMusicInvalidUri;
        private readonly WeakEventManager _playerLosedAudioFocus;
        private readonly WeakEventManager<string> _playerException;

        private ICommonMusicModel _musicPlaying;
        private MusicStatusBottomModel _musicStatusBottom;
        private bool _eventsIsBound;
        private bool _isPlaying;
        private string _videoId;
        private MainActivity _mainActivity;

        public AudioPlayerAndroid()
        {
            AudioService.Init();
            AudioService.PlayerException += AudioService_PlayerException;

            _playerInitializing = new WeakEventManager();
            _playerReady = new WeakEventManager<ICommonMusicModel>();
            _playerReadyBuffering = new WeakEventManager<ICommonMusicModel>();
            _playerSeekComplete = new WeakEventManager();
            _playingChanged = new WeakEventManager<bool>();
            _playerAlbumPlaylistChanged = new WeakEventManager<ItemServicePlayer>();
            _playerAlbumMusicPlaylistChanged = new WeakEventManager<ItemServicePlayer>();
            _playerAlbumInvalidUri = new WeakEventManager<string>();
            _playerMusicInvalidUri = new WeakEventManager<ICommonMusicModel>();
            _playerLosedAudioFocus = new WeakEventManager();
            _playerException = new WeakEventManager<string>();
        }
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
        public event EventHandler<bool> PlayingChanged
        {
            add => _playingChanged.AddEventHandler(value);
            remove => _playingChanged.RemoveEventHandler(value);
        }
        public event EventHandler<ICommonMusicModel> PlayerReadyBuffering
        {
            add => _playerReadyBuffering.AddEventHandler(value);
            remove => _playerReadyBuffering.RemoveEventHandler(value);
        }
        public event EventHandler<ItemServicePlayer> PlayerAlbumPlaylistChanged
        {
            add => _playerAlbumPlaylistChanged.AddEventHandler(value);
            remove => _playerAlbumPlaylistChanged.RemoveEventHandler(value);
        }
        public event EventHandler<ItemServicePlayer> PlayerAlbumMusicPlaylistChanged
        {
            add => _playerAlbumMusicPlaylistChanged.AddEventHandler(value);
            remove => _playerAlbumMusicPlaylistChanged.RemoveEventHandler(value);
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
        public event EventHandler<string> PlayerException
        {
            add => _playerException.AddEventHandler(value);
            remove => _playerException.RemoveEventHandler(value);
        }
        public bool EventsBinded => _audioServiceConnection != null;
        public bool IsPlaying => GetIsPlaying();
        public long CurrentPosition() => _audioServiceConnection?.GetBackgroundService()
                                                                ?.CurrentPosition() ?? -1;
        public void Start()
        {
            _eventsIsBound = false;

            MainActivity main = CrossCurrentActivity.Current.Activity as MainActivity;

            if (main != null && !_eventsIsBound)
            {
                _mainActivity = main;
                _mainActivity.BinderConnected -= MainActivity_BinderConnected;
                _mainActivity.BinderConnected += MainActivity_BinderConnected;

                _eventsIsBound = true;
            }
        }
        public long Max() => AppDroidHelper.ExoplayerTimeToTocaTudo(_audioServiceConnection?.GetBackgroundService()?.Max() ?? -1);
        public void Pause()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionPause);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);

            _musicPlaying?.UpdateMusicPlayingIcon();
        }
        public bool Play()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionPlay);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);

            _musicPlaying?.UpdateMusicPlayingIcon();

            return true;
        }
        public bool Stop()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionStop);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);

            _musicPlaying?.UpdateMusicPlayingIcon();

            return true;
        }
        public void Seek(long milisegundos)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSeek);
            intent.PutExtra("seek", milisegundos);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Seek(long milisegundos, ItemServicePlayer playlistItem)
        {
            Context context = Android.App.Application.Context.ApplicationContext;
            Intent intent = new Intent(context, typeof(AudioService));
            intent.SetAction(AudioService.ActionSeek);
            intent.PutExtra("seek", milisegundos);

            MainActivity.PlaylistItemServicePlayerParameter = playlistItem;

            BindAudioServiceEvents();
            StartForegroundService(context, intent);
        }
        public void Seek(long milisegundos, MusicModelServicePlayer musicModel)
        {
            Context context = Android.App.Application.Context.ApplicationContext;
            Intent intent = new Intent(context, typeof(AudioService));
            intent.SetAction(AudioService.ActionSeek);
            intent.PutExtra("seek", milisegundos);

            MainActivity.AlbumModelServicePlayerParameter = null;
            MainActivity.MusicModelServicePlayerParameter = musicModel;

            BindAudioServiceEvents();
            StartForegroundService(context, intent);
        }
        public void Next(ItemServicePlayer playlistItem)
        {
            Context context = Android.App.Application.Context.ApplicationContext;
            Intent intent = new Intent(context, typeof(AudioService));
            intent.SetAction(AudioService.ActionNext);

            MainActivity.PlaylistItemServicePlayerParameter = playlistItem;

            BindAudioServiceEvents();
            StartForegroundService(context, intent);
        }
        public void Source(AlbumMusicModelServicePlayer musicServicePlayer)
        {
            MainActivity.AlbumModelServicePlayerParameter = null;
            MainActivity.MusicModelServicePlayerParameter = null;
            MainActivity.AlbumMusicModelServicePlayerParameter = musicServicePlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, string videoId)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            _videoId = videoId;

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom)
        {
            _musicPlaying = musicPlayer;
            _musicStatusBottom = musicStatusBottom;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, string videoId, MusicStatusBottomModel musicStatusBottom, AlbumModelServicePlayer albumServicePlayer)
        {
            _videoId = videoId;
            _musicStatusBottom = musicStatusBottom;
            _musicPlaying = null;

            MainActivity.MusicModelServicePlayerParameter = null;
            MainActivity.AlbumMusicModelServicePlayerParameter = null;
            MainActivity.AlbumModelServicePlayerParameter = albumServicePlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, string videoId, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer)
        {
            _videoId = videoId;
            _musicStatusBottom = musicStatusBottom;

            MainActivity.AlbumModelServicePlayerParameter = null;
            MainActivity.AlbumMusicModelServicePlayerParameter = null;
            MainActivity.MusicModelServicePlayerParameter = musicServicePlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, AlbumModelServicePlayer albumServicePlayer)
        {
            MainActivity.MusicModelServicePlayerParameter = null;
            MainActivity.AlbumMusicModelServicePlayerParameter = null;
            MainActivity.AlbumModelServicePlayerParameter = albumServicePlayer;
            _musicPlaying = musicPlayer;
            _musicStatusBottom = musicStatusBottom;

            AppHelper.HasMusicTotalTime = _musicPlaying.MusicTimeTotalSeconds > 0;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer)
        {
            MainActivity.AlbumModelServicePlayerParameter = null;
            MainActivity.AlbumMusicModelServicePlayerParameter = null;
            MainActivity.MusicModelServicePlayerParameter = musicServicePlayer;

            _musicPlaying = musicPlayer;
            _musicStatusBottom = musicStatusBottom;

            AppHelper.HasMusicTotalTime = _musicPlaying.MusicTimeTotalSeconds > 0;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            _mainActivity.AudioServiceMusicParameter = music;

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music, AlbumModelServicePlayer albumServicePlayer)
        {
            _musicPlaying = null;
            _mainActivity.AudioServiceMusicParameter = music;
            MainActivity.MusicModelServicePlayerParameter = null;
            MainActivity.AlbumModelServicePlayerParameter = albumServicePlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom)
        {
            _musicPlaying = musicPlayer;
            _musicStatusBottom = musicStatusBottom;

            AppHelper.HasMusicTotalTime = _musicPlaying.MusicTimeTotalSeconds > 0;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            _mainActivity.AudioServiceMusicParameter = music;

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music, ICommonMusicModel musicPlayer, MusicStatusBottomModel musicStatusBottom, MusicModelServicePlayer musicServicePlayer)
        {
            _mainActivity.AudioServiceMusicParameter = music;

            MainActivity.AlbumModelServicePlayerParameter = null;
            MainActivity.AlbumMusicModelServicePlayerParameter = null;
            MainActivity.MusicModelServicePlayerParameter = musicServicePlayer;

            _musicPlaying = musicPlayer;
            _musicStatusBottom = musicStatusBottom;

            AppHelper.HasMusicTotalTime = _musicPlaying.MusicTimeTotalSeconds > 0;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            BindAudioServiceEvents();
            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void ClearAlbumSource()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionClearSource);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void HideStatusBarPlayerControls()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionHidePlayerControls);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }

        #region Private Methods
        private bool GetIsPlaying()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _isPlaying = _audioServiceConnection?.GetBackgroundService()
                                                   ?.IsPlaying ?? false;
            });
            return _isPlaying;
        }
        private void MainActivity_BinderConnected(object sender, EventArgs e)
        {
            _audioServiceConnection = _mainActivity.Binder;
            BindAudioServiceEvents();
        }
        private void BindAudioServiceEvents()
        {
            if (_audioServiceConnection == null)
                return;

            _audioServiceConnection.PlayerInitializing -= AudioServiceConnection_PlayerInitializing;
            _audioServiceConnection.PlayerInitializing += AudioServiceConnection_PlayerInitializing;
            _audioServiceConnection.PlayerReady -= AudioServiceConnection_PlayerReady;
            _audioServiceConnection.PlayerReady += AudioServiceConnection_PlayerReady;
            _audioServiceConnection.PlayerReadyBuffering -= AudioServiceConnection_PlayerReadyBuffering;
            _audioServiceConnection.PlayerReadyBuffering += AudioServiceConnection_PlayerReadyBuffering;
            _audioServiceConnection.PlayerSeekComplete -= AudioServiceConnection_PlayerSeekComplete;
            _audioServiceConnection.PlayerSeekComplete += AudioServiceConnection_PlayerSeekComplete;
            _audioServiceConnection.PlayingChanged -= AudioServiceConnection_PlayingChanged;
            _audioServiceConnection.PlayingChanged += AudioServiceConnection_PlayingChanged;
            _audioServiceConnection.PlayerPlaylistChanged -= AudioServiceConnection_PlayerPlaylistChanged;
            _audioServiceConnection.PlayerPlaylistChanged += AudioServiceConnection_PlayerPlaylistChanged;
            _audioServiceConnection.PlayerInvalidUri -= AudioServiceConnection_PlayerInvalidUri;
            _audioServiceConnection.PlayerInvalidUri += AudioServiceConnection_PlayerInvalidUri;
            _audioServiceConnection.PlayerLosedAudioFocus -= AudioServiceConnection_PlayerLosedAudioFocus;
            _audioServiceConnection.PlayerLosedAudioFocus += AudioServiceConnection_PlayerLosedAudioFocus;
        }
        private void AudioServiceConnection_PlayerInitializing(object sender, EventArgs e)
        {
            _playerInitializing.RaiseEvent(sender, null, nameof(PlayerInitializing));
        }
        private void AudioServiceConnection_PlayerReady(object sender, EventArgs e)
        {
            if (_musicPlaying != null)
            {
                if (!AppHelper.HasMusicTotalTime)
                {
                    _musicPlaying.MusicTimeTotalSeconds = Max();
                }
            }

            _playerReady.RaiseEvent(sender, _musicPlaying, nameof(PlayerReady));
        }
        private void AudioServiceConnection_PlayerReadyBuffering(object sender, EventArgs e)
        {
            _playerReadyBuffering.RaiseEvent(sender, _musicPlaying, nameof(PlayerReadyBuffering));
        }
        private void AudioServiceConnection_PlayerSeekComplete(object sender, EventArgs e)
        {
            _playerSeekComplete.RaiseEvent(sender, null, nameof(PlayerSeekComplete));
        }
        private void AudioServiceConnection_PlayingChanged(object sender, bool playing)
        {
            _musicStatusBottom?.MusicIsPlayingButton(playing);
        }
        private void AudioServiceConnection_PlayerPlaylistChanged(object sender, ItemServicePlayer obj)
        {
            if (MainActivity.AlbumModelServicePlayerParameter != null)
                _playerAlbumPlaylistChanged.RaiseEvent(sender, obj, nameof(PlayerAlbumPlaylistChanged));
            else
                _playerAlbumMusicPlaylistChanged.RaiseEvent(sender, obj, nameof(PlayerAlbumMusicPlaylistChanged));
        }
        private void AudioServiceConnection_PlayerInvalidUri(object sender, EventArgs e)
        {
            if (_musicPlaying == null)
                _playerAlbumInvalidUri.RaiseEvent(sender, _videoId, nameof(PlayerAlbumInvalidUri));
            else
                _playerMusicInvalidUri.RaiseEvent(sender, _musicPlaying, nameof(PlayerMusicInvalidUri));
        }
        private void AudioServiceConnection_PlayerLosedAudioFocus(object sender, EventArgs e)
        {
            if (_musicPlaying != null)
                _musicPlaying.IsPlaying = IsPlaying;
            _playerLosedAudioFocus.RaiseEvent(sender, null, nameof(PlayerLosedAudioFocus));
        }
        private void AudioService_PlayerException(object sender, Exception e)
        {
            _playerException.RaiseEvent(sender, e.Message, nameof(PlayerException));
        }
        private void StartForegroundService(Context context, Intent intent)
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    context.StartForegroundService(intent);
                }
                else
                {
                    context.StartService(intent);
                }
            }
            catch
            {
                _playerException.RaiseEvent(this, _startForegroundServiceDefaultExceptionMsg, nameof(PlayerException));
            }
        }
        #endregion
    }
}