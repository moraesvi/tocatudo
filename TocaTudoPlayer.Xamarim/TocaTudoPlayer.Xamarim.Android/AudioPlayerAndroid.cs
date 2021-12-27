using Android.Content;
using Plugin.CurrentActivity;
using System;
using TocaTudo;
using TocaTudoPlayer.Xamarim;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayerAndroid))]
namespace TocaTudo
{
    public class AudioPlayerAndroid : IAudio
    {
        private AudioServiceConnection _audioServiceConnection;

        private Action _playerInitializing;
        private Action<ICommonMusicModel> _playerReady;
        private Action<ICommonMusicModel> _playerReadyBuffering;
        private Action _playerSeekComplete;
        private Action<PlaylistItemServicePlayer> _playerPlaylistChanged;
        private Action<ICommonMusicModel> _playerInvalidUri;
        private Action _playerLosedAudioFocus;

        private ICommonMusicModel _musicPlayer;
        private MainActivity _mainActivity;

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
        public event Action PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus += value;
            remove => _playerLosedAudioFocus -= value;
        }
        public bool EventsBinded => _audioServiceConnection?.Binder != null;
        public bool IsPlaying => _audioServiceConnection?.Binder
                                                        ?.GetBackgroundService()
                                                        ?.IsPlaying ?? false;
        public long CurrentPosition() => _audioServiceConnection?.Binder
                                                                ?.GetBackgroundService()
                                                                ?.CurrentPosition() ?? -1;
        public void Start()
        {
            MainActivity main = CrossCurrentActivity.Current.Activity as MainActivity;

            if (main != null)
            {
                _mainActivity = main;
                _mainActivity.BinderConnected -= MainActivity_BinderConnected;
                _mainActivity.BinderConnected += MainActivity_BinderConnected;
            }
        }
        public long Max() => _audioServiceConnection?.Binder
                                                    ?.GetBackgroundService()
                                                    ?.Max() ?? -1;
        public void Pause()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionPause);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);

            if (_musicPlayer != null) 
                _musicPlayer.UpdateMusicPlayingIcon();
        }
        public bool Play()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionPlay);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);

            if (_musicPlayer != null)
                _musicPlayer.UpdateMusicPlayingIcon();

            return true;
        }
        public bool Stop()
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionStop);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);

            if (_musicPlayer != null)
                _musicPlayer.UpdateMusicPlayingIcon();

            return true;
        }
        public void Seek(int milisegundos)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSeek);
            intent.PutExtra("seek", milisegundos);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Seek(int milisegundos, PlaylistItemServicePlayer playlistItem)
        {
            Context context = Android.App.Application.Context.ApplicationContext;
            Intent intent = new Intent(context, typeof(AudioService));
            intent.SetAction(AudioService.ActionSeek);
            intent.PutExtra("seek", milisegundos);

            _mainActivity.PlaylistItemServicePlayerParameter = playlistItem;

            StartForegroundService(context, intent);
        }
        public void Seek(int milisegundos, MusicModelServicePlayer musicModel)
        {
            Context context = Android.App.Application.Context.ApplicationContext;
            Intent intent = new Intent(context, typeof(AudioService));
            intent.SetAction(AudioService.ActionSeek);
            intent.PutExtra("seek", milisegundos);

            _mainActivity.MusicModelServicePlayerParameter = musicModel;

            StartForegroundService(context, intent);
        }
        public void Source(string url)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, ICommonMusicModel musicPlayer)
        {
            _musicPlayer = musicPlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, AlbumModelServicePlayer albumServicePlayer)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            _mainActivity.AlbumModelServicePlayerParameter = albumServicePlayer;

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, MusicModelServicePlayer musicServicePlayer)
        {
            _mainActivity.MusicModelServicePlayerParameter = musicServicePlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, ICommonMusicModel musicPlayer, AlbumModelServicePlayer albumServicePlayer)
        {
            _mainActivity.AlbumModelServicePlayerParameter = albumServicePlayer;
            _musicPlayer = musicPlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(string url, ICommonMusicModel musicPlayer, MusicModelServicePlayer musicServicePlayer)
        {
            _mainActivity.MusicModelServicePlayerParameter = musicServicePlayer;
            _musicPlayer = musicPlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-url", url);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            _mainActivity.AudioServiceMusicParameter = music;

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music, AlbumModelServicePlayer albumServicePlayer)
        {
            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            _mainActivity.AudioServiceMusicParameter = music;
            _mainActivity.AlbumModelServicePlayerParameter = albumServicePlayer;

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music, ICommonMusicModel musicPlayer)
        {
            _musicPlayer = musicPlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);
            _mainActivity.AudioServiceMusicParameter = music;

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }
        public void Source(byte[] music, ICommonMusicModel musicPlayer, MusicModelServicePlayer musicServicePlayer)
        {
            _mainActivity.AudioServiceMusicParameter = music;
            _mainActivity.MusicModelServicePlayerParameter = musicServicePlayer;
            _musicPlayer = musicPlayer;

            Intent intent = new Intent(Android.App.Application.Context.ApplicationContext, typeof(AudioService));
            intent.SetAction(AudioService.ActionSource);
            intent.PutExtra("source-byte", true);

            StartForegroundService(Android.App.Application.Context.ApplicationContext, intent);
        }

        #region Private Methods
        private void MainActivity_BinderConnected()
        {
            _audioServiceConnection = _mainActivity.AudioServiceConnection;
            BindAudioServiceEvents();
        }
        private void BindAudioServiceEvents()
        {
            _audioServiceConnection.PlayerInitializing += () =>
            {
                _playerInitializing();
            };
            _audioServiceConnection.PlayerReady += () =>
            {
                _playerReady(_musicPlayer);
            };
            _audioServiceConnection.PlayerReadyBuffering += () =>
            {
                _playerReadyBuffering(_musicPlayer);
            };
            _audioServiceConnection.PlayerSeekComplete += () =>
            {
                _playerSeekComplete();
            };
            _audioServiceConnection.PlayerPlaylistChanged += (obj) =>
            {
                _playerPlaylistChanged(obj);
            };
            _audioServiceConnection.PlayerInvalidUri += () =>
            {
                if(_musicPlayer != null)
                     _playerInvalidUri(_musicPlayer);
            };
            _audioServiceConnection.PlayerLosedAudioFocus += () =>
            {
                _playerLosedAudioFocus();
            };
        }
        private void StartForegroundService(Context context, Intent intent)
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
        #endregion
    }
}